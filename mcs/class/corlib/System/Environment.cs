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
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.IO;
//using System.Diagnostics;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
	public sealed class Environment
	{
		/*
		 * This is the version number of the corlib-runtime interface. When
		 * making changes to this interface (by changing the layout
		 * of classes the runtime knows about, changing icall semantics etc),
		 * increment this variable. Also increment the
		 * pair of this variable in the runtime in metadata/appdomain.c.
		 * Changes which are already detected at runtime, like the addition
		 * of icalls, do not require an increment.
		 */
		private const int mono_corlib_version = 24;

		private Environment ()
		{
		}
                
		[MonoTODO]
		public enum SpecialFolder
		{	// TODO: Determine if these windoze style folder identifiers 
			//       have unix/linux counterparts

#if NET_1_1
			Desktop = 0x00,
			MyComputer = 0x11,
#endif
			Programs = 0x02,
			Personal = 0x05,
			Favorites = 0x06,
			Startup = 0x07,
			Recent = 0x08,
			SendTo = 0x09,
			StartMenu = 0x0b,
			MyMusic = 0x0d,
			DesktopDirectory = 0x10,
			Templates = 0x15,
			ApplicationData	= 0x1a,
			LocalApplicationData = 0x1c,
			InternetCache = 0x20,
			Cookies = 0x21,
			History = 0x22,
			CommonApplicationData	= 0x23,
			System = 0x25,
			ProgramFiles = 0x26,
			MyPictures = 0x27,
			CommonProgramFiles = 0x2b,
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
				return String.Join (" ", GetCommandLineArgs ());
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
			get {
				return Directory.GetCurrentDirectory ();
			}
			set {
				Directory.SetCurrentDirectory (value);
			}
		}

		/// <summary>
		/// Gets or sets the exit code of this process
		/// </summary>
		public extern static int ExitCode
		{	
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			set;
		}

#if NET_1_1
		static
#endif
		public extern bool HasShutdownStarted
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
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

		//
		// Support methods and fields for OSVersion property
		//
		static OperatingSystem os;

		internal static extern PlatformID Platform {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern string GetOSVersionString ();

		/// <summary>
		/// Gets the current OS version information
		/// </summary>
		public static OperatingSystem OSVersion {
			get {
				if (os == null) {
					Version v = Version.CreateFromString (GetOSVersionString ());
					os = new OperatingSystem (Platform, v);
				}
				return os;
			}
		}

		/// <summary>
		/// Get StackTrace
		/// </summary>
		public static string StackTrace {
			get {
				System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace (1);
				return trace.ToString ();
			}
		}

		/// <summary>
		/// Get a fully qualified path to the system directory
		/// </summary>
		public static string SystemDirectory {
			get {
				return GetFolderPath (SpecialFolder.System);
			}
		}

		/// <summary>
		/// Get the number of milliseconds that have elapsed since the system was booted
		/// </summary>
		public extern static int TickCount {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Get UserDomainName
		/// </summary>
		public static string UserDomainName {
			get {
				return MachineName;
			}
		}

		/// <summary>
		/// Gets a flag indicating whether the process is in interactive mode
		/// </summary>
		[MonoTODO]
		public static bool UserInteractive {
			get {
				return false;
			}
		}

		/// <summary>
		/// Get the user name of current process is running under
		/// </summary>
		public extern static string UserName
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Get the version of the common language runtime 
		/// </summary>
		public static Version Version {
			get {
#if NET_2_0
				// FIXME: this is the version number for MS.NET 2.0 beta1. 
				// It must be changed when the final version is released.
				return new Version (2, 0, 40607, 16);
#elif NET_1_1				    
				return new Version (1, 1, 4322, 573);
#else
				return new Version (1, 0, 3705, 288);
#endif
			}
		}

		/// <summary>
		/// Get the amount of physical memory mapped to process
		/// </summary>
		[MonoTODO]
		public static long WorkingSet
		{
			get {
				return 0;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Exit (int exitCode);

		/// <summary>
		/// Substitute environment variables in the argument "name"
		/// </summary>
		public static string ExpandEnvironmentVariables (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			int off1 = name.IndexOf ('%');
			if (off1 == -1)
				return name;

			int len = name.Length;
			int off2 = 0;
			if (off1 == len - 1 || (off2 = name.IndexOf ('%', off1 + 1)) == -1)
				return name;

			PlatformID platform = Platform;
			StringBuilder result = new StringBuilder ();
			result.Append (name, 0, off1);
			Hashtable tbl = null;
			do {
				string var = name.Substring (off1 + 1, off2 - off1 - 1);
				string value = GetEnvironmentVariable (var);
				if (value == null && (int) platform != 128) {
					// On windows, env. vars. are case insensitive
					if (tbl == null)
						tbl = GetEnvironmentVariablesNoCase ();

					value = tbl [var] as string;
				}
				
				if (value == null) {
					result.Append ('%');
					result.Append (var);
					result.Append ('%');
				} else {
					result.Append (value);
				}

				if (off2 + 1 == len) {
					off1 = off2;
					off2 = -1;
				} else {
					off1 = off2 + 1;
					off2 = (off1 + 1 == len) ? -1 : name.IndexOf ('%', off1 + 1);
				}

			} while (off2 != -1);

			if (off1 + 1 < len)
				result.Append (name.Substring (off1));

			return result.ToString ();
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

		static Hashtable GetEnvironmentVariablesNoCase ()
		{
			Hashtable vars = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
							CaseInsensitiveComparer.Default);

			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = GetEnvironmentVariable (name);
			}

			return vars;
		}

		/// <summary>
		/// Return a set of all environment variables and their values
		/// </summary>
	   
		public static IDictionary GetEnvironmentVariables()
		{
			Hashtable vars = new Hashtable ();
			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = GetEnvironmentVariable (name);
			}

			return vars;
		}


		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string GetWindowsFolderPath (int folder);

		/// <summary>
		/// Returns the fully qualified path of the
		/// folder specified by the "folder" parameter
		/// </summary>
		public static string GetFolderPath (SpecialFolder folder)
		{
			if ((int) Platform != 128)
				return GetWindowsFolderPath ((int) folder);

			string home = internalGetHome ();

			// http://freedesktop.org/Standards/basedir-spec/basedir-spec-0.6.html
			string data = GetEnvironmentVariable ("XDG_DATA_HOME");
			if ((data == null) || (data == String.Empty)) {
				data = Path.Combine (home, ".local");
				data = Path.Combine (data, "share");
			}

			string config = GetEnvironmentVariable ("XDG_CONFIG_HOME");
			if ((config == null) || (config == String.Empty)) {
				config = Path.Combine (home, ".config");
			}

			switch (folder) {
#if NET_1_1
			// MyComputer is a virtual directory
			case SpecialFolder.MyComputer:
				return "";
#endif
			// personal == ~
			case SpecialFolder.Personal:
				return home;
			// use FDO's CONFIG_HOME. This data will be synced across a network like the windows counterpart.
			case SpecialFolder.ApplicationData:
				return config;
			//use FDO's DATA_HOME. This is *NOT* synced
			case SpecialFolder.LocalApplicationData:
				return data;
#if NET_1_1
			case SpecialFolder.Desktop:
#endif
			case SpecialFolder.DesktopDirectory:
				return Path.Combine (home, "Desktop");
			
			// these simply dont exist on Linux
			// The spec says if a folder doesnt exist, we
			// should return ""
			case SpecialFolder.Favorites:
			case SpecialFolder.Programs:
			case SpecialFolder.SendTo:
			case SpecialFolder.StartMenu:
			case SpecialFolder.Startup:
			case SpecialFolder.MyMusic:
			case SpecialFolder.MyPictures:
			case SpecialFolder.Templates:
			case SpecialFolder.Cookies:
			case SpecialFolder.History:
			case SpecialFolder.InternetCache:
			case SpecialFolder.Recent:
			case SpecialFolder.CommonProgramFiles:
			case SpecialFolder.ProgramFiles:
			case SpecialFolder.System:
				return "";
			// This is where data common to all users goes
			case SpecialFolder.CommonApplicationData:
				return "/usr/share";
			default:
				throw new ArgumentException ("Invalid SpecialFolder");
                        }
                }

		public static string[] GetLogicalDrives ()
		{
			return GetLogicalDrivesInternal ();
		}

		static internal string GetResourceString (string s) { return ""; }

                
#if NET_2_0
		public static string GetEnvironmentVariable (string variable, EnvironmentVariableTarget target)
		{
			return (string)(GetEnvironmentVariables (target) [variable]);
		}

		[MonoTODO]
		public static IDictionary GetEnvironmentVariables (EnvironmentVariableTarget target)
		{
			throw new NotImplementedException ();
		}

		public static void SetEnvironmentVariable (string variable, string value)
		{
			SetEnvironmentVariable (variable, value, EnvironmentVariableTarget.Process);
		}

		[MonoTODO]
		public static void SetEnvironmentVariable (string variable, string value, EnvironmentVariableTarget target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		static bool IsServerGC {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		static int ProcessorCount {
			get {
				throw new NotImplementedException ();
			}
		}
#endif                
                
		// private methods

		private static string GacPath {
			get {
				if ((int) Platform != 128) {
					/* On windows, we don't know the path where mscorlib.dll will be installed */
					string corlibDir = new DirectoryInfo (Path.GetDirectoryName (typeof (int).Assembly.Location)).Parent.Parent.FullName;
					return Path.Combine (Path.Combine (corlibDir, "mono"), "gac");
				}

				return Path.Combine (Path.Combine (internalGetGacPath (), "mono"), "gac");
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetLogicalDrivesInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetEnvironmentVariableNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string GetMachineConfigPath ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string internalGetGacPath ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string internalGetHome ();
	}
}

