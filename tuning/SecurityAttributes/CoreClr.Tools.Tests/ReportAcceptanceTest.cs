using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class ReportAcceptanceTest
	{
		[Test]
		public void Test()
		{
			// Step 1 - Step1: Detect methods that will fail at runtime if they're not [SC] or [SSC]

			const string requiresPrivilegesManual = @"
			System.Object Foo::Bar(System.Int32)
";

			var assembly1 = CompileTempAssembly(@"
			public class Foo {
				public static object Bar(int i) { return null; }
			}

			public class A {
				public unsafe void M1(int* i) {}
				public void M2() { Foo.Bar(0); }
				public void M3() { }
				public void M4() { M2(); }
				public unsafe void M5(int* i) { M1(i); }
			}

			unsafe public class CriticalType1 : I1
			{
				int* a;
				
				public void M6()
				{
					Foo.Bar(3);
				}
			}

			public interface I1 {
				void M6();
			}

			public interface I2 {
				void NotImplemented();
			}
");
			var assembly2 = CompileTempAssembly(@"

			using System.Runtime.CompilerServices;

			public class B : I1 {
				public void M1() {
					Foo.Bar(0);
				}

				[MethodImpl(MethodImplOptions.InternalCall)]
				public extern void M2();

				[MethodImpl(MethodImplOptions.InternalCall)]
				internal extern void M3();

				[MethodImpl(MethodImplOptions.InternalCall)]
				public extern void M4();

				public void M5() { M4(); }

				public void M6() { M1(); }
			}
", assembly1.MainModule.Image.FileInformation.FullName);

			var expectedRequiresPrivileges = new[]
			{ @"# This file has a list of methods that were automatically detected to require privileges ([sc] or [ssc])
# in order to not generate an exception at runtime.

# using 'System.Int32*' as a parameter type
System.Void A::M1(System.Int32*)

# using 'System.Int32*' as a parameter type
System.Void A::M5(System.Int32*)",

 @"# This file has a list of methods that were automatically detected to require privileges ([sc] or [ssc])
# in order to not generate an exception at runtime.

# internal call
System.Void B::M2()

# internal call
System.Void B::M3()

# internal call
System.Void B::M4()".Replace("\n","\r\n")
			};

			var assemblies = new[] { assembly1, assembly2 };
			AssemblySetResolver.SetUp(assemblies);

			var cdf = new CecilDefinitionFinder(assembly1);
			var criticalTypes = new[] { cdf.GetType("CriticalType1") };

			var actualRequiresPrivileges = DetectMethodsRequiringPrivileges(criticalTypes, assemblies);

			Assert.AreEqual(expectedRequiresPrivileges, actualRequiresPrivileges.ToArray());


			// Step 2 - "Floodfill" requiredprivileges methods to all their callers, only being stopped by [SSC]
			var canBeSscManual = new[]
			                     	{
										// assembly1
			                     		Compatibility.ParseMoonlightAuditFormat(@"
!SSC-AA4CC9592C6775C5327AAA826B6693FF
System.Void A::M2()
	lucas - I checked this, it's totally safe"),

										// assembly2
										Compatibility.ParseMoonlightAuditFormat(@"
!SSC-AA4CC9592C6775C5327AAA826B6693FF
System.Void B::M2()
	lucas - I checked this, it's totally safe")

			                     	};

			/*
			 * publicApis  -> shows for each public entry point wether it's going to be [SC] or not.  for each [SC] method.
			 * injectionInstructions -> all methods (public/nonpublic) that require decoration go in here.
			 */

			actualRequiresPrivileges[0] += requiresPrivilegesManual;

			var propagationReport = PropagateRequiredPrivileges(criticalTypes, canBeSscManual, actualRequiresPrivileges.ToArray(), assembly1, assembly2);

			const string expectedInjectionInstructions = @"
SC-M: System.Object Foo::Bar(System.Int32)
SC-M: System.Void A::M1(System.Int32*)
SC-M: System.Void A::M5(System.Int32*)
SSC-M: System.Void A::M2()
SC-M: System.Void B::M1()
SSC-M: System.Void B::M2()
SC-M: System.Void B::M3()
SC-M: System.Void B::M4()
SC-M: System.Void B::M5()
SC-M: System.Void B::M6()
SC-M: System.Void I1::M6()
SC-T: CriticalType1
";
			StringAssert.AssertLinesAreEquivalent(expectedInjectionInstructions, propagationReport.InjectionInstructions);

			const string expectedPublicApis = @"
System.Void A::.ctor()    #available
System.Void A::M2()    #available
System.Void A::M3()    #available
System.Void A::M4()    #available
System.Void A::M5(System.Int32*)    #unavailable: method itself requires privileges
System.Void B::.ctor()    #available
System.Void B::M2()    #available
System.Void Foo::.ctor()    #available
System.Void I2::NotImplemented()    #available
System.Void A::M1(System.Int32*)    #unavailable: method itself requires privileges
System.Void B::M1()    #unavailable: calls System.Object Foo::Bar(System.Int32) which requires privileges itself
System.Void B::M4()    #unavailable: method itself requires privileges
System.Void B::M5()    #unavailable: calls System.Void B::M4() which requires privileges itself
System.Void B::M6()    #unavailable: calls System.Void B::M1() which calls System.Object Foo::Bar(System.Int32) which requires privileges itself
System.Object Foo::Bar(System.Int32)    #unavailable: method itself requires privileges
System.Void I1::M6()    #unavailable: is the base method of System.Void B::M6() which calls System.Void B::M1() which calls System.Object Foo::Bar(System.Int32) which requires privileges itself
System.Void CriticalType1::.ctor()    #unavailable: lives in a critical type
System.Void CriticalType1::M6()    #unavailable: lives in a critical type
";
			Console.WriteLine(propagationReport.PublicApis);
			StringAssert.AssertLinesAreEquivalent(expectedPublicApis, propagationReport.PublicApis);

		}

		private static MethodPrivilegePropagationStringReport PropagateRequiredPrivileges(ICollection<TypeDefinition> criticalTypes, string[] canBeSscManual, string[] methodsRequiringPrivilegesThemselves, params AssemblyDefinition[] assemblies)
		{
			return new MethodPrivilegePropagation(assemblies, ResolveAllMethodDefinitions(assemblies, methodsRequiringPrivilegesThemselves), ResolveAllMethodDefinitions(assemblies, canBeSscManual), criticalTypes, new List<MethodToMethodCall>())
				.CreateReportBuilder()
				.BuildStringReport();
		}

		static IEnumerable<MethodDefinition> ResolveAllMethodDefinitions(IEnumerable<AssemblyDefinition> assemblies, IEnumerable<string> methodsInAssemblies)
		{
			return assemblies
				.Zip(methodsInAssemblies, (assembly, methods) => new { assembly, methods })
				.SelectMany(row => new CecilDefinitionFinder(row.assembly).FindMethods(ParseSignatureList(row.methods)));
		}
		private static IEnumerable<string> ParseSignatureList(string signatures)
		{
			return signatures.NonEmptyLines().Where(s => !s.StartsWith("#"));
		}

		private static List<string> DetectMethodsRequiringPrivileges(IEnumerable<TypeDefinition> criticalTypes, params AssemblyDefinition[] assemblies)
		{
			MethodPrivilegeDetector.CriticalTypes = criticalTypes.Cast<TypeReference>().ToList();
			return assemblies.Select(assembly => MethodPrivilegeDetector.ReportOfMethodsRequiringPrivilegesThemselves(assembly)).ToList();
		}

		private static AssemblyDefinition CompileTempAssembly(string code, params string[] references)
		{
			return AssemblyFactory.GetAssembly(CSharpCompiler.CompileTempAssembly(code, references));
		}
	}
}
