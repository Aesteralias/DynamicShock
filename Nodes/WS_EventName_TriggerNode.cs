using DynamicShock.Models;
using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DynamicShock.Nodes
{
    internal class WS_EventName_TriggerNode : EventNodeBase
    {
        public static new string Icon => "radio";

        private const string _typeId = "DynamicShock.WebSocket.EventName";
        public override string TypeId => _typeId;
        public override string DisplayName => "WebSocket Event Name";
        public override string? Description => "Triggers on recieving a valid WebSocket message on /Aes-DynamicShock/EventName";

        public override IReadOnlyList<FlowPort> OutputPorts { get; } =
        [
            FlowPort.FlowOut(),
            FlowPort.String("event_name","Event Name"),
            FlowPort.Number("dynamic_input","Dynamics Input"),
        ];

        public override IReadOnlyDictionary<string, FlowProperty> Properties => new Dictionary<string, FlowProperty>
        {

        };
        

        static readonly JsonSerializerOptions JSoptions = new() { IncludeFields = true };
        private struct Event_DeSerial
        {
            public string Event_Name;
            public double? Input;
            public Stored_Event[]? Register_Events;
        }


        internal static string? Event_WebSocket_OnMessage(string message)
        {
            try
            {
                Event_DeSerial eds = JsonSerializer.Deserialize<Event_DeSerial>(message, JSoptions);

                if (eds.Register_Events is Stored_Event[] sea)
                {
                    if (Config_Values.Websocket_Logging)
                        DynamicShockPlugin.Log_Info("Registering Events");
                    bool registered = false;
                    foreach (Stored_Event se in sea)
                    {
                        if (Config_Values.Get_Event(se.Get_Name()) is null)
                        {
                            if (!Config_Values.Default_Enabled)
                            {
                                se.Registered_Event = false;
                            }
                            Config_Values.Save_Event(se, "");
                            registered = true;
                        }
                    }
                    if (registered)
                    {
                        return "Successful Registration";
                    }
                    return "No Registrations";
                }
                else if (Config_Values.Get_Event(eds.Event_Name) is not null)
                {
                    Dictionary<string, object?> outputs = new()
                    {
                        ["event_name"] = eds.Event_Name,
                        ["dynamic_input"] = eds.Input
                    };
                    _ = DynamicShockPlugin.TriggerManager?.FireEvent(_typeId, outputs);
                    return "Successful Command";
                }
            }
            catch {}
            

            if (Config_Values.Websocket_Logging)
                DynamicShockPlugin.Log_Info("Invalid Event Message");

            return "Failed Command";
        }
    }
}
