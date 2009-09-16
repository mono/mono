using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

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
		var v = Expression.Variable (t);
		Expression e = Expression.Block (new[] { v }, Expression.Default (t));

		Expression restr = Expression.Constant (true);
		return new DynamicMetaObject (e, BindingRestrictions.GetExpressionRestriction (restr));
	}

	public override DynamicMetaObject BindBinaryOperation (BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		if (mock.BinaryOperation == null)
			throw new ApplicationException ("Unexpected BindBinaryOperation");

		mock.BinaryOperation ((CSharpBinaryOperationBinder) binder, arg.Value);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindConvert (ConvertBinder binder)
	{
		if (mock.ConvertOperation == null)
			throw new ApplicationException ("Unexpected BindConvert");

		var r = mock.ConvertOperation ((CSharpConvertBinder) binder);

		return GetFakeMetaObject (r);
	}

	public override DynamicMetaObject BindGetIndex (GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		if (mock.GetIndexOperation == null)
			throw new ApplicationException ("Unexpected TryGetIndex");

		mock.GetIndexOperation ((CSharpGetIndexBinder) binder, indexes.Select (l => l.Value).ToArray ());

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindGetMember (GetMemberBinder binder)
	{
		if (mock.GetMemberOperation == null)
			throw new ApplicationException ("Unexpected BindGetMember");

		mock.GetMemberOperation ((CSharpGetMemberBinder) binder);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindInvoke (InvokeBinder binder, DynamicMetaObject[] args)
	{
		if (mock.InvokeOperation == null)
			throw new ApplicationException ("Unexpected BindInvoke");

		mock.InvokeOperation ((CSharpInvokeBinder) binder, args.Select (l => l.Value).ToArray ());

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindInvokeMember (InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		if (mock.InvokeMemberOperation == null)
			throw new ApplicationException ("Unexpected BindInvokeMember");

		mock.InvokeMemberOperation ((CSharpInvokeMemberBinder) binder, args.Select (l => l.Value).ToArray ());

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindSetIndex (SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		if (mock.SetIndexOperation == null)
			throw new ApplicationException ("Unexpected TrySetIndex");

		mock.SetIndexOperation ((CSharpSetIndexBinder) binder, indexes.Select (l => l.Value).ToArray (), value.Value);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindSetMember (SetMemberBinder binder, DynamicMetaObject value)
	{
		if (mock.SetMemberOperation == null)
			throw new ApplicationException ("Unexpected BindSetMember");

		mock.SetMemberOperation ((CSharpSetMemberBinder) binder, value.Value);

		return GetFakeMetaObject (new object ());
	}

	public override DynamicMetaObject BindUnaryOperation (UnaryOperationBinder binder)
	{
		if (mock.UnaryOperation == null)
			throw new ApplicationException ("Unexpected BindUnaryOperation");

		var r = mock.UnaryOperation ((CSharpUnaryOperationBinder) binder);

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

	public Action<CSharpBinaryOperationBinder, object> BinaryOperation;
	public Func<CSharpConvertBinder, object> ConvertOperation;
	public Action<CSharpGetIndexBinder, object[]> GetIndexOperation;
	public Action<CSharpGetMemberBinder> GetMemberOperation;
	public Action<CSharpInvokeBinder, object[]> InvokeOperation;
	public Action<CSharpInvokeMemberBinder, object[]> InvokeMemberOperation;
	public Action<CSharpSetIndexBinder, object[], object> SetIndexOperation;
	public Action<CSharpSetMemberBinder, object> SetMemberOperation;
	public Func<CSharpUnaryOperationBinder, object> UnaryOperation;

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
			throw new ApplicationException (name + expected + " != " + value);
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

#pragma warning disable 168, 169, 219

	void BinaryAdd_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new [] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d + 1;
	}

	void BinaryAdd_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null) },
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
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.LiteralConstant, null)	// CSC bug?
			}, "ArgumentInfo");

			Assert (arg, Enum.A, "arg");
		};

		d = d + Enum.A;
	}

	void BinaryAdd_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 7, "arg");
		};

		d = d + Tester.field;
	}

	void BinaryAddChecked_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			Assert (binder.IsChecked, true, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.LiteralConstant, null) },
				"ArgumentInfo");

			Assert (arg, 3, "arg");
		};

		d = checked (d + 3);
	}
	
	void BinaryAddChecked_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Add, "Operation");
			Assert (binder.IsChecked, true, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.LiteralConstant, null) },
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
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d += 1;
	}

	void BinaryAddAssignChecked_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.AddAssign, "Operation");
			Assert (binder.IsChecked, true, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
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
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d & 1;
	}

	void BinaryAndAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.AndAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d &= 1;
	}

	void BinaryDivide_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Divide, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d / 1;
	}

	void BinaryDivideAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.DivideAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d /= 1;
	}

	void BinaryEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Equal, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d == 1;
	}

	void BinaryExclusiveOr_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.ExclusiveOr, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d ^ 1;
	}

	void BinaryExclusiveOrAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.ExclusiveOrAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d ^= 1;
	}

	void BinaryGreaterThan_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.GreaterThan, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d > 1;
	}

	void BinaryGreaterThanOrEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.GreaterThanOrEqual, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d >= 1;
	}

	void BinaryLeftShift_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LeftShift, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d << 1;
	}

	void BinaryLeftShiftAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LeftShiftAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d <<= 1;
	}

	void BinaryLessThan_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LessThan, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d < 1;
	}

	void BinaryLessThanOrEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.LessThanOrEqual, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d <= 1;
	}

	void BinaryModulo_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Modulo, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d % 1;
	}

	void BinaryModuloAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.ModuloAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d %= 1;
	}

	void BinaryMultiply_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Multiply, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d * 1;
	}

	void BinaryMultiplyAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.MultiplyAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d *= 1;
	}

	void BinaryNotEqual_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.NotEqual, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 4, "arg");
		};

		d = d != 4;
	}

	void BinaryOr_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Or, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 2, "arg");
		};

		d = d | 2;
	}

	void BinaryOrAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.OrAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 2, "arg");
		};

		d |= 2;
	}

	void BinaryRightShift_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.RightShift, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d >> 1;
	}

	void BinaryRightShiftAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.RightShiftAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d >>= 1;
	}

	void BinarySubtract_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Subtract, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d = d - 1;
	}

	void BinarySubtractAssign_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.SubtractAssign, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, 1, "arg");
		};

		d -= 1;
	}

	void Convert_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, true, "Explicit");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.Type, typeof (byte), "Type");
			return (byte) 1;
		};

		object b = (byte) d;
	}

	void Convert_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, false, "Explicit");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.Type, typeof (int), "Type");
			return 2;
		};

		object[] o = new object [2];
		d = o[d];
	}

	void Convert_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, true, "Explicit");
			Assert (binder.IsChecked, true, "IsChecked");
			Assert (binder.Type, typeof (byte), "Type");
			return (byte) 2;
		};

		object b = checked((byte) d);
	}

	void Convert_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.ConvertOperation = (binder) => {
			Assert (binder.Explicit, false, "Explicit");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.Type, typeof (int), "Type");
			return 5;
		};

		var g = new int[d];
	}

	void GetIndex_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.GetIndexOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert ((IList<object>)args, new object[] { 0 }, "args");
		};

		var o = d [0];
	}

	void GetIndex_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.GetIndexOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null) },
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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null) },
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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null) },
				"ArgumentInfo");
		};

		var g = d.Foo;
	}

	void Invoke_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (2, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null)
			}, 	"ArgumentInfo");

			Assert ((IList<object>) args, new object[] { "foo", null }, "args");
		};

		d ("foo", null);
	}

	void Invoke_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (0, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null) },
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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.NamedArgument | CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.LiteralConstant, "name")	// CSC bug?
			}, "ArgumentInfo");

			Assert ((IList<object>) args, new object[] { typeof (bool), -1 }, "args");
		};

		d (typeof (bool), name:-1);
	}

	void InvokeMember_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null)},
				"ArgumentInfo");

			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { 'a' }, "args");
		};

		d.Max ('a');
	}

	void InvokeMember_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)},
				"ArgumentInfo");

			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { mock }, "args");
		};

		mock.DMethod (d);
	}

	void InvokeMember_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.IsRef | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

			Assert ((IList<object>) args, new object[] { 9 }, "args");
		};

		int i = 9;
		d.Max (ref i);
	}

	void InvokeMember_4 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.IsOut | CSharpArgumentInfoFlags.UseCompileTimeType, null)	},
				"ArgumentInfo");

			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			Assert (binder.TypeArguments, new Type[0], "TypeArguments");

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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.LiteralConstant, null),
			}, "ArgumentInfo");
			Assert (binder.Flags, CSharpCallFlags.SimpleNameCall, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			Assert (binder.TypeArguments, Type.EmptyTypes, "TypeArguments");

			Assert ((IList<object>) args, new object[] { d, null }, "args");
		};

		InvokeMember_5 (d, null);
	}

	void InvokeMember_7 (dynamic d, DynamicObjectMock mock)
	{
		mock.InvokeMemberOperation = (binder, args) => {
			Assert (binder.CallInfo, new CallInfo (0, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");
			Assert (binder.Flags, CSharpCallFlags.None, "Flags");
			Assert (binder.IgnoreCase, false, "IgnoreCase");
			Assert (binder.TypeArguments, new Type[] { typeof (object) }, "TypeArguments");

			Assert ((IList<object>) args, new object[0], "args");
		};

		d.Max<dynamic> ();
	}

	void SetIndex_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.SetIndexOperation = (binder, args, value) => {
			Assert (binder.CallInfo, new CallInfo (1, new string[0]), "CallInfo");
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null)},
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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null)
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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType, null)
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
			Assert (binder.CallingContext, typeof (Tester), "CallingContext");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.LiteralConstant, null)	// CSC bug?
			}, 	"ArgumentInfo");

			Assert (value, d_const, "value");
		};

		d.Foo = d_const;
	}

	void UnaryPlus_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.UnaryPlus, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = +d;
	}

	void UnaryMinus_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Negate, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = -d;
	}

	void UnaryNot_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Not, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = !d;
	}

	void UnaryOnesComplement_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.OnesComplement, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = ~d;
	}

	void UnaryDecrement_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Decrement, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = --d;
	}

	void UnaryDecrement_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Decrement, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return new object ();
		};

		d = d--;
	}

	void UnaryIncrement_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Increment, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return null;
		};

		d = ++d;
	}

	void UnaryIncrement_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.Increment, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return new object ();
		};

		d = d++;
	}

	void UnaryIsFalse_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsFalse, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Equal, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d == null;
	}

	void UnaryIsFalse_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsFalse, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.NotEqual, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d != null;
	}

	void UnaryIsFalse_3 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsFalse, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.And, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
				"ArgumentInfo");

			Assert (arg, null, "arg");
		};

		object x = d && null;
	}

	void UnaryIsTrue_1 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsTrue, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return true;
		};

		object g = d ? 1 :4;
	}

	void UnaryIsTrue_2 (dynamic d, DynamicObjectMock mock)
	{
		mock.UnaryOperation = (binder) => {
			Assert (binder.Operation, ExpressionType.IsTrue, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null)
			}, "ArgumentInfo");

			return false;
		};

		mock.BinaryOperation = (binder, arg) => {
			Assert (binder.Operation, ExpressionType.Or, "Operation");
			Assert (binder.IsChecked, false, "IsChecked");
			Assert (binder.IsMemberAccess, false, "IsMemberAccess");
			Assert (binder.ArgumentInfo, new[] {
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.None, null),
				new CSharpArgumentInfo (CSharpArgumentInfoFlags.LiteralConstant | CSharpArgumentInfoFlags.UseCompileTimeType, null) },
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
			test.Invoke (new Tester (), new [] { d, d });
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
