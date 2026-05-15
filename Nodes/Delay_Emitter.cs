using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicShock.Nodes
{
    internal class Delay_Emitter : IFlowProcessNode, IDynamicPortNode, IDynamicPropertyNode
    {
        public string DisplayName => "Delay Emitter";
        public string Description => "Emits a delayed flow.";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "alarm-clock-plus";
        public string TypeId => "DynamicShock.Delay";

        public IFlowNodeInstance CreateInstance(string instanceId, Dictionary<string, object?> config)
        {
            return new FlowNodeInstance(instanceId, this, config);
        }
        public IReadOnlyDictionary<string, FlowProperty> Properties => Props();

        private static Dictionary<string, FlowProperty> Props() => new()
        {
            ["delay"] = new()
            {
                Name = "Delay (s)",
                Type = FlowPropertyType.Double,
                Default = 1,
                Min = 0,
                Step = 0.1,
                Description = "How long the signal delays.",
                Required = true,
            },
            ["signal"] = new()
            {
                Name = "Default Signal",
                Description = "Signal to use if no name is provided by the signal port.",
                Type = FlowPropertyType.String,
                Default = "Delay_0",
                Required = true,
            },
            ["ports"] = new()
            {
                Name = "Data Ports",
                Description = "Number of Data ports to send to the sending signal Node.",
                Type = FlowPropertyType.Int,
                Default = 0,
                Required = true,
                Min = 0,
                Step = 1,
            },
        };

        public IReadOnlyDictionary<string, FlowProperty> GetProperties(IReadOnlyDictionary<string, object?> config, IServiceProvider? serviceProvider)
        {
            Dictionary<string, FlowProperty> props = new()
            {
                ["delay"] = new()
                {
                    Name = "Delay (s)",
                    Type = FlowPropertyType.Double,
                    Default = 1,
                    Min = 0.1,
                    Step = 0.1,
                    Description = "How long the signal delays.",
                    Required = true,
                },
                ["signal"] = new()
                {
                    Name = "Default Signal",
                    Description = "Signal to use if no name is provided by the signal port.",
                    Type = FlowPropertyType.String,
                    Default = "Delay_0",
                    Required = true,
                },
                ["ports"] = new()
                {
                    Name = "Data Ports",
                    Description = "Number of Data ports to send to the sending signal Node.",
                    Type = FlowPropertyType.Int,
                    Default = 0,
                    Required = true,
                    Min = 0,
                    Step = 1,
                }
            };

            if (config.TryGetValue("ports", out object? obj) && obj is int ports)
            {
                for (int i = 0; i < ports; i++)
                {
                    props.Add($"port_{i}_name", new()
                    {
                        Name = $"Data Port {i} Name",
                        Type = FlowPropertyType.String,
                        Default = $"Data Port {i}"
                    });
                }
            }



            return props;
        }

        public IReadOnlyList<FlowPort> InputPorts { get; } =
        [
            FlowPort.FlowIn(),
            FlowPort.String("signal", "Signal"),
            FlowPort.Number("delay", "Delay"),
            .. Config_Values.Bonus_Ports()
        ];
        public IReadOnlyList<FlowPort> OutputPorts { get; } =
        [

        ];

        public IReadOnlyList<FlowPort> GetInputPorts(IReadOnlyDictionary<string, object?> config)
        {
            DynamicShockPlugin.Log_Info("Reading Input Ports");

            if (config.TryGetValue("ports", out object? obj))
            {
                int ports = 0;
                if (obj is int port_int)
                {
                    DynamicShockPlugin.Log_Info("Is Int");
                    ports = port_int;
                }
                if (obj is double port_double)
                {
                    try
                    {
                        DynamicShockPlugin.Log_Info("Is Double");
                        ports = Convert.ToInt32(port_double);
                    }
                    catch { }
                }

                List<FlowPort> data_ports = [];
                for (int i = 0; i < ports; i++)
                {
                    data_ports.Add(FlowPort.Any("data_port_" + i, "Data_Port_" + i));
                }
                return [
                    FlowPort.FlowIn(), FlowPort.String("signal", "Signal"),
                    .. data_ports];
            }


            return [
                FlowPort.FlowIn(), FlowPort.String("signal", "Signal")
                ];
        }

        public IReadOnlyList<FlowPort> GetOutputPorts(IReadOnlyDictionary<string, object?> config)
        {
            DynamicShockPlugin.Log_Info("Reading Output Ports");
            return [];
        }

        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            int delay = 1000;
            try
            {
                delay = Convert.ToInt32(context.GetInput<double>("delay", instance.GetConfig<double>("delay", 1))*1000);
            }
            catch{}

            string? signal = context.GetInput<string>("signal", instance.GetConfig<string>("signal", null));

            Dictionary<string, object?> outputs = [];
            try
            {
                int ports = instance.GetConfig<int>("ports", 4);
                for (int i = 0; i < ports; i++)
                {
                    outputs[$"data_port_{i}"] = context.GetInput<object?>($"data_port_{i}");
                }
            }
            catch {}

            if (signal is not null)
            {
                Signal_Reciever_Trigger.Delayed_Signal(delay, signal, outputs, null, ctoken);
            }

            return new()
            {
                StopExecution = true,
                DisplayValue = ""
            };
        }
    }
}
