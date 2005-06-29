// cs0657.cs: `assembly' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `type'
// Line : 7

using System.Reflection;

namespace N {
    [assembly: AssemblyKeyName("")]
    class A {}
}

