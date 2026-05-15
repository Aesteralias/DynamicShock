using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DynamicShock.Nodes
{
    internal class Rising_Edge_Node :IFlowProcessNode
    {
        public string DisplayName => "Rising Edge Gate";
        public string Description => "Prevents subsequent flows going through on a resetting timer.";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "waves-arrow-up";
        public string TypeId => "DynamicShock.Rising_Edge";

        public IFlowNodeInstance CreateInstance(string instanceId, Dictionary<string, object?> config)
        {
            return new FlowNodeInstance(instanceId, this, config);
        }
        public IReadOnlyDictionary<string, FlowProperty> Properties => Props();

        private static Dictionary<string, FlowProperty> Props() => new()
        {
            ["refresh_rate"] = new()
            {
                Name = "Refresh Rate (s)",
                Type = FlowPropertyType.Double,
                Default = 1,
                Min = 0.1,
                Step = 0.1,
                Description = "How long before another flow can go through, another happening before will reset this timer.",
                Required = true,
            }
        };

        public IReadOnlyList<FlowPort> InputPorts { get; } =
        [
            FlowPort.FlowIn(),
        ];
        public IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            FlowPort.FlowOut("blocked", "Blocked"),
            FlowPort.Number("time_remaining","Remaining Time")
        ];

        const string Tick_Gate = "Tick_Gate";
        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            double refresh_rate = context.GetConfig<double>("refresh_rate", 0.25);
            FlowNodeResult result = new()
            {
                Outputs = new Dictionary<string, object?>() { ["time_remaining"] = 0 },
                ActivatedPorts = ["done"],
                DisplayValue = "0"
            };

            long Limit = instance.GetState<long>(Tick_Gate, -1);
            if (Limit >= 0)
            {
                double diff = Limit - DateTime.UtcNow.Ticks;
                if (diff > 0)
                {
                    result = new()
                    {
                        Outputs = new Dictionary<string, object?>() { ["time_remaining"] = diff / 10000000 },
                        ActivatedPorts = ["blocked"],
                        DisplayValue = (diff / 10000000).ToString(),
                    };
                }
            }
            instance.SetState(Tick_Gate, Convert.ToDouble(DateTime.UtcNow.AddSeconds(refresh_rate).Ticks));
            return result;
        }
    }
}
