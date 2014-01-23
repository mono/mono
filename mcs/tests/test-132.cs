using System.Reflection;

class T {
	protected internal string s;
	public static int Main() {
		FieldInfo f = typeof(T).GetField ("s", BindingFlags.NonPublic|BindingFlags.Instance);
		if (f == null)
			return 2;
		FieldAttributes attrs = f.Attributes;
		if ((attrs & FieldAttributes.FieldAccessMask) != FieldAttributes.FamORAssem)
			return 1;
		return 0;
	}
}
