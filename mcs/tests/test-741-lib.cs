// Compiler options: -target:library

public interface IA
{
	string Prop { get; }
}

public interface IAA
{
	char NestedProp { set; }
}

public interface IB : IAA
{
	bool Prop { get; }
	long NestedProp { set; }
}

public interface IFoo : IA, IB
{
	int Prop { get; }
}
