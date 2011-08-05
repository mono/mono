// Compiler options: -t:library

public interface IAAA
{
}

public interface IBBB
{
}

public interface IYYY
{
	void Foo(IAAA query);
}


public interface IZZZ : IYYY
{
	void Foo(IBBB command);
}

