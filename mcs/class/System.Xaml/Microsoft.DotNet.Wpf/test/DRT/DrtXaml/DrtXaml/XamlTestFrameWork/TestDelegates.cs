using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrtXaml.XamlTestFramework
{
    public delegate void SimpleTest();
    public delegate object XamlStringParser(string xamlString);
    public delegate void PostTreeValidator(object root);
}
