using System;
using System.Collections;
using NUnit.Core;
using NUnit.Core.Filters;

namespace NUnit.Util
{
	/// <summary>
	/// CategoryExpression parses strings representing boolean
	/// combinations of categories according to the following
	/// grammar:
	///   CategoryName ::= string not containing any of ',', '&', '+', '-'
	///   CategoryFilter ::= CategoryName | CategoryFilter ',' CategoryName
	///   CategoryPrimitive ::= CategoryFilter | '-' CategoryPrimitive
	///   CategoryTerm ::= CategoryPrimitive | CategoryTerm '&' CategoryPrimitive
	/// </summary>
	public class CategoryExpression
	{
		static readonly char[] ops = new char[] { ',', ';', '-', '|', '+', '(', ')' };

		private string text;
		private int next;
		private string token;

		private TestFilter filter;

		public CategoryExpression(string text) 
		{
			this.text =  text;
			this.next = 0;
		}

		public TestFilter Filter
		{
			get
			{
				if( filter == null )
				{
					filter = GetToken() == null
						? TestFilter.Empty
						: GetExpression();
				}

				return filter;
			}
		}

		private TestFilter GetExpression()
		{
			TestFilter term = GetTerm();
			if ( token != "|" )
				return term;

			OrFilter filter = new OrFilter( term );
			
			while ( token == "|" )
			{
				GetToken();
				filter.Add( GetTerm() );
			}

			return filter;
		}

		private TestFilter GetTerm()
		{
			TestFilter prim = GetPrimitive();
			if ( token != "+" && token != "-" )
				return prim;

			AndFilter filter = new AndFilter( prim );
			
			while ( token == "+"|| token == "-" )
			{
				string tok = token;
				GetToken();
				prim = GetPrimitive();
				filter.Add( tok == "-" ? new NotFilter( prim ) : prim );
			}

			return filter;
		}

		private TestFilter GetPrimitive()
		{
			if( token == "-" )
			{
				GetToken();
				return new NotFilter( GetPrimitive() );
			}
			else if( token == "(" )
			{
				GetToken();
				TestFilter expr = GetExpression();
				GetToken(); // Skip ')'
				return expr;
			}

			return GetCategoryFilter();
		}

		private CategoryFilter GetCategoryFilter()
		{
			CategoryFilter filter = new CategoryFilter( token );

			while( GetToken() == "," || token == ";" )
				filter.AddCategory( GetToken() );

			return filter;
		}

		public string GetToken()
		{
			SkipWhiteSpace();

			if ( EndOfText() ) 
				token = null;
			else if ( NextIsOperator() )
				token = text.Substring(next++, 1);
			else
			{
				int index2 = text.IndexOfAny( ops, next );
				if ( index2 < 0 ) index2 = text.Length;

				token = text.Substring( next, index2 - next ).TrimEnd();
				next = index2;
			}

			return token;
		}

		private void SkipWhiteSpace()
		{
			while( next < text.Length && Char.IsWhiteSpace( text[next] ) )
				++next;
		}

		private bool EndOfText()
		{
			return next >= text.Length;
		}

		private bool NextIsOperator()
		{
			foreach( char op in ops )
				if( op == text[next] )
					return true;

			return false;
		}
	}
}
