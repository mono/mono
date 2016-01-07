// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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
// ***********************************************************************

using System;
using System.Collections;

namespace NUnit.Framework.Internal.Filters
{
	/// <summary>
	/// CategoryExpression parses strings representing boolean
	/// combinations of categories according to the following
	/// grammar:
	///   CategoryName ::= string not containing any of ',', '&amp;', '+', '-'
	///   CategoryFilter ::= CategoryName | CategoryFilter ',' CategoryName
	///   CategoryPrimitive ::= CategoryFilter | '-' CategoryPrimitive
	///   CategoryTerm ::= CategoryPrimitive | CategoryTerm '&amp;' CategoryPrimitive
	/// </summary>
	public class CategoryExpression
	{
		static readonly char[] ops = new char[] { ',', ';', '-', '|', '+', '(', ')' };

		private string text;
		private int next;
		private string token;

		private TestFilter filter;

        /// <summary>
        /// Construct expression from a text string
        /// </summary>
        /// <param name="text">The text of the expression</param>
		public CategoryExpression(string text) 
		{
			this.text =  text;
			this.next = 0;
		}

        /// <summary>
        /// Gets the TestFilter represented by the expression
        /// </summary>
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

		private string GetToken()
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
