using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>Namespace Test: [<see cref="N:Mono.DocTest" />]</summary>
/// <remarks><c>T:NoNamespace</c></remarks>
public class NoNamespace {}

namespace System {
	/* 
	 * The System namespace gets special treatment, e.g. instead of
	 * System.Environment C# declarations it's just Environment.
	 */
	/// <remarks><c>T:System.Action`1</c></remarks>
	public delegate void Action<T> (T obj);

	/// <remarks><c>T:System.Environment</c></remarks>
	public static class Environment {
		/// <remarks><c>T:System.Environment+SpecialFolder</c></remarks>
		public enum SpecialFolder {}

		/// <param name="folder">
		///   A <see cref="T:System.Environment+SpecialFolder" /> instance.
		/// </param>
		/// <remarks>
		///   <c>M:System.Environment.GetFolderPath(System.Environment+SpecialFolder)</c>
		/// </remarks>
		public static string GetFolderPath (SpecialFolder folder)
		{
			throw new NotSupportedException ();
		}
	}

	// to test ECMA doc importing...
	public class Array {
		// the ECMA docs have a different return type than .NET -- skip.
		public static System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly<T> (T[] array)
		{
			throw new NotImplementedException ();
		}

		// ECMA docs use <T,U> instead of <TInput,TOutput> --> map them.
		public static TOutput[] ConvertAll<TInput, TOutput> (TInput[] array, Converter<TInput, TOutput> converter)
		{
			throw new InvalidOperationException ();
		}

		// ECMA docs *incorrectly* document parameter -- skip
		public static void Resize<T> (ref T[] array, int newSize)
		{
			throw new Exception ();
		}
	}

	// to test ECMA doc importing...
	public delegate void AsyncCallback (IAsyncResult ar);
}

namespace Mono.DocTest {
	internal class Internal {
		public class ShouldNotBeDocumented {
		}
	}

	internal class MonoTODOAttribute : Attribute {
	}

	/// <remarks>
	///  <para>
	///   cref=<c>T:Mono.DocTest.DocAttribute</c>.
	///  </para>
	///  <format type="text/html">
	///   <table width="100%">
	///     <tr>
	///       <td style="color:red">red</td>
	///       <td style="color:blue">blue</td>
	///       <td style="color:green">green</td>
	///     </tr>
	///   </table>
	///  </format>
	///  <code lang="C#" src="../DocTest.cs#DocAttribute Example" />
	/// </remarks>
	[AttributeUsage (AttributeTargets.All)]
	public class DocAttribute : Attribute {
		#region DocAttribute Example
		[Doc ("documented class")]
		class Example {
			[Doc ("documented field")] public string field;
		}
		#endregion
		/// <remarks><c>C:Mono.DocTest.DocAttribute(System.String)</c></remarks>
		public DocAttribute (string docs)
		{
			if (docs == null)
				throw new ArgumentNullException ("docs");
		}

		/// <remarks><c>P:Mono.DocTest.DocAttribute.Property</c></remarks>
		public Type Property { get; set; }

		/// <remarks><c>F:Mono.DocTest.DocAttribute.Field</c></remarks>
		public bool Field;

		/// <remarks><c>F:Mono.DocTest.DocAttribute.FlagsEnum</c></remarks>
		public ConsoleModifiers FlagsEnum;

		/// <remarks><c>F:Mono.DocTest.DocAttribute.NonFlagsEnum</c></remarks>
		public Color NonFlagsEnum;
	}

	/// <summary>Possible colors</summary>
	/// <remarks>
	///   <see cref="T:Mono.DocTest.Color"/>.
	///   Namespace Test: [<see cref="N:Mono.DocTest" />]
	/// </remarks>
	[MonoTODO]
	public enum Color {
		/// <summary>Insert Red summary here</summary>
		/// <remarks><c>F:Mono.DocTest.Color.Red</c>.</remarks>
		Red, 
		/// <summary>Insert Blue summary here</summary>
		/// <remarks><c>F:Mono.DocTest.Color.Blue</c>.</remarks>
		Blue, 
		/// <summary>Insert Green summary here</summary>
		/// <remarks><c>F:Mono.DocTest.Color.Green</c>.</remarks>
		Green,

		AnotherGreen = Green,
	}

	/// <summary>Process interface</summary>
	/// <remarks><c>T:Mono.DocTest.IProcess</c>.</remarks>
	public interface IProcess {}

	/// <summary>Process interface</summary>
	/// <remarks><c>T:Mono.DocTest.DocValueType</c>.</remarks>
	public struct DocValueType : IProcess {
		/// <remarks><c>F:Mono.DocTest.DocValueType.total</c>.</remarks>
		public int total;

		/// <param name="i">A <see cref="T:System.Int32" />.</param>
		/// <remarks><see cref="M:Mono.DocTest.DocValueType.M(System.Int32)"/>.</remarks>
		public void M (int i)
		{
			if ((new Random().Next() % 2) == 0)
				throw new SystemException ();
			throw new ApplicationException ();
		}
	}

	/// <remarks><c>T:Mono.DocTest.Widget</c>.</remarks>
	/// <seealso cref="P:Mono.DocTest.Widget.Item(System.Int32)" />
	/// <extra>Some extra tag value</extra>
	public unsafe class Widget : IProcess {
		/// <remarks><c>T:Mono.DocTest.Widget.NestedClass</c>.</remarks>
		public class NestedClass {
			/// <remarks><c>F:Mono.DocTest.Widget.NestedClass.value</c>.</remarks>
			public int value;

			/// <param name="i">Some <see cref="T:System.Int32" />.</param>
			/// <remarks><c>M:Mono.DocTest.Widget.NestedClass.M(System.Int32)</c>.</remarks>
			public void M (int i) {}

			/// <remarks><c>T:Mono.DocTest.Widget.NestedClass.Double</c>.</remarks>
			public class Double {
				/// <remarks><c>T:Mono.DocTest.Widget.NestedClass.Double.Triple</c>.</remarks>
				public class Triple {
					/// <remarks><c>T:Mono.DocTest.Widget.NestedClass.Double.Triple.Quadruple</c>.</remarks>
					public class Quadruple {} // for good measure
				}
			}
		}

		/// <remarks><c>T:Mono.DocTest.Widget.NestedClass`1</c>.</remarks>
		public class NestedClass<T> {
			/// <remarks><c>F:Mono.DocTest.Widget.NestedClass`1.value</c>.</remarks>
			public int value;

			/// <param name="i">Another <see cref="T:System.Int32" />.</param>
			/// <remarks><c>M:Mono.DocTest.Widget.NestedClass`1.M(System.Int32)</c>.</remarks>
			public void M (int i) {}
		}

		/// <remarks><c>F:Mono.DocTest.Widget.classCtorError</c>.</remarks>
		public static readonly string[] classCtorError = CreateArray ();

		private static string[] CreateArray ()
		{
			throw new NotSupportedException ();
		}

		/// <remarks><c>F:Mono.DocTest.Widget.message</c>.</remarks>
		public string message;

		/// <remarks><c>F:Mono.DocTest.Widget.defaultColor</c>.</remarks>
		protected static Color defaultColor;

		/// <remarks><c>F:Mono.DocTest.Widget.PI</c>.</remarks>
		protected internal const double PI = 3.14159;

		/// <remarks><c>F:Mono.DocTest.Widget.monthlyAverage</c>.</remarks>
		internal protected readonly double monthlyAverage;

		/// <remarks><c>F:Mono.DocTest.Widget.array1</c>.</remarks>
		public long[] array1;

		/// <remarks><c>F:Mono.DocTest.Widget.array2</c>.</remarks>
		public Widget[,] array2;

		/// <remarks><c>F:Mono.DocTest.Widget.pCount</c>.</remarks>
		public unsafe int *pCount;

		/// <remarks><c>F:Mono.DocTest.Widget.ppValues</c>.</remarks>
		public unsafe float **ppValues;

		/// <remarks><c>T:Mono.DocTest.Widget.IMenuItem</c>.</remarks>
		public interface IMenuItem {
			/// <remarks><c>M:Mono.DocTest.Widget.IMenuItem.A</c>.</remarks>
			void A ();

			/// <remarks><c>P:Mono.DocTest.Widget.IMenuItem.P</c>.</remarks>
			int B {get; set;}
		}

		/// <remarks><c>T:Mono.DocTest.Widget.Del</c>.</remarks>
		public delegate void Del (int i);

		/// <remarks><c>T:Mono.DocTest.Widget.Direction</c>.</remarks>
		[Flags]
		public enum Direction {
			/// <remarks><c>T:Mono.DocTest.Widget.Direction.North</c>.</remarks>
			North,
			/// <remarks><c>T:Mono.DocTest.Widget.Direction.South</c>.</remarks>
			South,
			/// <remarks><c>T:Mono.DocTest.Widget.Direction.East</c>.</remarks>
			East,
			/// <remarks><c>T:Mono.DocTest.Widget.Direction.West</c>.</remarks>
			West,
		}

		/// <remarks>
		///  <para><c>C:Mono.DocTest.Widget</c>.</para>
		///  <para><c>M:Mono.DocTest.Widget.#ctor</c>.</para>
		///  <para><see cref="C:Mono.DocTest.Widget(System.String)" /></para>
		///  <para><see cref="C:Mono.DocTest.Widget(System.Converter{System.String,System.String})" /></para>
		/// </remarks>
		public Widget () {}

		/// <param name="s">A <see cref="T:System.String" />.</param>
		/// <remarks>
		///  <para><c>C:Mono.DocTest.Widget(System.String)</c>.</para>
		///  <para><c>M:Mono.DocTest.Widget.#ctor(System.String)</c>.</para>
		/// </remarks>
		public Widget (string s) {}

		/// <param name="c">A <see cref="T:System.Converter{System.String,System.String}" />.</param>
		/// <remarks>
		///  <para><c>C:Mono.DocTest.Widget(System.Converter{System.String,System.String})</c>.</para>
		/// </remarks>
		public Widget (Converter<string,string> c) {}

		/// <remarks><c>M:Mono.DocTest.Widget.M0</c>.</remarks>
		public static void M0 () {}

		/// <param name="c">A <see cref="T:System.Char" />.</param>
		/// <param name="f">A <see cref="T:System.Single" />.</param>
		/// <param name="v">A <see cref="T:Mono.DocTest.DocValueType" />.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.M1(System.Char,System.Signle@,Mono.DocTest.DocValueType@)</c>.</remarks>
		[return:Doc ("return:DocAttribute", Property=typeof(Widget))]
		[Doc("normal DocAttribute", Field=true)]
		public void M1 ([Doc ("c", FlagsEnum=ConsoleModifiers.Alt | ConsoleModifiers.Control)] char c, 
				[Doc ("f", NonFlagsEnum=Color.Red)] out float f, 
				[Doc ("v")] ref DocValueType v) {f=0;}

		/// <param name="x1">A <see cref="T:System.Int16" /> array.</param>
		/// <param name="x2">A <see cref="T:System.Int32" /> array.</param>
		/// <param name="x3">A <see cref="T:System.Int64" /> array.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.M2(System.Int16[],System.Int32[0:,0:],System.Int64[][])</c>.</remarks>
		public void M2 (short[] x1, int[,] x2, long[][] x3) {}

		/// <param name="x3">Another <see cref="T:System.Int64" /> array.</param>
		/// <param name="x4">A <see cref="T:Mono.DocTest.Widget" /> array.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.M3(System.Int64[][],Mono.DocTest.Widget[0:,0:,0:][])</c>.</remarks>
		protected void M3 (long[][] x3, Widget[][,,] x4) {}

		/// <param name="pc">A <see cref="T:System.Char" /> pointer.</param>
		/// <param name="ppf">A <see cref="T:Mono.DocTest.Color" /> pointer.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.M4(System.Char*,Mono.DocTest.Color**)</c>.</remarks>
		protected unsafe void M4 (char *pc, Color **ppf) {}

		/// <param name="pv">A <see cref="T:System.Void" /> pointer.</param>
		/// <param name="pd">A <see cref="T:System.Double" /> array.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.M5(System.Void*,System.Double*[0:,0:][])</c>.</remarks>
		protected unsafe void M5 (void *pv, double *[][,] pd) {}

		/// <param name="i">Yet another <see cref="T:System.Int32" />.</param>
		/// <param name="args">An <see cref="T:System.Object" /> array.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.M6(System.Int32,System.Object[])</c>.</remarks>
		protected void M6 (int i, params object[] args) {}

		/// <remarks><c>M:Mono.DocTest.Widget.M7(Mono.DocTest.Widget.NestedClass.Double.Triple.Quadruple)</c>.</remarks>
		public void M7(Widget.NestedClass.Double.Triple.Quadruple a) {}

		/// <value>A <see cref="T:System.Int32" /> value...</value>
		/// <remarks><c>P:Mono.DocTest.Widget.Width</c>.</remarks>
		[Doc ("Width property")]
		public int Width {
			[Doc ("Width get accessor")]
			get {return 0;}
			[Doc ("Width set accessor")]
			protected set {}
		}

		/// <value>A <see cref="T:System.Int64" /> value...</value>
		/// <remarks><c>P:Mono.DocTest.Widget.Height</c>.</remarks>
		[Doc ("Height property")]
		protected long Height {get {return 0;}}

		/// <value>A <see cref="T:System.Int16" /> value...</value>
		/// <remarks><c>P:Mono.DocTest.Widget.X</c>.</remarks>
		protected internal short X {set {}}

		/// <value>A <see cref="T:System.Double" /> value...</value>
		/// <remarks><c>P:Mono.DocTest.Widget.Y</c>.</remarks>
		internal protected double Y {get {return 0;} set {}}


		/// <param name="i">TODO</param>
		/// <remarks><c>P:Mono.DocTest.Widget.Item(System.Int32)</c>.</remarks>
		/// <value>A <see cref="T:System.Int32" /> instance.</value>
		[Doc ("Item property")]
		public int this [int i] {
			get {return 0;}
			[Doc ("Item property set accessor")]
			set {}
		}

		/// <param name="s">Some <see cref="T:System.String" />.</param>
		/// <param name="i">I love <see cref="T:System.Int32" />s.</param>
		/// <remarks><c>P:Mono.DocTest.Widget.Item(System.String,System.Int32)</c>.</remarks>
		/// <value>A <see cref="T:System.Int32" /> instance.</value>
		public int this [string s, int i] {get {return 0;} set {}}

		/// <remarks><c>E:Mono.DocTest.Widget.AnEvent</c>.</remarks>
		[Doc ("Del event")]
		public event Del AnEvent {
			[Doc ("Del add accessor")]
			add {}
			[Doc ("Del remove accessor")]
			remove {}
		}

		/// <remarks><c>E:Mono.DocTest.Widget.AnotherEvent</c>.</remarks>
		protected event Del AnotherEvent;

		/// <param name="x">Another <see cref="T:Mono.DocTest.Widget" />.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.op_UnaryPlus(Mono.DocTest.Widget)</c>.</remarks>
		/// <returns>A <see cref="T:Mono.DocTest.Widget" /> instance.</returns>
		public static Widget operator+ (Widget x) {return null;}

		/// <param name="x1">Yet Another <see cref="T:Mono.DocTest.Widget" />.</param>
		/// <param name="x2">Yay, <see cref="T:Mono.DocTest.Widget" />s.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.op_Addition(Mono.DocTest.Widget,Mono.DocTest.Widget)</c>.</remarks>
		/// <returns>A <see cref="T:Mono.DocTest.Widget" /> instance (2).</returns>
		public static Widget operator+ (Widget x1, Widget x2) {return null;}

		/// <param name="x"><see cref="T:Mono.DocTest.Widget" />s are fun!.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.op_Explicit(Mono.DocTest.Widget)~System.Int32</c>.</remarks>
		/// <returns>A <see cref="T:System.Int32" /> instance.</returns>
		public static explicit operator int (Widget x) {return 0;}

		/// <param name="x"><c>foo</c>; <see cref="T:Mono.DocTest.Widget" />.</param>
		/// <remarks><c>M:Mono.DocTest.Widget.op_Implicit(Mono.DocTest.Widget)~System.Int64</c>.</remarks>
		/// <returns>A <see cref="T:System.Int64" /> instance.</returns>
		public static implicit operator long (Widget x) {return 0;}
	}

	/// <remarks><c>T:Mono.DocTest.UseLists</c>.</remarks>
	public class UseLists
	{
		/// <param name="list">A <see cref="T:Mono.DocTest.Generic.MyList{System.Int32}" />.</param>
		/// <remarks><c>M:Mono.DocTest.UseLists.Process(Mono.DocTest.MyList{System.Int32})</c>.</remarks>
		public void Process (Generic.MyList<int> list) {}

		/// <param name="value">A <c>T</c>.</param>
		/// <typeparam name="T">Something</typeparam>
		/// <remarks><c>M:Mono.DocTest.UseLists.GetValues``1(``0)</c>.</remarks>
		/// <returns>A <see cref="T:Mono.DocTest.Generic.MyList`1" /> instance.</returns>
		public Generic.MyList<T> GetValues <T> (T value) where T : struct {return null;}

		/// <param name="list">Another <see cref="T:Mono.DocTest.Generic.MyList{System.Int32}" />.</param>
		/// <remarks>
		///  <para><c>M:Mono.DocTest.UseLists.Process(System.Collections.Generic.List{System.Int32})</c>.</para>
		/// <para><see cref="M:System.Collections.Generic.List{System.Int32}.Remove(`0)" /></para>
		/// </remarks>
		/// <exception invalid="foo">text!</exception>
		public void Process (List<int> list)
		{
			// Bug: only creation is looked for, so this generates an <exception/>
			// node:
			new Exception ();

			// Bug? We only look at "static" types, so we can't follow
			// delegates/interface calls:
			Func<int, int> a = x => {throw new InvalidOperationException ();};
			a (1);

			// Multi-dimensional arrays have "phantom" methods that Cecil can't
			// resolve, as they're provided by the runtime.  These should be
			// ignored.
			int[,] array = new int[1,1];
			array[0,0] = 42;
		}

		/// <param name="list">A <see cref="T:Mono.DocTest.Generic.MyList{System.Predicate{System.Int32}}" />.</param>
		/// <remarks><c>M:Mono.DocTest.UseLists.Process(System.Collections.Generic.List{System.Predicate{System.Int32}})</c>.</remarks>
		public void Process (List<Predicate<int>> list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			Process<int> (list);
		}

		/// <param name="list">A <see cref="T:Mono.DocTest.Generic.MyList{System.Predicate{``0}}" />.</param>
		/// <typeparam name="T">Something Else</typeparam>
		/// <remarks><c>M:Mono.DocTest.UseLists.Process``1(System.Collections.Generic.List{System.Predicate{``0}})</c>.</remarks>
		public void Process<T> (List<Predicate<T>> list)
		{
			if (list.Any (p => p == null))
				throw new ArgumentException ("predicate null");
		}

		/// <param name="helper">A <see cref="T:Mono.DocTest.Generic.MyList{``0}.Helper{``1,``2}" />.</param>
		/// <typeparam name="T"><c>T</c></typeparam>
		/// <typeparam name="U"><c>U</c></typeparam>
		/// <typeparam name="V"><c>V</c></typeparam>
		/// <remarks><c>M:Mono.DocTest.UseLists.UseHelper``3(Mono.DocTest.Generic.MyList{``0}.Helper{``1,``2})</c>.</remarks>
		public void UseHelper<T,U,V> (Generic.MyList<T>.Helper<U,V> helper) {}
	}
}

namespace Mono.DocTest.Generic {
	// Need to place this into a separate namespace to work around gmcs bug
	// where XML docs for type *following* this one aren't extracted.

	/// <typeparam name="TArg">argument type, with attributes!</typeparam>
	/// <typeparam name="TRet">return type, with attributes!</typeparam>
	/// <remarks><c>T:Mono.DocTest.Generic.Func`2</c>.</remarks>
	[Doc ("method")]
	[return:Doc ("return", Field=false)]
	public delegate TRet Func<[Doc ("arg!")] in TArg, [Doc ("ret!")] out TRet> (
			[Doc ("arg-actual")] TArg a
	) where TArg : Exception;
}

namespace Mono.DocTest.Generic {
	using Mono.DocTest;

	/// <summary>extension methods!</summary>
	/// <remarks><c>T:Mono.DocTest.Generic.Extensions</c></remarks>
	public static class Extensions {
		/// <summary><c>System.Object</c> extension method</summary>
		/// <remarks><c>M:Mono.DocTest.Generic.Extensions.ToEnumerable``1</c></remarks>
		public static IEnumerable<T> ToEnumerable<T> (this T self)
		{
			yield return self;
		}

		/// <summary><see cref="T:System.Collections.Generic.IEnumerable`1" /> extension method</summary>
		/// <remarks><c>M:Mono.DocTest.Generic.Extensions.ForEach``1</c></remarks>
		public static void ForEach<T> (this IEnumerable<T> self, Action<T> a)
		{
		}

		/// <summary><see cref="T:Mono.DocTest.Generic.IFoo`1" /> extension method</summary>
		/// <remarks><c>M:Mono.DocTest.Generic.Extensions.Bar``1</c></remarks>
		public static void Bar <T> (this IFoo<T> self, string s)
		{
		}

		/// <summary>
		///   <see cref="T:System.Collections.Generic.IEnumerable{System.Int32}" /> 
		///   extension method.
		/// </summary>
		/// <remarks><c>M:Mono.DocTest.Generic.Extensions.ToDouble</c></remarks>
		public static IEnumerable<double> ToDouble (this IEnumerable<int> list)
		{
			return null;
		}

		/// <summary>
		///   <see cref="T:Mono.DocTest.Generic.IFoo`1" /> extension method.
		/// </summary>
		/// <remarks><c>M:Mono.DocTest.Generic.Extensions.ToDouble</c></remarks>
		public static double ToDouble<T> (this T val) where T : IFoo<T>
		{
			// the target type is T:...IFoo<T>, NOT T:System.Object.
			return 0.0;
		}
	}

	/// <typeparam name="U">Insert <c>text</c> here.</typeparam>
	/// <remarks><c>T:Mono.DocTest.Generic.GenericBase`1</c>.</remarks>
	public class GenericBase<U> {
	
		/// <param name="genericParameter">Something</param>
		/// <typeparam name="S">Insert more <c>text</c> here.</typeparam>
		/// <remarks><c>M:Mono.DocTest.GenericBase`1.BaseMethod``1(``0)</c>.</remarks>
		/// <returns>The default value.</returns>
		public U BaseMethod<[Doc ("S")] S> (S genericParameter) {
			return default(U);
		}

		/// <remarks><c>F:Mono.DocTest.GenericBase`1.StaticField1</c></remarks>
		public static readonly GenericBase<U> StaticField1 = new GenericBase<U> ();

		/// <remarks><c>F:Mono.DocTest.GenericBase`1.ConstField1</c></remarks>
		public const int ConstField1 = 1;

		/// <param name="list">Insert description here</param>
		/// <remarks><c>M:Mono.DocTest.GenericBase`1.op_Explicit(Mono.DocTest.GenericBase{`0})~`0</c></remarks>
		/// <returns>The default value for <typeparamref name="U"/>.</returns>
		public static explicit operator U (GenericBase<U> list) {return default(U);}

		/// <remarks>T:Mono.DocTest.Generic.GenericBase`1.FooEventArgs</remarks>
		public class FooEventArgs : EventArgs {
		}

		/// <remarks>E:Mono.DocTest.Generic.GenericBase`1.MyEvent</remarks>
		public event EventHandler<FooEventArgs> MyEvent;

		/// <remarks>E:Mono.DocTest.Generic.GenericBase`1.ItemChanged</remarks>
		public event Action<MyList<U>, MyList<U>.Helper<U, U>> ItemChanged;

		/// <remarks>T:Mono.DocTest.Generic.GenericBase`1.NestedCollection</remarks>
		public class NestedCollection {
			/// <remarks>T:Mono.DocTest.Generic.GenericBase`1.NestedCollection.Enumerator</remarks>
			public struct Enumerator {
			}
		}
	}
	
	/// <typeparam name="T">I'm Dying Here!</typeparam>
	/// <remarks><c>T:Mono.DocTest.Generic.MyList`1</c>.</remarks>
	public class MyList<[Mono.DocTest.Doc("Type Parameter!")] T> : GenericBase <T>, IEnumerable<int[]>
	{
		/// <typeparam name="U">Seriously!</typeparam>
		/// <typeparam name="V">Too <c>many</c> docs!</typeparam>
		/// <remarks><c>T:Mono.DocTest.MyList`1.Helper`2</c>.</remarks>
		public class Helper <U, V> {
			/// <param name="a">Ako</param>
			/// <param name="b">bko</param>
			/// <param name="c">cko</param>
			/// <remarks><c>M:Mono.DocTest.MyList`1.Helper`2.UseT(`0,`1,`2)</c>.</remarks>
			public void UseT(T a, U b, V c) { }
		}

		/// <param name="t">tko</param>
		/// <remarks><c>M:Mono.DocTest.MyList`1.Test(`0)</c>.</remarks>
		public void Test (T t) {}

		/// <param name="t">Class generic type</param>
		/// <param name="u">Method generic type</param>
		/// <typeparam name="U">Method generic parameter</typeparam>
		/// <remarks><c>M:Mono.DocTest.MyList`1.Method``1(`0,``0)</c>.</remarks>
		public void Method <U> (T t, U u) {}

		// mcs "crashes" (CS1569) on this method; exclude it for now.
		// <remarks><c>M:Mono.DocTest.MyList`1.RefMethod``1(`0@,``0@)</c>.</remarks>
		public void RefMethod<U> (ref T t, ref U u) {}

		/// <param name="helper">A <see cref="T:Mono.DocTest.Generic.MyList`1.Helper`2" />.</param>
		/// <typeparam name="U">Argh!</typeparam>
		/// <typeparam name="V">Foo Argh!</typeparam>
		/// <remarks><c>M:Mono.DocTest.Generic.MyList`1.UseHelper``2(Mono.DocTest.Generic.MyList{``0}.Helper{``1,``2})</c>.</remarks>
		public void UseHelper<U,V> (Helper<U,V> helper) {}

		/// <remarks><c>M:Mono.DocTest.Generic.MyList`1.GetHelper``2</c>.</remarks>
		/// <returns><see langword="null" />.</returns>
		public Helper<U,V> GetHelper<U,V> () {return null;}

		/// <remarks><c>M:Mono.DocTest.MyList`1.System#Collections#GetEnumerator</c>.</remarks>
		IEnumerator IEnumerable.GetEnumerator () {return null;}

		/// <remarks><c>M:Mono.DocTest.MyList`1.GetEnumerator</c>.</remarks>
		public IEnumerator<int[]> GetEnumerator () {return null;}
	}

	/// <typeparam name="T">T generic param</typeparam>
	/// <remarks><c>T:Mono.DocTest.IFoo`1</c>.</remarks>
	public interface IFoo<T> {
		/// <typeparam name="U">U generic param</typeparam>
		/// <remarks><c>T:Mono.DocTest.IFoo`1.Method``1(`0,``0)</c>.</remarks>
		T Method <U> (T t, U u);
	}

	/// <typeparam name="A">Ako generic param</typeparam>
	/// <typeparam name="B">Bko generic param</typeparam>
	/// <remarks><c>T:Mono.DocTest.MyList`2</c>.</remarks>
	public class MyList<A,B> : GenericBase<Dictionary<A,B>>, IEnumerable<A>, 
				 IEnumerator<A>, ICollection<A>, IFoo<A>
		where A : class, IList<B>, new()
		where B : class, A
	{
		// IEnumerator

		// shown?
		object IEnumerator.Current {get {return null;}}

		/// <remarks><c>M:Mono.DocTest.MyList`2.MoveNext</c>.</remarks>
		/// <returns><see cref="T:System.Boolean" /></returns>
		public bool MoveNext () {return false;}

		/// <remarks><c>M:Mono.DocTest.MyList`2.Reset</c>.</remarks>
		public void Reset () {}

		// IDisposable
		/// <remarks><c>M:Mono.DocTest.MyList`2.Dispose</c>.</remarks>
		public void Dispose () {}

		// IEnumerator<T>
		/// <remarks><c>P:Mono.DocTest.MyList`2.Current</c>.</remarks>
		/// <value>The current value.</value>
		public A Current {get {return default(A);}}
		/// <remarks><c>P:Mono.DocTest.MyList`2.Current</c>.</remarks>
		/// <value>The current value.</value>
		A IEnumerator<A>.Current {get {return default(A);}}

		// IEnumerable
		/// <remarks><c>M:Mono.DocTest.MyList`2.System#Collections#GetEnumerator</c>.</remarks>
		IEnumerator IEnumerable.GetEnumerator () {return this;}

		// IEnumerable<T>
		/// <remarks><c>M:Mono.DocTest.MyList`2.System#Collections#Generic#IEnumerable{A}#GetEnumerator</c>.</remarks>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator{`0}" />.</returns>
		IEnumerator<A> IEnumerable<A>.GetEnumerator () {return this;}
		/// <remarks><c>M:Mono.DocTest.MyList`2.GetEnumerator</c>.</remarks>
		/// <returns>A <see cref="T:System.Collections.Generic.List{`0}.Enumerator" />.</returns>
		public List<A>.Enumerator GetEnumerator () {return new List<A>.Enumerator ();}

		// ICollection<T>
		/// <remarks><c>P:Mono.DocTest.MyList`2.Count</c>.</remarks>
		/// <value>A <see cref="T:System.Int32" />.</value>
		public int Count {get {return 0;}}
		/// <remarks><c>P:Mono.DocTest.MyList`2.System#Collections#Generic#ICollection{A}#IsReadOnly</c>.</remarks>
		/// <value>A <see cref="T:System.Boolean" />.</value>
		bool ICollection<A>.IsReadOnly {get {return false;}}
		/// <param name="item">The item to add.</param>
		/// <remarks><c>M:Mono.DocTest.MyList`2.System#Collections#Generic#ICollection{A}#Add(`0)</c>.</remarks>
		void ICollection<A>.Add (A item) {}
		/// <remarks><c>M:Mono.DocTest.MyList`2.System#Collections#Generic#ICollection{A}#Clear</c>.</remarks>
		void ICollection<A>.Clear () {}
		/// <param name="item">The item to check for</param>
		/// <remarks><c>M:Mono.DocTest.MyList`2.System#Collections#Generic#ICollection{A}.Contains(`0)</c>.</remarks>
		/// <returns>A <see cref="T:System.Boolean" /> instance (<see langword="false" />).</returns>
		bool ICollection<A>.Contains (A item) {return false;}
		/// <param name="array">Where to copy elements to</param>
		/// <param name="arrayIndex">Where to start copyingto</param>
		/// <remarks><c>M:Mono.DocTest.MyList`2.CopyTo(`0[],System.Int32)</c>.</remarks>
		public void CopyTo (A[] array, int arrayIndex) {}
		/// <param name="item">the item to remove</param>
		/// <remarks><c>M:Mono.DocTest.MyList`2.System#Collections#Generic#ICollection{A}#Remove(`0)</c>.</remarks>
		/// <returns>Whether the item was removed.</returns>
		bool ICollection<A>.Remove (A item) {return false;}

		/// <remarks>M:Mono.DocTest.Generic.MyList`2.Foo</remarks>
		public KeyValuePair<IEnumerable<A>, IEnumerable<B>> Foo ()
		{
			return new KeyValuePair<IEnumerable<A>, IEnumerable<B>> ();
		}

		// IFoo members
		/// <typeparam name="U">U generic param on MyList`2</typeparam>
		/// <remarks><c>M:Mono.DocTest.Generic.MyList`2.Mono#DocTest#Generic#IFoo{A}#Method``1(`0,``0)</c>.</remarks>
		A IFoo<A>.Method <U> (A a, U u)
		{
			return default (A);
		}
	}
}

