#if NET_2_0

using NunitWeb;
using NUnit.Framework;
using System.Web.UI.WebControls;
using System.Reflection;
using System.ComponentModel;
using System.Threading;

namespace MonoTests.System.Web.Compilation {
	public class ReadOnlyPropertyControl:TextBox {
		[Bindable (true)]
		public bool MyProp
		{
			get { return true; }
		}

	}
	
	[TestFixture]
	public class TemplateControlCompilerTest
	{
        	[Test]
		[NUnit.Framework.Category ("NunitWeb")]
		public void ReadOnlyPropertyBindTest ()
		{
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (),
				"ReadOnlyPropertyBind.aspx", "ReadOnlyPropertyBind.aspx");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (),
				"ReadOnlyPropertyControl.ascx", "ReadOnlyPropertyControl.ascx");
			Helper.Instance.RunUrl ("ReadOnlyPropertyBind.aspx");
		}
		[TestFixtureTearDown]
		public void TearDown ()
		{
			Thread.Sleep (100);
			Helper.Unload ();
		}
	}
}

#endif

