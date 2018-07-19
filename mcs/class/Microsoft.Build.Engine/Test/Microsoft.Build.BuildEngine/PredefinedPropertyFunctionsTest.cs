//
// PredefinedPropertyFunctionsTest.cs
//
// Authors:
//   Alexander Köplinger (alex.koeplinger@outlook.com)
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
using System.IO;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class PredefinedPropertyFunctionsTest {

		[Test]
		public void TestMakeRelative ()
		{
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Path1>c:\users\</Path1>
						<Path2>c:\users\username\</Path2>
						<Path3>/home/user/</Path3>
						<Path4>/home/user/username/</Path4>
						<Path5>/home/user/</Path5>
						<Path6>/tmp/test/file</Path6>
						<Path7>/home/user/</Path7>
						<Path8>/home/user/username/path with spaces + special? chars (1)/</Path8>
						<Path9>c:\users</Path9>
						<Path10>c:\users\username\test\</Path10>
						<Path11>/home/user</Path11>
						<Path12>/home/user/username/test/</Path12>
						<MakeRelative1>$([MSBuild]::MakeRelative($(Path1), $(Path2)))</MakeRelative1>
						<MakeRelative2>$([MSBuild]::MakeRelative($(Path2), $(Path1)))</MakeRelative2>
						<MakeRelative3>$([MSBuild]::MakeRelative($(Path3), $(Path4)))</MakeRelative3>
						<MakeRelative4>$([MSBuild]::MakeRelative($(Path4), $(Path3)))</MakeRelative4>
						<MakeRelative5>$([MSBuild]::MakeRelative($(Path5), $(Path6)))</MakeRelative5>
						<MakeRelative6>$([MSBuild]::MakeRelative($(Path7), $(Path8)))</MakeRelative6>
						<MakeRelative7>$([MSBuild]::MakeRelative($(Path9), $(Path10)))</MakeRelative7>
						<MakeRelative8>$([MSBuild]::MakeRelative($(Path11), $(Path12)))</MakeRelative8>
					</PropertyGroup>
				</Project>
			";

			if (Path.DirectorySeparatorChar == '\\') {
				documentString = documentString.Replace ("/home", "c:/home");
				documentString = documentString.Replace ("/tmp", "c:/tmp");
			}

			var engine = new Engine (Consts.BinPath);
			var project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (@"username\", project.EvaluatedProperties ["MakeRelative1"].FinalValue, "#1");
			Assert.AreEqual (@"..\", project.EvaluatedProperties ["MakeRelative2"].FinalValue, "#2");
			Assert.AreEqual (@"username\", project.EvaluatedProperties ["MakeRelative3"].FinalValue, "#3");
			Assert.AreEqual (@"..\", project.EvaluatedProperties ["MakeRelative4"].FinalValue, "#4");
			Assert.AreEqual (@"..\..\tmp\test\file", project.EvaluatedProperties ["MakeRelative5"].FinalValue, "#5");
			Assert.AreEqual (@"username\path with spaces + special? chars (1)\", project.EvaluatedProperties ["MakeRelative6"].FinalValue, "#6");
			Assert.AreEqual (@"username\test\", project.EvaluatedProperties ["MakeRelative7"].FinalValue, "#7");
			Assert.AreEqual (@"username\test\", project.EvaluatedProperties ["MakeRelative8"].FinalValue, "#8");
		}
	}
}
