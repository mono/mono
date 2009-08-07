// CS0533: `MyAbstract.Initialize()' hides inherited abstract member `MyAbstractBase.Initialize()'
// Line: 10
public abstract class MyAbstractBase
{
    public abstract void Initialize();
}

public abstract class MyAbstract : MyAbstractBase
{
    public void Initialize() {
    }
}


public class Program
{
    public static void Main(string[] args)
    {
    }
}


