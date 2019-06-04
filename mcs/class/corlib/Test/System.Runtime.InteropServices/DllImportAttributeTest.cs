//
// DllImportAttributeTest.cs
//
// Authors:
//  Alexander Köplinger (alkpli@microsoft.com)
//
// (c) 2017 Microsoft
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
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Runtime.InteropServices {

	/// <summary>
	/// Summary description for DllImportAttributeTest.
	/// </summary>
	[TestFixture]
	public class DllImportAttributeTest
	{
#if !MOBILE
		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		DllImportAttribute attr;
		
		public DllImportAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain ().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (DllImportAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type [] { typeof (string) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder (ctrInfo, new object [1] { "libc.dylib" });
			dynAssembly.SetCustomAttribute (attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as DllImportAttribute;
		}

		[Test]
		public void DllImportTest ()
		{
			Assert.AreEqual (
				attr.Value,
				"libc.dylib", "#1");
		}

		[Test]
		public void TypeIdTest ()
		{
			Assert.AreEqual (
				attr.TypeId,
				typeof (DllImportAttribute), "#1"
				);
		}

		[Test]
		public void MatchTestForTrue ()
		{
			Assert.AreEqual (
				attr.Match (attr),
				true, "#1");
		}

		[Test]
		public void MatchTestForFalse ()
		{
			Assert.AreEqual (
				attr.Match (new DllImportAttribute ("libcups.dylib")),
				false, "#1");
		}
#endif
		[Test]
		public void CtorTest ()
		{
			var a = new DllImportAttribute ("some text");
			Assert.AreEqual ("some text", a.Value);
		}

		[Test]
		public void FieldsTest ()
		{
			var a = new DllImportAttribute ("libc.dylib");

			Assert.Null (a.EntryPoint);
			Assert.AreEqual ((CharSet)0, a.CharSet);
			Assert.False (a.SetLastError);
			Assert.False (a.ExactSpelling);
			Assert.AreEqual ((CallingConvention)0, a.CallingConvention);
			Assert.False (a.BestFitMapping);
			Assert.False (a.PreserveSig);
			Assert.False (a.ThrowOnUnmappableChar);
		}
	}
}

