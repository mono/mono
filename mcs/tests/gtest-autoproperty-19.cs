abstract class Node
{
	public virtual int Next { get; }
}

class NodeLinked : Node
{
	public NodeLinked (int next)
	{
		this.Next = next;
	}

	public override int Next { get; }

	public static int Main ()
	{
		var nl = new NodeLinked (5);
		if (nl.Next != 5)
			return 1;

		return 0;
	}
}
