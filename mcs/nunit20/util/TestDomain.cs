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

using System;

namespace NUnit.Util
{
	using System.Runtime.Remoting;
	using System.Security.Policy;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.IO;

	using NUnit.Core;

	public class TestDomain : TestRunner
	{
		#region Instance Variables

		/// <summary>
		/// The appdomain used  to load tests
		/// </summary>
		private AppDomain domain; 

		/// <summary>
		/// The path to our cache
		/// </summary>
		private string cachePath;
		
		/// <summary>
		/// The remote runner loaded in the test appdomain
		/// </summary>
		private TestRunner testRunner;

		/// <summary>
		/// Writer for console standard output
		/// </summary>
		private TextWriter outWriter;

		/// <summary>
		/// Writer for console error output
		/// </summary>
		private TextWriter errorWriter;

		/// <summary>
		/// Holds the event listener while we are running
		/// </summary>
		private EventListener listener;

		/// <summary>
		/// Indicate whether files should be shadow copied
		/// </summary>
		private bool shadowCopyFiles = true;

		#endregion

		#region Properties

		public AppDomain AppDomain
		{
			get { return domain; }
		}

		public TextWriter Out
		{
			get { return outWriter; }
			set { outWriter = value; }
		}

		public TextWriter Error
		{
			get { return errorWriter; }
			set { errorWriter = value; }
		}

		private TestRunner Runner
		{
			get 
			{
				if ( testRunner == null )
					testRunner = MakeRemoteTestRunner( domain );

				return testRunner; 
			}
		}

		public bool DisplayTestLabels
		{
			get { return Runner.DisplayTestLabels; }
			set { Runner.DisplayTestLabels = value; }
		}

		private TestRunner MakeRemoteTestRunner( AppDomain runnerDomain )
		{
			object obj = runnerDomain.CreateInstanceAndUnwrap(
				typeof(RemoteTestRunner).Assembly.FullName, 
				typeof(RemoteTestRunner).FullName,
				false, BindingFlags.Default,null,null,null,null,null);
			
			RemoteTestRunner runner = (RemoteTestRunner) obj;

			runner.Out = this.outWriter;
			runner.Error = this.errorWriter;

			return runner;
		}

		public Version FrameworkVersion
		{
			get { return Runner.FrameworkVersion; }
		}

		public bool ShadowCopyFiles
		{
			get { return shadowCopyFiles; }
			set
			{
				if ( this.domain != null )
					throw new ArgumentException( "ShadowCopyFiles may not be set after domain is created" );
				shadowCopyFiles = value;
			}
		}

		public TestResult[] Results
		{
			get { return Runner.Results; }
		}

		public TestResult Result
		{
			get { return Runner.Result; }
		}

		#endregion

		#region Constructors

		public TestDomain( TextWriter outWriter, TextWriter errorWriter )
		{ 
			this.outWriter = outWriter;
			this.errorWriter = errorWriter;
		}

		public TestDomain() : this( TextWriter.Null, TextWriter.Null ) { }

		#endregion

		#region Loading and Unloading Tests

		public Test Load( string assemblyFileName )
		{
			return Load( assemblyFileName, string.Empty );
		}

		public Test Load(string assemblyFileName, string testFixture)
		{
			Unload();

			try
			{
				CreateDomain( assemblyFileName );
				string assemblyPath = Path.GetFullPath( assemblyFileName );

				if ( testFixture != null && testFixture != string.Empty )
					return Runner.Load( assemblyPath, testFixture );
				else
					return Runner.Load( assemblyPath );
			}
			catch
			{
				Unload();
				throw;
			}
		}

		public Test Load( string testFileName, string[] assemblies )
		{
			return Load( testFileName, assemblies, null );
		}

		public Test Load( string testFileName, string[] assemblies, string testFixture )
		{
			FileInfo testFile = new FileInfo( testFileName );		
			return Load( testFileName, testFile.DirectoryName, testFile.FullName + ".config", GetBinPath(assemblies), assemblies, testFixture );
		}

		public Test Load( string testFileName, string appBase, string configFile, string binPath, string[] assemblies, string testFixture )
		{
			Unload();

			try
			{
				CreateDomain( testFileName, appBase, configFile, binPath, assemblies );

				if ( testFixture != null )
					return Runner.Load( testFileName, assemblies, testFixture );
				else
					return Runner.Load( testFileName, assemblies );
			}
			catch
			{
				Unload();
				throw;
			}
		}

		public Test Load( NUnitProject project )
		{
			return Load( project, null );
		}

		public Test Load( NUnitProject project, string testFixture )
		{
			ProjectConfig cfg = project.ActiveConfig;

			if ( project.IsAssemblyWrapper )
				return Load( cfg.Assemblies[0].FullPath, testFixture );
			else
				return Load( project.ProjectPath, cfg.BasePath, cfg.ConfigurationFile, cfg.PrivateBinPath, cfg.TestAssemblies, testFixture );
		}

		public void Unload()
		{
			testRunner = null;

			if(domain != null) 
			{
				try
				{
					AppDomain.Unload(domain);
					if ( this.ShadowCopyFiles )
						DeleteCacheDir( new DirectoryInfo( cachePath ) );
				}
				catch( CannotUnloadAppDomainException )
				{
					// TODO: Do something useful. For now we just
					// leave the orphaned AppDomain "out there"
					// rather than aborting the application.
				}
				finally
				{
					domain = null;
				}
			}
		}

		public static string GetBinPath( string[] assemblies )
		{
			ArrayList dirs = new ArrayList();
			string binPath = null;

			foreach( string path in assemblies )
			{
				string dir = Path.GetDirectoryName( Path.GetFullPath( path ) );
				if ( !dirs.Contains( dir ) )
				{
					dirs.Add( dir );

					if ( binPath == null )
						binPath = dir;
					else
						binPath = binPath + ";" + dir;
				}
			}

			return binPath;
		}

		#endregion

		#region Counting Tests

		public int CountTestCases()
		{
			return Runner.CountTestCases();
		}

		public int CountTestCases( string testName )
		{
			return Runner.CountTestCases( testName );
		}

		
		public int CountTestCases( string[] testNames )
		{
			return Runner.CountTestCases( testNames );
		}

		#endregion

		public ICollection GetCategories()
		{
			return Runner.GetCategories();
		}

		#region Running Tests

//		public TestResult Run(NUnit.Core.EventListener listener, IFilter filter)
//		{
//			return Runner.Run( listener, filter );
//		}

		public void SetFilter( IFilter filter )
		{
			Runner.SetFilter( filter );
		}

		public TestResult Run(NUnit.Core.EventListener listener)
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener;
				return Runner.Run( listener );
			}
		}
		
		public TestResult Run(NUnit.Core.EventListener listener, string testName)
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener;
				return Runner.Run( listener, testName );
			}
		}

		public TestResult[] Run(NUnit.Core.EventListener listener, string[] testNames)
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener;
				return Runner.Run( listener, testNames );
			}
		}

		public void RunTest(NUnit.Core.EventListener listener )
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener;
				Runner.RunTest( listener );
			}
		}
		
		public void RunTest(NUnit.Core.EventListener listener, string testName )
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener;
				Runner.RunTest( listener, testName );
			}
		}

		public void RunTest(NUnit.Core.EventListener listener, string[] testNames)
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener;
				Runner.RunTest( listener, testNames );
			}
		}

		public void CancelRun()
		{
			Runner.CancelRun();
		}

		public void Wait()
		{
			Runner.Wait();
		}

		// For now, just publish any unhandled exceptions and let the listener
		// figure out what to do with them.
		private void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			this.listener.UnhandledException( (Exception)e.ExceptionObject );
		}

		#endregion

		#region Helpers Used in AppDomain Creation and Removal

		/// <summary>
		/// Construct an application domain for testing a single assembly
		/// </summary>
		/// <param name="assemblyFileName">The assembly file name</param>
		private void CreateDomain( string assemblyFileName )
		{
			FileInfo testFile = new FileInfo( assemblyFileName );
			
			string assemblyPath = Path.GetFullPath( assemblyFileName );
			string domainName = string.Format( "domain-{0}", Path.GetFileName( assemblyFileName ) );

			domain = MakeAppDomain( domainName, testFile.DirectoryName, testFile.FullName + ".config", testFile.DirectoryName );
		}

		/// <summary>
		/// Construct an application domain for testing multiple assemblies
		/// </summary>
		/// <param name="testFileName">The file name of the project file</param>
		/// <param name="appBase">The application base path</param>
		/// <param name="configFile">The configuration file to use</param>
		/// <param name="binPath">The private bin path</param>
		/// <param name="assemblies">A collection of assemblies to load</param>
		private void CreateDomain( string testFileName, string appBase, string configFile, string binPath, string[] assemblies )
		{
			string domainName = string.Format( "domain-{0}", Path.GetFileName( testFileName ) );
			domain = MakeAppDomain( testFileName, appBase, configFile, binPath );
		}

		/// <summary>
		/// This method creates appDomains for the framework.
		/// </summary>
		/// <param name="domainName">Name of the domain</param>
		/// <param name="appBase">ApplicationBase for the domain</param>
		/// <param name="configFile">ConfigurationFile for the domain</param>
		/// <param name="binPath">PrivateBinPath for the domain</param>
		/// <returns></returns>
		private AppDomain MakeAppDomain( string domainName, string appBase, string configFile, string binPath )
		{
			Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
			Evidence evidence = new Evidence(baseEvidence);

			AppDomainSetup setup = new AppDomainSetup();

			// We always use the same application name
			setup.ApplicationName = "Tests";
			// Note that we do NOT
			// set ShadowCopyDirectories because we  rely on the default
			// setting of ApplicationBase plus PrivateBinPath
			if ( this.ShadowCopyFiles )
			{
				setup.ShadowCopyFiles = "true";
				setup.ShadowCopyDirectories = appBase;
			}
			else
			{
				setup.ShadowCopyFiles = "false";
			}

			setup.ApplicationBase = appBase;
			setup.ConfigurationFile =  configFile;
			setup.PrivateBinPath = binPath;

			AppDomain runnerDomain = AppDomain.CreateDomain(domainName, evidence, setup);
			
			if ( this.ShadowCopyFiles )
				ConfigureCachePath(runnerDomain);

			return runnerDomain;
		}

		/// <summary>
		/// Set the location for caching and delete any old cache info
		/// </summary>
		/// <param name="domain">Our domain</param>
		private void ConfigureCachePath(AppDomain domain)
		{
			cachePath = String.Format(@"{0}\{1}", 
				ConfigurationSettings.AppSettings["shadowfiles.path"], DateTime.Now.Ticks);
			cachePath = Environment.ExpandEnvironmentVariables(cachePath);

			DirectoryInfo dir = new DirectoryInfo(cachePath);
			if(dir.Exists) dir.Delete(true);

			domain.SetCachePath(cachePath);

			return;
		}

		/// <summary>
		/// Helper method to delete the cache dir. This method deals 
		/// with a bug that occurs when pdb files are marked read-only.
		/// </summary>
		/// <param name="cacheDir"></param>
		private void DeleteCacheDir( DirectoryInfo cacheDir )
		{
			if(cacheDir.Exists)
			{
				foreach( DirectoryInfo dirInfo in cacheDir.GetDirectories() )
				{
					dirInfo.Attributes &= ~FileAttributes.ReadOnly;
					DeleteCacheDir( dirInfo );
				}

				foreach( FileInfo fileInfo in cacheDir.GetFiles() )
				{
					fileInfo.Attributes &= ~FileAttributes.ReadOnly;
				}

				cacheDir.Delete(true);
			}
		}
		
		#endregion
	}
}
