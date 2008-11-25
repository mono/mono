// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// SuiteBuilderCollection is an ExtensionPoint for SuiteBuilders and
	/// implements the ISuiteBuilder interface itself, passing calls 
	/// on to the individual builders.
	/// 
	/// The builders are added to the collection by inserting them at
	/// the start, as to take precedence over those added earlier. 
	/// </summary>
	public class SuiteBuilderCollection : ExtensionPoint, ISuiteBuilder
	{
		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public SuiteBuilderCollection(IExtensionHost host)
			: base("SuiteBuilders", host ) { }
		#endregion

		#region ISuiteBuilder Members

		/// <summary>
		/// Examine the type and determine if it is suitable for
		/// any SuiteBuilder to use in building a TestSuite
		/// </summary>
		/// <param name="type">The type of the fixture to be used</param>
		/// <returns>True if the type can be used to build a TestSuite</returns>
		public bool CanBuildFrom(Type type)
		{
			foreach( ISuiteBuilder builder in extensions )
				if ( builder.CanBuildFrom( type ) )
					return true;
			return false;
		}

		/// <summary>
		/// Build a TestSuite from type provided.
		/// </summary>
		/// <param name="type">The type of the fixture to be used</param>
		/// <returns>A TestSuite or null</returns>
		public Test BuildFrom(Type type)
		{
			foreach( ISuiteBuilder builder in extensions )
				if ( builder.CanBuildFrom( type ) )
					return builder.BuildFrom( type );
			return null;
		}

		#endregion

		#region ExtensionPoint Overrides
		protected override bool ValidExtension(object extension)
		{
			return extension is ISuiteBuilder; 
		}
		#endregion
	}
}
