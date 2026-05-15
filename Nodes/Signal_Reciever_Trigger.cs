using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicShock.Nodes
{
    internal class Signal_Reciever_Trigger : EventNodeBase, IDynamicPortNode
    {

        new public string Icon => "radio-tower";

        private const string _typeId = "DynamicShock.Signal_Trigger";
        public override string TypeId => _typeId;
        public override string DisplayName => "Signal Trigger";
        public override string? Description => "Triggers on recieving a matching signal from an emitter node.";

        private readonly static Dictionary<string, List<IFlowNodeInstance>> signal_pairs = [];

        public override IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            .. Config_Values.Bonus_Ports()
        ];

        public override IReadOnlyDictionary<string, FlowProperty> Properties => new Dictionary<string, FlowProperty>
        {
            ["signal"] = new()
            {
                Name = "Signal Filter",
                Description = "Filter for which falling edge nodes trigger this output",
                Type = FlowPropertyType.String,
                Default = "Signal_0",
                Required = true,
            },
            ["ports"] = new()
            {
                Name = "Data Ports",
                Description = "Number of Data ports to recieve from the sending signal Node.",
                Type = FlowPropertyType.Int,
                Default = 1,
                Required = true,
                Min = 0,
                Step = 1,
            }
        };

        IReadOnlyList<FlowPort> IDynamicPortNode.GetInputPorts(IReadOnlyDictionary<string, object?> config)
        {
            return [];
        }

        IReadOnlyList<FlowPort> IDynamicPortNode.GetOutputPorts(IReadOnlyDictionary<string, object?> config)
        {
            if (config.TryGetValue("ports", out object? obj))
            {
                int ports = 0;
                if (obj is int port_int)
                {
                    ports = port_int;
                }
                if (obj is double port_double)
                {
                    try
                    {
                        ports = Convert.ToInt32(port_double);
                    }
                    catch {}
                }

                List<FlowPort> data_ports = [];
                for (int i = 0; i < ports; i++)
                {
                    data_ports.Add(FlowPort.Any("data_port_" + i, "Data_Port_" + i));
                }
                return [FlowPort.FlowOut(), .. data_ports];
            }


            return [FlowPort.FlowOut()];
        }


        public static async void Delayed_Signal(int delay, string signal, Dictionary<string, object?> data, CancellationTokenSource? cts, CancellationToken ctoken)
        {
            try
            {
                await Task.Delay(delay, ctoken);
            }
            catch {}
            
            if (ctoken.IsCancellationRequested)
            {
                return;
            }

            cts?.Cancel();
            DynamicShockPlugin.TriggerManager?.Fire_Signal_Event(_typeId, data, signal, Is_Node_Signal);
        }

        private static bool Is_Node_Signal(IFlowNodeInstance instance, string signal)
        {
            string? node_signal = instance.GetConfig<string>("signal", null);
            return signal == node_signal;
        }
    }
}
