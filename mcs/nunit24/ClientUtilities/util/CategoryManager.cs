// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;
using NUnit.Core;

namespace NUnit.Util
{
	public class CategoryManager
	{
		private Hashtable categories = new Hashtable();

		public void Add(string name) 
		{
			categories[name] = name;
		}

		public void Add(IList list) 
		{
			foreach(string name in list) 
			{
				Add(name);
			}
		}

		public void AddCategories( ITest test )
		{
			if ( test.Categories != null )
				Add( test.Categories );
		}

		public void AddAllCategories( ITest test )
		{
			AddCategories( test );
			if ( test.IsSuite )
				foreach( ITest child in test.Tests )
					AddAllCategories( child );
		}

		public ICollection Categories 
		{
			get { return categories.Values; }
		}

		public void Clear() 
		{
			categories = new Hashtable();
		}
	}
}
