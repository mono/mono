//------------------------------------------------------------------------------
// 
// System.Environment.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.IO;
//using System.Diagnostics;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace System
{
	public sealed class Environment
	{
		private Environment () {}

		[MonoTODO]
		public enum SpecialFolder
		{	// TODO: Determine if these windoze style folder identifiers 
			//       have unix/linux counterparts
			ApplicationData,
			CommonApplicationData,
			CommonProgramFiles,
			Cookies,
			DesktopDirectory,
			Favorites,
			History,
			InternetCache,
			LocalApplicationData,
			Personal,
			ProgramFiles,
			Programs,
			Recent,
			SendTo,
			StartMenu,
			Startup,
			System,
			Templates
		}

		// TODO: Make sure the security attributes do what I expect
			
		/// <summary>
		/// Gets the command line for this process
		/// </summary>
		public static string CommandLine
		{	// TODO: Coordinate with implementor of EnvironmentPermissionAttribute
			// [EnvironmentPermissionAttribute(SecurityAction.Demand, Read = "COMMANDLINE")]
			get
			{
				// FIXME: we may need to quote, but any sane person
				// should use GetCommandLineArgs () instead.
				return String.Join ("", GetCommandLineArgs ());
			}
		}

		/// <summary>
		/// Gets or sets the current directory. Actually this is supposed to get
		/// and/or set the process start directory acording to the documentation
		/// but actually test revealed at beta2 it is just Getting/Setting the CurrentDirectory
		/// </summary>
		public static string CurrentDirectory
		{
			// originally it was my thought that the external call would be made in
			// the directory class however that class has additional security requirements
			// so the Directory class will call this class for its get/set current directory
			
			// [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
			get
			{
				return MonoIO.GetCurrentDirectory ();
			}
			[MonoTODO("disabled because of compile error. Need mcs magic.")]
			//[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				MonoIO.SetCurrentDirectory (value);
			}
		}

		/// <summary>
		/// Gets or sets the exit code of this process
		/// </summary>
		[MonoTODO]
		public static int ExitCode
		{	// TODO: find a way to implement this property
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets the name of the local computer
		/// </summary>
		public extern static string MachineName {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Gets the standard new line value
		/// </summary>
		public extern static string NewLine {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Gets the current OS version information
		/// </summary>
		[MonoTODO]
		public static OperatingSystem OSVersion {
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get StackTrace
		/// </summary>
		[MonoTODO]
		public static string StackTrace
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get a fully qualified path to the system directory
		/// </summary>
		public static string SystemDirectory
		{
			get
			{
				return GetFolderPath(SpecialFolder.System);
			}
		}

		/// <summary>
		/// Get the number of milliseconds that have elapsed since the system was booted
		/// </summary>
		[MonoTODO]
		public static int TickCount
		{
			get
			{
				return 0;
				//return getTickCount();
			}
		}

		/// <summary>
		/// Get UserDomainName
		/// </summary>
		[MonoTODO]
		public static string UserDomainName
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Gets a flag indicating whether the process is in interactive mode
		/// </summary>
		[MonoTODO]
		public static bool UserInteractive
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Get the user name of current process is running under
		/// </summary>
		[MonoTODO]
		public static string UserName
		{
			get
			{
				// TODO: needs more research/work/thought
				string result = GetEnvironmentVariable("USERNAME");
				if(result == null || result.Equals(string.Empty))
				{
					result = GetEnvironmentVariable("USER");
				}
				return result;
			}
		}

		/// <summary>
		/// Get the version of an assembly
		/// </summary>
		[MonoTODO]
		public static Version Version
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get the amount of physical memory mapped to process
		/// </summary>
		[MonoTODO]
		public static long WorkingSet
		{
			get
			{
				return 0;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Exit(int exitCode);

		/// <summary>
		/// Substitute environment variables in the argument "name"
		/// </summary>
		[MonoTODO]
		public static string ExpandEnvironmentVariables(string name)
		{
			return name;
		}

		/// <summary>
		/// Return an array of the command line arguments of the current process
		/// </summary>
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string[] GetCommandLineArgs();

		/// <summary>
		/// Return a string containing the value of the environment
		/// variable identifed by parameter "variable"
		/// </summary>
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string GetEnvironmentVariable (string name);

		/// <summary>
		/// Return a set of all environment variables and their values
		/// </summary>
	   
		public static IDictionary GetEnvironmentVariables()
		{
			Hashtable vars = new Hashtable ();
			foreach (string name in GetEnvironmentVariableNames ())
				vars [name] = GetEnvironmentVariable (name);
			
			return vars;
		}

		/// <summary>
		/// Returns the fully qualified path of the
		/// folder specified by the "folder" parameter
		/// </summary>
		[MonoTODO]
		public static string GetFolderPath(SpecialFolder folder)
		{
			return null;
		}

		/// <summary>
		/// Returns an array of the logical drives
		/// </summary>
		[MonoTODO]
		public static string[] GetLogicalDrives()
		{
			return null;
		}

		// private methods

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetEnvironmentVariableNames ();

	}
}
