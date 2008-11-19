using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestCode
{
    public class Country
    {
        public Country(int id, string name, string capital, double population)
        {
            ID = id;
            Name = name;
            Capital = capital;
            Population = population;
        }

        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Capital
        {
            get;
            set;
        }

        public double Population
        {
            get;
            set;
        }
    }
}