// Compiler options: -warnaserror -warn:4 -nowarn:169

using System;

[Obsolete]
class ObsoleteClass
{
}

public class Test
{
	private string _name;

	[Obsolete()]
	public Test() : this("layout", false)
	{
	}

	[Obsolete()]
	public Test(string a, bool writeToErrorStream)
	{
		Name = a;
	}

	[Obsolete()]
	public string Name
	{
		get { return _name; }
		set { _name = value; }
	}
}

[Obsolete]
public class DerivedTest : Test
{
        ObsoleteClass member;
    
        [Obsolete]
	public DerivedTest(string a) : base(a, false)
        {
		Name = a;
	}
        
        public string Method ()
        {
            return base.Name;
        }
        
        public static void Main () {}
}

[Obsolete]
class ObsoleteClass2: ObsoleteClass
{
}
