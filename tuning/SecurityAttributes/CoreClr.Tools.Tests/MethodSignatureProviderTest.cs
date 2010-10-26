using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class MethodSignatureProviderTest
	{
		[Test]
		public void GenericMethodSignature()
		{
			var assembly = CecilUtilsForTests.GetExecutingAssemblyDefinition();
			Assert.AreEqual(
				string.Format("System.Void {0}::GenericMethod<T>(T&)", GetType().FullName),
				MethodSignatureProvider.SignatureFor(assembly.MainModule.Types[GetType().FullName].Methods.GetMethod("GenericMethod")[0]));
		}

		[Test]
		public void MethodInGenericType()
		{
			var assembly = CecilUtilsForTests.GetExecutingAssemblyDefinition();
			var cecilTypeName = typeof(GenericType<>).FullName.Replace('+', '/');
			Assert.AreEqual(
				string.Format("System.Void {0}::Foo(T&)", cecilTypeName),
				MethodSignatureProvider.SignatureFor(assembly.MainModule.Types[cecilTypeName].Methods.GetMethod("Foo")[0]));
		}

		class GenericType<T>
		{
			public static void Foo(ref T t)
			{	
			}
		}

		void GenericMethod<T>(ref T t)
		{	
		}
	}
}
