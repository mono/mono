//
// CSharpBinaryOperationBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Compiler = Mono.CSharp;

namespace Microsoft.CSharp.RuntimeBinder
{
	public class CSharpBinaryOperationBinder : BinaryOperationBinder
	{
		IList<CSharpArgumentInfo> argumentInfo;
		bool is_checked, is_member_access;
		
		public CSharpBinaryOperationBinder (ExpressionType operation, bool isChecked, bool isMemberAccess, IEnumerable<CSharpArgumentInfo> argumentInfo)
			: base (operation)
		{
			this.argumentInfo = new ReadOnlyCollectionBuilder<CSharpArgumentInfo> (argumentInfo);
			if (this.argumentInfo.Count != 2)
				throw new ArgumentException ("argumentInfo != 2");

			this.is_checked = isChecked;
			this.is_member_access = isMemberAccess;
		}
		
		public IList<CSharpArgumentInfo> ArgumentInfo {
			get {
				return argumentInfo;
			}
		}

		public bool IsChecked {
			get {
				return is_checked;
			}
		}
		
		public bool IsMemberAccess {
			get {
				return is_member_access;
			}
		}

		public override bool Equals (object obj)
		{
			var other = obj as CSharpBinaryOperationBinder;
			return other != null && base.Equals (obj) && other.is_checked == is_checked && other.is_member_access == is_member_access &&
				other.argumentInfo.SequenceEqual (argumentInfo);
		}
		
		public override int GetHashCode ()
		{
			return Extensions.HashCode (
				base.GetHashCode (),
				is_checked.GetHashCode (),
				is_member_access.GetHashCode (),
				argumentInfo[0].GetHashCode (), argumentInfo[1].GetHashCode ());
		}

		Compiler.Binary.Operator GetOperator ()
		{
			switch (Operation) {
			case ExpressionType.Add:
				return Compiler.Binary.Operator.Addition;
			default:
				throw new NotImplementedException (Operation.ToString ());
			}
		}
		
		public override DynamicMetaObject FallbackBinaryOperation (DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
		{
			var left = CSharpBinder.CreateCompilerExpression (argumentInfo [0], target);
			var right = CSharpBinder.CreateCompilerExpression (argumentInfo [1], arg);
			Compiler.Expression expr = new Compiler.Binary (GetOperator (), left, right);
			expr = new Compiler.Cast (new Compiler.TypeExpression (typeof (object), Compiler.Location.Null), expr);
			
			if (is_checked)
				expr = new Compiler.CheckedExpr (expr, Compiler.Location.Null);

			var restrictions = CreateRestrictionsOnTarget (target).Merge (CreateRestrictionsOnTarget (arg));
			return CSharpBinder.Bind (target, expr, restrictions, errorSuggestion);
		}

		static BindingRestrictions CreateRestrictionsOnTarget (DynamicMetaObject arg)
		{
			return arg.HasValue && arg.Value == null ?
				BindingRestrictions.GetInstanceRestriction (arg.Expression, null) :
				BindingRestrictions.GetTypeRestriction (arg.Expression, arg.LimitType);
		}
	}
}
