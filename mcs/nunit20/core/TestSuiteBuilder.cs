#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Collections;

	/// <summary>
	/// Summary description for TestSuiteBuilder.
	/// </summary>
	public class TestSuiteBuilder
	{
		#region Private Fields

		/// <summary>
		/// Hashtable of all test suites we have created to represent namespaces.
		/// Used to locate namespace parent suites for fixtures.
		/// </summary>
		Hashtable namespaceSuites  = new Hashtable();

		/// <summary>
		/// The root of the test suite being created by this builder. This
		/// may be a simple TestSuite, an AssemblyTestSuite or a RootTestSuite
		/// encompassing multiple assemblies.
		/// </summary>
		TestSuite rootSuite;

		/// <summary>
		/// The version of the nunit framework referenced by the loaded assembly.
		/// </summary>
		Version frameworkVersion = null;

		#endregion

		#region Properties

		public Version FrameworkVersion
		{
			get { return frameworkVersion; }
		}

		#endregion

		#region Public Methods

		public Assembly Load(string assemblyName)
		{
			// Change currentDirectory in case assembly references unmanaged dlls
			string currentDirectory = Environment.CurrentDirectory;
			string assemblyDirectory = Path.GetDirectoryName( assemblyName );
			bool swap = assemblyDirectory != null && assemblyDirectory != string.Empty;

			try
			{
				if ( swap )
					Environment.CurrentDirectory = assemblyDirectory;

				Assembly assembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assemblyName));

				foreach( AssemblyName refAssembly in assembly.GetReferencedAssemblies() )
				{
					if ( refAssembly.Name == "nunit.framework" )
						this.frameworkVersion = refAssembly.Version;
				}

				return assembly;
			}
			finally
			{
				if ( swap )
					Environment.CurrentDirectory = currentDirectory;
			}
		}

		public TestSuite Build(string projectName, IList assemblies)
		{
			RootTestSuite rootSuite = new RootTestSuite( projectName );

			int assemblyKey = 0;
			foreach(string assembly in assemblies)
			{
				TestSuite suite = Build( assembly, assemblyKey++ );
				rootSuite.Add( suite );
			}

			return rootSuite;
		}

		public TestSuite Build( string assemblyName )
		{
			return Build( assemblyName, 0 );
		}

		public TestSuite Build(string assemblyName, string testName )
		{
			TestSuite suite = null;

			Assembly assembly = Load(assemblyName);

			if(assembly != null)
			{
				Type testType = assembly.GetType(testName);
				if( testType != null )
					return MakeSuite( testType );

				// Assume that testName is a namespace
				string prefix = testName + '.';

				Type[] testTypes = assembly.GetExportedTypes();
				int testFixtureCount = 0;

				foreach(Type type in testTypes)
				{
					if( CanMakeSuite( type ) && type.Namespace != null )
					{
						if( type.Namespace == testName || type.Namespace.StartsWith(prefix) )
						{
							suite = BuildFromNameSpace(testName, 0);
						
							//suite.Add( new TestFixture( type ) );
							suite.Add( MakeSuite( type ) );
							testFixtureCount++;
						}
					}
				}

				return testFixtureCount == 0 ? null : rootSuite;
			}

			return suite;
		}

		public TestSuite Build( IList assemblies, string testName )
		{
			TestSuite suite = null;

			foreach(string assemblyName in assemblies)
			{
				suite = Build( assemblyName, testName );
				if ( suite != null ) break;
			}

			return suite;
		}
		
		// TODO: Only used in tests
		public object BuildTestFixture( Type fixtureType )
		{
			Reflect.CheckFixtureType( fixtureType );

			object testFixture;
			ConstructorInfo ctor = Reflect.GetConstructor( fixtureType );

			try
			{
				testFixture = ctor.Invoke( Type.EmptyTypes );
			}
			catch( Exception ex )
			{
				throw new InvalidTestFixtureException( ctor.Name + " threw a exception", ex );
			}

			if(testFixture == null) throw new InvalidTestFixtureException(ctor.Name + " cannot be invoked");

			return testFixture;
		}

		#endregion

		#region Nested TypeFilter Class

		private class TypeFilter
		{
			private string rootNamespace;

			TypeFilter( string rootNamespace ) 
			{
				this.rootNamespace = rootNamespace;
			}

			public bool Include( Type type )
			{
				if ( type.Namespace == rootNamespace )
					return true;

				return type.Namespace.StartsWith( rootNamespace + '.' );
			}
		}

		#endregion

		#region Helper Methods

		private TestSuite BuildFromNameSpace( string nameSpace, int assemblyKey )
		{
			if( nameSpace == null || nameSpace  == "" ) return rootSuite;
			TestSuite suite = (TestSuite)namespaceSuites[nameSpace];
			if(suite!=null) return suite;

			int index = nameSpace.LastIndexOf(".");
			string prefix = string.Format( "[{0}]", assemblyKey );
			if( index == -1 )
			{
				suite = new NamespaceSuite( nameSpace, assemblyKey );
				if ( rootSuite == null )
					rootSuite = suite;
				else
					rootSuite.Add(suite);
				namespaceSuites[nameSpace]=suite;
			}
			else
			{
				string parentNameSpace = nameSpace.Substring( 0,index );
				TestSuite parent = BuildFromNameSpace( parentNameSpace, assemblyKey );
				string suiteName = nameSpace.Substring( index+1 );
				suite = new NamespaceSuite( parentNameSpace, suiteName, assemblyKey );
				parent.Add( suite );
				namespaceSuites[nameSpace] = suite;
			}

			return suite;
		}

		private TestSuite Build( string assemblyName, int assemblyKey )
		{
			TestSuiteBuilder builder = new TestSuiteBuilder();

			Assembly assembly = Load( assemblyName );

			builder.rootSuite = new AssemblyTestSuite( assemblyName, assemblyKey );
			int testFixtureCount = 0;
			Type[] testTypes = assembly.GetExportedTypes();
			foreach(Type testType in testTypes)
			{
				if( CanMakeSuite( testType ) )
				{
					testFixtureCount++;
					string namespaces = testType.Namespace;
					TestSuite suite = builder.BuildFromNameSpace( namespaces, assemblyKey );

					//suite.Add( new TestFixture( testType ) );
					suite.Add( MakeSuite( testType ) );
				}
			}

			if(testFixtureCount == 0)
			{
				builder.rootSuite.ShouldRun = false;
				builder.rootSuite.IgnoreReason = "Has no TestFixtures";
			}

			return builder.rootSuite;
		}

		/// <summary>
		/// Helper routine that makes a suite from either a TestFixture or
		/// a legacy Suite property.
		/// </summary>
		/// <param name="testType"></param>
		/// <returns></returns>
		private TestSuite MakeSuite( Type testType )
		{
			TestSuite suite = null;

			if(testType != null)
			{
				if( TestFixture.IsValidType( testType ) )
				{
					suite = new TestFixture( testType );
				}
				else if( LegacySuite.IsValidType( testType ) )
				{
					suite = new LegacySuite( testType );
				}
			}
			
			return suite;
		}

		private bool CanMakeSuite( Type testType )
		{
			//return TestFixture.IsValidType( testType ) || LegacySuite.IsValidType( testType );
			return TestFixture.IsValidType( testType );
		}
	}

	#endregion
}
