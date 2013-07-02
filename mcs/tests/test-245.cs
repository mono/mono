public class Class2 
{
	private AliasDefOperations __delegate;

	public string def_kind {
		get {
			return __delegate.def_kind;
		}
	}

	public static void Main ()
	{ }
}

public interface AliasDefOperations : ContainedOperations, IDLTypeOperations 
{
}

public interface ContainedOperations : IRObjectOperations 
{
}

public interface IDLTypeOperations : IRObjectOperations 
{
}

public interface IRObjectOperations
{
   string def_kind { get; }
}
