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

namespace StandAloneTests.OutputCacheProvider
{
	[TestCase ("OutputCacheProvider 01", "OutputCacheProvider - custom provider test")]
	public sealed class OutputCacheProvider_01 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("OutputCacheProvider", "OutputCacheProviderTest_01")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""output"">Default provider name: TestInMemoryProvider
Null context: TestInMemoryProvider
Default context: TestInMemoryProvider
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("OutputCacheProvider 02", "OutputCacheProvider - missing provider test")]
	public sealed class OutputCacheProvider_02 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("OutputCacheProvider", "OutputCacheProviderTest_02")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			Assert.IsTrue (Helpers.HasException (result, typeof (ConfigurationErrorsException)), "#A1");
		}
	}

	[TestCase ("OutputCacheProvider 03", "OutputCacheProvider - per request provider test")]
	public sealed class OutputCacheProvider_03 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					Path.Combine ("OutputCacheProvider", "OutputCacheProviderTest_03")
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("/Default.aspx?ocp=InMemory", Default_InMemory_Aspx));
			runItems.Add (new TestRunItem ("/Default.aspx?ocp=AnotherInMemory", Default_AnotherInMemory_Aspx));
			runItems.Add (new TestRunItem ("/Default.aspx?ocp=invalid", Default_Invalid_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""output"">Default provider name: AspNetInternalProvider
Default context: AspNetInternalProvider
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_InMemory_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""output"">Default provider name: AspNetInternalProvider
Default context: TestInMemoryProvider
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_AnotherInMemory_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""output"">Default provider name: AspNetInternalProvider
Default context: TestAnotherInMemoryProvider
</pre>";
			
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_Invalid_Aspx (string result, TestRunItem runItem)
		{
			Assert.IsTrue (Helpers.HasException (result, typeof (ProviderException)), "#A1");
		}
	}
}
#endif
