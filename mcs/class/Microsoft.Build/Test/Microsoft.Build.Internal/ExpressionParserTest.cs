using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using Microsoft.Build.Execution;
using Microsoft.Build.Exceptions;
using System.Collections.Generic;

namespace MonoTests.Microsoft.Build.Internal
{
	[TestFixture]
	public class ExpressionParserTest
	{
			string [] invalid_as_boolean = {
				"$",
				"@",
				"%",
				"%-1",
				"$(",
				"%(",
				"$)",
				"%)",
				"%24",
				"()",
				"{}",
				"A", // must be evaluated as a boolean
				"1", // ditto (no default conversion to bool)
				"$ (foo) == ''",
				"@ (foo) == ''",
				"$(Foo) == And", // reserved keyword 'and'
				"$(Foo) == Or", // reserved keyword 'or'
				"$(Foo) == $(Bar) == $(Baz)", // unexpected '=='
				"$([System.String]::Format('Tr'))$([System.String]::Format('ue'))", // only one expression is accepted
			};
			string [] valid = {
				"'%24' == 0",
				"true",
				"fAlSe",
			};
			string [] depends = {
				// valid only if evaluated to boolean
				"$(foo)",
				"@(foo)",
			};
			
		[Test]
		public void EvaluateAsBoolean ()
		{
			foreach (var expr in invalid_as_boolean.Concat (valid)) {
				string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""{0}"" />
  </ItemGroup>
</Project>";
				var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
				var root = ProjectRootElement.Create (xml);
				try {
					new Project (root);
					if (invalid_as_boolean.Contains (expr))
						Assert.Fail ("Parsing Condition '{0}' should fail", expr);
				} catch (InvalidProjectFileException) {
					if (valid.Contains (expr))
						throw;
					continue;
				}
			}
		}
		
		[Test]
		public void EvaluateAsString ()
		{
			foreach (var expr in invalid_as_boolean.Concat (valid)) {
				string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""{0}"" />
  </ItemGroup>
</Project>";
				var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
				var root = ProjectRootElement.Create (xml);
				// everything should pass
				new Project (root);
			}
		}
		
		[Test]
		public void EvaluateReferencesWithProperties ()
		{
			foreach (var expr in depends) {
				string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""{0}"" />
  </ItemGroup>
</Project>";
				var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
				var root = ProjectRootElement.Create (xml);
				var props = new Dictionary<string,string> ();
				props ["foo"] = "true";
				new Project (root, props, null);
			}
		}
		
		[Test]
		public void EvaluateReferencesWithoutProperties ()
		{
			foreach (var expr in depends) {
				string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""{0}"" />
  </ItemGroup>
</Project>";
				var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
				var root = ProjectRootElement.Create (xml);
				try {
					new Project (root);
					Assert.Fail ("Parsing Condition '{0}' should fail", expr);
				} catch (InvalidProjectFileException) {
					continue;
				}
			}
		}		
	}
}

