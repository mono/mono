// CS738: `CB' does not implement interface member `IG<IA>.Method()' and the best implementing candidate `CA<IB>.Method()' return type `IB' does not match interface member return type `IA'
// Line: 29

public interface IA
{
}

public interface IB : IA
{
}

public interface IG<out U>
{
	U Method ();
}

public interface IDerived : IG<IA>
{
}

public abstract class CA<T> : IG<T>
{
	public T Method ()
	{
		return default (T);
	}
}

public class CB : CA<IB>, IG<IA>
{
}
