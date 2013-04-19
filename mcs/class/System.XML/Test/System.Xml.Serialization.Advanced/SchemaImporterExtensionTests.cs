// 
// SchemaImporterExtensionTests.cs 
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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
using System.CodeDom;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;
using NUnit.Framework;

namespace MonoTests.System.Xml.Serialization.Advanced
{
	[TestFixture]
	public class SchemaImporterExtensionTests
	{
		class MyExtension : SchemaImporterExtension
		{
		}

		[Test]
		public void ImportAnyElement ()
		{
			Assert.IsNull(new MyExtension ().ImportAnyElement (
				null, false, null, null, null, null, CodeGenerationOptions.None, null));
		}

		[Test]
		public void ImportDefaultValue ()
		{
			Assert.IsNull (new MyExtension ().ImportDefaultValue (null, null), "#1");
			Assert.IsNull (new MyExtension ().ImportDefaultValue ("test", "string"), "#2");
		}

		[Test]
		public void ImportSchemaType ()
		{
			Assert.IsNull (new MyExtension ().ImportSchemaType (
				null, null, null, null, null, null, CodeGenerationOptions.None, null), "#1");
			Assert.IsNull (new MyExtension ().ImportSchemaType (
				null, null, null, null, null, null, null, CodeGenerationOptions.None, null), "#2");
		}
	}
}

#endif
