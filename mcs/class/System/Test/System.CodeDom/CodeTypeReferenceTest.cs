//
// CodeTypeReferenceTest.cs - NUnit Test Cases for System.CodeDom.CodeTypeReference
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.CodeDom;
#if NET_2_0
using System.Collections.Generic;
#endif

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
		public void NullBaseType ()
		{
			CodeTypeReference reference = new CodeTypeReference ((string) null);
			Assert.AreEqual (typeof (void).FullName, reference.BaseType);
		}

		[Test]
		public void ZeroLengthBaseType ()
		{
			CodeTypeReference reference = new CodeTypeReference (string.Empty);
			Assert.AreEqual (typeof (void).FullName, reference.BaseType, "#1");
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
			CodeTypeReference reference = new CodeTypeReference ("A[,,]");
			Assert.AreEqual ("A", reference.BaseType, "#1");
			Assert.AreEqual (3, reference.ArrayRank, "#2");
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
		public void BaseTypeTest5 ()
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
		public void BaseTypeTest6 ()
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

		[Test]
		public void BaseTypeTest7 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[,B,,C]");
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
			Assert.AreEqual ("A[,B,,C]", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
#endif
		}

		[Test]
#if NET_2_0
		// CodeTypeReference should parse basetype from right to left in 2.0 
		// profile
		[Category ("NotWorking")]
#endif
		public void BaseTypeTest8 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[,,][,]");
			Assert.AreEqual ("A", reference.BaseType, "#1");
#if NET_2_0
			Assert.AreEqual (3, reference.ArrayRank, "#2");
#else
			Assert.AreEqual (2, reference.ArrayRank, "#2");
#endif
			Assert.IsNotNull (reference.ArrayElementType, "#3");

#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
#endif

			CodeTypeReference arrayElementType = reference.ArrayElementType;
			Assert.AreEqual ("A", arrayElementType.BaseType, "#7");
#if NET_2_0
			Assert.AreEqual (2, arrayElementType.ArrayRank, "#8");
#else
			Assert.AreEqual (3, arrayElementType.ArrayRank, "#8");
#endif
			Assert.IsNotNull (arrayElementType.ArrayElementType, "#9");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, arrayElementType.Options, "#10");
			Assert.IsNotNull (arrayElementType.TypeArguments, "#11");
			Assert.AreEqual (0, arrayElementType.TypeArguments.Count, "#12");
#endif

			arrayElementType = arrayElementType.ArrayElementType;
			Assert.AreEqual ("A", arrayElementType.BaseType, "#13");
			Assert.AreEqual (0, arrayElementType.ArrayRank, "#14");
			Assert.IsNull (arrayElementType.ArrayElementType, "#15");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, arrayElementType.Options, "#16");
			Assert.IsNotNull (arrayElementType.TypeArguments, "#17");
			Assert.AreEqual (0, arrayElementType.TypeArguments.Count, "#18");
#endif
		}

		[Test]
#if NET_2_0
		// CodeTypeReference should parse basetype from right to left in 2.0 
		// profile
		[Category ("NotWorking")]
#endif
		public void BaseTypeTest9 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("A[B,,D][,]");
#if NET_2_0
			Assert.AreEqual ("A`2", reference.BaseType, "#1");
			Assert.AreEqual (2, reference.ArrayRank, "#2");
#else
			Assert.AreEqual ("A[B,,D]", reference.BaseType, "#1");
			Assert.AreEqual (2, reference.ArrayRank, "#2");
#endif
			Assert.IsNotNull (reference.ArrayElementType, "#3");

#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (2, reference.TypeArguments.Count, "#6");
#endif

			CodeTypeReference arrayElementType = reference.ArrayElementType;
#if NET_2_0
			Assert.AreEqual ("A`2", arrayElementType.BaseType, "#7");
			Assert.AreEqual (0, arrayElementType.ArrayRank, "#8");
#else
			Assert.AreEqual ("A[B,,D]", arrayElementType.BaseType, "#7");
			Assert.AreEqual (0, arrayElementType.ArrayRank, "#8");
#endif
			Assert.IsNull (arrayElementType.ArrayElementType, "#9");
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, arrayElementType.Options, "#10");
			Assert.IsNotNull (arrayElementType.TypeArguments, "#11");
			Assert.AreEqual (2, arrayElementType.TypeArguments.Count, "#12");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("B", typeArgument.BaseType, "#13");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#14");
			Assert.IsNull (typeArgument.ArrayElementType, "#15");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#16");
			Assert.IsNotNull (typeArgument.TypeArguments, "#17");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#18");

			typeArgument = reference.TypeArguments[1];
			Assert.AreEqual ("D", typeArgument.BaseType, "#19");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#20");
			Assert.IsNull (typeArgument.ArrayElementType, "#21");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#22");
			Assert.IsNotNull (typeArgument.TypeArguments, "#23");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#24");
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

		[Test]
		public void GenericTypeTest1 () {
			CodeTypeReference reference = new CodeTypeReference (
				typeof (Dictionary<int,string>));
			Assert.AreEqual ("System.Collections.Generic.Dictionary`2", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (0, (int) reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (2, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("System.Int32", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");

			typeArgument = reference.TypeArguments[1];
			Assert.AreEqual ("System.String", typeArgument.BaseType, "#13");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#14");
			Assert.IsNull (typeArgument.ArrayElementType, "#15");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#16");
			Assert.IsNotNull (typeArgument.TypeArguments, "#17");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#18");
		}

		[Test]
		public void GenericTypeTest2 () {
			CodeTypeReference reference = new CodeTypeReference (
				typeof (Dictionary<List<int>, string>));
			Assert.AreEqual ("System.Collections.Generic.Dictionary`2", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (0, (int) reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (2, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("System.Collections.Generic.List`1", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual (0, (int) typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (1, typeArgument.TypeArguments.Count, "#12");

			CodeTypeReference nestedTypeArgument = typeArgument.TypeArguments[0];
			Assert.AreEqual ("System.Int32", nestedTypeArgument.BaseType, "#13");
			Assert.AreEqual (0, nestedTypeArgument.ArrayRank, "#14");
			Assert.IsNull (nestedTypeArgument.ArrayElementType, "#15");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#16");
			Assert.IsNotNull (nestedTypeArgument.TypeArguments, "#17");
			Assert.AreEqual (0, nestedTypeArgument.TypeArguments.Count, "#18");

			typeArgument = reference.TypeArguments[1];
			Assert.AreEqual ("System.String", typeArgument.BaseType, "#19");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#20");
			Assert.IsNull (typeArgument.ArrayElementType, "#21");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#22");
			Assert.IsNotNull (typeArgument.TypeArguments, "#23");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#24");
		}

		[Test]
		public void GenericTypeTest3 () 
		{
			CodeTypeReference reference = new CodeTypeReference (
				"System.Nullable", new CodeTypeReference (typeof (int)));
			Assert.AreEqual ("System.Nullable`1", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (0, (int) reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (1, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("System.Int32", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");
		}

		[Test]
		public void GenericTypeTest4 () 
		{
			CodeTypeReference reference = new CodeTypeReference (
				"System.Nullable`1", new CodeTypeReference (typeof (int)));
			Assert.AreEqual ("System.Nullable`1", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (0, (int) reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (1, reference.TypeArguments.Count, "#6");

			CodeTypeReference typeArgument = reference.TypeArguments[0];
			Assert.AreEqual ("System.Int32", typeArgument.BaseType, "#7");
			Assert.AreEqual (0, typeArgument.ArrayRank, "#8");
			Assert.IsNull (typeArgument.ArrayElementType, "#9");
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, typeArgument.Options, "#10");
			Assert.IsNotNull (typeArgument.TypeArguments, "#11");
			Assert.AreEqual (0, typeArgument.TypeArguments.Count, "#12");
		}

		[Test]
		public void GenericTypeTest5 () 
		{
			CodeTypeReference reference = new CodeTypeReference (
				typeof (int?).GetGenericTypeDefinition ());
			Assert.AreEqual ("System.Nullable`1", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (0, (int) reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
		}

		[Test (Description="Bug #523341")]
		public void GenericTypeTest6 ()
		{
			CodeTypeReference reference = new CodeTypeReference ("System.Collections.List<System.Globalization.CultureInfo[]>");
			Assert.AreEqual ("System.Collections.List<System.Globalization.CultureInfo[]>", reference.BaseType, "#1");
			Assert.AreEqual (0, reference.ArrayRank, "#2");
			Assert.IsNull (reference.ArrayElementType, "#3");
			Assert.AreEqual (0, (int) reference.Options, "#4");
			Assert.IsNotNull (reference.TypeArguments, "#5");
			Assert.AreEqual (0, reference.TypeArguments.Count, "#6");
		}
#endif

		// bug #76535
		[Test]
		public void BaseType_ArrayElementType ()
		{
			CodeTypeReference ctr = new CodeTypeReference ("System.Int32");
			Assert.IsNull (ctr.ArrayElementType, "#1");
			Assert.AreEqual (0, ctr.ArrayRank, "#2");
			ctr.ArrayElementType = new CodeTypeReference ("System.String");
			Assert.IsNotNull (ctr.ArrayElementType, "#3");
			Assert.AreEqual (0, ctr.ArrayRank, "#4");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "#4");
			ctr.ArrayRank = -1;
			Assert.AreEqual (-1, ctr.ArrayRank, "#5");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "#6");
			ctr.ArrayRank = 1;
			Assert.AreEqual (1, ctr.ArrayRank, "#7");
			Assert.AreEqual ("System.String", ctr.BaseType, "#8");
		}

		[Test]
		public void Null_ArrayElementType ()
		{
			CodeTypeReference ctr = new CodeTypeReference ((string) null, 0);
			Assert.IsNotNull (ctr.ArrayElementType, "#1");
			Assert.AreEqual (typeof(void).FullName, ctr.ArrayElementType.BaseType, "#2");
			Assert.AreEqual (0, ctr.ArrayRank, "#3");
			Assert.AreEqual (string.Empty, ctr.BaseType, "#4");
			ctr.ArrayRank = 1;
			Assert.AreEqual (1, ctr.ArrayRank, "#5");
			Assert.AreEqual (typeof (void).FullName, ctr.BaseType, "#6");

			ctr = new CodeTypeReference ((string) null, 1);
			Assert.IsNotNull (ctr.ArrayElementType, "#7");
			Assert.AreEqual (typeof (void).FullName, ctr.ArrayElementType.BaseType, "#8");
			Assert.AreEqual (1, ctr.ArrayRank, "#9");
			Assert.AreEqual (typeof (void).FullName, ctr.BaseType, "#10");

			ctr = new CodeTypeReference ((CodeTypeReference) null, 0);
			Assert.IsNull (ctr.ArrayElementType, "#11");
			Assert.AreEqual (0, ctr.ArrayRank, "#12");
			Assert.AreEqual (string.Empty, ctr.BaseType, "#13");
			ctr.ArrayRank = 1;
			Assert.AreEqual (1, ctr.ArrayRank, "#14");
			Assert.AreEqual (string.Empty, ctr.BaseType, "#15");

			ctr = new CodeTypeReference ((CodeTypeReference) null, 1);
			Assert.IsNull (ctr.ArrayElementType, "#16");
			Assert.AreEqual (1, ctr.ArrayRank, "#17");
			Assert.AreEqual (string.Empty, ctr.BaseType, "#18");
		}

		[Test]
		public void Empty_ArrayElementType ()
		{
			CodeTypeReference ctr = new CodeTypeReference (string.Empty, 0);
			Assert.IsNotNull (ctr.ArrayElementType, "#1");
			Assert.AreEqual (typeof (void).FullName, ctr.ArrayElementType.BaseType, "#2");
			Assert.AreEqual (0, ctr.ArrayRank, "#3");
			Assert.AreEqual (string.Empty, ctr.BaseType, "#4");
			ctr.ArrayRank = 1;
			Assert.AreEqual (1, ctr.ArrayRank, "#5");
			Assert.AreEqual (typeof (void).FullName, ctr.BaseType, "#6");

			ctr = new CodeTypeReference (string.Empty, 1);
			Assert.IsNotNull (ctr.ArrayElementType, "#7");
			Assert.AreEqual (typeof (void).FullName, ctr.ArrayElementType.BaseType, "#8");
			Assert.AreEqual (1, ctr.ArrayRank, "#9");
			Assert.AreEqual (typeof (void).FullName, ctr.BaseType, "#10");
		}
	}
}
