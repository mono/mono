using System;
using System.Collections;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for CategoryFilter.
	/// </summary>
	/// 
	[Serializable]
	public class CategoryFilter : Filter
	{
		ArrayList categories;

		public CategoryFilter() : this( false ) { }

		public CategoryFilter( bool exclude ) : base( exclude )
		{
			categories = new ArrayList();
		}

		public CategoryFilter( string name ) : this( name, false ) { }

		public CategoryFilter( string name, bool exclude ) : base( exclude )
		{
			categories = new ArrayList();
			categories.Add( name );
		}

		public CategoryFilter( string[] names ) : this( names, false ) { }

		public CategoryFilter( string[] names, bool exclude ) : base( exclude )
		{
			categories = new ArrayList();
			categories.AddRange( names );
		}

		public void AddCategory(string name) 
		{
			categories.Add( name );
		}

		#region IFilter Members

		public override bool Pass(TestSuite suite)
		{
//			return CheckCategories( suite ) ? !Exclude : Exclude;

			if ( categories.Count == 0 ) return true;

			bool pass = Exclude;

			if (CheckCategories(suite))
				return !Exclude;

			foreach (Test test in suite.Tests) 
			{
				if ( test.Filter(this) == !Exclude )
				{
					pass=true;
					break;
				}
			}

			return pass;
		}

		public override bool Pass(TestCase test)
		{
			if ( categories.Count == 0 )
				return true;
			return CheckCategories( test ) ? !Exclude : Exclude ;

//			if (CheckCategories(test.Parent))
//				return true;
//
//			return CheckCategories(test);
		}

		#endregion

		/// <summary>
		/// Method returns true if the test has a particular
		/// category or if an ancestor test does. We don't
		/// worry about whether this is an include or an
		/// exclude filter at this point because only positive
		/// categories are inherited, not their absence.
		/// </summary>
		private bool CheckCategories(Test test) 
		{
			return test.HasCategory( categories )
				|| test.Parent != null 
				&& test.Parent.HasCategory( categories );

//			if (test.Categories != null) 
//			{
//				foreach (string name in categories) 
//				{
//					if (test.Categories.Contains(name))
//						return true;
//				}
//			}
//
//			return false;
		}
	}
}
