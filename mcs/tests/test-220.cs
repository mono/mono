//
// Tests for bug #51446, where MCS did not pick the right enumerator
// from a class.
//

using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoBUG
{

	public class Bug
	{
		public static int Main(string[] args)
		{
			FooList l = new FooList();
			Foo f1 = new Foo("First");
			Foo f2 = new Foo("Second");

			l.Add(f1);
			l.Add(f2);

			foreach (Foo f in l) {
			}

			if (FooList.foo_current_called != true)
				return 1;
			if (FooList.ienumerator_current_called != false)
				return 2;
			Console.WriteLine ("Test passes");
			return 0;
		}
	}

	public class Foo
	{
		private string m_name;
		
		public Foo(string name)
		{
			m_name = name;
		}
		
		public string Name {
			get { return m_name; }
		}
	}

	[Serializable()]
	public class FooList : DictionaryBase  
	{
		public static bool foo_current_called = false;
		public static bool ienumerator_current_called = false;
			
		public FooList() 
		{
		}
		
		public void Add(Foo value) 
		{
			Dictionary.Add(value.Name, value);
		}
		
		public new FooEnumerator GetEnumerator() 
		{
			return new FooEnumerator(this);
		}
		
		public class FooEnumerator : object, IEnumerator 
		{
			
			private IEnumerator baseEnumerator;
			
			private IEnumerable temp;
			
			public FooEnumerator(FooList mappings) 
			{
				this.temp = (IEnumerable) (mappings);
				this.baseEnumerator = temp.GetEnumerator();
			}
			
			public Foo Current 
			{
				get 
				{
					Console.WriteLine("Foo Current()");
					foo_current_called = true;
					return (Foo) ((DictionaryEntry) (baseEnumerator.Current)).Value;
				}
			}
			
			object IEnumerator.Current 
			{
				get 
				{
					Console.WriteLine("object IEnumerator.Current()");
					ienumerator_current_called = true;
					return baseEnumerator.Current;
				}
			}
			
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
			
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
			
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
			
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
	}
}
