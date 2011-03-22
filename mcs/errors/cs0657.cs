// CS0657.cs: `assembly' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `type'. All attributes in this section will be ignored
// Line: 8
// Compiler options: -warnaserror

using System.Reflection;

namespace N {
    [assembly: AssemblyKeyName("")]
    class A {}
}

