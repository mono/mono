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
				w.WriteNode (nav, false);
			} else
				w.WriteString (item.Value);
		}

		// get EBV (fn:boolean())
		public virtual bool EvaluateAsBoolean (XPathSequence iter)
		{
			XPathSequence result = Evaluate (iter);
			if (!result.MoveNext ())
				return false;
			XPathItem v = result.Current;
			if (v is XPathNavigator)
				return true;
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
			case XmlTypeCode.Decimal:
				return v.ValueAsDecimal != 0;
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
					return new XPathAtomicValue (nav.Value, XmlSchemaSimpleType.XdtUntypedAtomic);
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
			if (nav != null) {
				// FIXME: is it really always untypedAtomic?
				// It might be complex content.
				XmlSchemaType type = nav.SchemaInfo == null ? XmlSchemaSimpleType.XdtUntypedAtomic : nav.SchemaInfo.SchemaType;
				return new XPathAtomicValue (nav.TypedValue, type);
			}
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
			if (whereClause != null)
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
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter.Context);
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			return EvaluateQuantification (iter, BodyList.GetEnumerator ());
		}

		private bool EvaluateQuantification (XPathSequence iter, IEnumerator bodies)
		{
			if (bodies.MoveNext ()) {
				QuantifiedExprBody qb = bodies.Current as QuantifiedExprBody;
				XPathSequence seq = qb.Expression.Evaluate (iter);
				bool passed = false;
				foreach (XPathItem item in seq) {
					passed = true;
					// FIXME: consider qb.Type
					try {
						iter.Context.PushVariable (qb.VarName, item);
						if (EvaluateQuantification (iter, bodies)) {
							if (!Every)
								return true;
						}
						else if (Every)
							return false;
					} finally {
						iter.Context.PopVariable ();
					}
				}
				return passed;
			}
			return Satisfies.EvaluateAsBoolean (iter);
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
			return new SingleItemIterator (EvaluateAsBoolean (iter) ?AtomicTrue : AtomicFalse, iter.Context);
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
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter.Context);
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
			this.targetType = SequenceType.Create (type, optional ? Occurence.Optional : Occurence.One);
		}

		ExprSingle expr;
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
			return new SingleItemIterator (EvaluateAsBoolean (iter) ? AtomicTrue : AtomicFalse, iter.Context);
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
			return new SingleItemIterator (new XPathAtomicValue (EvaluateAsBoolean (iter), XmlSchemaSimpleType.XsBoolean), iter.Context);
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
			this.oper = oper;
		}

		ComparisonOperator oper;

		public ComparisonOperator Operation {
			get { return oper; }
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

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			bool isEmpty;
			bool result = EvaluateAsBoolean (iter, out isEmpty);
			if (isEmpty)
				return new XPathEmptySequence (iter.Context);
			return new SingleItemIterator (result ? AtomicTrue : AtomicFalse, iter.Context);
		}

		public override bool EvaluateAsBoolean (XPathSequence iter)
		{
			bool isEmpty;
			return EvaluateAsBoolean (iter, out isEmpty);
		}

		private bool EvaluateAsBoolean (XPathSequence iter, out bool isEmpty)
		{
			XPathSequence lseq, rseq;
			isEmpty = false;

			switch (Operation) {
			// FIXME: it is curious but currently gmcs requires full typename.
			case Mono.Xml.XPath2.ComparisonOperator.ValueEQ:
			case Mono.Xml.XPath2.ComparisonOperator.ValueNE:
			case Mono.Xml.XPath2.ComparisonOperator.ValueLT:
			case Mono.Xml.XPath2.ComparisonOperator.ValueLE:
			case Mono.Xml.XPath2.ComparisonOperator.ValueGT:
			case Mono.Xml.XPath2.ComparisonOperator.ValueGE:
				XPathItem itemVL = ExamineOneItem (Left.Evaluate (iter));
				XPathItem itemVR = ExamineOneItem (Right.Evaluate (iter));
				if (itemVL == null || itemVR == null) {
					isEmpty = true;
					return false;
				}
				return CompareAtomic (itemVL, itemVR);

			case Mono.Xml.XPath2.ComparisonOperator.GeneralEQ:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralNE:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralLT:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralLE:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralGT:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralGE:
				lseq = Left.Evaluate (iter);
				rseq = Right.Evaluate (iter);
				foreach (XPathItem itemGL in lseq) {
					foreach (XPathItem itemGR in rseq.Clone ()) {
						if (CompareAtomic (itemGL, itemGR))
							return true;
					}
				}
				return false;

			case Mono.Xml.XPath2.ComparisonOperator.NodeIs:
			case Mono.Xml.XPath2.ComparisonOperator.NodeFWD:
			case Mono.Xml.XPath2.ComparisonOperator.NodeBWD:
				XPathNavigator lnav = ExamineOneNode (Left.Evaluate (iter));
				XPathNavigator rnav = ExamineOneNode (Right.Evaluate (iter));
				if (lnav == null || rnav == null) {
					isEmpty = true;
					return false;
				}
				switch (Operation) {
				case Mono.Xml.XPath2.ComparisonOperator.NodeIs:
					return lnav.IsSamePosition (rnav);
				case Mono.Xml.XPath2.ComparisonOperator.NodeFWD:
					return lnav.ComparePosition (rnav) == XmlNodeOrder.Before;
				case Mono.Xml.XPath2.ComparisonOperator.NodeBWD:
					return lnav.ComparePosition (rnav) == XmlNodeOrder.After;
				}
				break;
			}
			throw new SystemException ("XQuery internal error: should not happen.");
		}

		// returns null if sequence was empty
		private XPathItem ExamineOneItem (XPathSequence seq)
		{
			if (!seq.MoveNext ())
				return null;
			XPathItem item = seq.Current;
			if (seq.MoveNext ())
				throw new XmlQueryException ("Operand of value comparison expression must be evaluated as a sequence that contains exactly one item.");
			return item;
		}

		// returns null if sequence was empty
		private XPathNavigator ExamineOneNode (XPathSequence seq)
		{
			if (!seq.MoveNext ())
				return null;
			XPathNavigator nav = seq.Current as XPathNavigator;
			if (nav == null || seq.MoveNext ())
				throw new XmlQueryException ("Operand of node comparison expression must be evaluated as a sequence that contains exactly one node.");
			return nav;
		}

		private bool CompareAtomic (XPathItem itemL, XPathItem itemR)
		{
			XmlSchemaSimpleType ua = XmlSchemaSimpleType.XdtUntypedAtomic;
			XmlSchemaSimpleType str = XmlSchemaSimpleType.XsString;
			// FIXME: XPathNavigator might be complex content.
			bool uaL = itemL.XmlType == null || itemL.XmlType == ua;
			bool uaR = itemR.XmlType == null || itemR.XmlType == ua;
			bool bothUA = uaL && uaR;
			XPathAtomicValue avL =
				(uaL) ?
				bothUA ? new XPathAtomicValue (itemL.Value, str) :
				new XPathAtomicValue (itemL.Value, itemR.XmlType) :
				Atomize (itemL);
			XPathAtomicValue avR =
				uaR ?
				bothUA ? new XPathAtomicValue (itemR.Value, str) :
				new XPathAtomicValue (itemR.Value, itemL.XmlType) :
				Atomize (itemR);

			switch (Operation) {
			// FIXME: it is curious but currently gmcs requires full typename.
			case Mono.Xml.XPath2.ComparisonOperator.ValueEQ:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralEQ:
				return XQueryComparisonOperator.ValueEQ (avL, avR);
			case Mono.Xml.XPath2.ComparisonOperator.ValueNE:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralNE:
				return XQueryComparisonOperator.ValueNE (avL, avR);
			case Mono.Xml.XPath2.ComparisonOperator.ValueLT:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralLT:
				return XQueryComparisonOperator.ValueLT (avL, avR);
			case Mono.Xml.XPath2.ComparisonOperator.ValueLE:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralLE:
				return XQueryComparisonOperator.ValueLE (avL, avR);
			case Mono.Xml.XPath2.ComparisonOperator.ValueGT:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralGT:
				return XQueryComparisonOperator.ValueGT (avL, avR);
			case Mono.Xml.XPath2.ComparisonOperator.ValueGE:
			case Mono.Xml.XPath2.ComparisonOperator.GeneralGE:
				return  XQueryComparisonOperator.ValueGE (avL, avR);
			}
			return false; // should not happen
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
			return new IntegerRangeIterator (iter.Context, start, end);
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
		Add,
		Sub,
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
			this.oper = oper;
		}

		ArithmeticOperator oper;

		public ArithmeticOperator Operation {
			get { return oper; }
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
			XPathSequence lseq = Left.Evaluate (iter);
			if (!lseq.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			XPathSequence rseq = Right.Evaluate (iter);
			if (!rseq.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			XPathAtomicValue lvalue = Atomize (lseq.Current);
			XPathAtomicValue rvalue = Atomize (rseq.Current);
			if (lseq.MoveNext ())
				throw new XmlQueryException ("XP0006: Left operand resulted in an sequence that contains more than one item.");
			if (rseq.MoveNext ())
				throw new XmlQueryException ("XP0006: Left operand resulted in an sequence that contains more than one item.");

			// FIXME: handle "untypedAtomic to xs:double" casting

			return new SingleItemIterator (Compute (lvalue, rvalue), iter.Context);
		}

		private XPathAtomicValue Compute (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			switch (Operation) {
			case ArithmeticOperator.Add:
				return XQueryArithmeticOperator.Add (lvalue, rvalue);
			case ArithmeticOperator.Sub:
				return XQueryArithmeticOperator.Subtract (lvalue, rvalue);
			case ArithmeticOperator.Mul:
				return XQueryArithmeticOperator.Multiply (lvalue, rvalue);
			case ArithmeticOperator.Div:
				return XQueryArithmeticOperator.Divide (lvalue, rvalue);
			case ArithmeticOperator.IDiv:
				return XQueryArithmeticOperator.IntDivide (lvalue, rvalue);
			case ArithmeticOperator.IMod:
				return XQueryArithmeticOperator.Remainder (lvalue, rvalue);
			default:
				throw new SystemException ("XQuery internal error: should not happen.");
			}
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
			return new ArithmeticOperationExpr (new DecimalLiteralExpr (-1), Expr, ArithmeticOperator.Mul).Compile (compiler);
		}

		public override SequenceType StaticType {
			get { return Expr.StaticType; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new SystemException ("XQuery internal error: should not happen.");
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
			return new GroupIterator (iter, this);
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
			XPathNavigator nav = iter.Context.CurrentItem as XPathNavigator;
			if (nav == null)
				throw new XmlQueryException ("Context item is not a node when evaluating expression '/'.");
			nav = nav.Clone ();
			nav.MoveToRoot ();
			return new SingleItemIterator (nav, iter.Context);
		}
#endregion
	}

	internal abstract class PathStepExpr : PathExpr
	{
		ExprSingle first;
		ExprSingle next;

		public PathStepExpr (ExprSingle first, ExprSingle next)
		{
			this.first = first;
			this.next = next;
		}

		public ExprSingle First {
			get { return first; }
			set { first = value; }
		}

		public ExprSingle Next {
			get { return next; }
			set { next = value; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			first.CheckReference (compiler);
			next.CheckReference (compiler);
		}

		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			first = first.Compile (compiler);
			next = next.Compile (compiler);
			return this;
		}

	}

	// 'foo/bar'
	internal class PathSlashExpr : PathStepExpr
	{
		public PathSlashExpr (ExprSingle first, ExprSingle next)
			: base (first, next)
		{
		}

#region CompileAndEvaluate
		// FIXME: It can be optimized by comparing l/r value types.
		public override SequenceType StaticType {
			get { return SequenceType.Node; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new PathStepIterator (First.Evaluate (iter), this);
		}
#endregion
	}

	// 'foo//bar'
	internal class PathSlash2Expr : PathStepExpr
	{
		public PathSlash2Expr (ExprSingle first, ExprSingle next)
			: base (first, next)
		{
		}

#region CompileAndEvaluate
		// FIXME: It can be optimized by comparing l/r value types.
		public override SequenceType StaticType {
			get { return SequenceType.Node; }
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence seq = First.Evaluate (iter);
			if (!seq.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			return new PathStepIterator (
				new DescendantOrSelfIterator (seq.Current as XPathNavigator, seq.Context), this);
		}
#endregion
	}

	internal class AxisStepExpr : PathExpr
	{
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
			XQueryContext ctx = iter.Context;

			if (iter.Position == 0) {
				iter = iter.Clone ();
				if (!iter.MoveNext ())
					return new XPathEmptySequence (iter.Context);
			}

			XPathNavigator nav = iter.Current as XPathNavigator;
			if (nav == null)
				throw new XmlQueryException ("Node set is expected.");

			NodeIterator argIter = null;

			switch (Axis.AxisType) {
			case XPathAxisType.Child:
				argIter = new ChildIterator (nav, ctx); break;
			case XPathAxisType.Descendant:
				argIter = new DescendantIterator (nav, ctx); break;
			case XPathAxisType.Attribute:
				argIter = new AttributeIterator (nav, ctx); break;
			case XPathAxisType.Self:
				argIter = new SelfIterator (nav, ctx); break;
			case XPathAxisType.DescendantOrSelf:
				argIter = new DescendantOrSelfIterator (nav, ctx); break;
			case XPathAxisType.FollowingSibling:
				argIter = new FollowingSiblingIterator (nav, ctx); break;
			case XPathAxisType.Following:
				argIter = new FollowingIterator (nav, ctx); break;
			case XPathAxisType.Parent:
				argIter = new ParentIterator (nav, ctx); break;
			case XPathAxisType.Ancestor:
				argIter = new AncestorIterator (nav, ctx); break;
			case XPathAxisType.PrecedingSibling:
				argIter = new PrecedingSiblingIterator (nav, ctx); break;
			case XPathAxisType.Preceding:
				argIter = new PrecedingIterator (nav, ctx); break;
			case XPathAxisType.AncestorOrSelf:
				argIter = new AncestorOrSelfIterator (nav, ctx); break;
			case XPathAxisType.Namespace: // only applicable under XPath 2.0: not XQuery 1.0
				argIter = new NamespaceIterator (nav, ctx); break;
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
		public FilterStepExpr (ExprSingle expr, ExprSequence predicate)
		{
			this.expr = expr;
			this.predicate = predicate;
		}

		ExprSingle expr;
		ExprSequence predicate;

		public ExprSingle Expr {
			get { return expr; }
			set { expr = value; }
		}

		public ExprSequence Predicate {
			get { return predicate; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			expr.CheckReference (compiler);
			predicate.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			Expr = Expr.Compile (compiler);
			for (int i = 0; i < predicate.Count; i++)
				predicate [i] = predicate [i].Compile (compiler);
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

/*
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
*/

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
			return new SingleItemIterator (EvaluateAsAtomic (iter), iter.Context);
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
			XPathSequence variable = iter.Context.ResolveVariable (VariableName);
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
			if (expr == null)
				expr = new ExprSequence ();
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
			switch (Expr.Count) {
			case 0:
				return new XPathEmptySequence (iter.Context);
			case 1:
				return Expr [0].Evaluate (iter);
			default:
				return new ExprSequenceIterator (iter, Expr);
			}
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
			return new SingleItemIterator (iter.Context.CurrentItem, iter.Context);
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
		internal static DefaultFunctionCall Create (
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

	internal class FunctionCallExpr : FunctionCallExprBase
	{
		public FunctionCallExpr (XmlQualifiedName name, ExprSequence args)
			: base (name, args)
		{
		}

		XQueryFunction function;

		public XQueryFunction Function {
			get { return function; }
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			// resolve function
			function = compiler.ResolveFunction (Name);
			CheckArguments (compiler);
			for (int i = 0; i < Args.Count; i++)
				Args [i] = Args [i].Compile (compiler);
			return this;
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

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return Function.Evaluate (iter, Args);
		}

		// FIXME: add all overrides that delegates to XQueryFunction
#endregion
	}

/*
#region CompileAndEvaluate

	// It is instantiated per function call expression.
	// (e.g. the example below contains 4 FunctionCallExpression instances:
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
			for (int i = 0; i < Args.Count; i++)
				Args [i] = Args [i].Compile (compiler);
			return this;
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return Function.Evaluate (iter, Args);
		}

		// FIXME: add all overrides that delegates to XQueryFunction
	}
#endregion
*/

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
