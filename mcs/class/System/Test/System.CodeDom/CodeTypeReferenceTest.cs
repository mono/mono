//
// CodeTypeReferenceTest.cs - NUnit Test Cases for System.CodeDom.CodeTypeReference
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
//
using System;
using System.CodeDom;

using NUnit.Framework;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeTypeReferenceTest
	{
		[Test]
		public void EmptyTypeName ()
		{
			CodeTypeReference reference = new CodeTypeReference (string.Empty);
			Assert.AreEqual (typeof (void).FullName, reference.BaseType);
			Assert.AreEqual (0, reference.ArrayRank);
			Assert.IsNull (reference.ArrayElementType);
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
#endif
		}

		[Test]
		public void NullTypeName ()
		{
			CodeTypeReference reference = new CodeTypeReference ((string) null);
			Assert.AreEqual (typeof (void).FullName, reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof(ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void NullType ()
		{
			new CodeTypeReference ((Type) null);
		}

		[Test]
		public void BaseTypeTest1 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[B]");
#if NET_2_0
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (1, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("B", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");
#else
			Assert.AreEqual ("A[B]", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
#endif
		}

		[Test]
		public void BaseTypeTest2 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[]");
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (1, reference.ArrayRank, "#2");
			Assert.IsNotNull (reference.ArrayElementType, "#3");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
#endif

			CodeTypeReference arrayElementType = reference.ArrayElementType;
			Assert.AreEqual ("A", arrayElementType.BaseType, "#7");
			Assert.AreEqual (0, arrayElementType.ArrayRank, "#8");
			Assert.IsNull (arrayElementType.ArrayElementType, "#9");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, arrayElementType.Options, "#10");
			Assert.IsNotNull (arrayElementType.TypeArguments, "#11");
			Assert.AreEqual (0, arrayElementType.TypeArguments.Count, "#12");
#endif
		}

		[Test]
		public void BaseTypeTest3 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[,]");
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (2, reference.ArrayRank, "#2");
			Assert.IsNotNull (reference.ArrayElementType, "#3");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
#endif

			CodeTypeReference arrayElementType = reference.ArrayElementType;
			Assert.AreEqual ("A", arrayElementType.BaseType, "#7");
			Assert.AreEqual (0, arrayElementType.ArrayRank, "#8");
			Assert.IsNull (arrayElementType.ArrayElementType, "#9");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, arrayElementType.Options, "#10");
			Assert.IsNotNull (arrayElementType.TypeArguments, "#11");
			Assert.AreEqual (0, arrayElementType.TypeArguments.Count, "#12");
#endif
		}

		[Test]
		public void BaseTypeTest4 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[B,C]");
#if NET_2_0
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (2, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("B", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");

			typeArgument = reference.TypeArguments[1];
			Assert.AreEqual ("C", typeArgument.BaseType, "#13");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#14");
			Assert.IsNull (typeArgument.ArrayElementType, "#15");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#16");
			Assert.IsNotNull (typeArgument.TypeArguments, "#17");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#18");
#else
			Assert.AreEqual ("A[B,C]", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
#endif
		}

		[Test]
		public void BaseTypeTest5 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[");
			Assert.AreEqual ("A[", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
#endif
		}

#if NET_2_0
		[Test]
		public void Defaults ()
		{
			CodeTypeReference reference = new CodeTypeReference ();

			Assert.IsNull (reference.ArrayElementType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.AreEqual (string.Empty, reference.BaseType, "#3");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
		}

		[Test]
		public void CodeTypeParameter1 ()
		{
			CodeTypeReference reference = new CodeTypeReference(
				new CodeTypeParameter ("A"));
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (CodeTypeReferenceOptions.GenericTypeParameter, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
		}
	
		[Test]
		public void CodeTypeParameter2 ()
		{
			CodeTypeReference reference = new CodeTypeReference (
				new CodeTypeParameter ("A[B]"));
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (CodeTypeReferenceOptions.GenericTypeParameter, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (1, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("B", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");
		}

		[Test]
		public void CodeTypeParameter3 ()
		{
			CodeTypeReference reference = new CodeTypeReference (
				new CodeTypeParameter ("A[B, C]"));
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (CodeTypeReferenceOptions.GenericTypeParameter, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (2, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("B", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");

			typeArgument = reference.TypeArguments[1];
			Assert.AreEqual (" C", typeArgument.BaseType, "#13");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#14");
			Assert.IsNull (typeArgument.ArrayElementType, "#15");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#16");
			Assert.IsNotNull (typeArgument.TypeArguments, "#17");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#18");
		}

		[Test]
		public void CodeTypeParameter4 ()
		{
			CodeTypeReference reference = new CodeTypeReference (
				new CodeTypeParameter ("A[]"));
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (1, reference.ArrayRank, "#2");
			Assert.IsNotNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (CodeTypeReferenceOptions.GenericTypeParameter, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");

			CodeTypeReference arrayElementType = reference.ArrayElementType;
			Assert.AreEqual ("A", arrayElementType.BaseType, "#7");
			Assert.AreEqual (0, arrayElementType.ArrayRank, "#8");
			Assert.IsNull (arrayElementType.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, arrayElementType.Options, "#10");
			Assert.IsNotNull (arrayElementType.TypeArguments, "#11");
			Assert.AreEqual (0, arrayElementType.TypeArguments.Count, "#12");
		}

		[Test]
		public void CodeTypeParameter5 ()
		{
			CodeTypeReference reference = new CodeTypeReference (
				new CodeTypeParameter ("A[,"));
			Assert.AreEqual ("A[,", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (CodeTypeReferenceOptions.GenericTypeParameter, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
		}
#endif
	}
}
