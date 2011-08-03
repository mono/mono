// CS0601: The DllImport attribute must be specified on a method marked `static' and `extern'
// Line : 9

using System;
using System.Runtime.InteropServices;
      
class Test {
	[DllImport("cygwin1.dll", EntryPoint="puts", CharSet=CharSet.Ansi)]
	public extern int puts (string name);
	
}
