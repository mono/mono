// CS0545: `B.Prop': cannot override because `A.Prop' does not have accessible get accessor
// Line: 13

public class A
{
	public virtual string Prop {
		set; private get;
	}  
}
 
public class B : A
{
	sealed override public string Prop {
		set { }
	}   
}