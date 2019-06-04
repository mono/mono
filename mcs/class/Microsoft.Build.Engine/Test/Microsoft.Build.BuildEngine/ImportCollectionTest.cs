//
// ImportCollectionTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ImportCollectionTest {
		
		Engine			engine;
		Project			project;
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestAdd1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='project_that_doesnt_exist'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);
		}

		[Test]
		public void TestAdd2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.AreEqual (1, project.Imports.Count, "A1");
			Assert.AreEqual (false, project.Imports.IsSynchronized, "A2");
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCopyTo1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Imports.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestCopyTo2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Imports.CopyTo (new Import [1], -1);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void TestCopyTo3 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Imports.CopyTo (new Import [][] { new Import [] { null } }, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCopyTo4 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Imports.CopyTo (new Import [1], 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCopyTo5 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/Import.csproj") + @"'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Imports.CopyTo (new Import [1], 1);
		}
	}
}
