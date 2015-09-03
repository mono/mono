using System;

namespace BrokenOverrideProperty
{
	abstract class BaseClass
	{
		protected BaseClass (string text)
		{
			Whatever = text;
		}

		public virtual string Whatever { get; set; }
	}

	class DerivedClass : BaseClass
	{
		public string CalledValue;

		public DerivedClass (string text) : base (text)
		{
		}

		public override string Whatever {
			get {
				return "DerivedClass";
			}
			set {
				CalledValue = value;
				Console.WriteLine ("set called with {0}", value);
			}
		}
	}

	class MainClass
	{
		public static int Main ()
		{
			var klass = new DerivedClass ("test-value");
			if (klass.CalledValue != "test-value")
				return 1;

			return 0;
		}
	}
}
