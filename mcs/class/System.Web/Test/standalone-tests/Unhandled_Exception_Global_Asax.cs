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

namespace StandAloneTests.Unhandled_Exception_Global_Asax
{
	[TestCase ("Unhandled_Exception_Global_Asax 01", "Unhandled exception not handled in Global.asax, test 01")]
	public sealed class Unhandled_Exception_Global_Asax_01 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("Unhandled_Exception_Global_Asax", "test_01")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
#if NET_4_0
			string originalHtml1 = @"[System.Web.HttpUnhandledException]: Exception of type &#39;System.Web.HttpUnhandledException&#39; was thrown.";
#else
			string originalHtml1 = @"[System.Web.HttpUnhandledException]: Exception of type 'System.Web.HttpUnhandledException' was thrown.";
#endif
			string originalHtml2 = @"[System.InvalidOperationException]: test";
			
			Assert.IsTrue (result.IndexOf (originalHtml1) != -1, "#A1");
			Assert.IsTrue (result.IndexOf (originalHtml2) != -1, "#A2");
		}
	}

	[TestCase ("Unhandled_Exception_Global_Asax 02", "Parsing exception is not handled in Global.asax, test 02")]
	public sealed class Unhandled_Exception_Global_Asax_02 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("Unhandled_Exception_Global_Asax", "test_02")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<p><strong>Parser Error Message: </strong><code>Cannot find type DoesNotExist</code></p><p><strong>Source Error: </strong></p>";

			Assert.IsTrue (result.IndexOf (originalHtml) != -1, "#A1");
		}
	}

	[TestCase ("Unhandled_Exception_Global_Asax 03", "Unhandled exception handled in Global.asax, test 03")]
	public sealed class Unhandled_Exception_Global_Asax_03 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("Unhandled_Exception_Global_Asax", "test_03")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<strong>Application error handled</strong>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("Unhandled_Exception_Global_Asax 04", "Unhandled exception handled in Global.asax, test 04")]
	public sealed class Unhandled_Exception_Global_Asax_04 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("Unhandled_Exception_Global_Asax", "test_04")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<h2>Object moved to <a href=""http://google.com/"">here</a></h2>";
			
			Assert.IsTrue (result.IndexOf (originalHtml) != -1, "#A1");
		}
	}
}

