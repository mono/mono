// Compiler options: -t:library

public interface IG<T>
{
	T Value { get; }
}

public class DynamicReference
{
	public IG<dynamic> DynType;
	public IG<dynamic[][]> DynArray;
}
