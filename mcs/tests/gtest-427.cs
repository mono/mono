
delegate void EventHandler (object sender);
delegate void EventHandler<T> (T sender);

class T
{
	void Test ()
	{
		Attach (OnClick);
	}

	void Attach (EventHandler handler)
	{
		throw null;
	}

	void Attach (EventHandler<string> handler)
	{
	}

	void OnClick (string sender)
	{
	}

	public static void Main ()
	{
		new T ().Test ();
	}
}



