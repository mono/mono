//
// MonoTests.System.ComponentModel.AllTests
//
// Author:
// 	Gonzalo Panigua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc (http://www.ximian.com)
//

using NUnit.Framework;
using System.ComponentModel;

namespace MonoTests.System.ComponentModel
{
	public class AllTests : TestCase
	{
		public AllTests () : base ("MonoTests.System.ComponentModel testcase") { }

		public AllTests (string name) : base (name)
		{
		}

		public static ITest Suite
		{
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (EventHandlerListTests.Suite);
				return suite;
			}
		}
	}
}

