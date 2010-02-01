//
// ScriptBehaviorDescriptorTest.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.Web.UI;

namespace Tests.System.Web.UI
{
	[TestFixture]
	public class ScriptBehaviorDescriptorTest
	{
		class PokerScriptBehaviorDescriptor : ScriptBehaviorDescriptor
		{
			public PokerScriptBehaviorDescriptor (string type, string elementID) : base (type, elementID) { }

			public string DoGetScript () {
				return GetScript ();
			}
		}

		[Test]
		public void ScriptBehaviorDescriptor_Defaults ()
		{
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");

			Assert.AreEqual ("My.Type", scd.Type, "Type");
			Assert.AreEqual ("Type", scd.Name, "Name");
			Assert.AreEqual (String.Empty, scd.ID, "ID");
			Assert.AreEqual ("Element1$Type", scd.ClientID, "ClientID");
			Assert.AreEqual ("Element1", scd.ElementID, "ElementID");

			string script = scd.DoGetScript ();
			Assert.AreEqual ("$create(My.Type, null, null, null, $get(\"Element1\"));", script, "#A1");

			scd.ID = "SomeID";
			script = scd.DoGetScript ();
			Assert.AreEqual ("$create(My.Type, {\"id\":\"SomeID\"}, null, null, $get(\"Element1\"));", script, "#A2");

			scd.Name = "SomeName";
			script = scd.DoGetScript ();
			Assert.AreEqual ("$create(My.Type, {\"id\":\"SomeID\",\"name\":\"SomeName\"}, null, null, $get(\"Element1\"));", script, "#A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScriptBehaviorDescriptor_ctor_exception_1 () {
			ScriptBehaviorDescriptor scd = new ScriptBehaviorDescriptor ("My.Type", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScriptBehaviorDescriptor_ctor_exception_2 () {
			ScriptBehaviorDescriptor scd = new ScriptBehaviorDescriptor ("My.Type", String.Empty);
		}

		[Category("NotWorking")] // One must not depend on the order of keys in dictionary
		[Test]
		public void ScriptBehaviorDescriptor_AddComponentProperty () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.AddComponentProperty ("myName1", "myCompId1");
			scd.AddComponentProperty ("myName2", "myCompId2");

			string script = scd.DoGetScript ();
#if TARGET_JVM
			Assert.AreEqual ("$create(My.Type, null, null, {\"myName2\":\"myCompId2\",\"myName1\":\"myCompId1\"}, $get(\"Element1\"));", script);
#else
			Assert.AreEqual ("$create(My.Type, null, null, {\"myName1\":\"myCompId1\",\"myName2\":\"myCompId2\"}, $get(\"Element1\"));", script);
#endif
		}

		[Category("NotWorking")] // One must not depend on the order of keys in dictionary
		[Test]
		public void ScriptBehaviorDescriptor_AddElementProperty () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.AddElementProperty ("myName1", "myElemId1");
			scd.AddElementProperty ("myName2", "myElemId2");

			string script = scd.DoGetScript ();
#if TARGET_JVM
			Assert.AreEqual ("$create(My.Type, {\"myName2\":$get(\"myElemId2\"),\"myName1\":$get(\"myElemId1\")}, null, null, $get(\"Element1\"));", script);
#else
			Assert.AreEqual ("$create(My.Type, {\"myName1\":$get(\"myElemId1\"),\"myName2\":$get(\"myElemId2\")}, null, null, $get(\"Element1\"));", script);
#endif
		}

		[Category("NotWorking")] // One must not depend on the order of keys in dictionary
		[Test]
		public void ScriptBehaviorDescriptor_AddProperty () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.AddProperty ("myName1", "myValue1");
			scd.AddProperty ("myName2", "myValue2");

			string script = scd.DoGetScript ();
#if TARGET_JVM
			Assert.AreEqual ("$create(My.Type, {\"myName2\":\"myValue2\",\"myName1\":\"myValue1\"}, null, null, $get(\"Element1\"));", script);
#else
			Assert.AreEqual ("$create(My.Type, {\"myName1\":\"myValue1\",\"myName2\":\"myValue2\"}, null, null, $get(\"Element1\"));", script);
#endif
		}

		[Category("NotWorking")] // One must not depend on the order of keys in dictionary
		[Test]
		public void ScriptBehaviorDescriptor_AddProperty_Null () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.AddProperty ("myName1", null);
			scd.AddProperty ("myName2", null);

			string script = scd.DoGetScript ();
#if TARGET_JVM
			Assert.AreEqual ("$create(My.Type, {\"myName2\":null,\"myName1\":null}, null, null, $get(\"Element1\"));", script);
#else
			Assert.AreEqual ("$create(My.Type, {\"myName1\":null,\"myName2\":null}, null, null, $get(\"Element1\"));", script);
#endif
		}

		[Category("NotWorking")] // One must not depend on the order of keys in dictionary
		[Test]
		public void ScriptBehaviorDescriptor_AddEvent () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.AddEvent ("myName1", "myHandler1");
			scd.AddEvent ("myName2", "myHandler2");

			string script = scd.DoGetScript ();
#if TARGET_JVM
			Assert.AreEqual ("$create(My.Type, null, {\"myName2\":myHandler2,\"myName1\":myHandler1}, null, $get(\"Element1\"));", script);
#else
			Assert.AreEqual ("$create(My.Type, null, {\"myName1\":myHandler1,\"myName2\":myHandler2}, null, $get(\"Element1\"));", script);
#endif
		}

		[Category("NotWorking")] // One must not depend on the order of keys in dictionary
		[Test]
		public void ScriptBehaviorDescriptor_AddScriptProperty () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.AddScriptProperty ("myName1", "myScript1");
			scd.AddScriptProperty ("myName2", "myScript2");

			string script = scd.DoGetScript ();
#if TARGET_JVM
			Assert.AreEqual ("$create(My.Type, {\"myName2\":myScript2,\"myName1\":myScript1}, null, null, $get(\"Element1\"));", script);
#else
			Assert.AreEqual ("$create(My.Type, {\"myName1\":myScript1,\"myName2\":myScript2}, null, null, $get(\"Element1\"));", script);
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScriptBehaviorDescriptor_Type_exception_1 () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.Type = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScriptBehaviorDescriptor_Type_exception_2 () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.Type = String.Empty;
		}

		[Test]
		public void ScriptBehaviorDescriptor_Type () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.Type = "New.Type";
			Assert.AreEqual ("New.Type", scd.Type, "Type");
		}

		[Test]
		public void ScriptBehaviorDescriptor_ID () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.ID = null;
			Assert.AreEqual (String.Empty, scd.ID, "#1");
			scd.ID = String.Empty;
			Assert.AreEqual (String.Empty, scd.ID, "#2");
			scd.ID = "My ID";
			Assert.AreEqual ("My ID", scd.ID, "#3");
			Assert.AreEqual ("My ID", scd.ClientID, "#4");
		}

		[Test]
		public void ScriptBehaviorDescriptor_Name () {
			PokerScriptBehaviorDescriptor scd = new PokerScriptBehaviorDescriptor ("My.Type", "Element1");
			scd.Name = null;
			Assert.AreEqual ("Type", scd.Name, "#1");
			scd.Name = String.Empty;
			Assert.AreEqual ("Type", scd.Name, "#2");
			scd.Name = "MyName";
			Assert.AreEqual ("MyName", scd.Name, "#3");
			Assert.AreEqual ("Element1$MyName", scd.ClientID, "#4");
			scd.Name = "MyName.MyType";
			Assert.AreEqual ("MyName.MyType", scd.Name, "#5");
			scd.Name = null;
			Assert.AreEqual ("Type", scd.Name, "#6");
		}
	}
}
