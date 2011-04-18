// CS0579: The attribute `System.Diagnostics.DebuggableAttribute' cannot be applied multiple times
// Line: 7

using System.Diagnostics;

[module: DebuggableAttribute (false, false)] 
[module: DebuggableAttribute (false, false)] 

class MainClass {
        static void Main()
        {
        }
}
