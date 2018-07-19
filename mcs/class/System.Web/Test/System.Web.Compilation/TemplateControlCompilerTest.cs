using System;
using System.Collections.Generic;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;
using System.Web;
using System.Web.Compilation;
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

	public class BindTestDataItem
	{
		int data;
		public int Data {
			get { return data; }
			set { data = value; }
		}

		public BindTestDataItem (int data)
		{
			this.data = data;
		}
	}
	
	public class BindTestDataSource 
	{
		public IList <BindTestDataItem> GetData ()
		{
			return new List <BindTestDataItem> {new BindTestDataItem (0), new BindTestDataItem (1)};
		}
	}
	
	[TestFixture]
	public class TemplateControlCompilerTest
	{
		[TestFixtureSetUp]
		public void TemplateControlCompiler_Init ()
		{
			WebTest.CopyResource (GetType (), "ReadOnlyPropertyBind.aspx", "ReadOnlyPropertyBind.aspx");
			WebTest.CopyResource (GetType (), "ReadOnlyPropertyControl.ascx", "ReadOnlyPropertyControl.ascx");
			WebTest.CopyResource (GetType (), "TemplateControlParsingTest.aspx", "TemplateControlParsingTest.aspx");
			WebTest.CopyResource (GetType (), "ServerSideControlsInScriptBlock.aspx", "ServerSideControlsInScriptBlock.aspx");
			WebTest.CopyResource (GetType (), "ServerControlInClientSideComment.aspx", "ServerControlInClientSideComment.aspx");
			WebTest.CopyResource (GetType (), "UnquotedAngleBrackets.aspx", "UnquotedAngleBrackets.aspx");
			WebTest.CopyResource (GetType (), "FullTagsInText.aspx", "FullTagsInText.aspx");
			WebTest.CopyResource (GetType (), "TagsExpressionsAndCommentsInText.aspx", "TagsExpressionsAndCommentsInText.aspx");
			WebTest.CopyResource (GetType (), "NewlineInCodeExpression.aspx", "NewlineInCodeExpression.aspx");
			WebTest.CopyResource (GetType (), "DuplicateControlsInClientComment.aspx", "DuplicateControlsInClientComment.aspx");
			WebTest.CopyResource (GetType (), "TagsNestedInClientTag.aspx", "TagsNestedInClientTag.aspx");
			WebTest.CopyResource (GetType (), "ConditionalClientComments.aspx", "ConditionalClientComments.aspx");
			WebTest.CopyResource (GetType (), "InvalidPropertyBind1.aspx", "InvalidPropertyBind1.aspx");
			WebTest.CopyResource (GetType (), "InvalidPropertyBind2.aspx", "InvalidPropertyBind2.aspx");
			WebTest.CopyResource (GetType (), "InvalidPropertyBind3.aspx", "InvalidPropertyBind3.aspx");
			WebTest.CopyResource (GetType (), "InvalidPropertyBind4.aspx", "InvalidPropertyBind4.aspx");
			WebTest.CopyResource (GetType (), "ValidPropertyBind1.aspx", "ValidPropertyBind1.aspx");
			WebTest.CopyResource (GetType (), "ValidPropertyBind2.aspx", "ValidPropertyBind2.aspx");
			WebTest.CopyResource (GetType (), "ValidPropertyBind3.aspx", "ValidPropertyBind3.aspx");
			WebTest.CopyResource (GetType (), "ValidPropertyBind4.aspx", "ValidPropertyBind4.aspx");
			WebTest.CopyResource (GetType (), "ValidPropertyBind5.aspx", "ValidPropertyBind5.aspx");
			WebTest.CopyResource (GetType (), "NoBindForMethodsWithBindInName.aspx", "NoBindForMethodsWithBindInName.aspx");
			WebTest.CopyResource (GetType (), "ReadWritePropertyControl.ascx", "ReadWritePropertyControl.ascx");
			WebTest.CopyResource (GetType (), "ContentPlaceHolderInTemplate.aspx", "ContentPlaceHolderInTemplate.aspx");
			WebTest.CopyResource (GetType (), "ContentPlaceHolderInTemplate.master", "ContentPlaceHolderInTemplate.master");
			WebTest.CopyResource (GetType (), "LinkInHeadWithEmbeddedExpression.aspx", "LinkInHeadWithEmbeddedExpression.aspx");
			WebTest.CopyResource (GetType (), "ExpressionInListControl.aspx", "ExpressionInListControl.aspx");
			WebTest.CopyResource (GetType (), "PreprocessorDirectivesInMarkup.aspx", "PreprocessorDirectivesInMarkup.aspx");
			WebTest.CopyResource (GetType (), "OneLetterIdentifierInCodeRender.aspx", "OneLetterIdentifierInCodeRender.aspx");
			WebTest.CopyResource (GetType (), "NestedParserFileText.aspx", "NestedParserFileText.aspx");
			WebTest.CopyResource (GetType (), "TagWithExpressionWithinAttribute.aspx", "TagWithExpressionWithinAttribute.aspx");
			WebTest.CopyResource (GetType (), "EnumConverter_Bug578586.aspx", "EnumConverter_Bug578586.aspx");
		}
		
        	[Test]
		[NUnit.Framework.Category ("NunitWeb")]
		[NUnit.Framework.Category ("NotWorking")]
		public void ReadOnlyPropertyBindTest ()
		{
			new WebTest ("ReadOnlyPropertyBind.aspx").Run ();
		}

		// Test for bug #449970
		[Test]
		public void MasterPageContentPlaceHolderInTemplate ()
		{
			new WebTest ("ContentPlaceHolderInTemplate.aspx").Run ();
		}
		
		[Test]
		[ExpectedException ("System.Web.Compilation.CompilationException")]
		public void InvalidPropertyBindTest1 ()
		{
			new WebTest ("InvalidPropertyBind1.aspx").Run ();
		}

		[Test]
		[ExpectedException (typeof (HttpParseException))]
		public void InvalidPropertyBindTest2 ()
		{
			new WebTest ("InvalidPropertyBind2.aspx").Run ();
		}

		[Test]
		[ExpectedException ("System.Web.Compilation.CompilationException")]
		public void InvalidPropertyBindTest3 ()
		{
			new WebTest ("InvalidPropertyBind3.aspx").Run ();
		}

		[Test]
		[ExpectedException (typeof (HttpParseException))]
		public void InvalidPropertyBindTest4 ()
		{
			new WebTest ("InvalidPropertyBind4.aspx").Run ();
		}

		[Test]
		public void ValidPropertyBindTest1 ()
		{
			new WebTest ("ValidPropertyBind1.aspx").Run ();
		}

		[Test]
		public void ValidPropertyBindTest2 ()
		{
			new WebTest ("ValidPropertyBind2.aspx").Run ();
		}

		[Test]
		public void ValidPropertyBindTest3 ()
		{
			new WebTest ("ValidPropertyBind3.aspx").Run ();
		}

		[Test]
		public void ValidPropertyBindTest4 ()
		{
			new WebTest ("ValidPropertyBind4.aspx").Run ();
		}

		[Test]
		public void ValidPropertyBindTest5 ()
		{
			new WebTest ("ValidPropertyBind5.aspx").Run ();
		}

		// bug #493639
		[Test]
		public void NoBindForMethodsWithBindInNameTest ()
		{
			string pageHtml = new WebTest ("NoBindForMethodsWithBindInName.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = "<span id=\"grid_ctl02_lblTest\">Test</span>";
			
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		// bug #498637
		[Test]
		public void LinkInHeadWithEmbeddedExpression ()
		{
			string pageHtml = new WebTest ("LinkInHeadWithEmbeddedExpression.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = "<link href=\"Themes/Default/Content/Site.css\" rel=\"stylesheet\" type=\"text/css\" />";

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test]
		public void ExpressionInListControl ()
		{
			string pageHtml = new WebTest ("ExpressionInListControl.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<select name=""DropDown1"" id=""DropDown1"">
	<option value=""strvalue"">str</option>

</select>";
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug #508888")]
		public void ServerSideControlsInScriptBlock ()
		{
			string pageHtml = new WebTest ("ServerSideControlsInScriptBlock.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<script type=""text/javascript"">alert (escape(""reporting/location?report=ViewsByDate&minDate=minDate&maxDate=maxDate""));</script>";
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug #520024")]
		public void PreprocessorDirectivesInMarkup ()
		{
			// Just test if it doesn't throw an exception
			new WebTest ("PreprocessorDirectivesInMarkup.aspx").Run ();
		}

		[Test (Description="Bug #526449")]
		public void NewlineInCodeExpression ()
		{
			string pageHtml = new WebTest ("NewlineInCodeExpression.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = "<a href=\"test\">bla</a>";
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug #524358")]
		public void DuplicateControlsInClientComment ()
		{
			// Just test if it throws an exception
			string pageHtml = new WebTest ("DuplicateControlsInClientComment.aspx").Run ();
			Assert.IsTrue (pageHtml.IndexOf ("[System.Web.Compilation.ParseException]:") != -1, "#A1");
		}

		[Test (Description="Bug #367723")]
		public void ConditionalClientComments ()
		{
			string pageHtml = new WebTest ("ConditionalClientComments.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<!--[if IE 6]>
		<link rel=""styleheet"" type=""text/css"" href=""~/compat-ie6.css""></link>
	<![endif]-->";
			HtmlDiff.AssertAreEqual (originalHtml.Replace ("\r\n","\n"), renderedHtml, "#A1");
		}
		
		[Test (Description="Bug #400807")]
		public void OneLetterIdentifierInCodeRender ()
		{
			string pageHtml = new WebTest ("OneLetterIdentifierInCodeRender.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"bDoR called";

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug #562286")]
		public void NestedParserFileText ()
		{
			// Just test if it doesn't throw an exception
			new WebTest ("NestedParserFileText.aspx").Run ();
		}

		[Test (Description="Bug #568631")]
		public void TagWithExpressionWithinAttribute ()
		{
			// Just test if it doesn't throw an exception
			new WebTest ("TagWithExpressionWithinAttribute.aspx").Run ();
		}

		[Test (Description="Bug #578586")]
		public void EnumConverter_Bug578586 ()
		{
			WebTest t = new WebTest ("EnumConverter_Bug578586.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<input type=""text"" value=""FlagOne"" name=""test"" id=""test"" />";

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}
		[Test (Description="Bug #323719")]
		public void TagsNestedInClientTag ()
		{
			string pageHtml = new WebTest ("TagsNestedInClientTag.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<script language=""javascript"" src=""/js/test.js"" type=""text/javascript""></script>
<sometag language=""javascript"" src=""/js/test.js"" type=""text/javascript""></sometag>";

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}
		
		[Test (Description="Bug #517656")]
		public void ServerControlInClientSideComment ()
		{
			string pageHtml = new WebTest ("ServerControlInClientSideComment.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<!-- comment start
  <input id=""testBox"" type=""checkbox"" name=""testBox"" />
comment end -->";
			HtmlDiff.AssertAreEqual (originalHtml.Replace ("\r\n","\n"), renderedHtml, "#A1");
		}

		[Test]
		public void UnquotedAngleBrackets ()
		{
			// We just test if it doesn't throw an exception
			new WebTest ("UnquotedAngleBrackets.aspx").Run ();
		}

		[Test]
		public void FullTagsInText ()
		{
			// We just test if it doesn't throw an exception
			new WebTest ("FullTagsInText.aspx").Run ();
		}

		[Test]
		public void TagsExpressionsAndCommentsInText ()
		{
			// We just test if it doesn't throw an exception
			new WebTest ("TagsExpressionsAndCommentsInText.aspx").Run ();
		}
		
		[Test]
		public void ChildTemplatesTest ()
		{
			try {
				WebTest.Host.AppDomain.AssemblyResolve += new ResolveEventHandler (ResolveAssemblyHandler);
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

