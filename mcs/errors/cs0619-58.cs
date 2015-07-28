// CS0619: `Program.TestEventArgs' is obsolete: `FooBar'
// Line: 10

using System;

namespace Program
{
	public class TestClass
	{
		public EventHandler<TestEventArgs> Foo;
	}

	[Obsolete(Messages.Test, true)]
	public sealed class TestEventArgs : EventArgs
	{
	}
}

namespace Program
{
	public static class Messages
	{
		public const string Test = "FooBar";
	}
}