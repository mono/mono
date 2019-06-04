#if !MOBILE
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class MessageFilterTableTest
	{
		[Test]
		public void TestGetPriority ()
		{
			MessageFilterTable<int> table = new MessageFilterTable<int> ();
			MessageFilter f = new XPathMessageFilter ();

			table.Add (f, 0);

			Console.WriteLine (table.GetPriority (f));
		}
		[Test]
		public void TestAdd ()
		{
			Console.WriteLine ();
		}
	}
}
#endif
