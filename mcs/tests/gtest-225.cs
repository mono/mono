public class Set<Element>
{
	protected readonly Node[] sub;
	public Set () { }

	public struct Locator
	{
		public delegate void Replace (Node node);
		public Locator (Replace put) { }
	}

	public class Node : Set<Element>
	{ }

	protected Locator locate (Element x)
	{
		Set<Element> parent = this;
		return new Locator (new Locator.Replace (delegate (Node n) {
			parent.sub[0] = n;
		}));
	}
}

static class SetTest
{
	public static void Main ()
	{
	}
}
