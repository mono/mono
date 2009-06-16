// CS1066: The default value specified for optional parameter `s' will never be used
// Line: 9
// Compiler options: -warnaserror -langversion:future

public partial class C
{
	partial void Test (int u, string s);
	
	partial void Test (int u, string s = "optional")
	{
	}
}
