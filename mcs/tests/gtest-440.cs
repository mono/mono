public interface I<X>
{
}

public abstract class AnyObjectId : I<ObjectId>
{
	public int W1 { get; set; }
}

public class ObjectId : AnyObjectId
{
	ObjectId ()
	{
		W1 = 1;
	}

	public static void Main ()
	{
	}
}
