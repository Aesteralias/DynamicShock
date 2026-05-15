using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicShock.Nodes
{
    internal class Await_All : IFlowProcessNode
    {
        public string DisplayName => "Await All";
        public string Description => "Waits until a specified number of input triggers happen in a single execution";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "badge-alert";
        public string TypeId => "DynamicShock.Await_All";

        public IFlowNodeInstance CreateInstance(string instanceId, Dictionary<string, object?> config)
        {
            return new FlowNodeInstance(instanceId, this, config);
        }
        public IReadOnlyDictionary<string, FlowProperty> Properties => Props();

        private static Dictionary<string, FlowProperty> Props() => new()
        {
            ["inputs"] = new()
            {
                Name = "Inputs",
                Description = "Number of inputs required for an output",
                Type = FlowPropertyType.Int,
                Step = 1,
                Min = 1,
                Default = 2,
                Required = true
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
            FlowPort.Number("remaining", "Remaining Triggers"),
        ];

        const string Current_Execution = "Exe";
        const string Triggers = "Trig";

        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            if (instance.GetState<string>(Current_Execution, "") != context.ExecutionId)
            {
                instance.SetState(Current_Execution, context.ExecutionId);
                instance.SetState(Triggers, (int)1);
            }
            else
            {
                instance.SetState(Triggers, instance.GetState<int>(Triggers, 1)+1);
            }
            int remaining = instance.GetConfig<int>("inputs", 1) - instance.GetState<int>(Triggers, 0);
            if (remaining <= 0)
            {
                return new()
                {
                    Outputs = new Dictionary<string, object?>()
                    {
                        ["remaining"] = remaining
                    },
                    DisplayValue = "Success",
                    ActivatedPorts = ["done"],
                };
            }

            return new()
            {
                Outputs = new Dictionary<string, object?>()
                {
                    ["remaining"] = remaining
                },
                DisplayValue = $"R: {remaining}",
                ActivatedPorts = ["blocked"],
            };
        }
    }
}
