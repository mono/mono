using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test_NUnit.Linq_101_Samples
{
    [global::System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class Linq101SamplesModifiedAttribute : Attribute
    {
        readonly string description;
        public Linq101SamplesModifiedAttribute(string description)
        {
            this.description = description;
        }

        public string PositionalString { get; private set; }
    }
}
