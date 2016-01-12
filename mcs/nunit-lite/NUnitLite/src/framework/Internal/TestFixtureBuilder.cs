// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Builders;

namespace NUnit.Framework.Internal
{
	/// <summary>
	/// TestFixtureBuilder contains static methods for building
	/// TestFixtures from types. It uses builtin SuiteBuilders
	/// and any installed extensions to do it.
	/// </summary>
	public class TestFixtureBuilder
	{
        private static Extensibility.ISuiteBuilder builder = new NUnitTestFixtureBuilder();

        /// <summary>
        /// Determines whether this instance [can build from] the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can build from] the specified type; otherwise, <c>false</c>.
        /// </returns>
		public static bool CanBuildFrom( Type type )
		{
            return builder.CanBuildFrom(type);
		}

		/// <summary>
		/// Build a test fixture from a given type.
		/// </summary>
		/// <param name="type">The type to be used for the fixture</param>
		/// <returns>A TestSuite if the fixture can be built, null if not</returns>
		public static Test BuildFrom( Type type )
		{
            return builder.BuildFrom( type );
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

		/// <summary>
		/// Private constructor to prevent instantiation
		/// </summary>
		private TestFixtureBuilder() { }
	}
}
