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
		
		[Obsolete]
		public void T2 () {}
        
        public static void Main () {}
}

[Obsolete]
class ObsoleteClass2: ObsoleteClass
{
}


class ObsoleteClass3
{
	public static readonly double XSmall = 0.6444444444444;

	[Obsolete ("E1")]
	public readonly double X_Small = XSmall;

	[Obsolete ("E2")]
	public static readonly double X_Small2 = XSmall;
}


class ObsoleteClass4
{
	[Obsolete]
	public void T ()
	{
		lock (typeof (ObsoleteClass4)) {}
		lock (typeof (ObsoleteClass2)) {}
	}
}