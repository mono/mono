// Compiler options: -t:library

public interface IA<T>
{
}

public interface IB<T>
{
}

public abstract class A : IA<A>, IB<A>
{
}

public abstract class B : A, IA<B>, IB<B>, IB<C>
{
}

public sealed class C : B, IA<C>
{
}
