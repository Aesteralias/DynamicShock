using MultiShock.PluginSdk.Flow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicShock.Models
{
    internal class Config_Value_Values
    {
        public bool Websocket_Enabled = true;
        public bool Default_Enabled = false;
        public bool Websocket_Logging = false;

        public List<string> Default_Shockers = [];
        public double Global_Max_Intensity = 100;
        public int Signal_Port_Number = 3;

        internal Config_Value_Values()
        {

        }

        [JsonConstructor]
        public Config_Value_Values(bool websocket_Enabled, bool default_Enabled, bool websocket_Logging, List<string> default_Shockers, double global_Max_Intensity, int signal_Port_Number)
        {
            Websocket_Enabled = websocket_Enabled;
            Default_Enabled = default_Enabled;
            Websocket_Logging = websocket_Logging;
            Default_Shockers = [.. default_Shockers];
            Global_Max_Intensity = global_Max_Intensity;
            Signal_Port_Number = signal_Port_Number;
        }

    }

    internal static class Config_Values
    {
        private static Config_Value_Values Values = new();

        internal static bool Websocket_Enabled { get => Values.Websocket_Enabled; set { Values.Websocket_Enabled = value; Save_Configs(); } }
        internal static bool Websocket_Logging { get => Values.Websocket_Logging; set { Values.Websocket_Logging = value; Save_Configs(); } }
        internal static bool Default_Enabled { get => Values.Default_Enabled; set { Values.Default_Enabled = value; Save_Configs(); } }
        internal static List<string> Default_Shockers { get => Values.Default_Shockers; set { Values.Default_Shockers = [.. value]; Save_Configs(); } }
        internal static double Global_Max_Intensity { get => Values.Global_Max_Intensity; set { Values.Global_Max_Intensity = value; Save_Configs(); }}
        internal static int Signal_Port_Number { get => Values.Signal_Port_Number; set { Values.Signal_Port_Number = value; Save_Configs(); }}

        internal static List<FlowPort> Bonus_Ports()
        {
            List<FlowPort> fp = [];
            for (int i = 0; i < Signal_Port_Number; i++)
            {
                fp.Add(FlowPort.Any($"data_port_{i}", $"Data_Port_{i}"));
            }
            return fp;
        }

        internal static void Load(string? path = null)
        {
            if (path is string s)
            {
                event_file_path = Path.Combine(s, event_file_name);
                config_file_path = Path.Combine(s, config_file_name);
                transform_file_path = Path.Combine(s, transform_file_name);
            }

            Load_Events();
            Load_Configs();
            Load_Transforms();
        }


        private static Dictionary<string, Transform> Saved_Transforms = [];
        internal readonly static Dictionary<string, List<string>> Transform_Tree = [];

        private static Dictionary<string, Stored_Event> Saved_Events = [];
        internal readonly static Dictionary<string, List<string>> Event_Tree = [];

        internal readonly static Dictionary<string, List<string>> Combined_Tree = [];

        public static Stored_Event? Get_Event(string event_name)
        {
            if (Saved_Events.TryGetValue(event_name, out Stored_Event? se))
            {
                return se;
            }
            return null;
        }

        public static Transform? Get_Transform(string transform_name)
        {
            if (Saved_Transforms.TryGetValue(transform_name, out Transform? g))
            {
                return g;
            }

            try
            {
                string[] parts = transform_name.Split('/');
                string combined = "";
                for (int i = 0; i < parts.Length - 2; i++)
                {
                    combined += parts[i] + "/";
                }
                combined += parts[^2];

                if (Saved_Events.TryGetValue(combined, out Stored_Event? se))
                {
                    foreach (var item in se.Values)
                    {
                        if (item.Key.ToString() == parts[^1])
                        {
                            if (item.Value.Dynamic_Value is Transform t)
                            {
                                Transform t_copy = t.Copy();
                                t_copy.T_Name = transform_name + "-Copy";
                                return t_copy;
                            }
                        }
                    }
                }
            }
            catch {}
            


            

            return null;
        }

        public static string[] Get_Event_Names()
        {
            return [.. Saved_Events.Keys];
        }

        public static string[] Get_Transform_Names()
        {
            return [.. Saved_Transforms.Keys];
        }

        public static void Remove_Events(Stored_Event[] ses)
        {
            foreach (Stored_Event e in ses)
            {
                if (Get_Event(e.Get_Name()) is Stored_Event r)
                {
                    Remove_Group(Event_Tree, r.Get_Name());
                    Combined_Tree[r.Get_Name()+"/"] = [];
                    Remove_Group(Combined_Tree, r.Get_Name() + "/");
                    Saved_Events.Remove(r.Get_Name());
                }
            }
        }

        public static void Remove_Transforms(Transform[] ts)
        {
            foreach (Transform e in ts)
            {
                if (Get_Transform(e.Get_Name()) is Transform t)
                {
                    Remove_Group(Combined_Tree, t.Get_Name());
                    Remove_Group(Event_Tree, t.Get_Name());
                    Saved_Transforms.Remove(t.Get_Name());
                }
            }
        }

        public static string? Save_Event(Stored_Event se, string old_name)
        {
            if (se.Name == "")
            {
                return null;
            }

            if (se.Get_Name() != old_name)
            {
                Saved_Events.Remove(old_name);

                string name_check = se.Get_Name();
                int? index_checked = null;
                while (Saved_Events.ContainsKey(name_check))
                {
                    if (index_checked == null)
                    {
                        index_checked = 0;
                    }
                    else
                    {
                        index_checked++;
                    }
                    name_check = se.Get_Name() + "_" + index_checked.ToString();
                }
                if (index_checked != null)
                {
                    se.Name = se.Name + "_" + index_checked.ToString();
                }
                Saved_Events[name_check] = se;
                Save_Events();
                Add_Group(Event_Tree, se.Get_Name(), old_name);

                Combined_Tree[old_name + "/"] = [];
                Remove_Group(Combined_Tree, old_name + "/");
                foreach (var e in se.Values)
                {
                    if (e.Value.Dynamic_Value != null)
                    {
                        Add_Group(Combined_Tree, se.Get_Name() + "/" + e.Key.ToString());
                    }
                }

                return se.Name;
            }
            else
            {
                Saved_Events[se.Get_Name()] = se;
                Save_Events();

                Remove_Group(Combined_Tree, old_name + "/");
                foreach (var e in se.Values)
                {
                    if (e.Value.Dynamic_Value != null)
                    {
                        Add_Group(Combined_Tree, se.Get_Name() + "/" + e.Key.ToString());
                    }
                }
                return se.Name;
            }
        }

        public static string? Save_Transform(Transform transform, string old_name)
        {
            if (transform.T_Name == "")
            {
                return null;
            }

            if (transform.Get_Name() != old_name)
            {
                Saved_Transforms.Remove(old_name);

                string name_check = transform.Get_Name();
                int? index_checked = null;
                while (Saved_Transforms.ContainsKey(name_check))
                {
                    if (index_checked == null)
                    {
                        index_checked = 0;
                    }
                    else
                    {
                        index_checked++;
                    }
                    name_check = transform.Get_Name() + "_" + index_checked.ToString();
                }
                if (index_checked != null)
                {
                    transform.T_Name = transform.T_Name + "_" + index_checked.ToString();
                }
                Saved_Transforms[name_check] = transform;
                Save_Transforms();
                Add_Group(Transform_Tree, transform.Get_Name(), old_name);
                Add_Group(Combined_Tree, transform.Get_Name(), old_name);
                return transform.T_Name;
            }
            else
            {
                Saved_Transforms[transform.Get_Name()] = transform;
                Save_Transforms();
                return transform.T_Name;
            }
        }

        private static readonly JsonSerializerOptions jso = new()
        {
            IncludeFields = true,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };


        internal static void Save_Events()
        {
            try
            {
                using StreamWriter sw = new(event_file_path);
                string temp = JsonSerializer.Serialize(Saved_Events, jso);
                sw.WriteLine(temp);
            }
            catch { }
        }

        internal static void Load_Events()
        {
            try
            {
                using StreamReader sr = new(event_file_path);
                if (sr.ReadToEnd() is string data)
                {
                    if (JsonSerializer.Deserialize<Dictionary<string, Stored_Event>>(data, jso) is Dictionary<string, Stored_Event> event_data)
                    {
                        Saved_Events = event_data;

                        foreach (var event_ in event_data.Values)
                        {
                            Add_Group(Event_Tree, event_.Get_Name());

                            foreach (var e in event_.Values)
                            {
                                if (e.Value.Dynamic_Value != null)
                                {
                                    Add_Group(Combined_Tree, event_.Get_Name() + "/" + e.Key.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch { DynamicShockPlugin.Log_Info("Events Failed To Load"); }
        }


        internal static void Save_Transforms()
        {
            try
            {
                using StreamWriter sw = new(transform_file_path);
                string temp = JsonSerializer.Serialize(Saved_Transforms, jso);
                sw.WriteLine(temp);
            }
            catch { }
        }

        internal static void Load_Transforms()
        {
            try
            {
                using StreamReader sr = new(transform_file_path);
                if (sr.ReadToEnd() is string data)
                {
                    if (JsonSerializer.Deserialize<Dictionary<string, Transform>>(data, jso) is Dictionary<string, Transform> transform_data)
                    {
                        Saved_Transforms = transform_data;

                        foreach (var transform in transform_data.Values)
                        {
                            Add_Group(Transform_Tree, transform.Get_Name());
                            Add_Group(Combined_Tree, transform.Get_Name());
                        }
                    }
                }
            }
            catch { DynamicShockPlugin.Log_Info("Transforms Failed To Load"); }
        }

        internal static void Save_Configs()
        {
            try
            {
                using StreamWriter sw = new(config_file_path);
                string temp = JsonSerializer.Serialize(Values, jso);
                sw.WriteLine(temp);
            }
            catch { }
        }
        internal static void Load_Configs()
        {
            try
            {
                using StreamReader sr = new(config_file_path);
                if (sr.ReadToEnd() is string data)
                {
                    if (JsonSerializer.Deserialize<Config_Value_Values>(data, jso) is Config_Value_Values value_data)
                    {
                        Values = value_data;
                    }
                }
            }
            catch { DynamicShockPlugin.Log_Info("Config Failed To Load"); }
        }

        private static void Add_Group(Dictionary<string, List<string>> holder, string name)
        {
            string[] group_parts = name.Split('/');

            string previous = "";
            for (int i = 0; i < group_parts.Length; i++)
            {
                string combined = "";
                for (int j = 0; j <= i; j++)
                {
                    combined += group_parts[j];
                    if (j != group_parts.Length - 1)
                    {
                        combined += "/";
                    }
                }

                if (!holder.TryGetValue(previous, out List<string>? value))
                {
                    value = [];
                    holder[previous] = value;
                }
                if (combined != "" && !value.Contains(combined))
                {
                    value.Add(combined);
                }

                previous = combined;
            }
        }



        private static void Add_Group(Dictionary<string, List<string>> holder, string name, string old_name)
        {
            if (name == old_name)
            {
                return;
            }
            else if (old_name == "")
            {
                Add_Group(holder, name);
                return;
            }
            Remove_Group(holder, old_name);
            Add_Group(holder, name);
        }

        static void Remove_Group(Dictionary<string, List<string>> holder, string old_name)
        {
            string previous = old_name;
            string[] group_parts = ["", .. old_name.Split("/")];
            for (int i = group_parts.Length - 2; i >= 0; i--)
            {
                string combined = "";
                for (int j = 0; j <= i; j++)
                {
                    combined += group_parts[j];
                    if (group_parts[j] != "")
                    {
                        combined += "/";
                    }
                }

                if (holder.TryGetValue(combined, out List<string>? values))
                {
                    values.Remove(previous);
                    if (values.Count == 0)
                    {
                        previous = combined;
                        continue;
                    }
                }
                break;
            }
        }

        

        internal const string event_file_name = "stored_events.txt";
        internal static string event_file_path =
            Path.Combine([Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PiShock", "MultiShock","PluginData", "Aes-DynamicShock",
            event_file_name]);

        internal const string transform_file_name = "stored_transforms.txt";
        internal static string transform_file_path =
            Path.Combine([Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PiShock", "MultiShock","PluginData", "Aes-DynamicShock",
            transform_file_name]);

        internal const string config_file_name = "config.txt";
        internal static string config_file_path =
            Path.Combine([Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PiShock", "MultiShock","PluginData", "Aes-DynamicShock",
            config_file_name]);

    }
}
