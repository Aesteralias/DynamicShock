using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace DynamicShock.Models
{
    public class Transform
    {
        public List<Keyframe> Keyframes = [
            new Keyframe(10, 5, Interpolation.linear),
            new Keyframe(90, 15, Interpolation.linear)
            ];
        public string T_Name = "";
        public string Group = "";
        public string Description = "";

        private double _min_x = 0;
        private double _max_x = 100;
        public string Label_X = "Input Value";
        public int X_Precision = 0;

        private double _min_y = 0;
        private double _max_y = 100;
        public string Label_Y = "Intensity";
        public int Y_Precision = 0;

        public double Clamp_Min = 0;
        public double Clamp_Max = 100;



        public double Min_X
        {
            get
            {
                return _min_x;
            }
            set
            {
                _min_x = value;
                foreach (Keyframe key in Keyframes)
                {
                    if (key.Input < _min_x)
                    {
                        key.Input = _min_x;
                    }
                }
                
            }
        }
        public double Max_X
        {
            get
            {
                return _max_x;
            }
            set
            {
                _max_x = value;
                foreach (Keyframe key in Keyframes)
                {
                    if (key.Input > _max_x)
                    {
                        key.Input = _max_x;
                    }
                }
                
            }
        } 


        public double Min_Y
        {
            get
            {
                return _min_y;
            }
            set
            {
                _min_y = value;
                foreach (Keyframe key in Keyframes)
                {
                    if (key.Output < _min_y)
                    {
                        key.Output = _min_y;
                    }
                }
                if (Clamp_Min < _min_y)
                {
                    Clamp_Min = _min_y;
                }
            }
        } 
        public double Max_Y
        {
            get
            {
                return _max_y;
            }
            set
            {
                _max_y = value;
                foreach (Keyframe key in Keyframes)
                {
                    if (key.Output > _max_y)
                    {
                        key.Output = _max_y;
                    }
                }
                if (Clamp_Max > _max_y)
                {
                    Clamp_Min = _max_y;
                }
            }
        }

        public string Get_Name()
        {
            string name = Group;
            if (!name.EndsWith('/') && Group != "")
            {
                name += "/";
            }
            name += T_Name;
            return name;
        }

        public Transform(Event_Value_Types t = Event_Value_Types.intensity)
        {
            switch (t)
            {
                
                case Event_Value_Types.duration:
                    Keyframes = [
                        new Keyframe(10, 0.5, Interpolation.linear),
                        new Keyframe(90, 5, Interpolation.linear)
                    ];

                    Min_Y = 0.1;
                    Max_Y = 15;
                    Label_Y = "Duration";
                    Y_Precision = 1;

                    Clamp_Min = 0.1;
                    Clamp_Max = 15;
                    break;
                case Event_Value_Types.delay:
                    Keyframes = [
                        new Keyframe(10, 0, Interpolation.linear),
                        new Keyframe(90, 15, Interpolation.linear)
                    ];

                    Min_Y = 0;
                    Max_Y = 60;
                    Label_Y = "Delay";
                    Y_Precision = 1;

                    Clamp_Min = 0;
                    Clamp_Max = 60;
                    break;
                case Event_Value_Types.chance:
                    Keyframes = [
                        new Keyframe(20, 45, Interpolation.linear),
                        new Keyframe(40, 100, Interpolation.linear)
                    ];

                    Min_Y = 0;
                    Max_Y = 100;
                    Label_Y = "Chance";
                    Y_Precision = 1;

                    Clamp_Min = 0;
                    Clamp_Max = 100;
                    break;
                case Event_Value_Types.failure:
                    Keyframes = [
                        new Keyframe(0, 10, Interpolation.linear),
                        new Keyframe(90, 100, Interpolation.linear)
                    ];

                    Min_Y = 0;
                    Max_Y = 100;
                    Label_Y = "Failure";
                    Y_Precision = 1;

                    Clamp_Min = 0;
                    Clamp_Max = 100;
                    break;
                case Event_Value_Types.intensity:
                default:
                    break;
            }
        }


        public Transform Copy()
        {
            Transform g = new();

            CopyTo(g);

            return g;
        }

        public void CopyTo(Transform target, bool copy_keyframes = false, bool copy_x_axis = false, bool copy_y_axis = false, bool copy_details = false)
        {
            if (!copy_keyframes && !copy_x_axis && !copy_y_axis && !copy_details)
            {
                copy_keyframes = true;
                copy_x_axis = true; 
                copy_y_axis = true;
                copy_details = true;
            }

            if (copy_keyframes)
            {
                target.Keyframes.Clear();

                foreach (Keyframe key in Keyframes)
                {
                    target.Keyframes.Add(key.Copy());
                }
            }
            
            if (copy_details)
            {
                target.T_Name = T_Name;
                target.Group = Group;
                target.Description = Description;
            }
            
            if (copy_x_axis)
            {
                target.Min_X = Min_X;
                target.Max_X = Max_X;
                target.Label_X = Label_X;
                target.X_Precision = X_Precision;
            }
            
            if (copy_y_axis)
            {
                target.Min_Y = Min_Y;
                target.Max_Y = Max_Y;
                target.Label_Y = Label_Y;
                target.Y_Precision = Y_Precision;

                target.Clamp_Min = Clamp_Min;
                target.Clamp_Max = Clamp_Max;
            }
        }


        public bool IsSaved(Transform other)
        {
            if (T_Name == "" || other.T_Name == "")
            {
                return false;
            }
            return IsSame(other);
        }

        public bool IsSame(Transform other)
        {
            if (Keyframes.Count != other.Keyframes.Count) return false;

            for (int i = 0; i < Keyframes.Count; i++)
            {
                if (!Keyframes[i].IsSame(other.Keyframes[i]))
                {
                    return false;
                }
            }

            if (T_Name != other.T_Name ||
                Group != other.Group ||
                Description != other.Description)
                return false;

            if (Min_X != other.Min_X ||
                Max_X != other.Max_X ||
                Label_X != other.Label_X ||
                X_Precision != other.X_Precision)
                return false;

            if (Min_Y != other.Min_Y ||
                Max_Y != other.Max_Y ||
                Label_Y != other.Label_Y ||
                Y_Precision != other.Y_Precision)
                return false;

            if (Clamp_Min != other.Clamp_Min ||
                Clamp_Max != other.Clamp_Max)
                return false;

            return true;
        }


        public double Transform_Input(double input)
        {
            if (Keyframes.Count == 0)
            {
                return 0;
            }
            else
            {
                if (input <= Keyframes[0].Input)
                {
                    return Keyframes[0].Output;
                }
                if (input >= Keyframes[^1].Input)
                {
                    return Keyframes[^1].Output;
                }
                
                
                for (int i = 0; i < Keyframes.Count - 1; i++)
                {
                    if (input >= Keyframes[i].Input && input < Keyframes[i+1].Input)
                    {
                        return Keyframes[i].Transform_Value(Keyframes[i + 1], input);
                    }
                }
            }

            return 0;
        }


        [JsonConstructor]
        public Transform(List<Keyframe> keyframes, string t_name, string group, string description, 
            double min_x, double max_x, string label_x, int x_precision, 
            double min_y, double max_y, string label_y, int y_precision, 
            double clamp_min, double clamp_max)
        {
            Keyframes = keyframes;
            T_Name = t_name;
            Group = group;
            Description = description;

            _min_x = min_x;
            _max_x = max_x;
            Label_X = label_x;
            X_Precision = x_precision;

            _min_y = min_y;
            _max_y = max_y;
            Label_Y = label_y;
            Y_Precision = y_precision;

            Clamp_Min = clamp_min;
            Clamp_Max = clamp_max;
        }
    }
}
