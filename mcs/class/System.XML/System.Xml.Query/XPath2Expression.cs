//
// XPath2Expression.cs - abstract syntax tree for XPath 2.0
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using System.Xml.XPath;
using Mono.Xml.XQuery;

namespace Mono.Xml.XPath2
{
	public class ExprSequence : CollectionBase
	{
		public ExprSequence ()
		{
		}

		public void Add (ExprSingle expr)
		{
			List.Add (expr);
		}

		public void AddRange (ICollection items)
		{
			if (items != null)
				foreach (ExprSingle e in items)
					List.Add (e);
		}

		public void Insert (int pos, ExprSingle expr)
		{
			List.Insert (pos, expr);
		}

		public ExprSingle this [int i] {
			get { return List [i] as ExprSingle; }
			set { List [i] = value; }
		}

		internal void CheckReference (XQueryASTCompiler compiler)
		{
			foreach (ExprSingle expr in List)
				expr.CheckReference (compiler);
		}
	}

	public abstract class ExprSingle
	{
		internal abstract void CheckReference (XQueryASTCompiler compiler);

#region CompileAndEvaluate
		internal static readonly XPathAtomicValue AtomicTrue = new XPathAtomicValue (true, XmlSchemaSimpleType.XsBoolean);
		internal static readonly XPathAtomicValue AtomicFalse = new XPathAtomicValue (false, XmlSchemaSimpleType.XsBoolean);

		XQueryStaticContext ctx;

		internal ExprSingle Compile (XQueryASTCompiler compiler)
		{
			this.ctx = ctx;
			return CompileCore (compiler);
		}

		// If internal&&protected is available in C#, it is the best signature.
		internal abstract ExprSingle CompileCore (XQueryASTCompiler compiler);

		internal XQueryStaticContext Context {
			get { return ctx; }
		}

		public abstract SequenceType StaticType { get; }

		/** <summary>
			This is the core part of ExprSingle. It is
			generally used to evaluate expression and returns
			XPathItem sequence (iterator). The result is unordered
		*/
		public abstract XPathSequence Evaluate (XPathSequence iter);

		public virtual XPathSequence EvaluateOrdered (XPathSequence iter)
		{
			if (RequireSorting) {
				ArrayList al = new ArrayList ();
				foreach (XPathItem item in Evaluate (iter))
					al.Add (item);
				return new ListIterator (iter, al);
			}
			else
				return Evaluate (iter);
		}

		public virtual void Serialize (XPathSequence iter)
		{
			XmlWriter w = iter.Context.Writer;
			XPathSequence result = Evaluate (iter);
			bool initial = true;
			foreach (XPathItem item in result) {
				if (initial)
					initial = false;
				else
					w.WriteWhitespace (" ");
				WriteXPathItem (item, w);
			}
		}

		private void WriteXPathItem (XPathItem item, XmlWriter w)
		{
			if (item.IsNode) {
				XPathNavigator nav = item as XPathNavigator;
				if (w.WriteState != WriteState.Start && nav.NodeType == XPathNodeType.Root)
					throw new XmlQueryException ("Current output can not accept root node.");
				nav.WriteSubtree (w);
			} else
				w.WriteValue (item.Value);
		}

		// get EBV (fn:boolean())
		public virtual bool EvaluateAsBoolean (XPathSequence iter)
		{
			XPathSequence result = Evaluate (iter);
			if (!result.MoveNext ())
				return false;
			XPathAtomicValue v = Atomize (result.Current);
			if (result.MoveNext ())
				return true;
			switch (v.XmlType.TypeCode) {
			case XmlTypeCode.Boolean:
				return v.ValueAsBoolean;
			case XmlTypeCode.String:
			case XmlTypeCode.UntypedAtomic:
				return v.Value != String.Empty;
			case XmlTypeCode.Float:
				return v.ValueAsSingle != Single.NaN && v.ValueAsSingle != 0.0;
			case XmlTypeCode.Double:
				return v.ValueAsDouble != Double.NaN && v.ValueAsSingle != 0.0;
			case XmlTypeCode.Integer:
			case XmlTypeCode.NonPositiveInteger:
			case XmlTypeCode.NegativeInteger:
			case XmlTypeCode.Long:
			case XmlTypeCode.Int:
			case XmlTypeCode.Short:
			case XmlTypeCode.Byte:
			case XmlTypeCode.UnsignedInt:
			case XmlTypeCode.UnsignedShort:
			case XmlTypeCode.UnsignedByte:
				return v.ValueAsInt64 != 0;
			case XmlTypeCode.NonNegativeInteger:
			case XmlTypeCode.UnsignedLong:
			case XmlTypeCode.PositiveInteger:
				return (ulong) (v.ValueAs (typeof (ulong))) != 0;
			}
			// otherwise, return true
			return true;
		}

		public virtual int EvaluateAsInt (XPathSequence iter)
		{
			XPathAtomicValue v = Atomize (Evaluate (iter));
			return v != null ? v.ValueAsInt32 : 0;
		}

		public virtual string EvaluateAsString (XPathSequence iter)
		{
			XPathAtomicValue v = Atomize (Evaluate (iter));
			return v != null ? v.Value : String.Empty;
		}

		public static XPathAtomicValue Atomize (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			if (nav != null) {
				if (nav.SchemaInfo != null)
					return new XPathAtomicValue (nav.TypedValue, nav.SchemaInfo.SchemaType);
				else
					return new XPathAtomicValue (nav.Value, XmlSchemaSimpleType.XsString);
			}
			else
				return (XPathAtomicValue) item;
		}

		// FIXME: What if iter contains list value?
		public static XPathAtomicValue Atomize (XPathSequence iter)
		{
			if (!iter.MoveNext ())
				return null;
			XPathNavigator nav = iter.Current as XPathNavigator;
			if (nav != null)
				return new XPathAtomicValue (nav.TypedValue, nav.SchemaInfo.SchemaType);
			else
				return (XPathAtomicValue) iter.Current;
		}

		public virtual XPathAtomicValue EvaluateAsAtomic (XPathSequence iter)
		{
			return Atomize (Evaluate (iter));
		}

		public virtual bool RequireSorting {
			get { return false; }
		}
#endregion
	}

	// FLWORExpr

	internal class FLWORExpr : ExprSingle
	{
		public FLWORExpr (ForLetClauseCollection forlet, ExprSequence whereClause, OrderSpecList orderBy, ExprSingle ret)
		{
			this.fl = forlet;
			this.whereClause = new ParenthesizedExpr (whereClause);
			this.orderBy = orderBy;
			this.ret = ret;
		}

		ForLetClauseCollection fl;
		ExprSingle whereClause;
		OrderSpecList orderBy;
		ExprSingle ret;

		public ForLetClauseCollection ForLetClauses {
			get { return fl; }
		}

		public ExprSingle WhereClause {
			get { return whereClause; }
		}

		public OrderSpecList OrderBy {
			get { return orderBy; }
		}

		public ExprSingle ReturnExpr {
			get { return ret; }
			set { ret = value; }
		}

		// ExprSingle Overrides

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			foreach (ForLetClause flc in fl)
				foreach (ForLetSingleBody single in flc)
					single.CheckReference (compiler);
			if (whereClause != null)
				whereClause.CheckReference (compiler);
			if (orderBy != null)
				foreach (OrderSpec os in orderBy)
					os.Expression.CheckReference (compiler);
			ret.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			foreach (ForLetClause flc in ForLetClauses) {
				foreach (ForLetSingleBody flsb in flc) {
					flsb.Expression = flsb.Expression.Compile (compiler);
					if (flsb.ReturnType != null)
						compiler.CheckType (flsb.Expression, flsb.ReturnType);
				}
			}
			if (WhereClause != null)
//				for (int i = 0; i < WhereClause.Count; i++)
//					WhereClause [i] = WhereClause [i].Compile (compiler);
				whereClause = whereClause.Compile (compiler);
			if (OrderBy != null)
				foreach (OrderSpec os in OrderBy)
					os.Expression = os.Expression.Compile (compiler);
			ReturnExpr = ReturnExpr.Compile (compiler);

			return this;
		}

		public override SequenceType StaticType {
			get { return ReturnExpr.StaticType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new FLWORIterator (iter, this);
		}
#endregion
	}

	internal class ForLetClauseCollection : CollectionBase
	{
		public void Add (ForLetClause clause)
		{
			List.Add (clause);
		}

		public void Insert (int pos, ForLetClause clause)
		{
			List.Insert (pos, clause);
		}

		public ForLetClause this [int i] {
			get { return (ForLetClause) List [i]; }
		}
	}

	internal class OrderSpecList : CollectionBase
	{
		bool isStable;

		public OrderSpecList ()
		{
		}

		public bool IsStable {
			get { return isStable; }
			set { isStable = value; }
		}

		public void Insert (int pos, OrderSpec os)
		{
			List.Insert (pos, os);
		}

		public void Add (OrderSpec spec)
		{
			List.Add (spec);
		}

		public OrderSpec this [int i] {
			get { return (OrderSpec) List [i]; }
		}
	}

	internal class OrderSpec
	{
		public OrderSpec (ExprSingle expr, OrderModifier modifier)
		{
			this.expr = expr;
			this.mod = modifier;
		}

		ExprSingle expr;
		OrderModifier mod;

		public ExprSingle Expression {
			get {return expr; }
			set { expr = value; }
		}

		public OrderModifier Modifier {
			get { return mod; }
			set { mod = value; }
		}
	}

	internal class OrderModifier
	{
		public OrderModifier (XmlSortOrder order, XmlSortOrder emptyOrder, string collation)
		{
			this.sortOrder = sortOrder;
			this.emptyOrder = emptyOrder;
			if (collation != null)
				this.coll = new CultureInfo (collation);
		}

		XmlSortOrder sortOrder;
		XmlSortOrder emptyOrder;
		CultureInfo coll;

		public XmlSortOrder SortOrder {
			get { return sortOrder; }
		}

		public XmlSortOrder EmptyOrder {
			get { return emptyOrder; }
		}

		public CultureInfo Collation {
			get { return coll; }
		}
	}

	internal class ForLetClause : CollectionBase
	{
		public ForLetSingleBody this [int i] {
			get { return (ForLetSingleBody) List [i]; }
		}
	}

	internal class ForClause : ForLetClause
	{
		public ForClause ()
		{
		}

		public void Insert (int pos, ForSingleBody body)
		{
			List.Insert (pos, body);
		}

		public void Add (ForSingleBody body)
		{
			List.Add (body);
		}
	}

	internal class LetClause : ForLetClause
	{
		public LetClause ()
		{
		}

		public void Insert (int pos, LetSingleBody body)
		{
			List.Insert (pos, body);
		}

		public void Add (LetSingleBody body)
		{
			List.Add (body);
		}
	}

	internal abstract class ForLetSingleBody
	{
		XmlQualifiedName varName;
		SequenceType type;
		ExprSingle expr;

		public ForLetSingleBody (XmlQualifiedName varName, SequenceType type, ExprSingle expr)
		{
			this.varName = varName;
			this.type = type;
			this.expr = expr;
		}

		public XmlQualifiedName VarName {
			get { return varName; }
		}

		public SequenceType ReturnType {
			get { return type; }
		}

		public ExprSingle Expression {
			get { return expr; }
			set { expr = value; }
		}

		internal void CheckReference (XQueryASTCompiler compiler)
		{
			if (type != null)
				compiler.CheckSchemaType (type);
			expr.CheckReference (compiler);
		}
	}

	internal class ForSingleBody : ForLetSingleBody
	{
		public ForSingleBody (XmlQualifiedName varName, SequenceType type, XmlQualifiedName positionalVar, ExprSingle expr)
			: base (varName, type, expr)
		{
			this.positionalVar = positionalVar;
		}

		XmlQualifiedName positionalVar;

		public XmlQualifiedName PositionalVar {
			get { return positionalVar; }
		}
	}

	internal class LetSingleBody : ForLetSingleBody
	{
		public LetSingleBody (XmlQualifiedName varName, SequenceType type, ExprSingle expr)
			: base (varName, type, expr)
		{
		}
	}

	// QuantifiedExpr

	internal class QuantifiedExpr : ExprSingle
	{
		QuantifiedExprBodyList body;
		ExprSingle satisfies;
		bool every;

		public QuantifiedExpr (bool every, QuantifiedExprBodyList body, ExprSingle satisfies)
		{
			this.every = every;
			this.body = body;
			this.satisfies = satisfies;
		}

		public bool Every {
			get { return every; }
		}

		public QuantifiedExprBodyList BodyList {
			get { return body; }
		}

		public ExprSingle Satisfies {
			get { return satisfies; }
			set { satisfies = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			foreach (QuantifiedExprBody one in body) {
				if (one.Type != null)
					compiler.CheckSchemaType (one.Type);
				one.Expression.CheckReference (compiler);
			}
			Satisfies.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Satisfies = Satisfies.Compile (compiler);
			for (int i = 0; i < BodyList.Count; i++) {
				BodyList [i].Expression = BodyList [i].Expression.Compile (compiler);
				if (BodyList [i].Type != null)
					compiler.CheckType (BodyList [i].Expression, BodyList [i].Type);
			}
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Boolean; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter);
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			foreach (QuantifiedExprBody qb in BodyList) {
				XPathSequence seq = qb.Expression.Evaluate (iter);
				// FIXME: consider qb.Type
//				if (!qb.Type.IsValid (seq))
//					throw new XmlQueryException ("Quantified expression resulted in type promotion error.");
				iter.Context.PushVariable (qb.VarName, seq);
			}

			bool result = every;

			foreach (XPathItem item in iter) {
				if (satisfies.EvaluateAsBoolean (new SingleItemIterator (item, iter))) {
					if (!every) {
						result = true;
						break;
					}
				}
				else if (every) {
					result = false;
					break;
				}
			}

			for (int i = 0; i < BodyList.Count; i++)
				iter.Context.PopVariable ();

			return result;
		}
#endregion
	}

	internal class QuantifiedExprBodyList : CollectionBase
	{
		public QuantifiedExprBodyList ()
		{
		}

		public void Add (QuantifiedExprBody body)
		{
			List.Add (body);
		}

		public void Insert (int pos, QuantifiedExprBody body)
		{
			List.Insert (pos, body);
		}

		public QuantifiedExprBody this [int i] {
			get { return (QuantifiedExprBody) List [i]; }
		}
	}

	internal class QuantifiedExprBody
	{
		private XmlQualifiedName varName;
		private SequenceType type;
		private ExprSingle expr;

		public QuantifiedExprBody (XmlQualifiedName varName,
			SequenceType type, ExprSingle expr)
		{
			this.varName = varName;
			this.type = type ;
			this.expr = expr;
		}

		public XmlQualifiedName VarName {
			get { return varName; }
		}

		public SequenceType Type {
			get { return type; }
		}

		public ExprSingle Expression {
			get { return expr; }
			set { expr = value; }
		}
	}

	// TypeswitchExpr

	internal class TypeswitchExpr : ExprSingle
	{
		ExprSequence switchExpr;
		CaseClauseList caseList;
		XmlQualifiedName defaultVarName;
		ExprSingle defaultReturn;

		public TypeswitchExpr (ExprSequence switchExpr, CaseClauseList caseList, XmlQualifiedName defaultVarName, ExprSingle defaultReturn)
		{
			this.switchExpr = switchExpr;
			this.caseList = caseList;
			this.defaultVarName = defaultVarName;
			this.defaultReturn = defaultReturn;
		}

		public ExprSequence SwitchExpr {
			get { return switchExpr; }
		}

		public CaseClauseList Cases {
			get { return caseList; }
		}

		public XmlQualifiedName DefaultVarName {
			get { return defaultVarName; }
		}

		public ExprSingle DefaultReturn {
			get { return defaultReturn; }
			set { defaultReturn = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			switchExpr.CheckReference (compiler);
			foreach (CaseClause cc in caseList) {
				compiler.CheckSchemaType (cc.Type);
				cc.Expr.CheckReference (compiler);
			}
			defaultReturn.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			for (int i = 0; i < SwitchExpr.Count; i++)
				SwitchExpr [i] = SwitchExpr [i].Compile (compiler);
			foreach (CaseClause cc in Cases)
				cc.Expr = cc.Expr.Compile (compiler);
			DefaultReturn = DefaultReturn.Compile (compiler);
			return this;
		}

		// FIXME: it can be optimized by checking all case clauses.
		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			// FIXME: should move to iterator?
			XPathSequence cond = new ExprSequenceIterator (iter, SwitchExpr);
			XPathSequence ret = null;

			foreach (CaseClause ccc in Cases) {
				if (ccc.Type.Matches (cond)) {
					if (ccc.VarName != XmlQualifiedName.Empty)
						iter.Context.PushVariable (ccc.VarName, cond);
					ret = ccc.Expr.Evaluate (iter);
					// FIXME: The design should make sure that in-scope variables are held on actual iteration.
					if (ccc.VarName != XmlQualifiedName.Empty)
						iter.Context.PopVariable ();
					return ret;
				}
			}

			if (DefaultVarName != XmlQualifiedName.Empty)
				iter.Context.PushVariable (DefaultVarName, cond);
			ret = DefaultReturn.Evaluate (iter);
			if (DefaultVarName != XmlQualifiedName.Empty)
				iter.Context.PopVariable ();
			return ret;
		}
#endregion
	}

	internal class CaseClauseList : CollectionBase
	{
		public void Insert (int pos, CaseClause cc)
		{
			List.Insert (pos, cc);
		}

		public void Add (CaseClause cc)
		{
			List.Add (cc);
		}

		public CaseClause this [int i] {
			get { return (CaseClause) List [i]; }
		}
	}

	internal class CaseClause
	{
		public CaseClause (SequenceType type, ExprSingle expr, XmlQualifiedName varName)
		{
			this.type = type;
			this.expr = expr;
			this.varName = varName;
		}

		SequenceType type;
		ExprSingle expr;
		XmlQualifiedName varName;

		public SequenceType Type {
			get { return type; }
		}

		public ExprSingle Expr {
			get { return expr; }
			set { expr = value; }
		}

		public XmlQualifiedName VarName {
			get { return varName; }
			set { varName = value; }
		}
	}

	// IfExpr

	internal class IfExpr : ExprSingle
	{
		public IfExpr (ExprSequence condition, ExprSingle trueExpr, ExprSingle falseExpr)
		{
			this.condition = new ParenthesizedExpr (condition);
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}

		ExprSingle condition;
		ExprSingle trueExpr;
		ExprSingle falseExpr;

		public ExprSingle Condition {
			get { return condition; }
			set { condition = value; }
		}

		public ExprSingle TrueExpr {
			get { return trueExpr; }
			set { trueExpr = value; }
		}

		public ExprSingle FalseExpr {
			get { return falseExpr; }
			set { falseExpr = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			condition.CheckReference (compiler);
			trueExpr.CheckReference (compiler);
			falseExpr.CheckReference (compiler);
		}

#region CompileAndEvaluate
		SequenceType computedReturnType;

		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
//			for (int i = 0; i < Condition.Count; i++)
//				Condition [i] = Condition [i].Compile (compiler);
			condition = condition.Compile (compiler);
			// FIXME: check if condition is constant, and returns trueExpr or falseExpr
			TrueExpr = TrueExpr.Compile (compiler);
			FalseExpr = FalseExpr.Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get {
				if (Context == null)
					return SequenceType.AnyType;
				if (computedReturnType == null)
					computedReturnType = SequenceType.ComputeCommonBase (TrueExpr.StaticType, FalseExpr.StaticType);
				return computedReturnType;
			}
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
//			foreach (ExprSingle expr in Condition) {
//				if (expr.EvaluateAsBoolean (iter))
//					return TrueExpr.Evaluate (iter);
//			}
			if (condition.EvaluateAsBoolean (iter))
				return TrueExpr.Evaluate (iter);
			return FalseExpr.Evaluate (iter);
		}
#endregion

	}

	// logical expr

	internal abstract class BinaryOperationExpr : ExprSingle
	{
		protected BinaryOperationExpr (ExprSingle left, ExprSingle right)
		{
			this.left = left;
			this.right = right;
		}

		ExprSingle left, right;
		
		public ExprSingle Left {
			get { return left; }
			set { left = value; }
		}

		public ExprSingle Right{
			get { return right; }
			set { right = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			left.CheckReference (compiler);
			right.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Right = Right.Compile (compiler);
			return this;
		}
#endregion

	}

	internal class OrExpr : BinaryOperationExpr
	{
		public OrExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			base.CompileCore (compiler);
			// FIXME: check constant value and return true or false
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Boolean; }
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			return Left.EvaluateAsBoolean (iter) || Right.EvaluateAsBoolean (iter);
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (EvaluateAsBoolean (iter) ?AtomicTrue : AtomicFalse, iter);
		}

		/*
			- compiler -
			return leftExprBool (context) || rightExprBool (context);
		*/
#endregion
	}

	internal class AndExpr : BinaryOperationExpr
	{
		public AndExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			base.CompileCore (compiler);
			// FIXME: check constant value and return true or false
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Boolean; }
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			return Left.EvaluateAsBoolean (iter) && Right.EvaluateAsBoolean (iter);
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter);
		}

		/*
			- compiler -
			return leftExprBool (context) && rightExprBool (context);
		*/
#endregion
	}

	// TypeOperation expr

	internal abstract class TypeOperationExpr : ExprSingle
	{
		protected TypeOperationExpr (ExprSingle expr, SequenceType type)
		{
			this.expr = expr;
			this.type = type;
		}

		ExprSingle expr;
		SequenceType type;

		public ExprSingle Expr {
			get { return expr; }
			set { expr = value; }
		}

		public SequenceType TargetType {
			get { return type; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
			compiler.CheckSchemaType (type);
		}
	}

	internal abstract class AtomicTypeOperationExpr : ExprSingle
	{
		protected AtomicTypeOperationExpr (ExprSingle expr, XmlTypeCode type, bool optional)
		{
			this.expr = expr;
//			this.typeCode = type;
//			this.optional = optional;
			this.targetType = SequenceType.Create (type, optional ? Occurence.Optional : Occurence.One);
		}

		ExprSingle expr;
//		XmlTypeCode typeCode;
//		bool optional;
		SequenceType targetType;

		internal ExprSingle Expr {
			get { return expr; }
			set { expr = value; }
		}

/*
		public XmlTypeCode TypeCode {
			get { return typeCode; }
		}

		public bool Optional {
			get { return optional; }
		}
*/
		internal SequenceType TargetType {
			get { return targetType; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
		}
	}

	internal class InstanceOfExpr : TypeOperationExpr
	{
		public InstanceOfExpr (ExprSingle expr, SequenceType type)
			: base (expr, type)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			// FIXME: check return type and if it never matches then return false
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Boolean; }
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			bool occured = false;
			bool onlyOnce = (TargetType.Occurence == Occurence.One || TargetType.Occurence == Occurence.Optional);
			bool required = (TargetType.Occurence == Occurence.One || TargetType.Occurence == Occurence.OneOrMore);
			foreach (XPathItem item in iter) {
				if (occured && onlyOnce)
					return false;
				if (!TargetType.IsInstance (item))
					return false;
			}
			return occured || !required;
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter);
		}
#endregion
	}

	internal class TreatExpr : TypeOperationExpr
	{
		public TreatExpr (ExprSingle expr, SequenceType type)
			: base (expr, type)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			// FIXME: check return type and if it never matches then return false
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			if (TargetType.CanConvert (iter))
				return iter;
			else
				throw new XmlQueryException (String.Format ("Cannot treat as {1}", TargetType));
		}
#endregion
	}

	internal class CastableExpr : AtomicTypeOperationExpr
	{
		public CastableExpr (ExprSingle expr, XmlTypeCode atomicType, bool optional)
			: base (expr, atomicType, optional)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			// FIXME: check return type and if it never matches then return boolean
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Boolean; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (new XPathAtomicValue (EvaluateAsBoolean (iter), XmlSchemaSimpleType.XsBoolean), iter);
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			bool occured = false;
			bool onlyOnce = (TargetType.Occurence == Occurence.One || TargetType.Occurence == Occurence.Optional);
			bool required = (TargetType.Occurence == Occurence.One || TargetType.Occurence == Occurence.OneOrMore);
			foreach (XPathItem item in iter) {
				if (occured && onlyOnce)
					return false;
				if (!TargetType.CanConvert (item))
					return false;
			}
			return occured || !required;
		}
#endregion
	}

	internal class CastExpr : AtomicTypeOperationExpr
	{
		public CastExpr (ExprSingle expr, XmlTypeCode atomicType, bool optional)
			: base (expr, atomicType, optional)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			// FIXME: check return type and if it never matches then return boolean
			return this;
		}

		public override SequenceType StaticType {
			get { return TargetType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			if (TargetType.CanConvert (iter))
				return new ConvertingIterator (iter, TargetType);
			else
				throw new XmlQueryException (String.Format ("Cannot cast as {1}", TargetType));
		}
#endregion
	}

	// ComparisonExpr

	internal class ComparisonExpr : BinaryOperationExpr
	{
		public ComparisonExpr (ExprSingle left, ExprSingle right, ComparisonOperator oper)
			: base (left, right)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Right = Right.Compile (compiler);
			// FIXME: check return type and if it never matches then return boolean
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Boolean; }
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter);
		}
#endregion
	}

	public enum ComparisonOperator {
		ValueEQ,
		ValueNE,
		ValueLT,
		ValueLE,
		ValueGT,
		ValueGE,
		GeneralEQ,
		GeneralNE,
		GeneralLT,
		GeneralLE,
		GeneralGT,
		GeneralGE,
		NodeIs,
		NodeFWD,
		NodeBWD
	}

	// Range

	internal class RangeExpr : BinaryOperationExpr
	{
		public RangeExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Right = Right.Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.IntegerList; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			int start = Left.EvaluateAsInt (iter);
			int end = Right.EvaluateAsInt (iter);
			return new IntegerRangeIterator (iter, start, end);
		}

		public override void Serialize (XPathSequence iter)
		{
			int start = Left.EvaluateAsInt (iter);
			int end = Right.EvaluateAsInt (iter);
			for (int i = start; i <= end; i++) {
				iter.Context.Writer.WriteValue (i);
				if (i < end)
					iter.Context.Writer.WriteWhitespace (" ");
			}
		}
#endregion
	}

	// arithmetic operation expr

	public enum ArithmeticOperator {
		Plus,
		Minus,
		Mul,
		Div,
		IDiv,
		IMod
	}

	internal class ArithmeticOperationExpr : BinaryOperationExpr
	{
		public ArithmeticOperationExpr (ExprSingle left, ExprSingle right, ArithmeticOperator oper)
			: base (left, right)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Right = Right.Compile (compiler);
			return this;
		}

		// FIXME: It can be optimized by comparing l/r value types.
		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathAtomicValue lvalue = Left.EvaluateAsAtomic (iter);
			XPathAtomicValue rvalue = Right.EvaluateAsAtomic (iter);

			throw new NotImplementedException ();
		}
#endregion
	}

	internal class MinusExpr : ExprSingle
	{
		public MinusExpr (ExprSingle expr)
		{
			this.expr = expr;
		}

		ExprSingle expr;

		public ExprSingle Expr {
			get { return expr; }
			set { expr = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get { return Expr.StaticType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
#endregion
	}

	// aggregation expr

	public enum AggregationType {
		Union,
		Intersect,
		Except
	}

	internal class GroupExpr : BinaryOperationExpr
	{
		public GroupExpr (ExprSingle left, ExprSingle right, AggregationType aggrType)
			: base (left, right)
		{
			this.aggrType = aggrType;
		}

		AggregationType aggrType;

		public AggregationType AggregationType {
			get { return aggrType; }
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Right = Right.Compile (compiler);
			return this;
		}

		// FIXME: It can be optimized by comparing l/r value types.
		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		// only applicable against node-sets
		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence lvalue = Left.EvaluateOrdered (iter);
			XPathSequence rvalue = Right.EvaluateOrdered (iter);

			/*
			TBD (yield earlier node, skipping one of the same nodes)
				- or -
			TBD (yield earlier node, skipping non-intersection nodes)
				- or -
			TBD (yield earlier node, skipping both of the same nodes)
			*/
			throw new NotImplementedException ();
		}
#endregion
	}

	// validate expr

	internal class ValidateExpr : ExprSingle
	{
		XmlSchemaContentProcessing schemaMode;
		ExprSequence expr;

		public ValidateExpr (XmlSchemaContentProcessing schemaMode, ExprSequence expr)
		{
			this.schemaMode = schemaMode;
			this.expr = expr;
		}

		public ExprSequence Expr {
			get { return expr; }
		}

		public XmlSchemaContentProcessing SchemaMode {
			get { return schemaMode; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			for (int i = 0; i < expr.Count; i++)
				expr [i] = expr [i].Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			// TBD (see 3.13).
			throw new NotImplementedException ();
		}
#endregion
	}

	// Path expr

	internal abstract class PathExpr : ExprSingle
	{
	}

	// '/'
	internal class PathRootExpr : PathExpr
	{
		public PathRootExpr ()
		{
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Document; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			if (iter.MoveNext ())
				return new XPathEmptySequence (iter);
			XPathNavigator nav = iter.Current as XPathNavigator;
			if (nav == null)
				return new XPathEmptySequence (iter);
			nav = nav.Clone ();
			nav.MoveToRoot ();
			return new SingleItemIterator (nav, iter);
		}
#endregion
	}

	// 'foo/bar'
	internal class PathChildExpr : PathExpr
	{
		ExprSingle left;
		ExprSingle next;

		public PathChildExpr (ExprSingle left, ExprSingle next)
		{
			this.left = left;
			this.next = next;
		}

		public ExprSingle Left {
			get { return left; }
			set { left = value; }
		}

		public ExprSingle Next {
			get { return next; }
			set { next = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			left.CheckReference (compiler);
			next.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Next = Next.Compile (compiler);
			return this;
		}

		// FIXME: It can be optimized by comparing l/r value types.
		public override SequenceType StaticType {
			get { return SequenceType.Node; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new ChildPathIterator (iter, this);
		}
#endregion
	}

	// 'foo//bar'
	internal class PathDescendantExpr : PathExpr
	{
		ExprSingle left;
		ExprSingle descendant;

		public PathDescendantExpr (ExprSingle left, ExprSingle descendant)
		{
			this.left = left;
			this.descendant = descendant;
		}

		public ExprSingle Left {
			get { return left; }
			set { left = value; }
		}

		public ExprSingle Descendant {
			get { return descendant; }
			set { descendant = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			left.CheckReference (compiler);
			descendant.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Left = Left.Compile (compiler);
			Descendant = Descendant.Compile (compiler);
			return this;
		}

		// FIXME: It can be optimized by comparing l/r value types.
		public override SequenceType StaticType {
			get { return SequenceType.Node; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new DescendantPathIterator (iter, this);
		}
#endregion
	}

	internal class AxisStepExpr : PathExpr
	{
		static AxisStepExpr parentStep;

		static AxisStepExpr ()
		{
			parentStep = new AxisStepExpr (XPathAxis.Parent, null);
		}

		public static AxisStepExpr ParentStep {
			get { return parentStep; }
		}

		public AxisStepExpr (XPathAxis axis, XPath2NodeTest test)
		{
			this.axis = axis;
			if (test == null)
				nameTest = XmlQualifiedName.Empty;
			else {
				if (test.NameTest != null)
					this.nameTest = test.NameTest;
				else
					this.kindTest = test.KindTest;
			}
		}

		XPathAxis axis;
		XmlQualifiedName nameTest;
		KindTest kindTest;

		public XPathAxis Axis {
			get { return axis; }
		}

		public XmlQualifiedName NameTest {
			get { return nameTest; }
			set { nameTest = value; }
		}

		public KindTest KindTest {
			get { return kindTest; }
			set { kindTest = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (KindTest != null)
				KindTest.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (KindTest != null)
				KindTest.Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get {
				switch (Axis.AxisType) {
				case XPathAxisType.Attribute:
					return SequenceType.Attribute;
				case XPathAxisType.Namespace:
					return SequenceType.Namespace;
				}
				// FIXME: we can more filtering by KindTest
				return SequenceType.Node;
			}
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence argIter = null;
			switch (Axis.AxisType) {
			case XPathAxisType.Child:
				argIter = new ChildIterator (iter); break;
			case XPathAxisType.Descendant:
				argIter = new DescendantIterator (iter); break;
			case XPathAxisType.Attribute:
				argIter = new AttributeIterator (iter); break;
			case XPathAxisType.Self:
				argIter = new SingleItemIterator (iter.Current, iter); break;
			case XPathAxisType.DescendantOrSelf:
				argIter = new DescendantOrSelfIterator (iter); break;
			case XPathAxisType.FollowingSibling:
				argIter = new FollowingSiblingIterator (iter); break;
			case XPathAxisType.Following:
				argIter = new FollowingIterator (iter); break;
			case XPathAxisType.Parent:
				argIter = new ParentIterator (iter); break;
			case XPathAxisType.Ancestor:
				argIter = new AncestorIterator (iter); break;
			case XPathAxisType.PrecedingSibling:
				argIter = new PrecedingSiblingIterator (iter); break;
			case XPathAxisType.Preceding:
				argIter = new PrecedingIterator (iter); break;
			case XPathAxisType.AncestorOrSelf:
				argIter = new AncestorOrSelfIterator (iter); break;
			case XPathAxisType.Namespace: // only applicable under XPath 2.0: not XQuery 1.0
				argIter = new NamespaceIterator (iter); break;
			}
			return new AxisIterator (argIter, this);
		}

		internal bool Matches (XPathNavigator nav)
		{
			if (nameTest != null)
				return nameTest == XmlQualifiedName.Empty || 
					((nameTest.Name == nav.LocalName || nameTest.Name == "*") &&
					(nameTest.Namespace == nav.NamespaceURI || nameTest.Namespace == "*"));
			else
				return kindTest.Matches (nav);
		}
#endregion
	}

	internal class FilterStepExpr : PathExpr
	{
		public FilterStepExpr (ExprSingle expr, PredicateList predicates)
		{
			this.expr = expr;
			this.predicates = predicates;
		}

		ExprSingle expr;
		PredicateList predicates;

		public ExprSingle Expr {
			get { return expr; }
			set { expr = value; }
		}

		public PredicateList Predicates {
			get { return predicates; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
			foreach (ExprSequence seq in predicates)
				seq.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			foreach (ExprSequence seq in Predicates)
				for (int i = 0; i < seq.Count; i++)
					seq [i] = seq [i].Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get { return Expr.StaticType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new FilteredIterator (iter, this);
		}
#endregion
	}

	// predicates == exprsequence list == list of list of exprsingle
	internal class PredicateList : CollectionBase
	{
		public void Add (ExprSequence expr)
		{
			List.Add (expr);
		}

		public void Insert (int pos, ExprSequence expr)
		{
			List.Insert (pos, expr);
		}

		public ExprSequence this [int i] {
			get { return (ExprSequence) List [i]; }
		}
	}

	internal class XPath2NodeTest
	{
		public XPath2NodeTest (XmlQualifiedName nameTest)
		{
			this.NameTest = nameTest;
		}
		
		public XPath2NodeTest (KindTest kindTest)
		{
			this.KindTest = kindTest;
		}

		public XmlQualifiedName NameTest;

		public KindTest KindTest;
	}

	internal class EnclosedExpr : ExprSingle
	{
		ExprSequence expr;

		public EnclosedExpr (ExprSequence expr)
		{
			this.expr = expr;
		}

		public ExprSequence Expr {
			get { return expr; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Expr.Count == 0)
				return Expr [0].Compile (compiler);
			for (int i = 0; i < Expr.Count; i++)
				Expr [i] = Expr [i].Compile (compiler);
			return this;
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new ExprSequenceIterator (iter, Expr);
		}
#endregion
	}

	// PrimaryExpr

	internal abstract class PrimaryExpr : ExprSingle
	{
		internal override void CheckReference (XQueryASTCompiler compiler)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			return this;
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (EvaluateAsAtomic (iter), iter);
		}
#endregion
	}

	internal class StringLiteralExpr : PrimaryExpr
	{
		string literal;

		public StringLiteralExpr (string literal)
		{
			this.literal = literal;
		}

		public string Literal {
			get { return literal; }
		}

#region CompileAndEvaluate
		XmlSchemaSimpleType stringType = XmlSchemaType.GetBuiltInSimpleType (new XmlQualifiedName ("string", XmlSchema.Namespace));

		public override SequenceType StaticType {
			get { return SequenceType.AtomicString; }
		}

		public override string EvaluateAsString (XPathSequence iter)
		{
			return Literal;
		}

		public override XPathAtomicValue EvaluateAsAtomic (XPathSequence iter)
		{
			return new XPathAtomicValue (Literal, stringType);
		}
#endregion
	}

	internal class DecimalLiteralExpr : PrimaryExpr
	{
		decimal value;

		public DecimalLiteralExpr (decimal value)
		{
			this.value = value;
		}

		public decimal Value {
			get { return value; }
		}

#region CompileAndEvaluate
		XmlSchemaSimpleType decimalType = XmlSchemaType.GetBuiltInSimpleType (new XmlQualifiedName ("decimal", XmlSchema.Namespace));

		public override SequenceType StaticType {
			get { return SequenceType.Decimal; }
		}

		public override XPathAtomicValue EvaluateAsAtomic (XPathSequence iter)
		{
			return new XPathAtomicValue (Value, decimalType);
		}
#endregion
	}

	internal class DoubleLiteralExpr : PrimaryExpr
	{
		double value;

		public DoubleLiteralExpr (double value)
		{
			this.value = value;
		}

		public double Value {
			get { return value; }
		}

#region CompileAndEvaluate
		XmlSchemaSimpleType doubleType = XmlSchemaType.GetBuiltInSimpleType (new XmlQualifiedName ("double", XmlSchema.Namespace));

		public override SequenceType StaticType {
			get { return SequenceType.Double; }
		}

		public override XPathAtomicValue EvaluateAsAtomic (XPathSequence iter)
		{
			return new XPathAtomicValue (Value, doubleType);
		}
#endregion
	}

	internal class VariableReferenceExpr : PrimaryExpr
	{
		XmlQualifiedName varName;

		public VariableReferenceExpr (XmlQualifiedName varName)
		{
			this.varName = varName;
		}

		public XmlQualifiedName VariableName {
			get { return varName; }
		}

		// FIXME: variable name must be stacked in any area 
		// whereever variables are defined.
		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			compiler.CheckVariableName (varName);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			// FIXME: try to resolve static context variable and return the actual value expression
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence variable = iter.Context.ResolveVariable (VariableName, iter);
			// FIXME: if Evaluate() accepts XPathSequence, then XPathSequence must be public class (to make IXPath2Variable public).
			return variable;
		}
#endregion
	}

	internal class ParenthesizedExpr : PrimaryExpr
	{
		ExprSequence expr;

		public ParenthesizedExpr (ExprSequence expr)
		{
			this.expr = expr;
		}

		ExprSequence Expr {
			get { return expr; }
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Expr.Count == 1)
				return Expr [0].Compile (compiler);
			for (int i = 0; i < Expr.Count; i++)
				Expr [i] = Expr [i].Compile (compiler);
			return this;
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new ExprSequenceIterator (iter, Expr);
		}
#endregion
	}

	// "."
	internal class ContextItemExpr : PrimaryExpr
	{
		public ContextItemExpr ()
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.AnyType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (iter.Context.CurrentItem, iter);
		}
#endregion
	}

	internal abstract class FunctionCallExprBase : PrimaryExpr
	{
		XmlQualifiedName name;
		ExprSequence args;

		public FunctionCallExprBase (XmlQualifiedName name, ExprSequence args)
		{
			if (args == null)
				throw new ArgumentNullException (String.Format ("Function argument expressions for {0} is null.", name));
			this.name = name;
			this.args = args;
		}

		public XmlQualifiedName Name {
			get { return name; }
		}

		public ExprSequence Args {
			get { return args; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			compiler.CheckFunctionName (name);
		}

#region CompileAndEvaluate
		/*
		internal static FunctionCallExpr Create (
			XmlQualifiedName name,
			ExprSingle [] args,
			XQueryStaticContext ctx)
		{
			switch (name.Namespace) {
			case XQueryFunction.Namespace:
				switch (name.Name) {
				case "node-name":
					return new FnNodeNameCall (ctx, args);
				case "nilled":
					return new FnNilledCall (ctx, args);
				case "string":
					return new FnStringCall (ctx, args);
				case "data":
					return new FnDataCall (ctx, args);
				case "base-uri":
					return new FnBaseUriCall (ctx, args);
				case "document-uri":
					return new FnDocumentUriCall (ctx, args);
				case "error":
					return new FnErrorCall (ctx, args);
				case "trace":
					return new FnTraceCall (ctx, args);
				case "abs":
					return new FnAbsCall (ctx, args);
				case "ceiling":
					return new FnCeilingCall (ctx, args);
				case "floor":
					return new FnFloorCall (ctx, args);
				case "round":
					return new FnRoundCall (ctx, args);
				case "round-half-to-even":
					return new FnRoundHalfToEvenCall (ctx, args);
				case "codepoints-to-string":
					return new FnCodepointsToStringCall (ctx, args);
				case "string-to-codepoints":
					return new FnStringCallToCodepointsCall (ctx, args);
				}
				goto default;
			case XmlSchema.XdtNamespace:
			case XmlSchema.Namespace:
				XmlSchemaType type = XmlSchemaType.GetBuiltInSimpleType (name);
				if (type != null)
					return new AtomicConstructorCall (ctx, SequenceType.Create (type, Occurence.One), args);
				type = XmlSchemaType.GetBuiltInComplexType (name);
				if (type == null)
					goto default;
				return null;
			default:
				XQueryFunction func = ctx.CompileContext.InEffectFunctions [name];
				if (func != null)
					return new CustomFunctionCallExpression (ctx, args, func);
				return null;
			}
		}
		*/

		internal void CheckArguments (XQueryASTCompiler compiler)
		{
			if (args.Count < MinArgs || args.Count > MaxArgs)
				// FIXME: add more info
				throw new XmlQueryCompileException (String.Format ("{0} is invalid for the number of {1} function argument. MinArgs = {2}, MaxArgs = {3}.", args.Count, name, MinArgs, MaxArgs));
		}

		public abstract int MinArgs { get; }
		public abstract int MaxArgs { get; }
#endregion
	}

	// This class is used only in AST
	internal class FunctionCallExpr : FunctionCallExprBase
	{
		public FunctionCallExpr (XmlQualifiedName name, ExprSequence args)
			: base (name, args)
		{
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			// resolve function
			return new CustomFunctionCallExpr (Args, compiler.ResolveFunction (Name)).Compile (compiler);
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new InvalidOperationException ("XQuery internal error. Should not happen.");
		}

		public override SequenceType StaticType {
			get { throw new InvalidOperationException ("XQuery internal error. Should not happen."); }
		}

		public override int MinArgs {
			get { throw new InvalidOperationException ("XQuery internal error. Should not happen."); }
		}

		public override int MaxArgs {
			get { throw new InvalidOperationException ("XQuery internal error. Should not happen."); }
		}

#endregion
	}

#region CompileAndEvaluate

	// It is instantiated per function call expression.
	// (e.g. the example below contains 3 FunctionCallExpression instances:
	// "replace(node-name (node-before(/*)), 'foo', node-name($var))"
	internal class CustomFunctionCallExpr : FunctionCallExprBase
	{
		public CustomFunctionCallExpr (ExprSequence args, XQueryFunction function)
			: base (function.Name, args)
		{
			this.function = function;
		}

		XQueryFunction function;

		public XQueryFunction Function {
			get { return function; }
		}

		public override int MinArgs {
			get { return function.MinArgs; }
		}

		public override int MaxArgs {
			get { return function.MaxArgs; }
		}

		public override SequenceType StaticType {
			get { return function.ReturnType; }
		}

		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			CheckArguments (compiler);
			return this;
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return Function.Evaluate (iter, Args);
		}

		// FIXME: add all overrides that delegates to XQueryFunction
	}
#endregion

	// Ordered / Unordered
	internal class OrderSpecifiedExpr : ExprSingle
	{
		bool ordered;
		ExprSequence expr;
		
		public OrderSpecifiedExpr (ExprSequence expr, bool ordered)
		{
			this.ordered = ordered;
			this.expr = expr;
		}

		public ExprSequence Expr {
			get { return expr; }
		}

		public bool Ordered {
			get { return ordered; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
		}

#region CompileAndEvaluate
		public override SequenceType StaticType {
			// FIXME: could be optimized by checking all the expressions
			get { return SequenceType.AnyType; }
		}

		public override bool RequireSorting {
			get { return Ordered; }
		}

		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			for (int i = 0; i < Expr.Count; i++)
				Expr [i] = Expr [i].Compile (compiler);
			return this;
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
#endregion
	}
}

#endif
