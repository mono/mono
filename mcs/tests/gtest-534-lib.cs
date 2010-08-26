// Compiler options: -t:library

public interface IG<T>
{
}

public interface IA
{
	void Method (IG<double[][]> arg);
}
