// CS0535: `CC' does not implement interface member `IA.this[int].set'
// Line: 33

using System;

public interface IA
{
	object this[int arg] {
		get;
		set;
	}
}

public abstract class CA : IA
{
	public abstract object this[int arg] {
		get;
		set;
	}
}

public  partial class CB : CA
{
	public override object this[int arg] {
		get {
			throw new NotImplementedException ();
		}
		set {
		}
	}
}

public class CC : CB, IA
{
	public new object this[int arg] {
		get {
			throw new NotImplementedException ();
		}
	}
}
