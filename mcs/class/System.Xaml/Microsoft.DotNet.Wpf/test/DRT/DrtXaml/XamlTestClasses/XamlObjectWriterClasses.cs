using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace Test
{
    [DictionaryKeyPropertyAttribute("DKPProperty")]
    public class DKPClass
    {
        private string s = "DKPKey";

        public string DKPProperty { get { return s; } }
    }
}