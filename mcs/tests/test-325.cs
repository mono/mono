using System; 
 
public class RequestAttribute: Attribute { 
	public RequestAttribute(string a, string b, params string[] c) 
	{ 
	 
	} 
} 
 
public class MyClass { 
	[Request("somereq", "result")] 
	public static int SomeRequest() 
	{ 
		return 0; 
	} 
	 
	public static void Main() 
	{ 
		SomeRequest(); 
	} 
}
