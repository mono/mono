// CS0738: `C' does not implement interface member `I2.Key.get' and the best implementing candidate `C.Key.get' return type `IB' does not match interface member return type `IA'
// Line: 22

public interface I1
{
	IB Key { get; }
}

public interface I2
{
	IA Key { get; }
}

public interface IB : IA
{
}

public interface IA
{
}

public class C : I1, I2
{
	public IB Key { get { return null; } }
}
