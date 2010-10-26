using System.Linq;
using Mono.Cecil;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class InjectorTest
	{
		[Test]
		public void SecurityAttributesInjection()
		{
			var assemblyPath = CSharpCompiler.CompileTempAssembly(@"
			class Foo {
				public void Bar() {}
				public void Bar(int value) {}
				public void Bar(string value) {}
				public Foo() {}
				public Foo(string value) {}
			}

			class Baz {
			}

			class Gazonk {
			}
			");

			var descriptors = ParseSecurityAttributeDescriptors(@"
			SC-M: System.Void Foo::Bar(System.Int32)
			SSC-M: System.Void Foo::Bar(System.String)
			SC-M: System.Void Foo::.ctor(System.String)
			SC-T: Baz
			");

			new Injector(assemblyPath).InjectAll(descriptors);

			new AssemblySecurityVerifier(assemblyPath).Verify(descriptors);
		}
		
        [Test]
        public void InjectFromCecilSecurityAttributeDefinitions()
        {
            var assembly = AssemblyCompiler.CompileTempAssembly(@"
			class Foo {
				public void CriticalMethod() {}
				public void Bar(int value) {}
				public void Bar(string value) {}
				public void SafeCriticalMethod() {}
				public Foo(string value) {}
			}

			class CriticalType {
			}
			");

            var finder = new CecilDefinitionFinder(assembly);
            var safecriticalmethod = finder.FindMethod("System.Void Foo::SafeCriticalMethod()");
            var criticalmethod = finder.FindMethod("System.Void Foo::CriticalMethod()");
            var criticaltype = finder.FindType("CriticalType");

            var instructions = new List<CecilSecurityAttributeDescriptor>()
                                   {
                                       new CecilSecurityAttributeDescriptor(criticalmethod,SecurityAttributeType.Critical),
                                       new CecilSecurityAttributeDescriptor(safecriticalmethod,SecurityAttributeType.SafeCritical),
                                       new CecilSecurityAttributeDescriptor(criticaltype, SecurityAttributeType.Critical),
                                   };

            var injector = new Injector(assembly);
            injector.InjectAll(instructions);

            new AssemblySecurityVerifier(assembly.AssemblyPath()).Verify(instructions);
        }


	    [Test]
		public void OutputDirectory()
		{
			var assembly = GetType ().Assembly.ManifestModule.FullyQualifiedName;
			var tempPath = Path.GetTempPath ();
			var outputFile = Path.Combine (tempPath, Path.GetFileName (assembly));
			
			File.Delete(outputFile);
			
			var injector = new Injector(assembly);
			injector.OutputDirectory = tempPath;
			injector.InjectAll(ParseSecurityAttributeDescriptors(""));
			
			Assert.IsTrue(File.Exists(outputFile));
		}

		IEnumerable<SecurityAttributeDescriptor> ParseSecurityAttributeDescriptors(string descriptors)
		{
			return new SecurityAttributeDescriptorParser(new StringReader(descriptors)).Parse().ToList();
		}
	}
}

