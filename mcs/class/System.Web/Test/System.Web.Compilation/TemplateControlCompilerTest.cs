using System;
using MonoTests.SystemWeb.Framework;
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
#if !TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void ReadOnlyPropertyBindTest ()
		{
			WebTest.CopyResource (GetType (), "ReadOnlyPropertyBind.aspx", "ReadOnlyPropertyBind.aspx");
			WebTest.CopyResource (GetType (), "ReadOnlyPropertyControl.ascx", "ReadOnlyPropertyControl.ascx");
			new WebTest ("ReadOnlyPropertyBind.aspx").Run ();
		}

		[Test]
		public void ChildTemplatesTest ()
		{
			try {
				WebTest.Host.AppDomain.AssemblyResolve += new ResolveEventHandler (ResolveAssemblyHandler);
				WebTest.CopyResource (GetType (), "TemplateControlParsingTest.aspx", "TemplateControlParsingTest.aspx");
				new WebTest ("TemplateControlParsingTest.aspx").Run ();
			} finally {
				WebTest.Host.AppDomain.AssemblyResolve -= new ResolveEventHandler (ResolveAssemblyHandler);
			}
		}
		
		[TestFixtureTearDown]
		public void TearDown ()
		{
			Thread.Sleep (100);
			WebTest.Unload ();
		}

		public static Assembly ResolveAssemblyHandler (object sender, ResolveEventArgs e)
		{
			if (e.Name != "System.Web_test")
				return null;

			return Assembly.GetExecutingAssembly ();
		}
	}
}

