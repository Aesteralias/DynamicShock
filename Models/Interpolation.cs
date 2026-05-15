using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicShock.Models
{
    public enum Interpolation
    {
        linear = 0,
        squared = 1,
        cubed = 2,
        quartic = 3,

        square_root = 5,
        cube_root = 6,

        sin = 10,



        step_middle = 20,
        step_start = 21,
        step_end = 22,

        random = 90,
    }
}
