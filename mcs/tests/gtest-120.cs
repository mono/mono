using System;

public interface ISequenced<T>
{
	bool Equals (ISequenced<T> that);
}

public class SequencedHasher <S,W>
	where S : ISequenced<W>
{
        public bool Equals (S i1, S i2)
	{
		return i1 == null ? i2 == null : i1.Equals (i2);
	}
}

public class Sequenced<T> : ISequenced<T>
{
	public bool Equals (ISequenced<T> that)
	{
		return false;
	}
}

class X
{
	public static void Main ()
	{
		Sequenced<int> s = new Sequenced<int> ();
		SequencedHasher<Sequenced<int>,int> hasher = new SequencedHasher<Sequenced<int>,int> ();
		hasher.Equals (s, s);
	}
}
