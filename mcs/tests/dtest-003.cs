using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;

enum Enum
{
	A = 3
}

class AssertDynamicObject : DynamicMetaObject
{
	DynamicObjectMock mock;

	public AssertDynamicObject (DynamicObjectMock mock, Expression parameter)
		: base (parameter, BindingRestrictions.Empty, mock)
	{
		this.mock = mock;
	}

	DynamicMetaObject GetFakeMetaObject (object value)
	{
		Type t = value == null ? typeof (object) : value.GetType ();
		Expression<Func<object>> et = () => value;

		Expression restr = Expression.Constant (true);
		return new DynamicMetaObject (Expression.Convert (et.Body, t), BindingRestrictions.GetExpressionRestriction (restr));
	}

	public override DynamicMetaObject BindBinaryOperation (BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		if (mock.BinaryOperation == null)
			throw new ApplicationException ("Unexpected BindBinaryOperation");

		mock.BinaryOperation (binder, arg.Value);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindConvert (ConvertBinder binder)
	{
		if (mock.ConvertOperation == null)
			throw new ApplicationException ("Unexpected BindConvert");

		var r = mock.ConvertOperation (binder);

		return GetFakeMetaObject (r);
	}

	public override DynamicMetaObject BindGetIndex (GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		if (mock.GetIndexOperation == null)
			throw new ApplicationException ("Unexpected TryGetIndex");

		mock.GetIndexOperation (binder, indexes.Select (l => l.Value).ToArray ());

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindGetMember (GetMemberBinder binder)
	{
		if (mock.GetMemberOperation == null)
			throw new ApplicationException ("Unexpected BindGetMember");

		mock.GetMemberOperation (binder);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindInvoke (InvokeBinder binder, DynamicMetaObject[] args)
	{
		if (mock.InvokeOperation == null)
			throw new ApplicationException ("Unexpected BindInvoke");

		mock.InvokeOperation (binder, args.Select (l => l.Value).ToArray ());

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindInvokeMember (InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		if (mock.InvokeMemberOperation == null)
			throw new ApplicationException ("Unexpected BindInvokeMember");

		mock.InvokeMemberOperation (binder, args.Select (l => l.Value).ToArray ());

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindSetIndex (SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		if (mock.SetIndexOperation == null)
			throw new ApplicationException ("Unexpected TrySetIndex");

		mock.SetIndexOperation (binder, indexes.Select (l => l.Value).ToArray (), value.Value);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindSetMember (SetMemberBinder binder, DynamicMetaObject value)
	{
		if (mock.SetMemberOperation == null)
			throw new ApplicationException ("Unexpected BindSetMember");

		mock.SetMemberOperation (binder, value.Value);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindUnaryOperation (UnaryOperationBinder binder)
	{
		if (mock.UnaryOperation == null)
			throw new ApplicationException ("Unexpected BindUnaryOperation");

		var r = mock.UnaryOperation (binder);

		return GetFakeMetaObject (r);
	}

}

class DynamicObjectMock : DynamicObject
{
	public int HitCounter;

	public DynamicObjectMock ()
	{
	}

	public override DynamicMetaObject GetMetaObject (System.Linq.Expressions.Expression parameter)
	{
		HitCounter++;
		return new AssertDynamicObject (this, parameter);
	}

	public Action<BinaryOperationBinder, object> BinaryOperation;
	public Func<ConvertBinder, object> ConvertOperation;
	public Action<GetIndexBinder, object[]> GetIndexOperation;
	public Action<GetMemberBinder> GetMemberOperation;
	public Action<InvokeBinder, object[]> InvokeOperation;
	public Action<InvokeMemberBinder, object[]> InvokeMemberOperation;
	public Action<SetIndexBinder, object[], object> SetIndexOperation;
	public Action<SetMemberBinder, object> SetMemberOperation;
	public Func<UnaryOperationBinder, object> UnaryOperation;

	// Dynamic arguments methods
	public DynamicObjectMock (int i)
	{
	}

	public void DMethod (int a)
	{
	}

	public static void DStaticMethod (object t)
	{
	}

	public int this[int i] {
		get {
			return i;
		}
		set { }
	}

}

class Tester : DynamicObjectMock
{
	static readonly int field = 7;

	public Tester ()
	{
	}

	public Tester (dynamic d)
	{
	}

	static void Assert<T> (T expected, T value, string name)
	{
		if (!EqualityComparer<T>.Default.Equals (expected, value)) {
			if (!string.IsNullOrEmpty (name))
				name += ": ";
			throw new ApplicationException (name + "Expected " + expected + " != " + value);
		}
	}

	static void Assert<T> (IList<T> expected, IList<T> value, string name)
	{
		if (expected == null) {
			if (value != null)
				throw new ApplicationException (name + ": Both arrays expected to be null");
			return;
		}

		if (expected.Count != value.Count)
			throw new ApplicationException (name + ": Array length does not match " + expected.Count + " != " + value.Count);

		for (int i = 0; i < expected.Count; ++i) {
			if (!EqualityComparer<T>.Default.Equals (expected[i], value[i]))
				throw new ApplicationException (name + ": Index " + i + ": " + expected[i] + " != " + value[i]);
		}
	}

	static FieldInfo flags = typeof (CSharpArgumentInfo).GetField ("flags", BindingFlags.NonPublic | BindingFlags.Instance);

	static void AssertArgument (CallSiteBinder obj, CSharpArgumentInfo[] expected, string name)
	{
		var ai = obj.GetType ().GetField ("argumentInfo", BindingFlags.NonPublic | BindingFlags.Instance);
		IList<CSharpArgumentInfo> values = (IList<CSharpArgumentInfo>) ai.GetValue (obj);
		if (values.Count != expected.Length)
			throw new ApplicationException (name + ": Array length does not match " + values.Count + " != " + expected.Length);

		for (int i = 0; i < expected.Length; i++) {
			Assert (flags.GetValue (expected[i]), flags.GetValue (values[i]), "flags");
		}
	}

#pragma warning disable 168, 169, 219

	void BinaryAdd_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d + 1;
	}

	void BinaryAdd_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		int? v = null;
		d = d + v;
	}

	void BinaryAdd_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
			}, "ArgumentInfo");

			Assert (arg, Enum.A, "arg");
		};

		d = d + Enum.A;
	}

	void BinaryAdd_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 7, "arg");
		};

		d = d + Tester.field;
	}

	void BinaryAddChecked_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null) },
				"ArgumentInfo");

			Assert (arg, 3, "arg");
		};

		d = checked (d + 3);
	}

	void BinaryAddChecked_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null) },
				"ArgumentInfo");

			Assert (arg, 3, "arg");
		};

		Func<dynamic> r;
		checked {
			r = () => d + 3;
		}

		r ();
	}

	void BinaryAddAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.AddAssign, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d += 1;
	}

	void BinaryAddAssignChecked_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.AddAssign, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		checked {
			d += 1;
		}
	}

	void BinaryAnd_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.And, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d & 1;
	}

	void BinaryAndAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.AndAssign, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d &= 1;
	}

	void BinaryDivide_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Divide, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d / 1;
	}

	void BinaryDivideAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.DivideAssign, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d /= 1;
	}

	void BinaryEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Equal, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d == 1;
	}

	void BinaryExclusiveOr_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.ExclusiveOr, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d ^ 1;
	}

	void BinaryExclusiveOrAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.ExclusiveOrAssign, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d ^= 1;
	}

	void BinaryGreaterThan_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.GreaterThan, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d > 1;
	}

	void BinaryGreaterThanOrEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.GreaterThanOrEqual, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d >= 1;
	}

	void BinaryLeftShift_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LeftShift, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d << 1;
	}

	void BinaryLeftShiftAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LeftShiftAssign, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d <<= 1;
	}

	void BinaryLessThan_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LessThan, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d < 1;
	}

	void BinaryLessThanOrEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LessThanOrEqual, "Operation");
			AssertArgument (binder, new[] {
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
			    CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d <= 1;
	}

	void BinaryModulo_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Modulo, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d % 1;
	}

	void BinaryModuloAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.ModuloAssign, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d %= 1;
	}

	void BinaryMultiply_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Multiply, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d * 1;
	}

	void BinaryMultiplyAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.MultiplyAssign, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d *= 1;
	}

	void BinaryNotEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.NotEqual, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 4, "arg");
		};

		d = d != 4;
	}

	void BinaryOr_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Or, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 2, "arg");
		};

		d = d | 2;
	}

	void BinaryOrAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.OrAssign, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 2, "arg");
		};

		d |= 2;
	}

	void BinaryRightShift_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.RightShift, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d >> 1;
	}

	void BinaryRightShiftAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.RightShiftAssign, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d >>= 1;
	}

	void BinarySubtract_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Subtract, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d - 1;
	}

	void BinarySubtractAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.SubtractAssign, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d -= 1;
	}

	void Convert_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, true, "Explicit");
			Assert (binder.Type, typeof (byte), "Type");
			return (byte) 1;
		};

		object b = (byte) d;
	}

	void Convert_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, false, "Explicit");
			Assert (binder.Type, typeof (int), "Type");
			return 1;
		};

		object[] o = new object[2];
		d = o[d];
	}

	void Convert_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, true, "Explicit");
			//			Assert (binder.IsChecked, true, "IsChecked");
			Assert (binder.Type, typeof (byte), "Type");
			return (byte) 2;
		};

		object b = checked ((byte) d);
	}

	void Convert_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, false, "Explicit");
			Assert (binder.Type, typeof (int), "Type");
			return 5;
		};

		var g = new int[d];
	}

	void Convert_5 (dynamic d, DynamicObjectMock mock)
	{
		int counter = 0;
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, false, "Explicit");
			Assert (binder.Type, typeof (System.Collections.IEnumerable), "Type");
			return new object[] { 1 };
		};

		foreach (int v in d) {
			//			Console.WriteLine (v);
		}
	}

	void GetIndex_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.GetIndexOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert ((IList<object>) args, new object[] { 0 }, "args");
		};

		var o = d[0];
	}

	void GetIndex_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.GetIndexOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null) },
			"ArgumentInfo");

			Assert ((IList<object>) args, new object[] { 2, 3 }, "args");
		};

		object i = 3;
		var o = d[2, i];
	}

	void GetIndex_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.GetIndexOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null) },
				"ArgumentInfo");

			Assert ((IList<object>) args, new object[] { d }, "args");
		};

		var o = mock[d];
	}

	void GetMember_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.GetMemberOperation = (binder) => {
			Assert (binder.Name, "Foo", "Name");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null) },
				"ArgumentInfo");
		};

		var g = d.Foo;
	}

	void Invoke_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant, null)
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { "foo", null }, "args");
		};

		d ("foo", null);
	}

	void Invoke_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (0, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null) },
				"ArgumentInfo");

			Assert ((IList<object>) args, new object[0], "args");
		};

		d ();
	}
	
	void Invoke_3 (dynamic d, DynamicObjectMock mock)
	{
		try {
			Math.Max (d, d);
			Assert (true, false, "No hook expected to be hit");
		} catch (RuntimeBinderException) {
		}
	}

	void Invoke_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[] { "name" }), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.NamedArgument | CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, "name")
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { typeof (bool), -1 }, "args");
		};

		d (typeof (bool), name: -1);
	}

	void Invoke_5 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { "a" }, "args");
		};

		Action<dynamic> a = (i) => { i ("a"); };
		a (d);
	}

	void Invoke_6 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { 3 }, "args");
		};

		d (1 + 2);
	}

	void InvokeMember_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null)},
				"ArgumentInfo");

//			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
//			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { 'a' }, "args");
		};

		d.Max ('a');
	}

	void InvokeMember_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)},
				"ArgumentInfo");

//			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
//			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { mock }, "args");
		};

		mock.DMethod (d);
	}

	void InvokeMember_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.IsRef | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			//			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			//			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { 9 }, "args");
		};

		int i = 9;
		d.Max (ref i);
	}

	void InvokeMember_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.IsOut | CSharpArgumentInfoFlags.UseCompileTimeType, null)	},
				"ArgumentInfo");

//			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
//			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { 0 }, "args");
		};

		int i;
		d.Max (out i);
	}

	void InvokeMember_5 (dynamic d, DynamicObjectMock mock)
	{
		DynamicObjectMock.DStaticMethod (d);
	}

	void InvokeMember_6 (dynamic d, DynamicObjectMock mock)
	{
		InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant, null),
			}, "ArgumentInfo");
			//			Assert (binder.Flags, CSharpCallFlags.SimpleNameCall, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			//			Assert (binder.TypeArguments, Type.EmptyTypes, "TypeArguments");

			Assert ((IList<object>) args, new object[] { d, null }, "args");
		};

		InvokeMember_5 (d, null);
	}

	void InvokeMember_7 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (0, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");
//			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
//			Assert (binder.TypeArguments, new Type[] { typeof (object) }, "TypeArguments");

			Assert ((IList<object>) args, new object[0], "args");
		};

		d.Max<dynamic> ();
	}

	void SetIndex_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.SetIndexOperation = (binder, args, value) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null)},
				"ArgumentInfo");

			Assert ((IList<object>) args, new object[] { 0 }, "args");
			Assert (value, 2m, "value");
		};

		d[0] = 2m;
	}

	void SetIndex_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.SetIndexOperation = (binder, args, value) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null)
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { 2, 3 }, "args");
			Assert (value, -8, "value");
		};

		object i = 3;
		d[2, i] = -8;
	}

	void SetIndex_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.SetIndexOperation = (binder, args, value) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType, null)
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { d }, "args");
			Assert (value, this, "value");
		};

		mock[d] = this;
	}

	void SetMember_1 (dynamic d, DynamicObjectMock mock)
	{
		const double d_const = 2.4;

		mock.SetMemberOperation = (binder, value) => {
			Assert (binder.Name, "Foo", "Name");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
			}, "ArgumentInfo");

			Assert (value, d_const, "value");
		};

		d.Foo = d_const;
	}

	void UnaryPlus_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.UnaryPlus, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = +d;
	}

	void UnaryMinus_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Negate, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = -d;
	}

	void UnaryNot_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Not, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = !d;
	}

	void UnaryOnesComplement_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.OnesComplement, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = ~d;
	}

	void UnaryDecrement_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Decrement, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = --d;
	}

	void UnaryDecrement_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Decrement, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return new object ();
		};

		d = d--;
	}

	void UnaryIncrement_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Increment, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = ++d;
	}

	void UnaryIncrement_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Increment, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return new object ();
		};

		d = d++;
	}

	void UnaryIsFalse_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsFalse, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Equal, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d == null;
	}

	void UnaryIsFalse_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsFalse, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.NotEqual, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d != null;
	}

	void UnaryIsFalse_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsFalse, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.And, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d && null;
	}

	void UnaryIsTrue_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsTrue, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		object g = d ? 1 : 4;
	}

	void UnaryIsTrue_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsTrue, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return false;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Or, "Operation");
			AssertArgument (binder, new[] {
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.Constant, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d || null;
	}

#pragma warning restore 168, 169, 219

	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			var d = new DynamicObjectMock ();
			test.Invoke (new Tester (), new[] { d, d });
			if (d.HitCounter < 1)
				Assert (true, false, "HitCounter");

			Console.WriteLine ("OK");
			return true;
		} catch (Exception e) {
			Console.WriteLine ("FAILED");
			Console.WriteLine (e.ToString ());
			return false;
		}
	}

	public static int Main ()
	{
		var tests = from test in typeof (Tester).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					where test.GetParameters ().Length == 2
					orderby test.Name
					select RunTest (test);

		int failures = tests.Count (a => !a);
		Console.WriteLine (failures + " tests failed");
		return failures;
	}
}
