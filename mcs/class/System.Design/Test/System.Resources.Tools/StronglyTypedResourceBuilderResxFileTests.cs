//
// StronglyTypedResourceBuilderResxFileTests.cs - tests overloads of Create
// method that accept resx file names
// 
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) Gary Barnett (2012)
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
#if NET_2_0

using NUnit.Framework;
using System;
using System.Resources.Tools;
using Microsoft.CSharp;
using System.IO;
using System.CodeDom;
using System.Resources;
using System.Drawing;

namespace MonoTests.System.Resources.Tools {
	[TestFixture]
	public class StronglyTypedResourceBuilderResxFileTests 	{	
		CSharpCodeProvider provider = new CSharpCodeProvider ();
		
		[Test, ExpectedException (typeof (ArgumentException))]
		public void ResXFilenameEmpty ()
		{
			// in .NET framework throws exception
			string [] unmatchables;

			string resx = String.Empty;
			
			StronglyTypedResourceBuilder.Create (resx,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		/* FIXME: need platform dependant check, (file returns not found if invalid anyway)
		[Test, ExpectedException (typeof (ArgumentException))]
		public void ResXFilenameInvalid ()
		{
			// in .NET framework throws exception
			string [] unmatchables;

			string resx = @"C::::\\\\Hello/World";
			
			StronglyTypedResourceBuilder.Create (resx,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		*/
		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ResXFilenameNull ()
		{
			//should throw exception
			string [] unmatchables;

			string resx = null;
			
			StronglyTypedResourceBuilder.Create (resx,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		
		[Test, ExpectedException (typeof (FileNotFoundException))]
		public void ResXFileNotFound ()
		{
			// not documented on msdn but throws FileNotFoundException
			string [] unmatchables;

			//get a valid new filename and then make it not exist
			string resx = Path.GetTempFileName ();
			File.Delete (resx);
			
			StronglyTypedResourceBuilder.Create (resx,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		
		[Test]
		public void ResXFileNotResx ()
		{
			//***should throw exception but Not using ExpectedException as i want to delete temp file***
			string [] unmatchables;
			bool exceptionRaised = false;

			string resx = Path.GetTempFileName();
			
			try {
				StronglyTypedResourceBuilder.Create (resx,
									"TestRes",
									"TestNamespace",
									"TestResourcesNameSpace",
									provider,
									true,
									out unmatchables);
			} catch (Exception ex) {
				exceptionRaised = true;
				Assert.IsInstanceOfType (typeof (ArgumentException), ex);
			} finally {
				Assert.IsTrue (exceptionRaised);
				File.Delete (resx);
			}
		}
		
		[Test]
		public void ResxFileProcessed ()
		{
			// resources in resx should be present in codecompileunit with correct property type
			string [] unmatchables;
			CodeCompileUnit ccu;
			
			Bitmap bmp = new Bitmap (100, 100);
			MemoryStream wav = new MemoryStream (1000);
			
			string resxFileName = Path.GetTempFileName();
			
			using (ResXResourceWriter writer = new ResXResourceWriter(resxFileName)) {
				writer.AddResource ("astring", "myvalue"); // dont use key of "string" as its a keyword
				writer.AddResource ("bmp", bmp);
				writer.AddResource ("wav", wav);
				writer.Generate ();
			}
			
			ccu = StronglyTypedResourceBuilder.Create (resxFileName,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
			
			CodeMemberProperty cmp;
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("astring", ccu);
			Assert.IsNotNull (cmp);
			Assert.AreEqual ("System.String", cmp.Type.BaseType);
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("wav", ccu);
			Assert.IsNotNull (cmp);
			Assert.AreEqual ("System.IO.UnmanagedMemoryStream", cmp.Type.BaseType);
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("bmp", ccu);
			Assert.IsNotNull (cmp);
			Assert.AreEqual ("System.Drawing.Bitmap", cmp.Type.BaseType);
			
			wav.Close ();
			File.Delete (resxFileName);
		}
	}
}

#endif
