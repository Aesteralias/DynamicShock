using System.Text.Json.Serialization;

namespace DynamicShock.Models
{
    public class Keyframe
    {
        public double Input;
        public double Output;
        public Interpolation Interpolation = Interpolation.linear;
        private static Random rand = new();

        [JsonConstructor]
        public Keyframe(double input, double output, Interpolation interpolation = Interpolation.linear)
        {
            Input = input;
            Output = output;
            Interpolation = interpolation;
        }


        public Keyframe Copy()
        {
            Keyframe k = new(Input,Output, Interpolation);

            return k;
        }

        public bool IsSame(Keyframe other)
        {
            bool b = (Input == other.Input && 
                Output == other.Output && 
                Interpolation == other.Interpolation);

            return b;
        }

        public void Next_Interpolation()
        {
            Interpolation[] list = Enum.GetValues<Interpolation>();

            for (int i = 0; i < list.Length-1; i++)
            {
                if (Interpolation == list[i])
                {
                    Interpolation = list[i + 1];
                    return;
                }
            }
            Interpolation = Interpolation.linear;
        }


        public double[][] Interpolate_Data(Keyframe next_keyframe, double step_value)
        {
            return Interpolate_Data(next_keyframe.Input, next_keyframe.Output, step_value);
        }

        public double[][] Interpolate_Data(double end_input, double end_output, double step_value)
        {
            List<double[]> inter_data = [];

            for (double x = Input; x <= end_input; x += step_value) 
            {
                inter_data.Add(Interpolate_Output(Input, end_input, Output, end_output, x, Interpolation));
            }


            return [.. inter_data];
        }

        public double Transform_Value(Keyframe next_keyframe, double value)
        {
            return Interpolate_Output(Input, next_keyframe.Input, Output, next_keyframe.Output, value, Interpolation)[1];
        }

        public static double[] Interpolate_Output(double start_input, double end_input, double start_output, double end_output, double input_value, Interpolation inter_mode)
        {
            double output_value = 0;

            double fraction_inter = (input_value - start_input) / (end_input - start_input);
            double output_difference = end_output - start_output;

            switch (inter_mode)
            {
                default:
                case Interpolation.linear:
                    output_value = start_output + (fraction_inter * output_difference);
                    break;
                case Interpolation.squared:
                    output_value = start_output + (Math.Pow(fraction_inter, 2) * output_difference);
                    break;
                case Interpolation.cubed:
                    output_value = start_output + (Math.Pow(fraction_inter, 3) * output_difference);
                    break;
                case Interpolation.quartic:
                    output_value = start_output + (Math.Pow(fraction_inter, 4) * output_difference);
                    break;

                case Interpolation.square_root:
                    output_value = start_output + (Math.Sqrt(fraction_inter) * output_difference);
                    break;
                case Interpolation.cube_root:
                    output_value = start_output + (Math.Cbrt(fraction_inter) * output_difference);
                    break;

                case Interpolation.sin:
                    output_value = start_output + ((1 - Math.Cos(fraction_inter * Math.PI / 2)) * output_difference);
                    break;
                case Interpolation.step_middle:
                    if (fraction_inter <0.5)
                    {
                        output_value = start_output;
                    }
                    else
                    {
                        output_value = end_output;
                    }
                    break;
                case Interpolation.step_start:
                    output_value = end_output;
                    break;
                case Interpolation.step_end:
                    output_value = start_output;
                    break;
                case Interpolation.random:
                    output_value = start_output + (rand.NextDouble() * output_difference);
                    break;
            }


            return [input_value, output_value];
        }
    }
}
