//
// DataContractSerializerTest_FrameworkTypes.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft.co http://www.mainsoft.com
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
//
// This test code contains tests for attributes in System.Runtime.Serialization
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

using MonoTests.Helpers;

namespace MonoTests.System.Runtime.Serialization
{
	public class DataContractSerializerTest_FrameworkTypes
	{
		protected void Test<T> () where T : new () {
			T o = new T ();
			Type t = o.GetType ();
			string fileName = TestResourceHelper.GetFullPathOfResource ("Test/Resources/FrameworkTypes/" + t.FullName + ".xml");

			DataContractSerializer serializer = new DataContractSerializer (t);
			StringBuilder stringBuilder = new StringBuilder ();
			using (XmlWriter xmlWriter = XmlWriter.Create (new StringWriter (stringBuilder)))
				serializer.WriteObject (xmlWriter, o);
			string actualXml = stringBuilder.ToString ();
			string expectedXml = File.ReadAllText (fileName);

			XmlComparer.AssertAreEqual (expectedXml, actualXml, "Serialization of " + t.FullName + " failed.");

			using (FileStream fs = File.OpenRead (fileName)) {
				o = (T) serializer.ReadObject (fs);
			}
		}
	}
}
