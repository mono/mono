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

namespace StandAloneTests.EnableFormsAuthentication
{
	[TestCase ("EnableFormsAuthentication 01", "Check if null is accepted as the parameter")]
	public sealed class EnableFormsAuthentication_01 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_01"
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
			string originalHtml = @"<div>Default URL: /default.aspx<br />Login URL: /login.aspx</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 02", "Check if an empty collection is accepted as the parameter")]
	public sealed class EnableFormsAuthentication_02 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_02"
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
			string originalHtml = @"<div>Default URL: /default.aspx<br />Login URL: /login.aspx</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 03", "Sets both documented properties.")]
	public sealed class EnableFormsAuthentication_03 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_03"
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
			string originalHtml = @"<div>Default URL: /myDefault.aspx<br />Login URL: /myLogin.aspx</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 04", "Checks whether empty strings are accepted as config items value")]
	public sealed class EnableFormsAuthentication_04 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_04"
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
			string originalHtml = @"<div>Default URL: /default.aspx<br />Login URL: /login.aspx</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 05", "Checks whether null is accepted as config items value")]
	public sealed class EnableFormsAuthentication_05 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_05"
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
			string originalHtml = @"<div>Default URL: /default.aspx<br />Login URL: /login.aspx</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 06", "Checks whether config item names are case-sensitive.")]
	public sealed class EnableFormsAuthentication_06 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_06"
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
			string originalHtml = @"<div>Default URL: /myDefault.aspx<br />Login URL: /myLogin.aspx</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 07", "Checks if only loginUrl and defaultUrl are set with this method.")]
	public sealed class EnableFormsAuthentication_07 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_07"
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
			string originalHtml = @"<div>Default URL: /myDefault.aspx<br />Login URL: /myLogin.aspx<br />Cookie domain: </div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 08", "Checks whether multiple calls to the method are possible and that the last values passed take precedence.")]
	public sealed class EnableFormsAuthentication_08 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_08"
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
			string originalHtml = @"<div>Default URL: /myOtherDefault.aspx<br />Login URL: /myOtherLogin.aspx<br />Cookie domain: </div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("EnableFormsAuthentication 09", "Check whether values passed to the method take precedence over those in web.config")]
	public sealed class EnableFormsAuthentication_09 : ITestCase
	{
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"EnableFormsAuthentication",
					"Test_09"
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
			string originalHtml = @"<div>Default URL: /myDefault.aspx<br />Login URL: /myLogin.aspx<br />Cookie domain: </div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
#endif