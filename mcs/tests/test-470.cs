// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4

class X
{
	public string ASTNodeTypeName
	{
		get 
		{ 
			return typeof(int).FullName;; 
		}
	}
}

class Demo {
	public static void Main ()
	{
	}
}
