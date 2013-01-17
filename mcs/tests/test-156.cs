class Internal { }

public class Public { }

class X
{
	public class NestedPublic { }

	internal class NestedAssembly { }

	protected internal class NestedFamilyAndAssembly { }

	protected class NestedFamily { }

	private class NestedPrivate { }

	public static void Main () { }
}

public class A : Public { }

class B : Public { }
class C : Internal { }

class D : X.NestedPublic { }
class E : X.NestedAssembly { }
class F : X.NestedFamilyAndAssembly { }

public class Y
{
	public class YA : Public { }

	class YB : Internal { }

	class YC : X.NestedPublic { }
	class YD : X.NestedAssembly { }
	class YE : X.NestedFamilyAndAssembly { }

	internal class YF : Internal { }

	internal class YG : X.NestedAssembly { }
	internal class YH : X.NestedFamilyAndAssembly { }

	internal enum YI { A, B }

	internal void Test (YI yi) { }
}

public class Z
{
	public class P : Y.YA { }
}

class W : X
{
	public class WA : NestedPublic { }
	public class WB : NestedAssembly { }
	public class WC : NestedFamilyAndAssembly { }
	internal class WD : NestedPublic { }
	internal class WE : NestedFamilyAndAssembly { }

	private class WCA
	{
	}

	private class WCB
	{
		public class WCD
		{
			public class WCE : WCA
			{
			}
		}
	}
}

class G
{
	public void Test (X x) { }

	private enum Foo { A, B };

	enum Bar { C, D };

	private class I
	{
		public class J
		{
			public void Test (Foo foo) { }
		}
	}
}

public class H
{
	public void Test (int[] a) { }
}

internal interface L
{
	void Hello (string hello);
}

public class M : L
{
	public void Hello (string hello) { }
}
