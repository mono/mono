//
// AssociatedMetadataTypeTypeDescriptionProvider.cs
//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
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

#if !MOBILE
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class AssociatedMetadataTypeTypeDescriptionProviderTests
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructors_Null_Type ()
		{
			var v = new AssociatedMetadataTypeTypeDescriptionProvider (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructors_Null_Type_2 ()
		{
			var v = new AssociatedMetadataTypeTypeDescriptionProvider (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructors_Null_AssociatedMetadataType_2 ()
		{
			var v = new AssociatedMetadataTypeTypeDescriptionProvider (typeof (string), null);
		}

		[Test]
		public void GetTypeDescriptor ()
		{
			var v = new AssociatedMetadataTypeTypeDescriptionProvider (typeof (string));

			Assert.IsTrue (v != null, "#A1");

			string s = "test";
			var o = v.GetTypeDescriptor (s.GetType (), s);
			Assert.IsTrue (o != null, "#B1");

			Type oType = o.GetType ();
			Assert.AreEqual ("System.ComponentModel.DataAnnotations.AssociatedMetadataTypeTypeDescriptor", oType.ToString (), "#C1");
			Assert.IsTrue (typeof (ICustomTypeDescriptor).IsAssignableFrom (oType), "#C2");
			Assert.IsTrue (typeof (CustomTypeDescriptor).IsAssignableFrom (oType), "#C3");
		}
	}
}
#endif