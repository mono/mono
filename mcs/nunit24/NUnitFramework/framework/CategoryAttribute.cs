// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Framework
{
	/// <summary>
	/// Attribute used to apply a category to a test
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Assembly, AllowMultiple=true)]
	public class CategoryAttribute : Attribute
	{
		/// <summary>
		/// The name of the category
		/// </summary>
		protected string categoryName;

		/// <summary>
		/// Construct attribute for a given category
		/// </summary>
		/// <param name="name">The name of the category</param>
		public CategoryAttribute(string name)
		{
			this.categoryName = name;
		}

		/// <summary>
		/// Protected constructor uses the Type name as the name
		/// of the category.
		/// </summary>
		protected CategoryAttribute()
		{
			this.categoryName = this.GetType().Name;
			if ( categoryName.EndsWith( "Attribute" ) )
				categoryName = categoryName.Substring( 0, categoryName.Length - 9 );
		}

		/// <summary>
		/// The name of the category
		/// </summary>
		public string Name 
		{
			get { return categoryName; }
		}
	}
}
