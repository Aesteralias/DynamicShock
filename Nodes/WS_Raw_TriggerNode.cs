using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DynamicShock.Nodes
{
    internal class WS_Raw_TriggerNode : EventNodeBase
    {
        public static new string Icon => "radio";

        private const string _typeId = "DynamicShock.WebSocket.Raw";

        public override string TypeId => _typeId;
        public override string DisplayName => "WebSocket Raw";
        public override string? Description => "Triggers on recieving a valid WebSocket message on /Aes-DynamicShock/Raw or /Aes-DynamicShock/Compatability";

        public override IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            FlowPort.Number("intensity","Intensity"),
            FlowPort.Number("duration","Duration (s)"),
            FlowPort.Array("dev_ids","Device IDs"),
            FlowPort.Array("shock_ids","Shocker IDs"),
            FlowPort.String("command","Command"),
            FlowPort.String("selection","Shocker Selection"),
            FlowPort.String("event_name","Event Name"),
            FlowPort.Number("dynamic_input","Dynamics Input"),
        ];

        public override IReadOnlyDictionary<string, FlowProperty> Properties => new Dictionary<string, FlowProperty>
        {
            
        };


        private struct Raw_DeSerial
        {
            public int Intensity;
            public int Duration;
            public int[] Dev_Ids;
            public int[] Shocker_Ids;
            public string Command;
            public string Selection;
            public string? Event_Name;
            public double? Dynamic_Input;
        }

        static readonly JsonSerializerOptions JSoptions = new() { IncludeFields = true };
        internal static string? RawWebSocket_OnMessage(string message)
        {
            Raw_DeSerial raw = JsonSerializer.Deserialize<Raw_DeSerial>(message, JSoptions);

            try
            {
                if (raw.Dev_Ids.Length == 0 && raw.Shocker_Ids.Length == 0)
                {
                    List<int> temp_ids = [];
                    foreach (string shocker in Config_Values.Default_Shockers)
                    {
                        try
                        {
                            temp_ids.Add(Convert.ToInt32(shocker.Split(":")[^1]));
                        }
                        catch {}
                    }

                    raw.Shocker_Ids = [.. temp_ids];
                }

                double inten = Convert.ToDouble(raw.Intensity);
                if (inten < 0)
                {
                    inten = 0;
                }
                if (inten > 100)
                {
                    inten = 100;
                }


                double dura = Convert.ToDouble(raw.Duration);
                if (dura >= 100)
                {
                    dura /= 1000;
                }
                if (dura < 0.1)
                {
                    dura = 0.1;
                }
                if (dura > 15)
                {
                    dura = 15;
                }

                string comm = raw.Command;
                if (comm != "Shock" && comm != "Vibrate" && comm != "Stop")
                {
                    comm = "Vibrate";
                }

                string sel = raw.Selection;
                if (comm != "All" && comm != "Random")
                {
                    comm = "Random";
                }


                Dictionary<string, object?> outputs = new()
                {
                    ["intensity"] = inten,
                    ["duration"] = dura,
                    ["dev_ids"] = raw.Dev_Ids,
                    ["shock_ids"] = raw.Shocker_Ids,
                    ["command"] = comm,
                    ["selection"] = sel,
                    ["event_name"] = raw.Event_Name,
                    ["dynamic_input"] = raw.Dynamic_Input,
                };

                _ = DynamicShockPlugin.TriggerManager?.FireEvent(_typeId, outputs);

                return "Successful Command";
            }
            catch {}

            return "Failed Command";
        }


        internal static string Compatability_OnLoad()
        {
            //TODO Send All Currently Active Shockers


            return "";
        }

        internal static string? Compatability_OnMessage(string message)
        {
            //TODO 


            return "";
        }
    }
}
