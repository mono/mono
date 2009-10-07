//
// RemoveDuplicatesTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace MonoTests.Microsoft.Build.Tasks
{
	[TestFixture]
	public class RemoveDuplicatesTest
	{
		[Test]
		public void Test1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion='3.5'>
				
				<ItemGroup>
					<Items Include='A'>
						<MD>Value1</MD>
					</Items>
					<Items Include='A'>
						<MD>Value1</MD>
					</Items>

					<Items Include='A'>
						<MD>Value2</MD>
						<MD2>Value2</MD2>
					</Items>

					<Items Include='B'>
						<MD>Value1</MD>
					</Items>
					<Items Include='B'>
						<MD>Value1</MD>
					</Items>
					<Items Include='C'>
						<MD>Value1</MD>
					</Items>
					<Items Include='A'>
						<MD>Value3</MD>
						<MD3>Value3</MD3>
					</Items>
				</ItemGroup>

					<Target Name='Main'>
						<RemoveDuplicates Inputs='@(Items)'>
							<Output TaskParameter='Filtered' ItemName='Filtered'/>
						</RemoveDuplicates>
						<Message Text=""Filtered items: %(Filtered.Identity) MD: %(Filtered.MD) MD2: %(Filtered.MD2) MD3: %(Filtered.MD3)""/>
					</Target>
				</Project>
			";

			Engine engine = new Engine (Consts.BinPath);

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			Project project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			if (!project.Build ("Main")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			testLogger.CheckLoggedMessageHead ("Filtered items: A MD: Value1 MD2:  MD3: ", "A1");
			testLogger.CheckLoggedMessageHead ("Filtered items: B MD: Value1 MD2:  MD3: ", "A2");
			testLogger.CheckLoggedMessageHead ("Filtered items: C MD: Value1 MD2:  MD3: ", "A3");
			Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
		}
	}
}
