// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Text;
using System.Collections;

namespace NUnit.Core.Filters
{
	/// <summary>
	/// CategoryFilter is able to select or exclude tests
	/// based on their categories.
	/// </summary>
	/// 
	[Serializable]
	public class CategoryFilter : TestFilter
	{
		ArrayList categories;

		/// <summary>
		/// Construct an empty CategoryFilter
		/// </summary>
		public CategoryFilter()
		{
			categories = new ArrayList();
		}

		/// <summary>
		/// Construct a CategoryFilter using a single category name
		/// </summary>
		/// <param name="name">A category name</param>
		public CategoryFilter( string name )
		{
			categories = new ArrayList();
			if ( name != null && name != string.Empty )
				categories.Add( name );
		}

		/// <summary>
		/// Construct a CategoryFilter using an array of category names
		/// </summary>
		/// <param name="names">An array of category names</param>
		public CategoryFilter( string[] names )
		{
			categories = new ArrayList();
			if ( names != null )
				categories.AddRange( names );
		}

		/// <summary>
		/// Add a category name to the filter
		/// </summary>
		/// <param name="name">A category name</param>
		public void AddCategory(string name) 
		{
			categories.Add( name );
		}

		/// <summary>
		/// Check whether the filter matches a test
		/// </summary>
		/// <param name="test">The test to be matched</param>
		/// <returns></returns>
        public override bool Match(ITest test)
        {
			if ( test.Categories == null )
				return false;

			foreach( string cat in categories )
				if ( test.Categories.Contains( cat ) )
					return true;

			return false;
        }
		
		/// <summary>
		/// Return the string representation of a category filter
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for( int i = 0; i < categories.Count; i++ )
			{
				if ( i > 0 ) sb.Append( ',' );
				sb.Append( categories[i] );
			}
			return sb.ToString();
		}

		/// <summary>
		/// Gets the list of categories from this filter
		/// </summary>
		public IList Categories
		{
			get { return categories; }
		}
	}
}
