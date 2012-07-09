using System;
using NUnit.Framework;

namespace MonoTests {
	static class AssertEx {
		static public T Throws<T> (Action code) where T : Exception
		{
			T exception = null;
			try {
				code ();
			} catch (T ex) {
				exception = ex;
			}

			Assert.IsNotNull (exception);

			return exception;
		}
	}
}
