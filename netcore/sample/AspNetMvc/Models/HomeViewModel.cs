using System;

namespace AspNetMvc.Models
{
    public class HomeViewModel
    {
        public string Info 
        {
            get
            {
                // TODO: detect runtime
                return typeof(object).Assembly.FullName;
            }
        }
    }
}