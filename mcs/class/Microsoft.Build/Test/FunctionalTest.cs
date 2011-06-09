//
// FunctionalTest.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//
// (C) 2011 Leszek Ciesielski
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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.IO;
using System.Globalization;

namespace MonoTests.Microsoft.Build
{
        [TestFixture]
        public class FunctionalTest
        {
                [Test]
                public void TestFullProjectGeneration ()
                {
                        var project = ProjectRootElement.Create ();
                        var projectFileName = String.Format ("Test{0}FunctionalTestProject.csproj",
                                Path.DirectorySeparatorChar);
                        project.DefaultTargets = "Build";
                        SetKnownProperties (project.AddPropertyGroup ());
                        GenerateReferences (project.AddItemGroup ());
                        GenerateCompileIncludes (project.AddItemGroup ());
                        project.AddImport ("$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
                        
                        project.Save (projectFileName);
                        Assert.AreEqual (projectGuid, GetProjectId (project), "#01");
                        FileAssert.AreEqual (String.Format ("Test{0}FunctionalTestReferenceProject.csproj",
                                Path.DirectorySeparatorChar), String.Format ("Test{0}FunctionalTestProject.csproj",
                                Path.DirectorySeparatorChar), "#02");
                }

                [Test]
                public void TestLoadAndSave ()
                {
                        var project = ProjectRootElement.Open ("Microsoft.Build.csproj");
                        var projectFileName = String.Format ("Test{0}FunctionalTestProject2.csproj",
                                Path.DirectorySeparatorChar);
                        project.Save (projectFileName);
                        
                        Assert.AreEqual (new Guid ("{B2012E7F-8F8D-4908-8045-413F2BD1022D}"), GetProjectId (project),
                                "#03");
                        FileAssert.AreEqual ("Microsoft.Build.csproj", projectFileName, "#04");
                }

                [Test]
                public void TestLoadAndSave3 ()
                {
                        var referenceProject = String.Format (
                                "Test{0}FunctionalTestReferenceProject3.csproj", Path.DirectorySeparatorChar);
                        var project = ProjectRootElement.Open (referenceProject);
                        var projectFileName = String.Format ("Test{0}FunctionalTestProject3.csproj",
                                Path.DirectorySeparatorChar);
                        project.Save (projectFileName);

                        Assert.AreEqual (new Guid ("{793B20A9-E263-4B54-BB31-305B602087CE}"), GetProjectId (project),
                                "#05");
                        FileAssert.AreEqual (referenceProject, projectFileName, "#06");
                }

                public Guid GetProjectId (ProjectRootElement project)
                {
                        var value = project.Properties.Where (p => p.Name == "ProjectGuid").First ().Value;
                        return Guid.Parse (value);
                }

                void GenerateCompileIncludes (ProjectItemGroupElement itemGroup)
                {
                        foreach (var include in new[] { "Test.cs", "Test2.cs", "Test3.cs" }) {
                                itemGroup.AddItem ("Compile", ProjectCollection.Escape (include),
                                        new[] { new KeyValuePair<string, string> ("SubType", "Code") });
                        }
                        foreach (var resource in new[] { "Test.resx", "Test2.resx", "Test3.resx" }) {
                                itemGroup.AddItem ("EmbeddedResource", ProjectCollection.Escape (resource),
                                        new[] { new KeyValuePair<string, string> ("LogicalName", "Name.Space.Test") });
                        }
                }

                void GenerateReferences (ProjectItemGroupElement itemGroup)
                {
                        foreach (var reference in new[] { "Test.dll", "Test2.dll", "Test3.dll" }) {
                                var name = Path.GetFileNameWithoutExtension (reference);
                                itemGroup.AddItem ("Reference", name, new[] {
                                        new KeyValuePair<string, string> ("Name", name),
                                        new KeyValuePair<string, string> ("HintPath",
                                                ProjectCollection.Escape (reference)) });
                        }
                        foreach (var reference in new[] { "mscorlib", "System", "System.Xml" }) {
                                itemGroup.AddItem ("Reference", reference);
                        }
                }

                void SetKnownProperties (ProjectPropertyGroupElement properties)
                {
                        properties.AddProperty ("AssemblyName", Path.GetFileNameWithoutExtension ("ZigZag.exe"));
                        properties.AddProperty ("ProjectGuid", projectGuid.ToString ("B"));
                        properties.AddProperty ("CheckForOverflowUnderflow", false.ToString ());
                        properties.AddProperty ("CodePage", String.Empty);
                        properties.AddProperty ("DebugSymbols", true.ToString ());
                        properties.AddProperty ("DebugType", "Full");
                        properties.AddProperty ("Optimize", false.ToString ());
                        properties.AddProperty ("Platform", "AnyCPU");
                        properties.AddProperty ("ProjectTypeGuids", projectTypeGuid.ToString ("B"));
                        properties.AddProperty ("AllowUnsafeBlocks", false.ToString ());
                        properties.AddProperty ("WarningLevel", "4");
                        properties.AddProperty ("OutputPath", ProjectCollection.Escape ("bin\\Debug"));
                        properties.AddProperty ("OutputType", "winexe");
                        properties.AddProperty ("DefineConstants", "DEBUG,TRACE");
                        properties.AddProperty ("TreatWarningsAsErrors", true.ToString ());
                }

                static readonly Guid projectTypeGuid = Guid.Parse ("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
                static readonly Guid projectGuid = Guid.Parse ("{362F36B0-B26C-42DE-8586-20DF66BD05E8}");
        }
}
