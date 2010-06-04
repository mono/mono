//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_4_0
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Web.Hosting;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.WebFormsRouting
{
	[TestCase ("WebFormsRouting 01", "Web forms routing")]
	public sealed class WebFormsRouting_01 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (Consts.BasePhysicalDir, "WebFormsRouting");
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("/search/test", Search_Test));
			runItems.Add (new TestRunItem ("/search/true", Search_True));
			runItems.Add (new TestRunItem ("/search/red", Search_Red));
			
			return true;
		}
	
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<a href=""/search/test"">Search for 'test'</a>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Search_Test (string result, TestRunItem runItem)
		{
			string originalHtml = @"Search term is: <span id=""label1"">test</span><br />
	Search term from expression is: <span id=""label2"">test</span><br />
	<pre id=""testLog"">.: Missing key (key: &#39;SearchTermd&#39;)
	Returned null.
.: Missing property (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: test
.: No converter (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: test
.: Valid conversion to target (key: &#39;SearchTerm&#39;)
	Exception &#39;System.FormatException&#39; caught
.: Invalid conversion to target (key: &#39;SearchTerm&#39;)
	Exception &#39;System.Exception&#39; caught
.: Complex type converter (key: &#39;SearchTerm&#39;)
	Exception &#39;System.Exception&#39; caught
.: Null controlType (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: test
.: Null propertyName (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: test
.: Empty propertyName (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: test
.: Non-string value (key: &#39;intValue&#39;)
	Returned value of type &#39;System.Int32&#39;: 123
.: Non-string value (key: &#39;boolValue&#39;)
	Returned value of type &#39;System.Boolean&#39;: False
.: Non-string value (key: &#39;doubleValue&#39;)
	Returned value of type &#39;System.Double&#39;: 1,23
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Search_True (string result, TestRunItem runItem)
		{
			string originalHtml = @"Search term is: <span id=""label1"">true</span><br />
	Search term from expression is: <span id=""label2"">true</span><br />
	<pre id=""testLog"">.: Missing key (key: &#39;SearchTermd&#39;)
	Returned null.
.: Missing property (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: true
.: No converter (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: true
.: Valid conversion to target (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.Boolean&#39;: True
.: Invalid conversion to target (key: &#39;SearchTerm&#39;)
	Exception &#39;System.Exception&#39; caught
.: Complex type converter (key: &#39;SearchTerm&#39;)
	Exception &#39;System.Exception&#39; caught
.: Null controlType (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: true
.: Null propertyName (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: true
.: Empty propertyName (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: true
.: Non-string value (key: &#39;intValue&#39;)
	Returned value of type &#39;System.Int32&#39;: 123
.: Non-string value (key: &#39;boolValue&#39;)
	Returned value of type &#39;System.Boolean&#39;: False
.: Non-string value (key: &#39;doubleValue&#39;)
	Returned value of type &#39;System.Double&#39;: 1,23
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Search_Red (string result, TestRunItem runItem)
		{
			string originalHtml = @"Search term is: <span id=""label1"">red</span><br /> 
	Search term from expression is: <span id=""label2"">red</span><br /> 
	<pre id=""testLog"">.: Missing key (key: &#39;SearchTermd&#39;)
	Returned null.
.: Missing property (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: red
.: No converter (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: red
.: Valid conversion to target (key: &#39;SearchTerm&#39;)
	Exception &#39;System.FormatException&#39; caught
.: Invalid conversion to target (key: &#39;SearchTerm&#39;)
	Exception &#39;System.Exception&#39; caught
.: Complex type converter (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.Drawing.Color&#39;: Color [Red]
.: Null controlType (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: red
.: Null propertyName (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: red
.: Empty propertyName (key: &#39;SearchTerm&#39;)
	Returned value of type &#39;System.String&#39;: red
.: Non-string value (key: &#39;intValue&#39;)
	Returned value of type &#39;System.Int32&#39;: 123
.: Non-string value (key: &#39;boolValue&#39;)
	Returned value of type &#39;System.Boolean&#39;: False
.: Non-string value (key: &#39;doubleValue&#39;)
	Returned value of type &#39;System.Double&#39;: 1,23
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
#endif