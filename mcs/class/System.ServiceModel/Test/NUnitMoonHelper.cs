// Helper to allow 'same source' unit tests from Moonlight to be executed under NUnit

using System;
using NUnit.Framework;

namespace Mono.Moonlight.UnitTesting {

	public class MoonlightBugAttribute : IgnoreAttribute {
	}
}

namespace Microsoft.VisualStudio.TestTools.UnitTesting {

	public class TestClassAttribute : TestFixtureAttribute {
	}

	public class TestMethodAttribute : TestAttribute {
	}

	public class Assert : NUnit.Framework.Assert {

		public static void Throws<TException> (Action code, string message) where TException : Exception
		{
			Type expected_exception = typeof (TException);
			bool failed = false;
			try {
				code ();
				failed = true;
			}
			catch (Exception ex) {
				if (!(ex.GetType () == expected_exception))
					Assert.Fail (string.Format ("Expected '{0}', got '{1}'. {2}", expected_exception.FullName, ex.GetType ().FullName, message));
			}
			if (failed)
				Assert.Fail (string.Format ("Expected '{0}', but got no exception. {1}", expected_exception.FullName, message));
		}
	}
}

