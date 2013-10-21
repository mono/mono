using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Build.Internal
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
		
		//public int Line {
		//	get { return list.Count == 0 ? 0 : list [0].Line; }
		//}
		public int Column {
			get { return list.Count == 0 ? 0 : list [0].Column; }
		}
		//public string File {
		//	get { return list.Count == 0 ? null : list [0].File; }
		//}
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
	}

	abstract partial class Expression : Locatable, ILocation
	{
		//public int Line {
		//	get { return Location.Line; }
		//}
		public int Column {
			get { return Location.Column; }
		}
		//public string File {
		//	get { return Location.File; }
		//}
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
	}
	
	partial class BooleanLiteral : Expression
	{
		public bool Value { get; set; }
	}

	partial class NotExpression : Expression
	{
		public Expression Negated { get; set; }
	}

	partial class PropertyAccessExpression : Expression
	{
		public PropertyAccess Access { get; set; }
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
	}

	partial class ItemAccessExpression : Expression
	{
		public ItemApplication Application { get; set; }
	}
	
	class ItemApplication : Locatable
	{
		public NameToken Name { get; set; }
		public ExpressionList Expressions { get; set; }
	}

	partial class MetadataAccessExpression : Expression
	{
		public MetadataAccess Access { get; set; }
	}
	
	class MetadataAccess : Locatable
	{
		public NameToken Metadata { get; set; }
		public NameToken Item { get; set; }
	}
	
	partial class StringLiteral : Expression
	{
		public NameToken Value { get; set; }
	}

	partial class RawStringLiteral : Expression
	{
		public NameToken Value { get; set; }
	}
	
	partial class FunctionCallExpression : Expression
	{
		public NameToken Name { get; set; }
		public ExpressionList Arguments { get; set; }
	}
}

