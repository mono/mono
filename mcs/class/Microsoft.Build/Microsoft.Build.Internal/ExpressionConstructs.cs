using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Build.Internal
{
	
	class Locatable
	{
		public ILocation Location { get; set; }		
	}
	
	class ExpressionList : ILocation, IEnumerable<Expression>
	{
		public ExpressionList (Expression entry)
		{
			Append (entry);
		}
		
		//public int Line {
		//	get { return list [0].Line; }
		//}
		public int Column {
			get { return list [0].Column; }
		}
		//public string File {
		//	get { return list [0].File; }
		//}
			
		public IEnumerator<Expression> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		List<Expression> list = new List<Expression> ();
		
		public ExpressionList Append (Expression expr)
		{
			list.Add (expr);
			return this;
		}
	}

	class Expression : Locatable, ILocation
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
	}
	
	class BooleanLiteral : Expression
	{
		public bool Value { get; set; }
	}

	class NotExpression : Expression
	{
		public Expression Negated { get; set; }
	}

	class PropertyAccessExpression : Expression
	{
		public PropertyAccess Access { get; set; }
	}
	
	class PropertyAccess : Locatable
	{
		public NameToken Name { get; set; }
		public Expression Target { get; set; }
	}

	class ItemAccessExpression : Expression
	{
		public ItemApplication Application { get; set; }
	}
	
	class ItemApplication : Locatable
	{
		public NameToken Name { get; set; }
		public ExpressionList Expressions { get; set; }
	}

	class MetadataAccessExpression : Expression
	{
		public MetadataAccess Access { get; set; }
	}
	
	class MetadataAccess : Locatable
	{
		public NameToken Metadata { get; set; }
		public NameToken Item { get; set; }
	}

	class StringLiteral : Expression
	{
		public NameToken Value { get; set; }
	}
	
	class FunctionCallExpression : Expression
	{
		public NameToken Name { get; set; }
		public ExpressionList Arguments { get; set; }
	}
}

