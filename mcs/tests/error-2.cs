class Internal { }

public class Public { }

interface InternalInterface { }

class X
{
	public class NestedPublic { }

	internal class NestedAssembly { }

	protected internal class NestedFamilyAndAssembly { }

	protected class NestedFamily { }

	protected class NestedPrivate { }

	static void Main () { }
}

public class A : Internal { }

public class B : X.NestedPublic { }
public class C : X.NestedAssembly { }
public class D : X.NestedFamilyAndAssembly { }

public delegate void E (Internal i);
public delegate Internal F ();

public class Y
{
	public class YA : Internal { }
	public class YB : X.NestedPublic { }
	public class YC : X.NestedAssembly { }
	public class YD : X.NestedFamilyAndAssembly { }

	public void YMA (Internal a) { }
	public void YMB (X.NestedPublic a) { }
	public void YMC (X.NestedAssembly a) { }
	public void YMD (X.NestedFamilyAndAssembly a) { }

	public Internal YME () { }

	public Internal YE;

	public Internal YF {
		get { return null; }
	}

	public Internal this [int a] {
		get { return null; }
	}

	public event Internal YG;

	public int this [Internal i] {
		get { return; }
	}
}

class Z : X
{
	public class ZA : NestedFamily { }
	internal class ZB : NestedFamily { }
}

internal interface L
{
	void Hello (string hello);
}

public interface M : L
{
	void World (string world);
}

public class N : M
{
	public void Hello (string hello) { }

	public void World (string world) { }
}
