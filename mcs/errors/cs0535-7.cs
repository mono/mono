// CS0535: `CC' does not implement interface member `IA.Coordinate.set'
// Line: 33

using System;

public interface IA
{
	object Coordinate {
		get;
		set;
	}
}

public abstract class CA : IA
{
	public abstract object Coordinate {
		get;
		set;
	}
}

public  partial class CB : CA
{
	public override object Coordinate {
		get {
			throw new NotImplementedException ();
		}
		set {
		}
	}
}

public class CC : CB, IA
{
	public new object Coordinate {
		get {
			throw new NotImplementedException ();
		}
	}
}
