class C
{
	public static void Main ()
	{
		int a = 2;
#line 100 "mising-file"
		int b = 2;
#line default
	}
	
	void Hidden ()
	{
#line hidden
		int x = 9;
#line default // comment testing
		const int o = 2;

#line hidden
		x = 9;
#line hidden
		int x2 = 3;
#line 55
		int h = 7;
	}
	
	void HiddenRecurse ()
	{
		string s1 = "a";
#line hidden
		string s2 = "bb";
#line 29
		return;
	}
}
