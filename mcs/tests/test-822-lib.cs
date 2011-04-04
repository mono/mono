// Compiler options: -t:library

using System;

public class A
{
	public virtual int Prop {
		get {
			return 1;
		}
		
		set {
			throw new ApplicationException ();
		}
	}
}

public sealed class B : A
{
	public override int Prop {
		set {
		}
	}
}
