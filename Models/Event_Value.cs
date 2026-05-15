using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DynamicShock.Models
{
    public class Event_Value
    {
        public Event_Value_Flags Setup = 0;
        public Event_Value_Types Value_Type;
        public string String_Output = "";
        public double Static_Value;
        public Transform? Dynamic_Value;

        [JsonConstructor]
        public Event_Value(Event_Value_Flags setup, Event_Value_Types value_type, string string_output, double static_value, Transform? dynamic_value)
        {
            Setup = setup;
            Value_Type = value_type;
            String_Output = string_output;
            Static_Value = static_value;
            Dynamic_Value = dynamic_value;
        }


        public double Value(double? input = null)
        {
            if (input == null || Dynamic_Value == null)
            {
                return Static_Value;
            }
            return Dynamic_Value.Transform_Input((double)input);
        }

        public Event_Value Copy()
        {
            Event_Value ev = new();

            CopyTo(ev);

            return ev;
        }
        public void CopyTo(Event_Value target)
        {
            target.Setup = Setup;
            target.Value_Type = Value_Type;

            target.String_Output = String_Output;
            target.Static_Value = Static_Value;
            target.Dynamic_Value = Dynamic_Value?.Copy();
        }


        public string Name()
        {
            return Value_Type switch
            {
                Event_Value_Types.intensity => "Intensity (%)",
                Event_Value_Types.duration => "Duration (s)",
                Event_Value_Types.delay => "Delay (s)",
                Event_Value_Types.chance => "Success (%)",
                Event_Value_Types.failure => "Failure",
                _ => "",
            };
        }
        public string Description()
        {
            return Value_Type switch
            {
                Event_Value_Types.intensity => "How strong the Action is",
                Event_Value_Types.duration => "How long the Action (in seconds) lasts",
                Event_Value_Types.delay => "How long to delay (in seconds) the Action by",
                Event_Value_Types.chance => "The chance that when activated the shock goes through.",
                Event_Value_Types.failure => "Provides node outputs when the event fails to activate via Success (%)",
                _ => "",
            };
        }
        public Event_Value(Event_Value_Types t = Event_Value_Types.intensity)
        {
            Value_Type = t;
            Setup = t switch
            {
                Event_Value_Types.intensity => Event_Value_Flags.Main_Default,
                Event_Value_Types.duration => Event_Value_Flags.Main_Default,
                Event_Value_Types.delay => Event_Value_Flags.None,
                Event_Value_Types.chance => Event_Value_Flags.None,
                Event_Value_Types.failure => Event_Value_Flags.Failure_Default,
                _ => 0,
            };
            Static_Value = t switch
            {
                Event_Value_Types.intensity => 20,
                Event_Value_Types.duration => 1.5,
                Event_Value_Types.delay => 3,
                Event_Value_Types.chance => 100,
                Event_Value_Types.failure => 10,
                _ => 0,
            };

        }

        

        public bool IsSame(Event_Value other)
        {
            if ((other.Setup ^ Setup) != 0)
                return false;

            if (other.Value_Type != Value_Type) 
                return false;

            if (other.String_Output != String_Output ||
                other.Static_Value != Static_Value)
            {
                return false;
            }

            if (Dynamic_Value is not null && other.Dynamic_Value is not null)
            {
                if (!Dynamic_Value.IsSame(other.Dynamic_Value))
                {
                    return false;
                }
            }
            else if (other.Dynamic_Value != Dynamic_Value)
            {
                return false;
            }

            return true;
        }
    }

    public enum Event_Value_Types
    {
        intensity = 0,
        duration = 1,
        delay = 2,
        chance = 3,
        failure = 4,
    }

    [Flags]
    public enum Event_Value_Flags
    {
        None = 0,
        Flow_Output = 1,
        Output_String = 2,
        Output_Number = 4,
        Main_Value = 8,
        Enabled = 16,


        Main_Default = Enabled | Main_Value,
        Failure_Default = Output_Number | Output_String | Flow_Output,
    }
}
