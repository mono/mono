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

// CS0060
public class A : Internal { }

// CS0060
public class B : X.NestedPublic { }
// CS0060
public class C : X.NestedAssembly { }
// CS0060
public class D : X.NestedFamilyAndAssembly { }

// CS0059
public delegate void E (Internal i);
// CS0058
public delegate Internal F ();

public class Y
{
	// CS0060
	public class YA : Internal { }
	// CS0060
	public class YB : X.NestedPublic { }
	// CS0060
	public class YC : X.NestedAssembly { }
	// CS0060
	public class YD : X.NestedFamilyAndAssembly { }

	// CS0051
	public void YMA (Internal a) { }
	// CS0051
	public void YMB (X.NestedPublic a) { }
	// CS0051
	public void YMC (X.NestedAssembly a) { }
	// CS0051
	public void YMD (X.NestedFamilyAndAssembly a) { }

	// CS0050
	public Internal YME () { }

	// CS0052
	public Internal YE;

	// CS0053
	public Internal YF {
		get { return null; }
	}

	// CS0054
	public Internal this [int a] {
		get { return null; }
	}

	// CS0052
	public event Internal YG;

	// CS0055
	public int this [Internal i] {
		get { return; }
	}
}

class Z : X
{
	// CS0060
	public class ZA : NestedFamily { }
	// CS0060
	internal class ZB : NestedFamily { }
}

internal interface L
{
	void Hello (string hello);
}

// CS0061
public interface M : L
{
	void World (string world);
}

public class N : M
{
	public void Hello (string hello) { }

	public void World (string world) { }
}
