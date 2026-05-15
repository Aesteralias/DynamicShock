using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DynamicShock.Nodes
{
    internal class Stored_Event_Node : IFlowProcessNode
    {
        public string DisplayName => "Use Stored Event";
        public string Description => "Sends out action infomation based on Event name, optionally using a number input for any associated Transforms";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "database-zap";
        public string TypeId => "DynamicShock.UseEvent";

        public IFlowNodeInstance CreateInstance(string instanceId, Dictionary<string, object?> config)
        {
            return new FlowNodeInstance(instanceId, this, config);
        }
        public IReadOnlyDictionary<string, FlowProperty> Properties => Props();

        private static Dictionary<string, FlowProperty> Props() => [];

        public IReadOnlyList<FlowPort> InputPorts { get; } =
        [
            FlowPort.FlowIn(),
            FlowPort.String("event_name","Event Name"),
            FlowPort.Number("input_value","Number Input"),
        ];
        public IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            FlowPort.String("command_type","Command Type"),
            FlowPort.Number("delay", "Delay"),
            FlowPort.Number("intensity","Intensity"),
            FlowPort.Number("duration","Duration"),
            FlowPort.Array("raw_shockers","Shocker IDs"),
            FlowPort.String("shocker_selection", "Shocker Selection"),
            FlowPort.FlowOut("failure_port", "On Failure"),
            FlowPort.String("failure_string", "Event Name or String"),
            FlowPort.Number("failure", "Failure Output"),
        ];



        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            string? event_name = context.GetInput<string>("event_name", "");
            double? input = context.GetInput<double?>("input_value", null);

            if (event_name is not null)
            {
                if (Config_Values.Get_Event(event_name) is Stored_Event se)
                {
                    Dictionary<string, object?> outputs = await se.Execute_Event(input);

                    if (outputs.TryGetValue("ports",out object? ports_obj) && ports_obj is List<string> ports)
                    {
                        outputs.Remove("ports");
                        return new()
                        {
                            Outputs = outputs,
                            ActivatedPorts = ports,
                        };
                    }
                    else if (outputs.Count == 0)
                    {
                        return new()
                        {
                            Error = $"{se.Get_Name()} Not Enabled",
                            DisplayValue = $"Event is not Enabled",
                            StopExecution = true,
                        };
                    }
                    return new()
                    {
                        Error = "Execution error",
                        DisplayValue = "Error",
                        StopExecution = true,
                    };
                }
                else
                {
                    return new()
                    {
                        Error = "Could not find " + event_name,
                        StopExecution = true,
                    };
                }
            }
            else
            {
                return new()
                {
                    Error = "No Event Name",
                    StopExecution = true,
                };
            }

        }

    }
}
