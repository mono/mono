// cs0579.cs : Duplicate 'DebuggableAttribute' attribute// Line : 6
using System.Diagnostics;

[module: DebuggableAttribute (false, false)] 
[module: DebuggableAttribute (false, false)] 

class MainClass {
        static void Main()
        {
        }
}
