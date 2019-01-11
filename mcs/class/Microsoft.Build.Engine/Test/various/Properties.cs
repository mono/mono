//
// Properties.cs
//
// Authors:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Marek Safar (marek.safar@gmail.com)
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
using System.Xml;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.BuildEngine.Various {
	[TestFixture]
	public class Properties {

		Project proj;

		[SetUp]
		public void Setup ()
		{
			Engine engine = new Engine (Consts.BinPath);
			proj = engine.CreateNewProject ();
		}

		[Test]
		public void PropertyReference ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Config>debug</Config>
						<ExpProp>$(Config)-$(Config)</ExpProp>
						<ExpProp2> $(Config) $(Config) </ExpProp2>
						<InvProp1>$(Config-$(Config)</InvProp1>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.AreEqual (1, proj.PropertyGroups.Count, "A1");
			Assert.AreEqual ("debug", proj.GetEvaluatedProperty ("Config"), "A2");
			Assert.AreEqual ("debug-debug", proj.GetEvaluatedProperty ("ExpProp"), "A3");
			Assert.AreEqual (" debug debug ", proj.GetEvaluatedProperty ("ExpProp2"), "A4");	
			Assert.AreEqual ("$(Config-$(Config)", proj.GetEvaluatedProperty ("InvProp1"), "A5");
		}

		[Test]
		[Category ("NotDotNet")]
		public void PropertyReference2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>B</B>
					</PropertyGroup>

					<Target Name='Main' >
						<StringTestTask Array='$(A)$(B)'>
							<Output TaskParameter='Array' ItemName='Out' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			proj.Build ("Main");
			Assert.AreEqual (1, proj.GetEvaluatedItemsByName ("Out").Count, "A1");
			Assert.AreEqual ("AB", proj.GetEvaluatedItemsByName ("Out") [0].Include, "A2");
		}

		[Test]
		public void StringInstanceProperties ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<Config>debug</Config>
							<NullValue>null</NullValue>
							<TargetValue>  </TargetValue>
							<StringWithQuotes>abc""def</StringWithQuotes>
							<Prop1>$(Config.Substring(0,3)) </Prop1>
							<Prop2>$(Config.Length )</Prop2>
							<Prop3>$(Config.StartsWith ('DE', System.StringComparison.OrdinalIgnoreCase))</Prop3>
							<Prop4>$(NullValue.StartsWith ('Te', StringComparison.OrdinalIgnoreCase))</Prop4>
							<Prop5>$(TargetValue.Trim('\\'))</Prop5>
							<Prop6>$(StringWithQuotes.Replace('""', ""'""))</Prop6>
							<Prop7>$(StringWithQuotes.Replace('""', ''))</Prop7>
							<Prop8>$(StringWithQuotes.Replace('""', """"))</Prop8>
							<Prop9>$(StringWithQuotes.Replace('""', ``))</Prop9>
							<Prop9>$(StringWithQuotes.Replace(`c""d`, `2""'3`))</Prop9>
						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual ("deb ", proj.GetEvaluatedProperty ("Prop1"), "#1");
			Assert.AreEqual ("5", proj.GetEvaluatedProperty ("Prop2"), "#2");
			Assert.AreEqual ("True", proj.GetEvaluatedProperty ("Prop3"), "#3");
			Assert.AreEqual ("False", proj.GetEvaluatedProperty ("Prop4"), "#4");
			Assert.AreEqual ("", proj.GetEvaluatedProperty ("Prop5"), "#5");
			Assert.AreEqual ("abc'def", proj.GetEvaluatedProperty ("Prop6"), "#6");
			Assert.AreEqual ("abcdef", proj.GetEvaluatedProperty ("Prop7"), "#7");
			Assert.AreEqual ("abcdef", proj.GetEvaluatedProperty ("Prop8"), "#8");
			Assert.AreEqual ("ab2\"'3ef", proj.GetEvaluatedProperty ("Prop9"), "#9");
		}

		[Test]
		[SetCulture ("en-us")]
		public void AllowedFrameworkMembers ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<Prop1>$([System.Byte]::MaxValue)</Prop1>
							<Prop2>$([System.math]::Abs (-4.2) )</Prop2>
							<Prop3>$([System.DateTime]::Today )</Prop3>
							<Prop4>$([System.Char]::GetNumericValue('3'))</Prop4>
							<Prop5>$([System.String]::Compare (Null, nUll))</Prop5>
							<Prop6>$([System.Environment]::GetLogicalDrives ( ))</Prop6>
							<Prop7>$([System.String]::Concat (`,`, `n`, `,`))</Prop7>
						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual ("255", proj.GetEvaluatedProperty ("Prop1"), "#1");
			Assert.AreEqual ("4.2", proj.GetEvaluatedProperty ("Prop2"), "#2");
			Assert.AreEqual (DateTime.Today.ToString (), proj.GetEvaluatedProperty ("Prop3"), "#3");
			Assert.AreEqual ("3", proj.GetEvaluatedProperty ("Prop4"), "#4");
			Assert.AreEqual ("0", proj.GetEvaluatedProperty ("Prop5"), "#5");
			Assert.AreEqual (string.Join (";", Environment.GetLogicalDrives ()), proj.GetEvaluatedProperty ("Prop6"), "#6");
			Assert.AreEqual (",n,", proj.GetEvaluatedProperty ("Prop7"), "#7");
		}

		[Test]
		public void InstanceMethodOnStaticProperty ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<Prop1>$([System.DateTime]::Now.ToString(""yyyy.MM.dd""))</Prop1>
						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual (DateTime.Now.ToString ("yyyy.MM.dd"), proj.GetEvaluatedProperty ("Prop1"), "#1");
		}

		[Test]
		public void InstanceMemberOnStaticProperty ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<Prop1>$([System.DateTime]::Now.Year)</Prop1>
						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual (DateTime.Now.Year.ToString (), proj.GetEvaluatedProperty ("Prop1"), "#1");
		}

		[Test]
		public void InstanceMembersOnStaticMethod ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<Prop1>$([System.String]::Concat('a', 'bb', 'c').Length.GetHashCode ())</Prop1>

						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual (4.GetHashCode ().ToString (), proj.GetEvaluatedProperty ("Prop1"), "#1");
		}

		[Test]
		[SetCulture ("en-us")]
		public void MSBuildPropertyFunctions ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<NumberOne>0.6</NumberOne>
							<NumberTwo>6</NumberTwo>
							<Prop1>$([MSBuild]::Add($(NumberOne), $(NumberTwo)))</Prop1>
						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual ("6.6", proj.GetEvaluatedProperty ("Prop1"), "#1");
		}

		[Test]
		public void Constructor ()
		{
			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<PropertyGroup>
							<NumberOne>0.6</NumberOne>
							<NumberTwo>6</NumberTwo>
							<Prop1>$([System.String]::new('value').EndsWith ('ue'))</Prop1>
						</PropertyGroup>
					</Project>
				";

			proj.LoadXml (documentString);
			Assert.AreEqual ("True", proj.GetEvaluatedProperty ("Prop1"), "#1");
		}
	}
}
