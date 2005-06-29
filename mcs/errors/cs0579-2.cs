// cs0579-2.cs: Duplicate `DebuggableAttribute' attribute
// Line: 7

using System.Diagnostics;

[module: DebuggableAttribute (false, false)] 
[module: DebuggableAttribute (false, false)] 

class MainClass {
        static void Main()
        {
        }
}
