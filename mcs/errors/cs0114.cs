// CS0114: `Y.XX()' hides inherited member `X.XX()'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword
// Line: 8
// Compiler options: -warnaserror -warn:2

public abstract class X {
	public abstract void XX ();
}

class Y : X {
	void XX () {}
}
