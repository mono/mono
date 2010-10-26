using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	class MethodPrivilegeDetectorTest
	{
		[Test]
		public void Test()
		{
			var asssembly1src =
				@"
using System;
using System.Runtime.CompilerServices;

public class A
{
	public void Fine() {}
	unsafe public void NotFine(IntPtr* a) {}
}

unsafe public class B
{
	IntPtr* bad;

	public void Fine() {}
	public void AlsoFine(IntPtr* a) {}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void AlsoFine2();
}

public class C
{
	public void NotFine(B b) {}
	public void Fine() {}
}

public class SubB : B
{
}

public class D
{
	SubB b;
}
public class E
{
	public void NotFine(D d) {}
}

";

			var assembly = CSharpCompiler.CompileTempAssembly(asssembly1src);
			
			var ad = AssemblyFactory.GetAssembly(assembly);
			var cdf = new CecilDefinitionFinder(ad);
			MethodPrivilegeDetector.CriticalTypes = new[]
			                                        	{
			                                        		"B",
			                                        		"SubB",
			                                        		"D",
			                                        	}.Select(s => cdf.FindType(s)).Cast<TypeReference>().ToList();
			var results = MethodPrivilegeDetector.MethodsRequiringPrivilegesThemselvesOn(ad).Select(kvp => kvp.Key);

			
			var expectedresults = new[]
			                      	{
			                      		"System.Void A::NotFine(System.IntPtr*)",
			                      		"System.Void C::NotFine(B)",
			                      		"System.Void E::NotFine(D)",
			                      	}.Select(s => cdf.FindMethod(s));
			
			CollectionAssert.AreEquivalent(expectedresults.ToArray(),results.ToArray());
		}
	}
}
