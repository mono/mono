//------------------------------------------------------------------------------
// 
// System.Environment.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Security;
using System.PAL;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System
{
	public sealed class Environment
	{
		private static OpSys _os = Platform.OS;

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
			[EnvironmentPermissionAttribute(SecurityAction.Demand, Read = "COMMANDLINE")]
			get
			{
				return _os.CommandLine;
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
			
			[EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
			get
			{
				return _os.GetCurrentDirectory();
			}
			[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				_os.SetCurrentDirectory(value);
			}
		}

		/// <summary>
		/// Gets or sets the exit code of this process
		/// </summary>
		public static int ExitCode
		{	// TODO: find a way to implement this property
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets the name of the local computer
		/// </summary>
		public static string MachineName
		{
			get
			{
				return _os.MachineName;
			}
		}

		/// <summary>
		/// Gets the standard new line value
		/// </summary>
		public static string NewLine
		{
			get
			{
				return _os.NewLineSequence;
			}
		}

		/// <summary>
		/// Gets the current OS version information
		/// </summary>
		public static OperatingSystem OSVersion
		{
			get
			{
				return _os.OSVersion;
			}
		}

		/// <summary>
		/// Get StackTrace
		/// </summary>
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
		public static long WorkingSet
		{
			get
			{
				return 0;
			}
		}

		public static void Exit(int exitCode)
		{ 
		}

		/// <summary>
		/// Substitute environment variables in the argument "name"
		/// </summary>
		public static string ExpandEnvironmentVariables(string name)
		{
			return name;
		}

		/// <summary>
		/// Return an array of the command line arguments of the current process
		/// </summary>
		public static string[] GetCommandLineArgs()
		{
			char[] delimiter = new char[1];
			delimiter[0] = ' ';
			return _os.CommandLine.Split(delimiter);
		}

		/// <summary>
		/// Return a string containing the value of the environment
		/// variable identifed by parameter "variable"
		/// </summary>
		public static string GetEnvironmentVariable(string variable)
		{
			return _os.GetEnvironmentVariable(variable);
		}

		/// <summary>
		/// Return a set of all environment variables and their values
		/// </summary>
	   
		public static IDictionary GetEnvironmentVariables()
		{
			return _os.EnvironmentVariables;
		}

		/// <summary>
		/// Returns the fully qualified path of the
		/// folder specified by the "folder" parameter
		/// </summary>
		public static string GetFolderPath(SpecialFolder folder)
		{
			return null;
		}

		/// <summary>
		/// Returns an array of the logical drives
		/// </summary>
		public static string[] GetLogicalDrives()
		{
			return null;
		}

	}
}
