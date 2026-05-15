using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicShock.Nodes
{
    internal class Falling_Edge_Emitter : IFlowProcessNode, IDynamicPortNode, IDynamicPropertyNode
    {
        public string DisplayName => "Falling Edge Emitter";
        public string Description => "Emits a delayed flow going through on a resetting timer.";
        public string Category => "Dynamic_Shock";
        public string? Color => "#E795D1";
        public string Icon => "waves-arrow-down";
        public string TypeId => "DynamicShock.Falling_Edge";

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
                Min = 0.1,
                Step = 0.1,
                Description = "How long the flow needs to wait for no subsequent triggers before continuing.",
                Required = true,
            },
            ["signal"] = new()
            {
                Name = "Default Signal",
                Description = "Signal to use if no name is provided by the signal port.",
                Type = FlowPropertyType.String,
                Default = "Falling_Edge_0",
                Required = true,
            },
            ["ports"] = new()
            {
                Name = "Data Ports",
                Description = "Number of Data ports to send to the sending signal Node.",
                Type = FlowPropertyType.Int,
                Default = 1,
                Required = true,
                Min = 0,
                Step = 1,
            }
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
                    Description = "How long the flow needs to wait for no subsequent triggers before continuing.",
                    Required = true,
                },
                ["signal"] = new()
                {
                    Name = "Default Signal",
                    Description = "Signal to use if no name is provided by the signal port.",
                    Type = FlowPropertyType.String,
                    Default = "Falling_Edge_0",
                    Required = true,
                },
                ["ports"] = new()
                {
                    Name = "Data Ports",
                    Description = "Number of Data ports to send to the sending signal Node.",
                    Type = FlowPropertyType.Int,
                    Default = 1,
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
            .. Config_Values.Bonus_Ports()
        ];
        public IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut("blocked", "Blocked"),
        ];

        IReadOnlyList<FlowPort> IDynamicPortNode.GetInputPorts(IReadOnlyDictionary<string, object?> config) => GetInputPorts(config);
        public static IReadOnlyList<FlowPort> GetInputPorts(IReadOnlyDictionary<string, object?> config)
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
        IReadOnlyList<FlowPort> IDynamicPortNode.GetOutputPorts(IReadOnlyDictionary<string, object?> config) => GetOutputPorts(config);
        public static IReadOnlyList<FlowPort> GetOutputPorts(IReadOnlyDictionary<string, object?> config)
        {
            DynamicShockPlugin.Log_Info("Reading Output Ports");
            return [FlowPort.FlowOut() , FlowPort.String("test","Testing")];
        }

        const string Token_Source = "Token_Source";
        const string Start_Stamps = "Start_Stamps";

        public async Task<FlowNodeResult> ExecuteAsync(IFlowNodeInstance instance, FlowExecutionContext context, CancellationToken ctoken)
        {
            bool send_blocked = false;
            if (instance.GetState<CancellationTokenSource>(Token_Source,null) is CancellationTokenSource cts)
            {
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    send_blocked = true;
                }
            }
            if (!send_blocked)
            {
                instance.SetState(Start_Stamps, DateTime.UtcNow.Ticks);
            }
            CancellationTokenSource ctsn = CancellationTokenSource.CreateLinkedTokenSource([ctoken]);
            instance.SetState(Token_Source, ctsn);
            CancellationToken ct = ctsn.Token;
            int delay = 1000;

            try
            {
                delay = Convert.ToInt32(instance.GetConfig<double>("delay", 1)*1000);
            }
            catch {}

            double diff = ((DateTime.UtcNow.Ticks - instance.GetState<long>(Start_Stamps)) / 10000000);

            string? signal = context.GetInput<string>("signal",instance.GetConfig<string>("signal", null));

            if (signal is not null)
            {
                Signal_Reciever_Trigger.Delayed_Signal(delay, signal, new() { ["data_port_0"] = diff }, ctsn, ct);
            }

            if (send_blocked)
            {
                return new()
                {
                    ActivatedPorts = ["blocked"],
                    DisplayValue = diff.ToString()
                };
            }
            else
            {
                return new()
                {
                    StopExecution = true,
                    DisplayValue = ""
                };
            }
        }
    }
}
