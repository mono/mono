// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;

namespace NUnit.Core.Filters
{
	/// <summary>
	/// Combines multiple filters so that a test must pass all 
	/// of them in order to pass this filter.
	/// </summary>
	[Serializable]
	public class AndFilter : TestFilter
	{
		private ArrayList filters = new ArrayList();

		/// <summary>
		/// Constructs an empty AndFilter
		/// </summary>
		public AndFilter() { }

		/// <summary>
		/// Constructs an AndFilter from an array of filters
		/// </summary>
		/// <param name="filters"></param>
		public AndFilter( params ITestFilter[] filters )
		{
			this.filters.AddRange( filters );
		}

		/// <summary>
		/// Adds a filter to the list of filters
		/// </summary>
		/// <param name="filter">The filter to be added</param>
		public void Add( ITestFilter filter )
		{
			this.filters.Add( filter );
		}

		/// <summary>
		/// Return an array of the composing filters
		/// </summary>
		public ITestFilter[] Filters
		{
			get
			{
				return (ITestFilter[])filters.ToArray(typeof(ITestFilter));
			}
		}

		/// <summary>
		/// Checks whether the AndFilter is matched by a test
		/// </summary>
		/// <param name="test">The test to be matched</param>
		/// <returns>True if all the component filters pass, otherwise false</returns>
		public override bool Pass( ITest test )
		{
			foreach( ITestFilter filter in filters )
				if ( !filter.Pass( test ) )
					return false;

			return true;
		}

		/// <summary>
		/// Checks whether the AndFilter is matched by a test
		/// </summary>
		/// <param name="test">The test to be matched</param>
		/// <returns>True if all the component filters match, otherwise false</returns>
		public override bool Match( ITest test )
		{
			foreach( ITestFilter filter in filters )
				if ( !filter.Match( test ) )
					return false;

			return true;
		}
	}
}
