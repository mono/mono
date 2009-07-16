interface Interface<TImplementer>
  where TImplementer : Interface<TImplementer> 
{
	void Combine<TOImplementer> ()
	  where TOImplementer : Interface<TOImplementer>;
}

class Implementer : Interface<Implementer>
{
	public void Combine<TOImplementer> ()
	  where TOImplementer : Interface<TOImplementer>
	{
	}
}

class MainClass
{
	public static int Main ()
	{
		new Implementer ();
		return 0;
	}
}
