// CS0546: `B.Prop': cannot override because `A.Prop' does not have accessible set accessor
// Line: 13

public class A
{
	public virtual string Prop {
		get; private set;
	}
}
 
public class B : A
{
	sealed override public string Prop {
		get { return ""; }
	}
}