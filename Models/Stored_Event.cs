using MultiShock.PluginSdk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Timestamps;

namespace DynamicShock.Models
{
    public class Stored_Event
    {
        public string Name  = "";
        public string Group  = "";
        public string Description = "";

        public bool Registered_Event  = true;
        public string? Label_X;
        public double? Min_X;
        public double? Max_X;
        public int? X_Precision;

        public Dictionary<Event_Value_Types, Event_Value> Values = [];

        public List<string> Ids = [];

        public Selection_Type Selection = Selection_Type.All;
        public CommandType Command = CommandType.Vibrate;

        public string Get_Name()
        {
            string name = Group;
            if (!name.EndsWith('/') && Group != "")
            {
                name += "/";
            }
            name += Name;
            return name;
        }

        public Stored_Event()
        {
            foreach (Event_Value_Types t in Enum.GetValues<Event_Value_Types>())
            {
                Values[t] = new Event_Value(t);
            }
        }

        [JsonConstructor]
        public Stored_Event(string name, string group, bool registered_Event, string? label_X, double? min_X, double? max_X, int? x_Precision, 
            Dictionary<Event_Value_Types, Event_Value> values, List<string> ids, Selection_Type selection, CommandType command)
        {
            Name = name;
            Group = group;

            Registered_Event = registered_Event;
            Label_X = label_X;
            Min_X = min_X;
            Max_X = max_X;
            X_Precision = x_Precision;

            Values = values;
            Ids = ids;
            Selection = selection;
            Command = command;
        }


        private static Random r = new();
        const long TicksPerSecond = 10000000;
        public async Task<Dictionary<string, object?>> Execute_Event(double? iv = null)
        {
            if (Registered_Event)
            {
                Dictionary<string, object?> outputs = new()
                {
                    ["raw_shockers"] = Ids.Count == 0 ? [.. Config_Values.Default_Shockers] : Ids.ToArray() ,
                    ["shocker_selection"] = Selection.ToString(),
                    ["command_type"] = Command.ToString()
                };

                List<string> ports = ["done"];
                double rand = r.NextDouble() * 100;

                foreach (var item in Values)
                {
                    if ((item.Value.Setup & (Event_Value_Flags.Enabled | Event_Value_Flags.Main_Value)) != 0)
                    {
                        string name_base = item.Key.ToString();
                        double output = item.Value.Value(iv);

                        outputs[name_base] = item.Key switch
                        {
                            Event_Value_Types.chance => rand <= output,
                            Event_Value_Types.intensity => output > Config_Values.Global_Max_Intensity ? Config_Values.Global_Max_Intensity : output,
                            _ => output,
                        };

                        if ((item.Value.Setup & Event_Value_Flags.Flow_Output) == Event_Value_Flags.Flow_Output)
                        {
                            switch (item.Key)
                            {
                                case Event_Value_Types.failure:
                                    ports.Remove("done");
                                    ports.Add(name_base + "_port");
                                    break;
                                default:
                                    if (!ports.Contains(Event_Value_Types.failure.ToString() + "_port"))
                                    {

                                    }
                                    break;
                            }
                        }
                        if ((item.Value.Setup & Event_Value_Flags.Output_Number) == Event_Value_Flags.Output_Number)
                        {
                            outputs[name_base + "_number"] = item.Key switch
                            {
                                Event_Value_Types.duration => Convert.ToDouble(DateTime.UtcNow.Ticks) / TicksPerSecond,
                                Event_Value_Types.delay => Convert.ToDouble(DateTime.UtcNow.Ticks) / TicksPerSecond,
                                Event_Value_Types.chance => rand,
                                _ => output,
                            };
                        }
                        if ((item.Value.Setup & Event_Value_Flags.Output_String) == Event_Value_Flags.Output_String)
                        {
                            if (item.Value.String_Output == "")
                            {
                                outputs[name_base + "_string"] = item.Key switch
                                {
                                    Event_Value_Types.failure => Get_Name(),
                                    _ => ""
                                };
                            }
                            else
                            {
                                outputs[name_base + "_string"] = item.Value.String_Output;
                            }
                        }
                    }
                }


                {
                    if (outputs.TryGetValue(Event_Value_Types.delay.ToString(), out object? obj) && obj is double d)
                    {
                        int i = Convert.ToInt16(Math.Floor(d * 1000));
                        if (i > 0)
                        {
                            if (outputs.ContainsKey(Event_Value_Types.duration.ToString() + "_number"))
                            {
                                outputs[Event_Value_Types.duration.ToString() + "_number"] = Convert.ToDouble(DateTime.UtcNow.AddMilliseconds(i).Ticks) / TicksPerSecond;
                            }
                        }
                    }
                }


                {
                    bool remove_failure = true;
                    if (outputs.TryGetValue(Event_Value_Types.chance.ToString(), out object? obj) && obj is bool b)
                    {
                        remove_failure = b;
                    }
                    if (remove_failure)
                    {
                        foreach (var s in outputs.Where(kv => kv.Key.StartsWith(Event_Value_Types.failure.ToString())).ToList())
                        {
                            outputs.Remove(s.Key);
                        }
                        ports.Remove(Event_Value_Types.failure.ToString()+"_port");
                        ports.Insert(0, "done");
                    }
                }


                outputs["ports"] = ports;
                return outputs;
            }
            return [];
        }


        public Stored_Event Copy()
        {
            Stored_Event se = new();

            CopyTo(se);

            return se;
        }

        public void CopyTo(Stored_Event target)
        {
            target.Name = Name;
            target.Group = Group;
            target.Description = Description;

            target.Registered_Event = Registered_Event;
            target.Label_X = Label_X;
            target.Min_X = Min_X;
            target.Max_X = Max_X;  
            target.X_Precision = X_Precision;

            target.Values.Clear();
            foreach (KeyValuePair<Event_Value_Types, Event_Value> kp in Values)
            {
                target.Values[kp.Key] = kp.Value.Copy();
            }

            target.Ids.Clear();
            foreach (string id in Ids)
            {
                target.Ids.Add(id);
            }

            target.Selection = Selection;
            target.Command = Command;
        }

        public bool IsSame(Stored_Event other)
        {
            if (other.Name != Name ||
                other.Group != Group ||
                other.Description != Description)
            {
                //DynamicShockPlugin.Log_Info("Details Difference");
                return false;
            }
                

            if (other.Registered_Event != Registered_Event ||
                other.Label_X != Label_X ||
                other.Min_X != Min_X ||
                other.Max_X != Max_X ||
                other.X_Precision != X_Precision)
            {
                //DynamicShockPlugin.Log_Info("X-Axis Differnece");
                return false;
            }

            if (other.Values.Count != Values.Count)
            {
                //DynamicShockPlugin.Log_Info("Value Count Difference");
                return false;
            }

            foreach (var item in other.Values)
            {
                if (Values.TryGetValue(item.Key, out Event_Value? ev))
                {
                    if (!ev.IsSame(item.Value))
                    {
                        //DynamicShockPlugin.Log_Info("Value Difference Difference");
                        return false;
                    }
                }
                else
                {
                    //DynamicShockPlugin.Log_Info("Value Missing");
                    return false;
                }
            }

            if (other.Ids.Count != Ids.Count)
            {
                //DynamicShockPlugin.Log_Info("Id Count Difference");
                return false;
            }

            for (int i = 0; i < other.Ids.Count; i++)
            {
                if (other.Ids[i] != Ids[i])
                {
                    //DynamicShockPlugin.Log_Info("Id Difference");
                    return false;
                }
            }

            if (other.Selection != Selection ||
                other.Command != Command)
            {
                //DynamicShockPlugin.Log_Info("Enum Difference");
                return false;
            }

            return true;
        }

        public bool Any_Transforms()
        {
            foreach (var value in Values.Values)
            {
                if (value.Dynamic_Value != null)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public enum Selection_Type
    {
        All = 0,
        Random = 1
    }
    
}
