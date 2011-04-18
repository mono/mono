interface ICovariant<out T> { }
interface IContravariant<in T> { }

delegate ICovariant<U> Cov1<out U> (IContravariant<U> x);
delegate ICovariant<U> Cov2<out U> (IContravariant<ICovariant<U>> x);
delegate ICovariant<IContravariant<IContravariant<U>>> Cov3<out U> (IContravariant<IContravariant<IContravariant<ICovariant<U>>>> x);
delegate ICovariant<int> Cov4<out U> ();
delegate IContravariant<int> Cov5<out U> ();

delegate IContravariant<U[]> Contra5<in U> (U u, ICovariant<U> x);
delegate IContravariant<U[]> Contra6<in U> ();
delegate IContravariant<U> Contra7<in U> (U u, ICovariant<U> x);
delegate IContravariant<ICovariant<U>> Contra8<in U> (U u, ICovariant<U> x);

interface ITest_1<out T>
{
	ICovariant<T> CovariantHandler (IContravariant<T> x);
}

interface ITest_2<in T>
{
	IContravariant<T> CovariantHandler (ICovariant<T> x);
}

class Program
{
	static void Main ()
	{
	}
}
