// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// TestFixtureBuilder contains static methods for building
	/// TestFixtures from types. It uses builtin SuiteBuilders
	/// and any installed extensions to do it.
	/// </summary>
	public class TestFixtureBuilder
	{
		public static bool CanBuildFrom( Type type )
		{
			return CoreExtensions.Host.SuiteBuilders.CanBuildFrom( type );
		}

		/// <summary>
		/// Build a test fixture from a given type.
		/// </summary>
		/// <param name="type">The type to be used for the fixture</param>
		/// <returns>A TestSuite if the fixture can be built, null if not</returns>
		public static Test BuildFrom( Type type )
		{
			Test suite = CoreExtensions.Host.SuiteBuilders.BuildFrom( type );

			if ( suite != null )
				suite = CoreExtensions.Host.TestDecorators.Decorate( suite, type );

			return suite;
		}

		/// <summary>
		/// Build a fixture from an object. 
		/// </summary>
		/// <param name="fixture">The object to be used for the fixture</param>
		/// <returns>A TestSuite if fixture type can be built, null if not</returns>
		public static Test BuildFrom( object fixture )
		{
			Test suite = BuildFrom( fixture.GetType() );
			if( suite != null)
				suite.Fixture = fixture;
			return suite;
		}

		public static string GetAssemblyPath( Type fixtureType )
		{
			return GetAssemblyPath( fixtureType.Assembly );
		}

		// TODO: This logic should be in shared source
		public static string GetAssemblyPath( Assembly assembly )
		{
			string path = assembly.CodeBase;
			Uri uri = new Uri( path );
			
			// If it wasn't loaded locally, use the Location
			if ( !uri.IsFile )
				return assembly.Location;

			if ( uri.IsUnc )
				return path.Substring( Uri.UriSchemeFile.Length+1 );


			int start = Uri.UriSchemeFile.Length + Uri.SchemeDelimiter.Length;
			
			if ( path[start] == '/' && path[start+2] == ':' )
				++start;

			return path.Substring( start );
		}

		/// <summary>
		/// Private constructor to prevent instantiation
		/// </summary>
		private TestFixtureBuilder() { }
	}
}
