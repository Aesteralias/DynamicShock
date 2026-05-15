using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DynamicShock.Nodes
{
    internal class DynamicTransformNode : IFlowProcessNode
    {
        
        public string DisplayName => "Dynamic Transform";
        public string Description => "Change an input based on the selected transform";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "chart-spline";
        public string TypeId => "DynamicShock.Transform";

        private readonly static JsonSerializerOptions jso = new()
        {
            IncludeFields = true
        };

        public IFlowNodeInstance CreateInstance(string instanceId, Dictionary<string, object?> config)
        {
            return new FlowNodeInstance(instanceId, this, config);
        }
        public IReadOnlyDictionary<string, FlowProperty> Properties => Props();

        private static Dictionary<string, FlowProperty> Props()
        {
            List<FlowPropertyOption> transforms = [];

            string[] names = Config_Values.Get_Transform_Names();
            foreach (string name in names)
            {
                if (Config_Values.Get_Transform(name) is Transform t)
                {
                    transforms.Add(new()
                    {
                        Label = name,
                        Description = t.Description,
                        Value = JsonSerializer.Serialize(t, jso)
                    });
                }
            }

            Dictionary<string, FlowProperty> store = new() 
            {
                ["Transform"] = new()
                {
                    Name = "Transform",
                    Required = true,
                    Type = FlowPropertyType.Select,
                    Options = transforms
                }
            };

            return store;
        }

        public IReadOnlyList<FlowPort> InputPorts { get; } =
        [
            FlowPort.FlowIn(),
            FlowPort.Number("input_value","Input")
        ];
        public IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            FlowPort.Number("transformed_output","Output")
        ];

        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            double input = context.GetInput<double>("input_value", 0);
            double output = 0;

            

            if (context.GetConfig<string>("Transform") is string transform_string)
            {

                if (JsonSerializer.Deserialize<Transform>(transform_string, jso) is Transform t)
                {
                    output = t.Transform_Input(input);
                }
            }

            FlowNodeResult result = new()
            {
                Outputs = new Dictionary<string, object?>()
                {
                    ["transformed_output"] = output
                },
                DisplayValue = output.ToString(),
                ActivatedPorts = null
            };
            
            return result;
        }
    }
}
