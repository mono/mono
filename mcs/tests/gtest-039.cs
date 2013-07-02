//
// Important test for the runtime: check whether we're correctly
// creating the vtable for nested types.
//

using System;

interface IMonkey<T>
{
	T Jump ();
}

class Zoo<T>
{
	T t;

	public Zoo (T t)
	{
		this.t = t;
	}

	public T Name {
		get { return t; }
	}

	public IMonkey<U> GetTheMonkey<U> (U u)
	{
		return new Monkey<T,U> (this, u);
	}

	public class Monkey<V,W> : IMonkey<W>
	{
		public readonly Zoo<V> Zoo;
		public readonly W Data;

		public Monkey (Zoo<V> zoo, W data)
		{
			this.Zoo = zoo;
			this.Data = data;
		}

		public W Jump ()
		{
			Console.WriteLine ("Monkey {0} from {1} jumping!", Data, Zoo.Name);
			return Data;
		}
	}
}

class X
{
	public static void Main ()
	{
		Zoo<string> zoo = new Zoo<string> ("Boston");
		IMonkey<float> monkey = zoo.GetTheMonkey<float> (3.14F);
		monkey.Jump ();
	}
}
