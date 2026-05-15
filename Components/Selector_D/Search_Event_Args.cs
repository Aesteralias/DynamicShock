using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicShock.Components.Selector_D
{
    public class Search_Event_Args : EventArgs
    {
        public bool found = false;
        public string comparison = "";
    }


    public class Search_Container
    {
        public event EventHandler<Search_Event_Args>? Search_Update;

        public bool Run(string comparison)
        {
            Search_Event_Args e = new()
            {
                comparison = comparison
            };
            Search_Update?.Invoke(null, e);
            return e.found;
        }
    }
}
