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

namespace NUnit.Core
{
	using System.Runtime.Remoting;
	using System.Security.Policy;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.IO;

	public class TestDomain
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
		private RemoteTestRunner testRunner;

		#endregion

		#region Properties

		public bool IsTestLoaded
		{
			get { return testRunner != null; }
		}

		public Test Test
		{
			get { return IsTestLoaded ? testRunner.Test : null; }
		}

		public string TestName
		{
			get { return testRunner.TestName; }
			set { testRunner.TestName = value; }
		}

		#endregion

		#region Public Members

		#region Load a single assembly

		public Test LoadAssembly( string assemblyFileName )
		{
			return LoadAssembly( assemblyFileName, null );
		}

		public Test LoadAssembly(string assemblyFileName, string testFixture)
		{
			FileInfo testFile = new FileInfo( assemblyFileName );

			ThrowIfAlreadyLoaded();

			try
			{
				string assemblyPath = Path.GetFullPath( assemblyFileName );

				string domainName = string.Format( "domain-{0}", Path.GetFileName( assemblyFileName ) );
				domain = MakeAppDomain( domainName, testFile.DirectoryName, testFile.FullName + ".config", testFile.DirectoryName );
				testRunner = MakeRemoteTestRunner( domain );

				if(testRunner != null)
				{
					testRunner.TestFileName = assemblyPath;
					if ( testFixture != null )
						testRunner.TestName = testFixture;
					domain.DoCallBack( new CrossAppDomainDelegate( testRunner.BuildSuite ) );
					return testRunner.Test;
				}
				else
				{
					Unload();
					return null;
				}
			}
			catch
			{
				Unload();
				throw;
			}
		}

		#endregion

		#region Load multiple assemblies

		public Test LoadAssemblies( string testFileName, IList assemblies )
		{
			return LoadAssemblies( testFileName, assemblies, null );
		}

		public Test LoadAssemblies( string testFileName, IList assemblies, string testFixture )
		{
			FileInfo testFile = new FileInfo( testFileName );		
			return LoadAssemblies( testFileName, testFile.DirectoryName, testFile.FullName + ".config", GetBinPath(assemblies), assemblies, testFixture );
		}

		public Test LoadAssemblies( string testFileName, string appBase, string configFile, string binPath, IList assemblies, string testFixture )
		{
			ThrowIfAlreadyLoaded();

			try
			{
				string domainName = string.Format( "domain-{0}", Path.GetFileName( testFileName ) );
				domain = MakeAppDomain( testFileName, appBase, configFile, binPath );
				testRunner = MakeRemoteTestRunner( domain );

				if(testRunner != null)
				{
					testRunner.TestFileName = testFileName;
					testRunner.Assemblies = assemblies;
					if ( testFixture != null )
						testRunner.TestName = testFixture;
					domain.DoCallBack( new CrossAppDomainDelegate( testRunner.BuildSuite ) );
					return testRunner.Test;
				}
				else
				{
					Unload();
					return null;
				}
			}
			catch
			{
				Unload();
				throw;
			}
		}

		#endregion

		public TestResult Run(NUnit.Core.EventListener listener, TextWriter outStream, TextWriter errorStream )
		{
			return testRunner.Run(listener, outStream, errorStream);
		}

		public void Unload()
		{
			testRunner = null;

			if(domain != null) 
			{
				try
				{
					AppDomain.Unload(domain);
					DirectoryInfo cacheDir = new DirectoryInfo(cachePath);
					if(cacheDir.Exists) cacheDir.Delete(true);
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

		#endregion

		#region Helper Methods

		private void ThrowIfAlreadyLoaded()
		{
			if ( domain != null || testRunner != null )
				throw new InvalidOperationException( "TestDomain already loaded" );
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
			// We always want to do shadow copying. Note that we do NOT
			// set ShadowCopyDirectories because we  rely on the default
			// setting of ApplicationBase plus PrivateBinPath
			setup.ShadowCopyFiles = "true";
			setup.ShadowCopyDirectories = appBase;

			setup.ApplicationBase = appBase;
			setup.ConfigurationFile =  configFile;
			setup.PrivateBinPath = binPath;

			AppDomain runnerDomain = AppDomain.CreateDomain(domainName, evidence, setup);
			
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

		private static RemoteTestRunner MakeRemoteTestRunner( AppDomain runnerDomain )
		{
			object obj = runnerDomain.CreateInstanceAndUnwrap(
				typeof(RemoteTestRunner).Assembly.FullName, 
				typeof(RemoteTestRunner).FullName,
				false, BindingFlags.Default,null,null,null,null,null);
			
			return (RemoteTestRunner) obj;
		}

		public static string GetBinPath( IList assemblies )
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
	}
}
