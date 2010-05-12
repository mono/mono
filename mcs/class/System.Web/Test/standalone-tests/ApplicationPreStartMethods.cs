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

namespace StandAloneTests.ApplicationPreStartMethods
{
	[TestCase ("ApplicationPreStartMethods 01", "Tests whether pre application start methods work correctly")]
	public sealed class ApplicationPreStartMethods_01 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_01",
					"ApplicationPreStartMethods"
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
			string originalHtml = @"<div>Report:<pre id=""report"">Public static method called
ExternalAssembly1 added
</pre></div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 02", "Throw exception when pre-start method is not public and static - public instance method used here.")]
	public sealed class ApplicationPreStartMethods_02 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_02",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The method specified by the PreApplicationStartMethodAttribute on assembly"), "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 03", "Throw exception when pre-start method is not public and static - internal instance method used here.")]
	public sealed class ApplicationPreStartMethods_03 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_03",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The method specified by the PreApplicationStartMethodAttribute on assembly"), "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 04", "Throw exception when pre-start method is not public and static - internal static method used here.")]
	public sealed class ApplicationPreStartMethods_04 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_04",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The method specified by the PreApplicationStartMethodAttribute on assembly"), "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 05", "Throw exception when pre-start method is not public and static - private instance method used here.")]
	public sealed class ApplicationPreStartMethods_05 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_05",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The method specified by the PreApplicationStartMethodAttribute on assembly"), "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 06", "Throw exception when pre-start method is not public and static - private static method used here.")]
	public sealed class ApplicationPreStartMethods_06 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_06",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The method specified by the PreApplicationStartMethodAttribute on assembly"), "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 07", "Throw exception when pre-start method is not public and static - public static method found in the base class.")]
	public sealed class ApplicationPreStartMethods_07 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_07",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The method specified by the PreApplicationStartMethodAttribute on assembly"), "#A1");
		}
	}

	[TestCase ("ApplicationPreStartMethods 08", "Pre-start method throws an exception")]
	public sealed class ApplicationPreStartMethods_08 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"ApplicationPreStartMethods",
					"test_08",
					"ApplicationPreStartMethods"
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
			Assert.AreNotEqual (-1, result.IndexOf ("[System.Web.HttpException]: The pre-application start initialization method PublicStaticMethod on type ApplicationPreStartMethods.Tests.PreStartMethods"), "#A1");
		}
	}
}
#endif