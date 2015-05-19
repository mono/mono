//
// DataContractSerializerTest.cs
//
// Author:
//      Brendan Zagaeski 
//	Miguel de Icaza
//
// Copyright (C) 2013-2014 Xamarin Inc http://www.xamarin.com
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

namespace MonoTests.System.Runtime.Serialization
{
        [KnownType (typeof(MyObject[]))]
        [DataContract]
        public class MyObject2
        {
                [DataMember]
                public object NameList;
        }

        [KnownType(typeof(string[]))]
        [DataContract]
        public class MyObject
        {
                [DataMember]
                public object NameList;
        }
	
	[TestFixture]
	public class DataContractSerializerTestBugs {


		// Bug #15574
		[Test]
		public void SerializeMyObject ()
		{
                        var attrs = (KnownTypeAttribute[])typeof(MyObject).
                                GetCustomAttributes (typeof(KnownTypeAttribute), true);

                        for (int i = 0; i < attrs.Length; i ++) {
                                Console.WriteLine (attrs [i].Type);
                        }

                        var ser = new DataContractSerializer (typeof(MyObject));
		}

		[Test]
		public void Bug ()
		{
			var s = "<MyObject xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">\n" +
				"  <NameList i:type=\"a:ArrayOfstring\" xmlns:a=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">\n" +
				"    <a:string>Name1</a:string>\n" +
				"    <a:string>Name2</a:string>\n" + 
				"  </NameList>\n" +
				"</MyObject>";

 			//<MyObject xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/ServiceTest">

                        var ser = new DataContractSerializer(typeof(MyObject));
			ser.ReadObject (XmlReader.Create(new StringReader(s)));
		}

		[Flags ()]
		[Serializable]
		public enum FlagsEnum
		{
			None = 0,
			Flag1 = 0x10,
			Flag2 = 0x20,
			All = 0xffff,
		};

		[Serializable]
		public class ClassWithFlagsEnum
		{
			public FlagsEnum Flags = FlagsEnum.All;
		}

		// Bug #21072
		[Test]
		public void FlagsEnumTest ()
		{
			var ser = new DataContractSerializer (typeof (ClassWithFlagsEnum));

			using (var m = new MemoryStream ()) {
				ser.WriteObject (m, new ClassWithFlagsEnum ());
				var data = m.ToArray ();
				var s = Encoding.UTF8.GetString (data, 0, (int) data.Length);
				Assert.IsTrue (s.Contains ("<Flags>All</Flags>"));
			}
		}
	}
}
