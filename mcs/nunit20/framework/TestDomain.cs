#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright © 2000-2002 Philip A. Craig
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
' Portions Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright © 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;

namespace NUnit.Framework
{
	using NUnit.Core;
	using System.Runtime.Remoting;
	using System.Security.Policy;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.IO;

	/// <summary>
	/// Summary description for TestDomain.
	/// </summary>
	public class TestDomain
	{
		private string assemblyName; 
		private AppDomain domain; 
		private string cachePath;
		private RemoteTestRunner testRunner;
		private TextWriter outStream;
		private TextWriter errorStream;

		public TestDomain(TextWriter outStream, TextWriter errorStream)
		{
			this.outStream = outStream;
			this.errorStream = errorStream;
		}

		private void ThrowIfAlreadyLoaded()
		{
			if ( domain != null || testRunner != null )
				throw new InvalidOperationException( "TestDomain already loaded" );
		}

		public Test Load(string assemblyFileName)
		{
			ThrowIfAlreadyLoaded();

			assemblyName = assemblyFileName; 
			FileInfo file = new FileInfo(assemblyFileName);

			try
			{
				domain = MakeAppDomain(file);
				testRunner = MakeRemoteTestRunner(file, domain);
				return testRunner.Test;
			}
			catch (Exception e)
			{
				Unload();
				throw e;
			}
		}

		public Test Load(string testFixture, string assemblyFileName)
		{
			ThrowIfAlreadyLoaded();

			assemblyName = assemblyFileName; 
			FileInfo file = new FileInfo(assemblyFileName);

			try
			{
				domain = MakeAppDomain(file);

				testRunner = (
					RemoteTestRunner) domain.CreateInstanceAndUnwrap(
					typeof(RemoteTestRunner).Assembly.FullName, 
					typeof(RemoteTestRunner).FullName,
					false, BindingFlags.Default,null,null,null,null,null);
			
				if(testRunner != null)
				{
					testRunner.Initialize(testFixture, file.FullName);
					domain.DoCallBack(new CrossAppDomainDelegate(testRunner.BuildSuite));
					return testRunner.Test;
				}
				else
				{
					Unload();
					return null;
				}
			}
			catch (Exception e)
			{
				Unload();
				throw e;
			}
		}

		public string AssemblyName
		{
			get { return assemblyName; }
		}

		public string TestName
		{
			get { return testRunner.TestName; }
			set { testRunner.TestName = value; }
		}

		public TestResult Run(NUnit.Core.EventListener listener)
		{
			return testRunner.Run(listener, outStream, errorStream);
		}

		public void Unload()
		{
			testRunner = null;

			if(domain != null) 
			{
				AppDomain.Unload(domain);
				DirectoryInfo cacheDir = new DirectoryInfo(cachePath);
				if(cacheDir.Exists) cacheDir.Delete(true);
			}
			domain = null;
		}

		private AppDomain MakeAppDomain(FileInfo file)
		{
			AppDomainSetup setup = new AppDomainSetup();
			setup.ApplicationBase = Directory.GetDirectoryRoot(file.DirectoryName);
			setup.PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
				+ ";" + file.DirectoryName;
			setup.ApplicationName = "Tests";

			setup.ShadowCopyFiles = "true";
			setup.ShadowCopyDirectories = file.DirectoryName;


			setup.ConfigurationFile = file.DirectoryName + @"\" +
				file.Name + ".config";

			Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
			Evidence evidence = new Evidence(baseEvidence);

			string domainName = String.Format("domain-{0}", file.Name);
			AppDomain runnerDomain = AppDomain.CreateDomain(domainName, evidence, setup);
			ConfigureCachePath(runnerDomain);
			return runnerDomain;
		}

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

		private static RemoteTestRunner MakeRemoteTestRunner(FileInfo file, AppDomain runnerDomain)
		{
			RemoteTestRunner runner = (
				RemoteTestRunner) runnerDomain.CreateInstanceAndUnwrap(
				typeof(RemoteTestRunner).Assembly.FullName, 
				typeof(RemoteTestRunner).FullName,
				true, BindingFlags.Default,null,null,null,null,null);
			if(runner != null)
			{
				runner.Initialize(file.FullName);
				runnerDomain.DoCallBack(new CrossAppDomainDelegate(runner.BuildSuite));
			}
			return runner;
		}
	}
}
