//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//   
// 
// Copyright (c) 2002-2005 Mainsoft Corporation.
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
using System.Collections;
using System.Xml;

namespace MonoTests.stand_alone.WebHarness
{
	public class TestInfo
	{
		private string _url;
		private string _name;

		public string Url
		{
			get {return _url;}
			set {_url = value;}
		}
		public string Name
		{
			get {return _name;}
			set {_name = value;}
		}
	}

	public class TestsCatalog : IEnumerator, IEnumerable
	{
		private XmlDocument _testsListXml = null;
		private IEnumerator _tests = null;

		public TestsCatalog() : this("")
		{
		}
		public TestsCatalog(string catalogName) : this(catalogName, false)
		{
		}

		public TestsCatalog(string catalogName, bool collectExluded)
		{
			if (catalogName == "")
			{
				catalogName = "test_catalog.xml";
			}

			try
			{
				_testsListXml = new XmlDocument();
				_testsListXml.Load(catalogName);
			}
			catch(Exception e)
			{
				throw e;
			}

			try
			{
				if (collectExluded)
					_tests = _testsListXml.SelectNodes("/TestCases/TestCase").GetEnumerator();
				else
					_tests = _testsListXml.SelectNodes("/TestCases/TestCase[@Exclude='N']").GetEnumerator();
			}
			catch(Exception e)
			{
				throw e;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public object Current
		{
			get {return GetCurrentTestInfo();}
		}

		public bool MoveNext()
		{
			return _tests.MoveNext();
		}

		public void Reset()
		{
			_tests.Reset();
		}

		private TestInfo GetCurrentTestInfo()
		{
			XmlNode n = (XmlNode)_tests.Current;
			TestInfo ti = new TestInfo();
			ti.Name = n.Attributes["name"].Value;
			ti.Url = n.Attributes["url"].Value;
			return ti;
		}
	}
}
