//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com

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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class KnownTypeAttributeTest
	{
		void Serialize (object instance)
		{
			var ds = new DataContractSerializer (instance.GetType ());
			using (var xw = XmlWriter.Create (TextWriter.Null))
				ds.WriteObject (xw, instance);
		}

		[Test]
		public void MethodName ()
		{
			Serialize (new Data () { X = new Bar () });
		}

		[Test]
		public void MethodName2 ()
		{
			Serialize (new Data2 () { X = new Bar () });
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void MethodName3 ()
		{
			Serialize (new Data3 () { X = new Bar () });
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void MethodName4 ()
		{
			Serialize (new Data4 () { X = new Bar () });
		}

		public class Foo
		{
			public string S { get; set; }
		}

		public class Bar : Foo
		{
			public string T { get; set; }
		}

		[DataContract]
		[KnownType ("GetTypes")]
		public class Data
		{
			[DataMember]
			public Foo X { get; set; }

			public static IEnumerable<Type> GetTypes ()
			{
				yield return typeof (Bar);
			}
		}

		[DataContract]
		[KnownType ("GetTypes")]
		public class Data2
		{
			[DataMember]
			public Foo X { get; set; }

			static IEnumerable<Type> GetTypes () // non-public
			{
				yield return typeof (Bar);
			}
		}

		[DataContract]
		[KnownType ("GetTypes")]
		public class Data3
		{
			[DataMember]
			public Foo X { get; set; }

			public IEnumerable<Type> GetTypes () // non-static
			{
				yield return typeof (Bar);
			}
		}

		[DataContract]
		[KnownType ("GetTypes")]
		public class Data4
		{
			[DataMember]
			public Foo X { get; set; }

			public static IEnumerable<Type> GetTypes (ICustomAttributeProvider provider) // wrong args
			{
				yield return typeof (Bar);
			}
		}
	}
}
#endif
