//
// ExpressionTest_Convert.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
//

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_Convert {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullExpression ()
		{
			Expression.Convert (null, typeof (int));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullType ()
		{
			Expression.Convert (1.ToConstant (), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConvertIntToString ()
		{
			Expression.Convert (1.ToConstant (), typeof (string));
		}

		interface IFoo { }
		class Foo : IFoo { }
		class Bar : Foo { }
		class Baz { }

		interface ITzap { }

		[Test]
		public void ConvertBackwardAssignability ()
		{
			var c = Expression.Convert (
				Expression.Constant (null, typeof (Bar)), typeof (Foo));

			Assert.AreEqual ("Convert(null)", c.ToString ());
		}

		[Test]
		public void ConvertInterfaces ()
		{
			var p = Expression.Parameter (typeof (IFoo), null);

			var conv = Expression.Convert (p, typeof (ITzap));
			Assert.AreEqual (typeof (ITzap), conv.Type);
#if !NET_4_0
			Assert.AreEqual ("Convert(<param>)", conv.ToString ());
#endif
			p = Expression.Parameter (typeof (ITzap), null);
			conv = Expression.Convert (p, typeof (IFoo));

			Assert.AreEqual (typeof (IFoo), conv.Type);
#if !NET_4_0
			Assert.AreEqual ("Convert(<param>)", conv.ToString ());
#endif
		}

		[Test]
		public void ConvertCheckedInt32ToInt64 ()
		{
			var c = Expression.ConvertChecked (
				Expression.Constant (2, typeof (int)), typeof (long));

			Assert.AreEqual (ExpressionType.ConvertChecked, c.NodeType);
			Assert.AreEqual ("ConvertChecked(2)", c.ToString ());
		}

		[Test]
		public void ConvertCheckedFallbackToConvertForNonPrimitives ()
		{
			var p = Expression.ConvertChecked (
				Expression.Constant (null, typeof (object)), typeof (IFoo));

			Assert.AreEqual (ExpressionType.Convert, p.NodeType);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConvertBazToFoo ()
		{
			Expression.Convert (Expression.Parameter (typeof (Baz), ""), typeof (Foo));
		}

		struct EineStrukt { }

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConvertStructToFoo ()
		{
			Expression.Convert (Expression.Parameter (typeof (EineStrukt), ""), typeof (Foo));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConvertInt32ToBool ()
		{
			Expression.Convert (Expression.Parameter (typeof (int), ""), typeof (bool));
		}

		[Test]
		public void ConvertIFooToFoo ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (IFoo), ""), typeof (Foo));
			Assert.AreEqual (typeof (Foo), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		[Test]
		public void BoxInt32 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (int), ""), typeof (object));
			Assert.AreEqual (typeof (object), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		[Test]
		public void UnBoxInt32 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (object), ""), typeof (int));
			Assert.AreEqual (typeof (int), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		[Test]
		public void ConvertInt32ToInt64 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (int), ""), typeof (long));
			Assert.AreEqual (typeof (long), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		[Test]
		public void ConvertInt64ToInt32 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (long), ""), typeof (int));
			Assert.AreEqual (typeof (int), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		enum EineEnum {
			EineValue,
		}

		[Test]
		public void ConvertEnumToInt32 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (EineEnum), ""), typeof (int));
			Assert.AreEqual (typeof (int), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		[Test]
		public void ConvertNullableInt32ToInt32 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (int?), ""), typeof (int));
			Assert.AreEqual (typeof (int), c.Type);
			Assert.IsTrue (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}

		[Test]
		public void ConvertInt32ToNullableInt32 ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (int), ""), typeof (int?));
			Assert.AreEqual (typeof (int?), c.Type);
			Assert.IsTrue (c.IsLifted);
			Assert.IsTrue (c.IsLiftedToNull);
			Assert.IsNull (c.Method);
		}


		class Klang {
			int i;

			public Klang (int i)
			{
				this.i = i;
			}

			public static explicit operator int (Klang k)
			{
				return k.i;
			}
		}

		[Test]
		public void ConvertClassWithExplicitOp ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (Klang), ""), typeof (int));
			Assert.AreEqual (typeof (int), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNotNull (c.Method);
		}

		[Test]
		public void CompileConvertClassWithExplicitOp ()
		{
			var p = Expression.Parameter (typeof (Klang), "klang");
			var c = Expression.Lambda<Func<Klang, int>> (
				Expression.Convert (p, typeof (int)), p).Compile ();

			Assert.AreEqual (42, c (new Klang (42)));
		}

		[Test]
		public void ConvertClassWithExplicitOpToNullableInt ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (Klang), ""), typeof (int?));
			Assert.AreEqual (typeof (int?), c.Type);
			Assert.IsTrue (c.IsLifted);
			Assert.IsTrue (c.IsLiftedToNull);
			Assert.IsNotNull (c.Method);
		}

		struct Kling {
			int i;

			public Kling (int i)
			{
				this.i = i;
			}

			public static implicit operator int (Kling k)
			{
				return k.i;
			}
		}

		[Test]
		public void ConvertStructWithImplicitOp ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (Kling), ""), typeof (int));
			Assert.AreEqual (typeof (int), c.Type);
			Assert.IsFalse (c.IsLifted);
			Assert.IsFalse (c.IsLiftedToNull);
			Assert.IsNotNull (c.Method);
		}

		[Test]
		public void CompileConvertStructWithImplicitOp ()
		{
			var p = Expression.Parameter (typeof (Kling), "kling");
			var c = Expression.Lambda<Func<Kling, int>> (
				Expression.Convert (p, typeof (int)), p).Compile ();

			Assert.AreEqual (42, c (new Kling (42)));
		}

		[Test]
		public void ConvertStructWithImplicitOpToNullableInt ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (Kling), ""), typeof (int?));
			Assert.AreEqual (typeof (int?), c.Type);
			Assert.IsTrue (c.IsLifted);
			Assert.IsTrue (c.IsLiftedToNull);
			Assert.IsNotNull (c.Method);
		}

		[Test]
		public void ConvertNullableStructWithImplicitOpToNullableInt ()
		{
			var c = Expression.Convert (Expression.Parameter (typeof (Kling?), ""), typeof (int?));
			Assert.AreEqual (typeof (int?), c.Type);
			Assert.IsTrue (c.IsLifted);
			Assert.IsTrue (c.IsLiftedToNull);
			Assert.IsNotNull (c.Method);
		}

		[Test]
		public void CompiledBoxing ()
		{
			var b = Expression.Lambda<Func<object>> (
				Expression.Convert (42.ToConstant (), typeof (object))).Compile ();

			Assert.AreEqual ((object) 42, b ());
		}

		[Test]
		public void CompiledUnBoxing ()
		{
			var p = Expression.Parameter (typeof (object), "o");

			var u = Expression.Lambda<Func<object, int>> (
				Expression.Convert (p, typeof (int)), p).Compile ();

			Assert.AreEqual (42, u ((object) 42));
		}

		[Test]
		public void CompiledCast ()
		{
			var p = Expression.Parameter (typeof (IFoo), "foo");

			var c = Expression.Lambda<Func<IFoo, Bar>> (
				Expression.Convert (p, typeof (Bar)), p).Compile ();

			IFoo foo = new Bar ();

			Bar b = c (foo);

			Assert.AreEqual (b, foo);
		}

		[Test]
		public void CompileNotNullableToNullable ()
		{
			var p = Expression.Parameter (typeof (int), "i");
			var c = Expression.Lambda<Func<int, int?>> (
				Expression.Convert (p, typeof (int?)), p).Compile ();

			Assert.AreEqual ((int?) 0, c (0));
			Assert.AreEqual ((int?) 42, c (42));
		}

		[Test]
		public void CompileNullableToNotNullable ()
		{
			var p = Expression.Parameter (typeof (int?), "i");
			var c = Expression.Lambda<Func<int?, int>> (
				Expression.Convert (p, typeof (int)), p).Compile ();

			Assert.AreEqual (0, c ((int?) 0));
			Assert.AreEqual (42, c ((int?) 42));

			Action a = () => c (null);

			a.AssertThrows (typeof (InvalidOperationException));
		}

		[Test]
		public void CompiledConvertToSameType ()
		{
			var k = new Klang (42);

			var p = Expression.Parameter (typeof (Klang), "klang");
			var c = Expression.Lambda<Func<Klang, Klang>> (
				Expression.Convert (
					p, typeof (Klang)),
				p).Compile ();

			Assert.AreEqual (k, c (k));
		}

		[Test]
		public void CompiledConvertNullableToNullable ()
		{
			var p = Expression.Parameter (typeof (int?), "i");
			var c = Expression.Lambda<Func<int?, short?>> (
				Expression.Convert (p, typeof (short?)), p).Compile ();

			Assert.AreEqual ((short?) null, c (null));
			Assert.AreEqual ((short?) 12, c (12));
		}

		[Test]
		public void CompiledNullableBoxing ()
		{
			var p = Expression.Parameter (typeof (int?), "i");
			var c = Expression.Lambda<Func<int?, object>> (
				Expression.Convert (p, typeof (object)), p).Compile ();

			Assert.AreEqual (null, c (null));
			Assert.AreEqual ((object) (int?) 42, c (42));
		}

		[Test]
		public void CompiledNullableUnboxing ()
		{
			var p = Expression.Parameter (typeof (object), "o");
			var c = Expression.Lambda<Func<object, int?>> (
				Expression.Convert (p, typeof (int?)), p).Compile ();

			Assert.AreEqual ((int?) null, c (null));
			Assert.AreEqual ((int?) 42, c ((int?) 42));
		}

		[Test]
		public void ChainedNullableConvert ()
		{
			var p = Expression.Parameter (typeof (sbyte?), "a");

			var test = Expression.Lambda<Func<sbyte?, long?>> (
				Expression.Convert (
					Expression.Convert (
						p,
						typeof (int?)),
					typeof (long?)), p).Compile ();

			Assert.AreEqual ((long?) 3, test ((sbyte?) 3));
			Assert.AreEqual (null, test (null));
		}

		struct ImplicitToShort {
			short value;

			public ImplicitToShort (short v)
			{
				value = v;
			}

			public static implicit operator short (ImplicitToShort i)
			{
				return i.value;
			}
		}

		[Test]
		public void ConvertImplicitToShortToNullableInt ()
		{
			var a = Expression.Parameter (typeof (ImplicitToShort?), "a");

			var method = typeof (ImplicitToShort).GetMethod ("op_Implicit");

			var node = Expression.Convert (a, typeof (short), method);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (short), node.Type);
			Assert.AreEqual (method, node.Method);

			var conv = Expression.Lambda<Func<ImplicitToShort?, int?>> (
				Expression.Convert (
					node,
					typeof (int?)), a).Compile ();

			Assert.AreEqual ((int?) 42, conv (new ImplicitToShort (42)));

			Action convnull = () => Assert.AreEqual (null, conv (null));

			convnull.AssertThrows (typeof (InvalidOperationException));
		}

		[Test]
		public void NullableImplicitToShort ()
		{
			var i = Expression.Parameter (typeof (ImplicitToShort?), "i");

			var method = typeof (ImplicitToShort).GetMethod ("op_Implicit");

			var node = Expression.Convert (i, typeof (short?), method);

			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (short?), node.Type);
			Assert.AreEqual (method, node.Method);

			var convert = Expression.Lambda<Func<ImplicitToShort?, short?>> (node, i).Compile ();

			Assert.AreEqual ((short?) 42, convert (new ImplicitToShort (42)));
		}

		[Test]
		public void ConvertLongToDecimal ()
		{
			var p = Expression.Parameter (typeof (long), "l");

			var node = Expression.Convert (p, typeof (decimal));
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (decimal), node.Type);
			Assert.IsNotNull (node.Method);

			var convert = Expression.Lambda<Func<long, decimal>> (node, p).Compile ();

			Assert.AreEqual (42, convert (42));
		}

		[Test]
		public void ConvertNullableULongToNullableDecimal ()
		{
			var p = Expression.Parameter (typeof (ulong?), "l");

			var node = Expression.Convert (p, typeof (decimal?));
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (decimal?), node.Type);
			Assert.IsNotNull (node.Method);

			var convert = Expression.Lambda<Func<ulong?, decimal?>> (node, p).Compile ();

			Assert.AreEqual (42, convert (42));
			Assert.AreEqual (null, convert (null));
		}

		[Test]
		public void ConvertCheckedNullableIntToInt ()
		{
			var p = Expression.Parameter (typeof (int?), "i");

			var node = Expression.ConvertChecked (p, typeof (int));
			Assert.AreEqual (ExpressionType.ConvertChecked, node.NodeType);
			Assert.IsTrue (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);
			Assert.AreEqual (typeof (int), node.Type);
			Assert.IsNull (node.Method);
		}

		struct ImplicitToInt {
			int Value;

			public ImplicitToInt (int v)
			{
				Value = v;
			}

			public static implicit operator int (ImplicitToInt i)
			{
				return i.Value;
			}
		}

		[Test]
		public void ConvertNullableImplictToIntToNullableLong ()
		{
			var i = Expression.Parameter (typeof (ImplicitToInt?), "i");

			var method = typeof (ImplicitToInt).GetMethod ("op_Implicit");

			var node = Expression.Convert (i, typeof (int), method);
			node = Expression.Convert (node, typeof (long?));
			var conv = Expression.Lambda<Func<ImplicitToInt?, long?>> (node, i).Compile ();

			Assert.AreEqual ((long?) 42, conv (new ImplicitToInt (42)));
			Action convnull = () => Assert.AreEqual (null, conv (null));
			convnull.AssertThrows (typeof (InvalidOperationException));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void ConvertNullableIntToStringWithConvertMethod ()
		{
			Expression.Convert (
				Expression.Constant ((int?) 0),
				typeof (string),
				typeof (Convert).GetMethod ("ToString", new [] { typeof (object) }));
		}

		[Test] // #678897
		public void ConvertEnumValueToEnum ()
		{
			var node = Expression.Convert (
				Expression.Constant (EineEnum.EineValue, typeof (EineEnum)),
				typeof (Enum));

			Assert.IsNotNull (node);
			Assert.AreEqual (typeof (Enum), node.Type);
		}
	}
}
