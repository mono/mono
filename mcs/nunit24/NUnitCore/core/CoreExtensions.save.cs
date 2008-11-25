// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using NUnit.Core.Extensibility;

namespace NUnit.Core
{
	/// <summary>
	/// CoreExtensions is a singleton class that groups together all 
	/// the extension points that are supported in the test domain.
	/// It also provides access to the test builders and decorators
	/// by other parts of the NUnit core.
	/// </summary>
	public class CoreExtensions : ExtensionHost, IService
	{
		#region Instance Fields
		private IAddinRegistry addinRegistry;
		private bool initialized;

		private SuiteBuilderCollection suiteBuilders;
		private TestCaseBuilderCollection testBuilders;
		private TestDecoratorCollection testDecorators;
		private EventListenerCollection listeners;
		#endregion

		#region CoreExtensions Singleton
		private static CoreExtensions host;
		public static CoreExtensions Host
		{
			get
			{
				if (host == null)
				{
					host = new CoreExtensions();
//					host.InitializeService();
				}

				return host;
			}
		}
		#endregion

		#region Constructors
		public CoreExtensions() 
		{
			this.suiteBuilders = new SuiteBuilderCollection();
			this.testBuilders = new TestCaseBuilderCollection();
			this.testDecorators = new TestDecoratorCollection();
			this.listeners = new EventListenerCollection();

			this.extensions = new IExtensionPoint[]
				{ suiteBuilders, testBuilders, testDecorators };
			this.supportedTypes = ExtensionType.Core;
		}
		#endregion

		#region Properties

		public bool Initialized
		{
			get { return initialized; }
		}

		/// <summary>
		/// Our AddinRegistry may be set from outside or passed into the domain
		/// </summary>
		public IAddinRegistry AddinRegistry
		{
			get 
			{
				if ( addinRegistry == null )
					addinRegistry = AppDomain.CurrentDomain.GetData( "AddinRegistry" ) as IAddinRegistry;

				return addinRegistry; 
			}
			set { addinRegistry = value; }
		}

		public ISuiteBuilder SuiteBuilders
		{
			get { return suiteBuilders; }
		}

		public TestCaseBuilderCollection TestBuilders
		{
			get { return testBuilders; }
		}

		public ITestDecorator TestDecorators
		{
			get { return testDecorators; }
		}

		public FrameworkRegistry TestFrameworks
		{
			get { return frameworks; }
		}
		#endregion

		#region Public Methods	
		public void InstallBuiltins()
		{
			//Trace.WriteLine( "Installing Builtins" );

			// Define NUnit Framework
			FrameworkRegistry.Register( "NUnit", "nunit.framework" );

			// Install builtin SuiteBuilders - Note that the
			// NUnitTestCaseBuilder is installed whenever
			// an NUnitTestFixture is being populated and
			// removed afterward.
			suiteBuilders.Install( new Builders.NUnitTestFixtureBuilder() );
			suiteBuilders.Install( new Builders.SetUpFixtureBuilder() );

            //testDecorators.InstallFinal(new Builders.MultiCultureDecorator());
		}

		public void InstallAddins()
		{
			//Trace.WriteLine( "Installing Addins" );

			if( AddinRegistry != null )
			{
				foreach (Addin addin in AddinRegistry.Addins)
				{
					if ( (this.ExtensionTypes & addin.ExtensionType) != 0 )
					{
						try
						{
							Type type = Type.GetType(addin.TypeName);
							if ( type == null )
								AddinRegistry.SetStatus( addin.Name, AddinStatus.Error, "Could not locate type" );
							else if ( !InstallAddin( type ) )
								AddinRegistry.SetStatus( addin.Name, AddinStatus.Error, "Install returned false" );
							else
								AddinRegistry.SetStatus( addin.Name, AddinStatus.Loaded, null );
						}
						catch( Exception ex )
						{
							AddinRegistry.SetStatus( addin.Name, AddinStatus.Error, ex.Message );
						}
					}
				}
			}
		}

		public void InstallAdhocExtensions( Assembly assembly )
		{
			foreach ( Type type in assembly.GetExportedTypes() )
			{
				if ( type.GetCustomAttributes(typeof(NUnitAddinAttribute), false).Length == 1 )
					InstallAddin( type );
			}
		}
		#endregion

		#region Helper Methods
		private bool InstallAddin( Type type )
		{
			ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
			object obj = ctor.Invoke( new object[0] );
			IAddin theAddin = (IAddin)obj;

			return theAddin.Install(this);
		}
		#endregion

		#region Type Safe Install Helpers
        //internal void Install( ISuiteBuilder builder )
        //{
        //    suiteBuilders.Install( builder );
        //}

        //internal void Install( ITestCaseBuilder builder )
        //{
        //    testBuilders.Install( builder );
        //}

        //internal void Install( ITestDecorator decorator )
        //{
        //    testDecorators.Install( decorator );
        //}

        //internal void Install( EventListener listener )
        //{
        //    listeners.Install( listener );
        //}
		#endregion

		#region IService Members

		public void UnloadService()
		{
			// TODO:  Add CoreExtensions.UnloadService implementation
		}

		public void InitializeService()
		{
			InstallBuiltins();
			InstallAddins();

			initialized = true;
		}

		#endregion
	}
}
