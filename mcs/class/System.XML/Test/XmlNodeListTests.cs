using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlNodeListTests : TestCase
	{
		public XmlNodeListTests () : base ("Ximian.Mono.Tests.XmlNodeListTests testsuite") {}
		public XmlNodeListTests (string name) : base (name) {}

		private XmlElement element;

		protected override void SetUp ()
		{
			XmlDocument document = new XmlDocument ();

			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
		}

		///////////////////////////////////////////////////////////////////////
		//
		//  XmlNodeListChildren tests.
		//
		///////////////////////////////////////////////////////////////////////
		
		public void TestChildren()
		{
			Assert ("Incorrect number of children returned from Count property.", element.ChildNodes.Count == 3);
			AssertNull ("Index less than zero should have returned null.", element.ChildNodes [-1]);
			AssertNull ("Index greater than or equal to Count should have returned null.", element.ChildNodes [3]);
			AssertEquals ("Didn't return the correct child.", element.FirstChild, element.ChildNodes[0]);
			AssertEquals ("Didn't return the correct child.", "child1", element.ChildNodes[0].LocalName);
			AssertEquals ("Didn't return the correct child.", "child2", element.ChildNodes[1].LocalName);
			AssertEquals ("Didn't return the correct child.", "child3", element.ChildNodes[2].LocalName);

			int index = 1;
			foreach (XmlNode node in element.ChildNodes) {
				AssertEquals ("Enumerator didn't return correct node.", "child" + index.ToString(), node.LocalName);
				index++;
			}
		}


		///////////////////////////////////////////////////////////////////////
		//
		//  XmlNodeListSelect tests.
		//
		///////////////////////////////////////////////////////////////////////
	}
}
