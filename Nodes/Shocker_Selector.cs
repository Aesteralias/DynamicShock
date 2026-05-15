using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace DynamicShock.Nodes
{
    internal class Shocker_Selector : IFlowProcessNode
    {
        public string DisplayName => "Shocker Selector";
        public string Description => "Selects shocker(s) based on selection type.";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "battery-plus";
        public string TypeId => "DynamicShock.Shocker_Selector";

        public IFlowNodeInstance CreateInstance(string instanceId, Dictionary<string, object?> config)
        {
            return new FlowNodeInstance(instanceId, this, config);
        }
        public IReadOnlyDictionary<string, FlowProperty> Properties => Props();

        private static Dictionary<string, FlowProperty> Props()
        {
            List<FlowPropertyOption> selection_types = [];

            foreach (Selection_Type s_type in Enum.GetValues<Selection_Type>())
            {
                selection_types.Add(new()
                {
                    Label = s_type.ToString(),
                    Value = s_type.ToString()
                });
            }

            Dictionary<string, FlowProperty> store = new()
            {
                ["default_selection"] = new()
                {
                    Name = "Default Selection",
                    Type = FlowPropertyType.Select,
                    Default = Selection_Type.All.ToString(),
                    Options = selection_types,
                    Description = "What is the default selection method if none is provided",
                    Required = true,
                }
            };

            return store;
        }

        public IReadOnlyList<FlowPort> InputPorts { get; } =
        [
            FlowPort.FlowIn(),
            FlowPort.Array("shockers","Shockers"),
            FlowPort.String("selection_type","Selection Type"),
        ];
        public IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            FlowPort.Array("shockers", "Shockers"),
        ];

        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            if (context.GetInput<string[]>("shockers", []) is string[] shockers && 
                context.GetInput("selection_type", instance.GetConfig<string>("default_selection", "All")) is string method)
            {
                string[] selected = Select(shockers, method);
                return new()
                {
                    Outputs = new Dictionary<string,object?>()
                    {
                        ["shockers"] = selected
                    },
                };
            }

            return new()
            {

            };
        }

        private static readonly Random r = new();
        public static string[] Select(string[] shockers, string method)
        {
            List<string> selected = [];

            switch (method)
            {
                case ("Random"):
                    selected.Add(shockers[r.Next(0, shockers.Length)]);
                    break;
                case ("All"):
                default:
                    selected.AddRange(shockers);
                    break;
            }

            for (int i = 0; i < selected.Count; i++)
            {
                selected[i] = selected[i].Split(":")[^1];
            }

            return [.. selected];
        }
        public static string[] Select(string[] shockers, Selection_Type st)
        {
            return Select(shockers, st.ToString());
        }
    }
}
