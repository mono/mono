using System;
using System.Windows.Forms;
using System.Threading;
using Sys_Threading=System.Threading;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class FormThreadTest : TestHelper
	{
		private static void GuiThread()
		{
			Form form1;

			form1 = new Form();
			form1.Show();
			form1.Dispose();
		}

		[Test]
		public void TestThreadFormsInit ()
		{
			Sys_Threading.Thread thread;

			thread = new Sys_Threading.Thread(new ThreadStart(GuiThread));
			thread.Start();
			thread.Join();

			try
			{
				GuiThread();
			}
			catch (Exception e)
			{
				Assert.Fail ("#1");
			}
		}
	}
}
