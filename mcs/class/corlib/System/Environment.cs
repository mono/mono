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
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;

namespace System {

	[ComVisible (true)]
	public static class Environment {

		/*
		 * This is the version number of the corlib-runtime interface. When
		 * making changes to this interface (by changing the layout
		 * of classes the runtime knows about, changing icall signature or
		 * semantics etc), increment this variable. Also increment the
		 * pair of this variable in the runtime in metadata/appdomain.c.
		 * Changes which are already detected at runtime, like the addition
		 * of icalls, do not require an increment.
		 */
#pragma warning disable 169
		private const int mono_corlib_version = 93;
#pragma warning restore 169

		[ComVisible (true)]
		public enum SpecialFolder
		{	
			MyDocuments = 0x05,
			Desktop = 0x00,
			MyComputer = 0x11,
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
#if NET_4_0 || MOONLIGHT
			MyVideos = 0x0e,
#endif
#if NET_4_0
			NetworkShortcuts = 0x13,
			Fonts = 0x14,
			CommonStartMenu = 0x16,
			CommonPrograms = 0x17,
			CommonStartup = 0x18,
			CommonDesktopDirectory = 0x19,
			PrinterShortcuts = 0x1b,
			Windows = 0x24,
			UserProfile = 0x28,
			SystemX86 = 0x29,
			ProgramFilesX86 = 0x2a,
			CommonProgramFilesX86 = 0x2c,
			CommonTemplates = 0x2d,
			CommonDocuments = 0x2e,
			CommonAdminTools = 0x2f,
			AdminTools = 0x30,
			CommonMusic = 0x35,
			CommonPictures = 0x36,
			CommonVideos = 0x37,
			Resources = 0x38,
			LocalizedResources = 0x39,
			CommonOemLinks = 0x3a,
			CDBurning = 0x3b,
#endif
		}

#if NET_4_0
		public
#endif
		enum SpecialFolderOption {
			None = 0,
			DoNotVerify = 0x4000,
			Create = 0x8000
		}

		/// <summary>
		/// Gets the command line for this process
		/// </summary>
		public static string CommandLine {
			// note: security demand inherited from calling GetCommandLineArgs
			get {
				StringBuilder sb = new StringBuilder ();
				foreach (string str in GetCommandLineArgs ()) {
					bool escape = false;
					string quote = "";
					string s = str;
					for (int i = 0; i < s.Length; i++) {
						if (quote.Length == 0 && Char.IsWhiteSpace (s [i])) {
							quote = "\"";
						} else if (s [i] == '"') {
							escape = true;
						}
					}
					if (escape && quote.Length != 0) {
						s = s.Replace ("\"", "\\\"");
					}
					sb.AppendFormat ("{0}{1}{0} ", quote, s);
				}
				if (sb.Length > 0)
					sb.Length--;
				return sb.ToString ();
			}
		}

		/// <summary>
		/// Gets or sets the current directory. Actually this is supposed to get
		/// and/or set the process start directory acording to the documentation
		/// but actually test revealed at beta2 it is just Getting/Setting the CurrentDirectory
		/// </summary>
		public static string CurrentDirectory
		{
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

		static public extern bool HasShutdownStarted
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
		

		/// <summary>
		/// Gets the name of the local computer
		/// </summary>
		public extern static string MachineName {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			[EnvironmentPermission (SecurityAction.Demand, Read="COMPUTERNAME")]
			[SecurityPermission (SecurityAction.Demand, UnmanagedCode=true)]
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
					PlatformID p = Platform;
					os = new OperatingSystem (p, v);
				}
				return os;
			}
		}

		/// <summary>
		/// Get StackTrace
		/// </summary>
		public static string StackTrace {
			[EnvironmentPermission (SecurityAction.Demand, Unrestricted=true)]
			get {
				System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace (0, true);
				return trace.ToString ();
			}
		}
#if !NET_2_1
		/// <summary>
		/// Get a fully qualified path to the system directory
		/// </summary>
		public static string SystemDirectory {
			get {
				return GetFolderPath (SpecialFolder.System);
			}
		}
#endif
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
			// FIXME: this variable doesn't exist (at least not on WinXP) - reported to MS as FDBK20562
			[EnvironmentPermission (SecurityAction.Demand, Read="USERDOMAINNAME")]
			get {
				return MachineName;
			}
		}

		/// <summary>
		/// Gets a flag indicating whether the process is in interactive mode
		/// </summary>
		[MonoTODO ("Currently always returns false, regardless of interactive state")]
		public static bool UserInteractive {
			get {
				return false;
			}
		}

		/// <summary>
		/// Get the user name of current process is running under
		/// </summary>
		public extern static string UserName {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			[EnvironmentPermission (SecurityAction.Demand, Read="USERNAME;USER")]
			get;
		}

		/// <summary>
		/// Get the version of the common language runtime 
		/// </summary>
		public static Version Version {
			get {
				return new Version (Consts.FxFileVersion);
			}
		}

		/// <summary>
		/// Get the amount of physical memory mapped to process
		/// </summary>
		[MonoTODO ("Currently always returns zero")]
		public static long WorkingSet {
			[EnvironmentPermission (SecurityAction.Demand, Unrestricted=true)]
			get { return 0; }
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode=true)]
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

			StringBuilder result = new StringBuilder ();
			result.Append (name, 0, off1);
			Hashtable tbl = null;
			do {
				string var = name.Substring (off1 + 1, off2 - off1 - 1);
				string value = GetEnvironmentVariable (var);
				if (value == null && Environment.IsRunningOnWindows) {
					// On windows, env. vars. are case insensitive
					if (tbl == null)
						tbl = GetEnvironmentVariablesNoCase ();

					value = tbl [var] as string;
				}
				
				// If value not found, add %FOO to stream,
				//  and use the closing % for the next iteration.
				// If value found, expand it in place of %FOO%
				if (value == null) {
					result.Append ('%');
					result.Append (var);
					off2--;
				} else {
					result.Append (value);
				}
				int oldOff2 = off2;
				off1 = name.IndexOf ('%', off2 + 1);
				// If no % found for off1, don't look for one for off2
				off2 = (off1 == -1 || off2 > len-1)? -1 :name.IndexOf ('%', off1 + 1);
				// textLen is the length of text between the closing % of current iteration
				//  and the starting % of the next iteration if any. This text is added to output
				int textLen;
				// If no new % found, use all the remaining text
				if (off1 == -1 || off2 == -1)
					textLen = len - oldOff2 - 1;
				// If value found in current iteration, use text after current closing % and next %
				else if(value != null)
					textLen = off1 - oldOff2 - 1;
				// If value not found in current iteration, but a % was found for next iteration,
				//  use text from current closing % to the next %.
				else
					textLen = off1 - oldOff2;
				if(off1 >= oldOff2 || off1 == -1)
					result.Append (name, oldOff2+1, textLen);
			} while (off2 > -1 && off2 < len);
				
			return result.ToString ();

		}

		/// <summary>
		/// Return an array of the command line arguments of the current process
		/// </summary>
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[EnvironmentPermissionAttribute (SecurityAction.Demand, Read = "PATH")]
		public extern static string[] GetCommandLineArgs ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string internalGetEnvironmentVariable (string variable);

		/// <summary>
		/// Return a string containing the value of the environment
		/// variable identifed by parameter "variable"
		/// </summary>
		public static string GetEnvironmentVariable (string variable)
		{
#if !NET_2_1
			if (SecurityManager.SecurityEnabled) {
				new EnvironmentPermission (EnvironmentPermissionAccess.Read, variable).Demand ();
			}
#endif
			return internalGetEnvironmentVariable (variable);
		}

		static Hashtable GetEnvironmentVariablesNoCase ()
		{
			Hashtable vars = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
							CaseInsensitiveComparer.Default);

			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = internalGetEnvironmentVariable (name);
			}

			return vars;
		}

		/// <summary>
		/// Return a set of all environment variables and their values
		/// </summary>
#if !NET_2_1
		public static IDictionary GetEnvironmentVariables ()
		{
			StringBuilder sb = null;
			if (SecurityManager.SecurityEnabled) {
				// we must have access to each variable to get the lot
				sb = new StringBuilder ();
				// but (performance-wise) we do not want a stack-walk
				// for each of them so we concatenate them
			}

			Hashtable vars = new Hashtable ();
			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = internalGetEnvironmentVariable (name);
				if (sb != null) {
					sb.Append (name);
					sb.Append (";");
				}
			}

			if (sb != null) {
				new EnvironmentPermission (EnvironmentPermissionAccess.Read, sb.ToString ()).Demand ();
			}
			return vars;
		}
#else
		[EnvironmentPermission (SecurityAction.Demand, Unrestricted=true)]
		public static IDictionary GetEnvironmentVariables ()
		{
			Hashtable vars = new Hashtable ();
			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = internalGetEnvironmentVariable (name);
			}
			return vars;
		}
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string GetWindowsFolderPath (int folder);

		/// <summary>
		/// Returns the fully qualified path of the
		/// folder specified by the "folder" parameter
		/// </summary>
		public static string GetFolderPath (SpecialFolder folder)
		{
			return GetFolderPath (folder, SpecialFolderOption.None);
		}
#if NET_4_0
		[MonoTODO ("Figure out the folder path for all the new values in SpecialFolder. Use the 'option' argument.")]
		public
#endif
		static string GetFolderPath(SpecialFolder folder, SpecialFolderOption option)
		{
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			string dir = null;

			if (Environment.IsRunningOnWindows) {
				dir = GetWindowsFolderPath ((int) folder);
			} else {
				dir = InternalGetFolderPath (folder);
			}
#if !NET_2_1
			if ((dir != null) && (dir.Length > 0) && SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dir).Demand ();
			}
#endif
			return dir;
		}

		private static string ReadXdgUserDir (string config_dir, string home_dir, 
			string key, string fallback)
		{
			string env_path = internalGetEnvironmentVariable (key);
			if (env_path != null && env_path != String.Empty) {
				return env_path;
			}

			string user_dirs_path = Path.Combine (config_dir, "user-dirs.dirs");

			if (!File.Exists (user_dirs_path)) {
				return Path.Combine (home_dir, fallback);
			}

			try {
				using(StreamReader reader = new StreamReader (user_dirs_path)) {
					string line;
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						int delim_index = line.IndexOf ('=');
                        if(delim_index > 8 && line.Substring (0, delim_index) == key) {
                            string path = line.Substring (delim_index + 1).Trim ('"');
                            bool relative = false;

                            if (path.StartsWith ("$HOME/")) {
                                relative = true;
                                path = path.Substring (6);
                            } else if (!path.StartsWith ("/")) {
                                relative = true;
                            }

                            return relative ? Path.Combine (home_dir, path) : path;
                        }
					}
				}
			} catch (FileNotFoundException) {
			}

			return Path.Combine (home_dir, fallback);
		}


		// the security runtime (and maybe other parts of corlib) needs the
		// information to initialize themselves before permissions can be checked
		internal static string InternalGetFolderPath (SpecialFolder folder)
		{
			string home = internalGetHome ();

			// http://freedesktop.org/Standards/basedir-spec/basedir-spec-0.6.html

			// note: skip security check for environment variables
			string data = internalGetEnvironmentVariable ("XDG_DATA_HOME");
			if ((data == null) || (data == String.Empty)) {
				data = Path.Combine (home, ".local");
				data = Path.Combine (data, "share");
			}

			// note: skip security check for environment variables
			string config = internalGetEnvironmentVariable ("XDG_CONFIG_HOME");
			if ((config == null) || (config == String.Empty)) {
				config = Path.Combine (home, ".config");
			}

			switch (folder) {
			// MyComputer is a virtual directory
			case SpecialFolder.MyComputer:
				return String.Empty;

			// personal == ~
			case SpecialFolder.Personal:
#if MONOTOUCH
				return Path.Combine (home, "Documents");
#else
				return home;
#endif
			// use FDO's CONFIG_HOME. This data will be synced across a network like the windows counterpart.
			case SpecialFolder.ApplicationData:
#if MONOTOUCH
			{
				string dir = Path.Combine (Path.Combine (home, "Documents"), ".config");
				if (!Directory.Exists (dir))
					Directory.CreateDirectory (dir);

				return dir;
			}
#else
				return config;
#endif
			//use FDO's DATA_HOME. This is *NOT* synced
			case SpecialFolder.LocalApplicationData:
#if MONOTOUCH
			{
				string dir = Path.Combine (home, "Documents");
				if (!Directory.Exists (dir))
					Directory.CreateDirectory (dir);

				return dir;
			}
#else
				return data;
#endif

			case SpecialFolder.Desktop:
			case SpecialFolder.DesktopDirectory:
				return ReadXdgUserDir (config, home, "XDG_DESKTOP_DIR", "Desktop");

			case SpecialFolder.MyMusic:
				return ReadXdgUserDir (config, home, "XDG_MUSIC_DIR", "Music");

			case SpecialFolder.MyPictures:
				return ReadXdgUserDir (config, home, "XDG_PICTURES_DIR", "Pictures");
				
			// these simply dont exist on Linux
			// The spec says if a folder doesnt exist, we
			// should return ""
			case SpecialFolder.Favorites:
			case SpecialFolder.Programs:
			case SpecialFolder.SendTo:
			case SpecialFolder.StartMenu:
			case SpecialFolder.Startup:
			case SpecialFolder.Templates:
			case SpecialFolder.Cookies:
			case SpecialFolder.History:
			case SpecialFolder.InternetCache:
			case SpecialFolder.Recent:
			case SpecialFolder.CommonProgramFiles:
			case SpecialFolder.ProgramFiles:
			case SpecialFolder.System:
				return String.Empty;
			// This is where data common to all users goes
			case SpecialFolder.CommonApplicationData:
				return "/usr/share";
			default:
				throw new ArgumentException ("Invalid SpecialFolder");
                        }
                }

		[EnvironmentPermission (SecurityAction.Demand, Unrestricted=true)]
		public static string[] GetLogicalDrives ()
		{
			return GetLogicalDrivesInternal ();
		}

#if !NET_2_1
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern void internalBroadcastSettingChange ();

		public static string GetEnvironmentVariable (string variable, EnvironmentVariableTarget target)
		{
			switch (target) {
			case EnvironmentVariableTarget.Process:
				return GetEnvironmentVariable (variable);
			case EnvironmentVariableTarget.Machine:
				new EnvironmentPermission (PermissionState.Unrestricted).Demand ();
				if (!IsRunningOnWindows)
					return null;
				using (Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment")) {
					object regvalue = env.GetValue (variable);
					return (regvalue == null) ? null : regvalue.ToString ();
				}
			case EnvironmentVariableTarget.User:
				new EnvironmentPermission (PermissionState.Unrestricted).Demand ();
				if (!IsRunningOnWindows)
					return null;
				using (Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.CurrentUser.OpenSubKey ("Environment", false)) {
					object regvalue = env.GetValue (variable);
					return (regvalue == null) ? null : regvalue.ToString ();
				}
			default:
				throw new ArgumentException ("target");
			}
		}

		public static IDictionary GetEnvironmentVariables (EnvironmentVariableTarget target)
		{
			IDictionary variables = (IDictionary)new Hashtable ();
			switch (target) {
			case EnvironmentVariableTarget.Process:
				variables = GetEnvironmentVariables ();
				break;
			case EnvironmentVariableTarget.Machine:
				new EnvironmentPermission (PermissionState.Unrestricted).Demand ();
				if (IsRunningOnWindows) {
					using (Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment")) {
						string[] value_names = env.GetValueNames ();
						foreach (string value_name in value_names)
							variables.Add (value_name, env.GetValue (value_name));
					}
				}
				break;
			case EnvironmentVariableTarget.User:
				new EnvironmentPermission (PermissionState.Unrestricted).Demand ();
				if (IsRunningOnWindows) {
					using (Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.CurrentUser.OpenSubKey ("Environment")) {
						string[] value_names = env.GetValueNames ();
						foreach (string value_name in value_names)
							variables.Add (value_name, env.GetValue (value_name));
					}
				}
				break;
			default:
				throw new ArgumentException ("target");
			}
			return variables;
		}

		[EnvironmentPermission (SecurityAction.Demand, Unrestricted=true)]
		public static void SetEnvironmentVariable (string variable, string value)
		{
			SetEnvironmentVariable (variable, value, EnvironmentVariableTarget.Process);
		}

		[EnvironmentPermission (SecurityAction.Demand, Unrestricted = true)]
		public static void SetEnvironmentVariable (string variable, string value, EnvironmentVariableTarget target)
		{
			if (variable == null)
				throw new ArgumentNullException ("variable");
			if (variable == String.Empty)
				throw new ArgumentException ("String cannot be of zero length.", "variable");
			if (variable.IndexOf ('=') != -1)
				throw new ArgumentException ("Environment variable name cannot contain an equal character.", "variable");
			if (variable[0] == '\0')
				throw new ArgumentException ("The first char in the string is the null character.", "variable");

			switch (target) {
			case EnvironmentVariableTarget.Process:
				InternalSetEnvironmentVariable (variable, value);
				break;
			case EnvironmentVariableTarget.Machine:
				if (!IsRunningOnWindows)
					return;
				using (Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true)) {
					if (String.IsNullOrEmpty (value))
						env.DeleteValue (variable, false);
					else
						env.SetValue (variable, value);
					internalBroadcastSettingChange ();
				}
				break;
			case EnvironmentVariableTarget.User:
				if (!IsRunningOnWindows)
					return;
				using (Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.CurrentUser.OpenSubKey ("Environment", true)) {
					if (String.IsNullOrEmpty (value))
						env.DeleteValue (variable, false);
					else
						env.SetValue (variable, value);
					internalBroadcastSettingChange ();
				}
				break;
			default:
				throw new ArgumentException ("target");
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern void InternalSetEnvironmentVariable (string variable, string value);
#endif
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode=true)]
		public static void FailFast (string message)
		{
			throw new NotImplementedException ();
		}

#if NET_4_0 || MOONLIGHT
		[SecurityCritical]
		public static void FailFast (string message, Exception exception)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_4_0
		public static bool Is64BitOperatingSystem {
			get { return IntPtr.Size == 8; } // FIXME: is this good enough?
		}

		public static bool Is64BitProcess {
			get { return Is64BitOperatingSystem; }
		}

		public static int SystemPageSize {
			get { return GetPageSize (); }
		}
#endif

		public static extern int ProcessorCount {
			[EnvironmentPermission (SecurityAction.Demand, Read="NUMBER_OF_PROCESSORS")]
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;			
		}

		// private methods

		internal static bool IsRunningOnWindows {
			get { return ((int) Platform < 4); }
		}
#if !NET_2_1
		//
		// Used by gacutil.exe
		//
#pragma warning disable 169		
		private static string GacPath {
			get {
				if (Environment.IsRunningOnWindows) {
					/* On windows, we don't know the path where mscorlib.dll will be installed */
					string corlibDir = new DirectoryInfo (Path.GetDirectoryName (typeof (int).Assembly.Location)).Parent.Parent.FullName;
					return Path.Combine (Path.Combine (corlibDir, "mono"), "gac");
				}

				return Path.Combine (Path.Combine (internalGetGacPath (), "mono"), "gac");
			}
		}
#pragma warning restore 169
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string internalGetGacPath ();
#endif
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetLogicalDrivesInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetEnvironmentVariableNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string GetMachineConfigPath ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string internalGetHome ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static int GetPageSize ();

		static internal bool IsUnix {
			get {
				int platform = (int) Environment.Platform;

				return (platform == 4 || platform == 128 || platform == 6);
			}
		}
	}
}

