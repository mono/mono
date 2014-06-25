//
// ExpressionConstructs.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Build.Internal.Expressions
{
	
	class Locatable
	{
		public ILocation Location { get; set; }		
	}
	
	partial class ExpressionList : ILocation, IEnumerable<Expression>
	{
		public ExpressionList ()
		{
		}
		
		public ExpressionList (Expression entry)
		{
			Add (entry);
		}
		
		public int Count {
			get { return list.Count; }
		}
		
		//public int Line {
		//	get { return list.Count == 0 ? 0 : list [0].Line; }
		//}
		public int Column {
			get { return list.Count == 0 ? 0 : list [0].Column; }
		}
		public string File {
			get { return list.Count == 0 ? null : list [0].File; }
		}
		public string ToLocationString ()
		{
			return list.Count == 0 ? null : list [0].Location.ToLocationString ();
		}
			
		public IEnumerator<Expression> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		List<Expression> list = new List<Expression> ();
		
		public ExpressionList Add (Expression expr)
		{
			list.Add (expr);
			return this;
		}
		
		public ExpressionList Insert (int pos, Expression expr)
		{
			list.Insert (pos, expr);
			return this;
		}

		public override string ToString ()
		{
			return string.Join (" ", list.Select (i => i.ToString ()));
		}
	}

	abstract partial class Expression : Locatable, ILocation
	{
		//public int Line {
		//	get { return Location.Line; }
		//}
		public int Column {
			get { return Location.Column; }
		}
		public string File {
			get { return Location.File; }
		}
		public string ToLocationString ()
		{
			return Location.ToLocationString ();
		}
	}
	
	enum Operator
	{
		EQ,
		NE,
		LT,
		LE,
		GT,
		GE,
		And,
		Or
	}
	
	partial class BinaryExpression : Expression
	{
		public Operator Operator { get; set; }
		public Expression Left { get; set; }
		public Expression Right { get; set; }

		public override string ExpressionString {
			get { return string.Format ("{0} {1} {2}", Left, Operator, Right); }
		}
	}
	
	partial class BooleanLiteral : Expression
	{
		public bool Value { get; set; }

		public override string ExpressionString {
			get { return Value ? "true" : "false"; }
		}
	}

	partial class NotExpression : Expression
	{
		public Expression Negated { get; set; }
		public override string ExpressionString {
			get { return string.Format ("!{0}", Negated); }
		}
	}

	partial class PropertyAccessExpression : Expression
	{
		public PropertyAccess Access { get; set; }
		public override string ExpressionString {
			get { return Access.ExpressionString; }
		}
	}
	
	enum PropertyTargetType
	{
		Object,
		Type,
	}
	
	class PropertyAccess : Locatable
	{
		public NameToken Name { get; set; }
		public Expression Target { get; set; }
		public PropertyTargetType TargetType { get; set; }
		public ExpressionList Arguments { get; set; }
		public string ExpressionString {
			get { return string.Format ("$([{0}][{1}][{2}][{3}])", Target, TargetType, Name, Arguments != null && Arguments.Any () ? string.Join (", ", Arguments.Select (e => e.ExpressionString)) : null); }
		}
	}

	partial class ItemAccessExpression : Expression
	{
		public ItemApplication Application { get; set; }
		public override string ExpressionString {
			get { return Application.ExpressionString; }
		}
	}
	
	class ItemApplication : Locatable
	{
		public NameToken Name { get; set; }
		public ExpressionList Expressions { get; set; }
		public string ExpressionString {
			get { return string.Format ("@([{0}][{1}])", Name, Expressions != null && Expressions.Any () ? string.Join (" ||| ", Expressions.Select (e => e.ExpressionString)) : null); }
		}
	}

	partial class MetadataAccessExpression : Expression
	{
		public MetadataAccess Access { get; set; }
		public override string ExpressionString {
			get { return Access.ExpressionString; }
		}
	}
	
	class MetadataAccess : Locatable
	{
		public NameToken Metadata { get; set; }
		public NameToken ItemType { get; set; }
		public string ExpressionString {
			get { return string.Format ("%([{0}].[{1}])", ItemType, Metadata); }
		}
	}

	partial class QuotedExpression : Expression
	{
		public char QuoteChar { get; set; }
		public ExpressionList Contents { get; set; }

		public override string ExpressionString {
			get { return QuoteChar + string.Concat (Contents.Select (e => e.ExpressionString)).Replace (QuoteChar.ToString (), "\\" + QuoteChar) + QuoteChar; }
		}
	}
	
	partial class StringLiteral : Expression
	{
		public NameToken Value { get; set; }

		public override string ExpressionString {
			get { return '"' + Value.ToString () + '"'; }
		}
	}

	partial class RawStringLiteral : Expression
	{
		public NameToken Value { get; set; }

		public override string ExpressionString {
			get { return "[rawstr] " + Value; }
		}
	}
	
	partial class FunctionCallExpression : Expression
	{
		public NameToken Name { get; set; }
		public ExpressionList Arguments { get; set; }

		public override string ExpressionString {
			get { return string.Format ("[func] {0}({1})", Name, string.Join (", ", Arguments.Select (e => e.ExpressionString))); }
		}
	}
}

