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
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Mono.Xml.XPath2
{
	public class ExprSequence
	{
		ArrayList list = new ArrayList ();

		public ExprSequence ()
		{
		}

		public void Add (ExprSingle expr)
		{
			list.Add (expr);
		}

		public void Insert (int pos, ExprSingle expr)
		{
			list.Insert (pos, expr);
		}

		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public abstract class ExprSingle
	{
	}

	// FLWORExpr

	public class FLWORExpr : ExprSingle
	{
		public FLWORExpr (ForLetClauseCollection forlet, ExprSequence whereClause, OrderSpecList orderBy, ExprSingle ret)
		{
			this.fl = forlet;
			this.whereClause = whereClause;
			this.orderBy = orderBy;
			this.ret = ret;
		}

		ForLetClauseCollection fl;
		ExprSequence whereClause;
		OrderSpecList orderBy;
		ExprSingle ret;

		public ForLetClauseCollection ForLetClauses {
			get { return fl; }
		}

		public ExprSequence WhereClause {
			get { return whereClause; }
		}

		public OrderSpecList OrderBy {
			get { return orderBy; }
		}

		public ExprSingle ReturnExpr {
			get { return ret; }
		}
	}

	public class ForLetClauseCollection
	{
		ArrayList list = new ArrayList ();

		public void Add (ForLetClause clause)
		{
			list.Add (clause);
		}

		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public class OrderSpecList
	{
		bool isStable;
		ArrayList list = new ArrayList ();

		public OrderSpecList ()
		{
		}

		public bool IsStable {
			get { return isStable; }
			set { isStable = value; }
		}

		public void Add (OrderSpec spec)
		{
			list.Add (spec);
		}

		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public class OrderSpec
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

	public class OrderModifier
	{
		public OrderModifier (XmlSortOrder order, XmlSortOrder emptyOrder, string collation)
		{
			this.sortOrder = sortOrder;
			this.emptyOrder = emptyOrder;
			this.coll = collation;
		}

		XmlSortOrder sortOrder;
		XmlSortOrder emptyOrder;
		string coll;

		public XmlSortOrder SortOrder{
			get { return sortOrder; }
		}

		public XmlSortOrder EmptyOrder {
			get { return emptyOrder; }
		}

		public string Collation {
			get { return coll; }
		}
	}

	public class ForLetClause
	{
		ArrayList list = new ArrayList ();

		protected ArrayList List {
			get { return list; }
		}

		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public class ForClause : ForLetClause
	{
		public ForClause ()
		{
		}

		public void Add (ForSingleBody body)
		{
			List.Add (body);
		}
	}

	public class LetClause : ForLetClause
	{
		public LetClause ()
		{
		}

		public void Add (LetSingleBody body)
		{
			List.Add (body);
		}
	}

	public class ForSingleBody
	{
		public ForSingleBody (XmlQualifiedName varName, SequenceType type, XmlQualifiedName positionalVar, ExprSingle expr)
		{
			this.varName = varName;
			this.type = type;
			this.positionalVar = positionalVar;
			this.expr = expr;
		}

		XmlQualifiedName varName;
		XmlQualifiedName positionalVar;
		SequenceType type;
		ExprSingle expr;

		public XmlQualifiedName VarName {
			get { return varName; }
		}

		public SequenceType ReturnType {
			get { return type; }
		}

		public XmlQualifiedName PositionalVar {
			get { return positionalVar; }
		}

		public ExprSingle Expression {
			get { return expr; }
		}
	}

	public class LetSingleBody
	{
		public LetSingleBody (XmlQualifiedName varName, SequenceType type, ExprSingle expr)
		{
			this.varName = varName;
			this.type = type;
			this.expr = expr;
		}

		XmlQualifiedName varName;
		SequenceType type;
		ExprSingle expr;

		public XmlQualifiedName VarName {
			get { return varName; }
			set { varName = value; }
		}

		public SequenceType ReturnType {
			get { return type; }
			set { type =value; }
		}

		public ExprSingle Expression {
			get { return expr; }
			set { expr = value; }
		}
	}

	// QuantifiedExpr

	public class QuantifiedExpr : ExprSingle
	{
		public QuantifiedExpr (bool every, QuantifiedExprBodyList body, ExprSingle satisfies)
		{
		}
	}

	public class QuantifiedExprBodyList
	{
		ArrayList list = new ArrayList ();
		
		public QuantifiedExprBodyList ()
		{
		}

		public void Add (QuantifiedExprBody body)
		{
			list.Add (body);
		}

		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public class QuantifiedExprBody
	{
		private XmlQualifiedName varName;
		private SequenceType typeDeclaration;
		private ExprSingle expr;

		public QuantifiedExprBody (XmlQualifiedName varName,
			SequenceType typeDeclaration,
			ExprSingle expr)
		{
			this.varName = varName;
			this.typeDeclaration = typeDeclaration;
			this.expr = expr;
		}

		public XmlQualifiedName VarName {
			get { return varName; }
		}

		public SequenceType TypeDeclaration {
			get { return typeDeclaration; }
		}

		public ExprSingle Expression {
			get { return expr; }
		}
	}

	// TypeswitchExpr

	public class TypeswitchExpr : ExprSingle
	{
		public TypeswitchExpr (ExprSequence switchExpr, CaseClauseList caseList, XmlQualifiedName variableSpecName, ExprSingle defaultReturn)
		{
		}
	}

	public class CaseClauseList : CollectionBase
	{
		ArrayList list = new ArrayList ();

		public void Add (CaseClause cc)
		{
			list.Add (cc);
		}

		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public class CaseClause
	{
		public CaseClause (SequenceType type, ExprSingle expr, XmlQualifiedName varName)
		{
		}
	}

	// IfExpr

	public class IfExpr : ExprSingle
	{
		public IfExpr (ExprSequence condition, ExprSingle trueExpr, ExprSingle falseExpr)
		{
		}
	}

	// logical expr

	public abstract class BinaryOperationExpr : ExprSingle
	{
		protected BinaryOperationExpr (ExprSingle left, ExprSingle right)
		{
			this.left = left;
			this.right = right;
		}

		ExprSingle left, right;
		
		public ExprSingle Left {
			get { return left; }
		}

		public ExprSingle Right{
			get { return right; }
		}
	}

	public class OrExpr : BinaryOperationExpr
	{
		public OrExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}
	}

	public class AndExpr : BinaryOperationExpr
	{
		public AndExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}
	}

	// TypeOperation expr

	public abstract class TypeOperationExpr : ExprSingle
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
		}

		public SequenceType Type {
			get { return type; }
		}
	}

	public abstract class AtomicTypeOperationExpr : ExprSingle
	{
		protected AtomicTypeOperationExpr (ExprSingle expr, XmlTypeCode type, bool optional)
		{
			this.expr = expr;
			this.typeCode = type;
			this.optional = optional;
		}

		ExprSingle expr;
		XmlTypeCode typeCode;
		bool optional;

		public ExprSingle Expr {
			get { return expr; }
		}

		public XmlTypeCode TypeCode {
			get { return typeCode; }
		}

		public bool Optional {
			get { return optional; }
		}
	}

	public class InstanceOfExpr : TypeOperationExpr
	{
		public InstanceOfExpr (ExprSingle expr, SequenceType type)
			: base (expr, type)
		{
		}
	}

	public class TreatExpr : TypeOperationExpr
	{
		public TreatExpr (ExprSingle expr, SequenceType type)
			: base (expr, type)
		{
		}
	}

	public class CastableExpr : AtomicTypeOperationExpr
	{
		public CastableExpr (ExprSingle expr, XmlTypeCode atomicType, bool optional)
			: base (expr, atomicType, optional)
		{
		}
	}

	public class CastExpr : AtomicTypeOperationExpr
	{
		public CastExpr (ExprSingle expr, XmlTypeCode atomicType, bool optional)
			: base (expr, atomicType, optional)
		{
		}
	}

	// ComparisonExpr

	public class ComparisonExpr : BinaryOperationExpr
	{
		public ComparisonExpr (ExprSingle left, ExprSingle right, ComparisonOperator oper)
			: base (left, right)
		{
		}
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

	public class RangeExpr : BinaryOperationExpr
	{
		public RangeExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}
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

	public class ArithmeticOperationExpr : BinaryOperationExpr
	{
		public ArithmeticOperationExpr (ExprSingle left, ExprSingle right, ArithmeticOperator oper)
			: base (left, right)
		{
		}
	}

	public class MinusExpr : ExprSingle
	{
		public MinusExpr (ExprSingle expr)
		{
		}
	}

	// aggregation expr

	public class UnionExpr : BinaryOperationExpr
	{
		public UnionExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}
	}

	public class IntersectExpr : BinaryOperationExpr
	{
		public IntersectExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}
	}

	public class ExceptExpr : BinaryOperationExpr
	{
		public ExceptExpr (ExprSingle left, ExprSingle right)
			: base (left, right)
		{
		}
	}

	// validate expr

	public class ValidateExpr : ExprSingle
	{
		XmlSchemaContentProcessing schemaMode;
		SchemaContext schemaContext;
		ExprSequence expr;

		public ValidateExpr (XmlSchemaContentProcessing schemaMode,
			SchemaContext schemaContext, ExprSequence expr)
		{
			this.schemaMode = schemaMode;
			this.schemaContext = schemaContext;
			this.expr = expr;
		}
	}

	public class SchemaContext
	{
		SchemaContextLoc loc;

		public SchemaContext (SchemaContextLoc loc) // global if null
		{
			this.loc = loc;
		}
	}

	public abstract class SchemaContextLoc
	{
	}
	
	public class SchemaLocalContextLoc : SchemaContextLoc
	{
		SchemaContextPath path;

		public SchemaLocalContextLoc (SchemaContextPath path)
		{
			this.path = path;
		}
	}

	public class SchemaGlobalContextLoc : SchemaContextLoc
	{
		XmlQualifiedName typeName;

		public SchemaGlobalContextLoc (XmlQualifiedName typeName)
		{
			this.typeName = typeName;
		}
	}

	public class SchemaContextPath
	{
		SchemaGlobalContext schemaGlobalContext;
		ArrayList list = new ArrayList ();

		public SchemaContextPath ()
		{
		}

		public SchemaGlobalContext SchemaGlobalContext {
			get { return schemaGlobalContext; }
			set { schemaGlobalContext = value; }
		}

		public void Add (XmlQualifiedName step)
		{
			list.Add (step);
		}
		
		public void Reverse ()
		{
			list.Reverse ();
		}
	}

	public class SchemaGlobalContext
	{
		XmlQualifiedName name;
		bool global;

		public SchemaGlobalContext (XmlQualifiedName name, bool global)
		{
			this.name = name;
			this.global = global;
		}
	}

	public class SchemaContextStepList : CollectionBase
	{
	}

	// Path expr

	public abstract class PathExpr : ExprSingle
	{
	}

	// '/'
	public class PathExprRoot : PathExpr
	{
		public PathExprRoot ()
		{
		}
	}

	// 'foo/bar'
	public class PathExprChild : PathExpr
	{
		ExprSingle left;
		ExprSingle child;

		public PathExprChild (ExprSingle left, ExprSingle child)
		{
			this.left = left;
			this.child = child;
		}
	}

	// 'foo//bar'
	public class PathExprDescendant : PathExpr
	{
		ExprSingle left;
		ExprSingle descendant;

		public PathExprDescendant (ExprSingle left, ExprSingle descendant)
		{
			this.left = left;
			this.descendant = descendant;
		}
	}

	public abstract class StepExpr : PathExpr
	{
	}

	public class AxisStepExpr : StepExpr
	{
		public AxisStepExpr (XPathAxis axis, NodeTestExpr test)
		{
		}

		static AxisStepExpr parentStep;

		static AxisStepExpr ()
		{
			parentStep = new AxisStepExpr (XPathAxis.Parent, null);
		}

		public static AxisStepExpr ParentStep {
			get { return parentStep; }
		}
	}

	public class FilterStepExpr : StepExpr
	{
		public FilterStepExpr (ExprSingle expr, ExprSequenceList predicates)
		{
		}
	}

	// predicates == exprsequence list == list of list of exprsingle
	public class ExprSequenceList
	{
		ArrayList list = new ArrayList ();

		public void Add (ExprSequence expr)
		{
			list.Add (expr);
		}

		public void Insert (int pos, ExprSequence expr)
		{
			list.Insert (pos, expr);
		}
	}

	public class XPathAxis
	{
		// FIXME: add more parameters to distinguish them
		private XPathAxis (bool reverseOrderAxis)
		{
			this.reverse = reverseOrderAxis;
		}

		bool reverse;

		public bool ReverseAxis {
			get { return reverse; }
		}

		static XPathAxis child, descendant, attribute, self, 
			descendantOrSelf, followingSibling, following, 
			parent, ancestor, precedingSibling, preceding, 
			ancestorOrSelf;

		static XPathAxis ()
		{
			child = new XPathAxis (false);
			descendant = new XPathAxis (false);
			attribute = new XPathAxis (false);
			self = new XPathAxis (false);
			descendantOrSelf = new XPathAxis (false);
			followingSibling = new XPathAxis (false);
			following = new XPathAxis (false);
			parent = new XPathAxis (true);
			ancestor = new XPathAxis (true);
			precedingSibling = new XPathAxis (true);
			preceding = new XPathAxis (true);
			ancestorOrSelf = new XPathAxis (true);
		}

		public static XPathAxis Child {
			get { return child; }
		}

		public static XPathAxis Descendant {
			get { return descendant; }
		}

		public static XPathAxis Attribute {
			get { return attribute; }
		}

		public static XPathAxis Self {
			get { return self; }
		}

		public static XPathAxis DescendantOrSelf {
			get { return descendantOrSelf; }
		}

		public static XPathAxis FollowingSibling {
			get { return followingSibling; }
		}

		public static XPathAxis Following {
			get { return following; }
		}

		public static XPathAxis Parent {
			get { return parent; }
		}

		public static XPathAxis Ancestor {
			get { return ancestor; }
		}

		public static XPathAxis PrecedingSibling {
			get { return precedingSibling; }
		}

		public static XPathAxis Preceding {
			get { return preceding; }
		}

		public static XPathAxis AncestorOrSelf {
			get { return ancestorOrSelf; }
		}
	}

	// NodeTest

	public abstract class NodeTestExpr : PathExpr
	{
	}

	public class NodeNameTestExpr : NodeTestExpr
	{
		XmlQualifiedName name;

		public NodeNameTestExpr (XmlQualifiedName name)
		{
			this.name = name;
		}

		public XmlQualifiedName Name {
			get { return name; }
		}
	}

	public class NodeKindTestExpr : NodeTestExpr
	{
		public NodeKindTestExpr (XmlTypeCode type)
		{
			// item() -> XPathNodeType.All
			this.nodeKind = type;
		}

		XmlTypeCode nodeKind;

		public XmlTypeCode NodeKind {
			get { return nodeKind; }
		}
	}

	public class DocumentTestExpr : NodeKindTestExpr
	{
		ElementTestExpr content;

		public DocumentTestExpr (ElementTestExpr content)
			: base (XmlTypeCode.Document)
		{
			this.content = content;
		}
	}

	public class ElementTestExpr : NodeKindTestExpr
	{
		static ElementTestExpr ()
		{
			anyElement = new ElementTestExpr (new XmlQualifiedName ("*", "*"), null);
		}

		static ElementTestExpr anyElement;

		public static ElementTestExpr AnyElement {
			get { return anyElement; }
		}

		public ElementTestExpr (SchemaContextPath path, XmlQualifiedName name)
			: base (XmlTypeCode.Element)
		{
			this.name = name;
			this.path = path;
		}

		public ElementTestExpr (XmlQualifiedName name, NillableTypeName type)
			: base (XmlTypeCode.Element)
		{
			this.name = name;
			this.type = type;
		}

		XmlQualifiedName name;
		SchemaContextPath path;
		NillableTypeName type;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public SchemaContextPath Path {
			get { return path; }
		}

		public NillableTypeName ElementTypeSpec {
			get { return type; }
		}
	}

	public class NillableTypeName
	{
		public NillableTypeName (XmlQualifiedName name, bool nillable)
		{
			this.name = name;
			this.nillable = nillable;
		}

		XmlQualifiedName name;
		bool nillable;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public bool Nillable {
			get { return nillable; }
		}
	}

	public class AttributeTestExpr : NodeKindTestExpr
	{
		static AttributeTestExpr anyAttribute;

		static AttributeTestExpr ()
		{
			anyAttribute = new AttributeTestExpr (XmlQualifiedName.Empty);
		}

		public static AttributeTestExpr AnyAttribute {
			get { return anyAttribute; }
		}

		// FIXME: don't create new QName every time.
		public AttributeTestExpr (XmlQualifiedName name)
			: this (name, new XmlQualifiedName ("anyType", XmlSchema.Namespace))
		{
		}

		public AttributeTestExpr (SchemaContextPath schemaContextPath,
			XmlQualifiedName name)
			: base (XmlTypeCode.Attribute)
		{
			this.schemaContextPath = schemaContextPath;
			this.name = name;
		}

		public AttributeTestExpr (XmlQualifiedName name, XmlQualifiedName typeName)
			: base (XmlTypeCode.Attribute)
		{
			this.name = name;
			this.typeName = typeName;
		}

		XmlQualifiedName name;
		XmlQualifiedName typeName;
		SchemaContextPath schemaContextPath;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public XmlQualifiedName TypeName {
			get { return typeName; }
		}

		public SchemaContextPath SchemaContextPath {
			get { return schemaContextPath; }
		}
	}

	public class XmlPITestExpr : NodeKindTestExpr
	{
		XmlQualifiedName nameTest;
		string valueTest;

		public XmlPITestExpr (XmlQualifiedName nameTest)
			: base (XmlTypeCode.ProcessingInstruction)
		{
			this.nameTest = nameTest;
		}

		public XmlPITestExpr (string valueTest)
			: base (XmlTypeCode.ProcessingInstruction)
		{
			this.valueTest = valueTest;
		}
	}

	public class EnclosedExpr : ExprSingle
	{
		ExprSequence expr;

		public EnclosedExpr (ExprSequence expr)
		{
			this.expr = expr;
		}
	}

	// PrimaryExpr

	public abstract class PrimaryExpr : ExprSingle
	{
	}

	public class StringLiteralExpr : PrimaryExpr
	{
		string literal;
		public StringLiteralExpr (string literal)
		{
			this.literal = literal;
		}
	}

	public class NumberLiteralExpr : PrimaryExpr
	{
		decimal value;

		public NumberLiteralExpr (decimal value)
		{
			this.value = value;
		}
	}

	public class VariableReferenceExpr : PrimaryExpr
	{
		XmlQualifiedName varName;

		public VariableReferenceExpr (XmlQualifiedName varName)
		{
			this.varName = varName;
		}

		public XmlQualifiedName VariableName {
			get { return varName; }
		}
	}

	public class ParenthesizedExpr : PrimaryExpr
	{
		ExprSequence expr;

		public ParenthesizedExpr (ExprSequence expr)
		{
			this.expr = expr;
		}
	}

	// "."
	public class ContextItemExpr : PrimaryExpr
	{
		public ContextItemExpr ()
		{
		}
	}

	public class FunctionCallExpr : PrimaryExpr
	{
		XmlQualifiedName name;
		ExprSequence args;

		public FunctionCallExpr (XmlQualifiedName name, ExprSequence args)
		{
		}
	}

	public class SequenceType
	{
		ExprSingle test;
		Occurence occurence;

		public SequenceType (ExprSingle test, Occurence occurence)
		{
			this.test = test;
			this.occurence = occurence;
		}
	}

	public enum Occurence
	{
		One,
		Optional,
		ZeroOrMore,
		OneOrMore,
	}
}

#endif
