using System;
using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class CecilDefinitionFinderTest
	{
		[Test]
		public void FindMethodForStaticClosure()
		{
			AssertMethodRoundtrip(() => 42);
		}

		[Test]
		public void FindMethodForClosure()
		{
			var i = 42;
			AssertMethodRoundtrip(() => ++i);
			Assert.AreEqual(42, i); // just so the compiler doesn't optimize away anything
		}

		[Test]
		public void FindGenericMethod()
		{
			var assembly = CecilUtilsForTests.GetExecutingAssemblyDefinition();
			var finder = new CecilDefinitionFinder(assembly);
			var found = finder.FindMethod(string.Format("System.Void {0}::GenericMethod<T>(T&)", GetType().FullName));
			Assert.IsNotNull(found);
		}

		static void GenericMethod<T>(ref T t)
		{	
		}

		private void AssertMethodRoundtrip(Func<int> func)
		{
			var method = func.Method;
			var assembly = CecilUtilsForTests.GetExecutingAssemblyDefinition();
			var finder = new CecilDefinitionFinder(assembly);

			// cecil uses / to introduce nested types, the runtime uses +
			var declaringTypeCecilName = method.DeclaringType.CecilTypeName();
			var type = finder.FindType(declaringTypeCecilName);
			Assert.IsNotNull(type, "Type {0} not found!", declaringTypeCecilName);

			var actualDefinition = type.Methods.GetMethod(method.Name)[0];
			var signature = SignatureFor(actualDefinition);
			Assert.AreSame(actualDefinition, finder.FindMethod(signature));
		}

		private string SignatureFor(MethodDefinition actualDefinition)
		{
			return MethodSignatureProvider.SignatureFor(actualDefinition);
		}
	}
}
