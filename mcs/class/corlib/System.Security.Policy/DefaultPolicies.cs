//
// System.Security.Policy.DefaultPolicies.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security.Permissions;

namespace System.Security.Policy {

	/* NOTES
	 *
	 * [1]	Some permissions classes are defined _outside_ mscorlib.dll.
	 * 	In this case we're using SecurityElement to construct the 
	 * 	permissions manually.
	 *
	 */

#if NET_2_0
	internal static class DefaultPolicies {

		public static class ReservedNames {
#else
	internal sealed class DefaultPolicies {

		public sealed class ReservedNames {

			internal ReservedNames ()
			{
			}
#endif
			public const string FullTrust = "FullTrust";
			public const string LocalIntranet = "LocalIntranet";
			public const string Internet = "Internet";
			public const string SkipVerification = "SkipVerification";
			public const string Execution = "Execution";
			public const string Nothing = "Nothing";
			public const string Everything = "Everything";

			static public bool IsReserved (string name) 
			{
				switch (name) {
				case FullTrust:
				case LocalIntranet:
				case Internet:
				case SkipVerification:
				case Execution:
				case Nothing:
				case Everything:
					return true;
				default:
					return false;
				}
			}
		}

#if NET_2_0
		private const string DnsPermissionClass = "System.Net.DnsPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string EventLogPermissionClass = "System.Diagnostics.EventLogPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string PrintingPermissionClass = "System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string SocketPermissionClass = "System.Net.SocketPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string WebPermissionClass = "System.Net.WebPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string PerformanceCounterPermissionClass = "System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string DirectoryServicesPermissionClass = "System.DirectoryServices.DirectoryServicesPermission, System.DirectoryServices, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string MessageQueuePermissionClass = "System.Messaging.MessageQueuePermission, System.Messaging, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string ServiceControllerPermissionClass = "System.ServiceProcess.ServiceControllerPermission, System.ServiceProcess, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string OleDbPermissionClass = "System.Data.OleDb.OleDbPermission, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string SqlClientPermissionClass = "System.Data.SqlClient.SqlClientPermission, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

		private const string DataProtectionPermissionClass = "System.Security.Permissions.DataProtectionPermission, System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string StorePermissionClass = "System.Security.Permissions.StorePermission, System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
		private const string DnsPermissionClass = "System.Net.DnsPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string EventLogPermissionClass = "System.Diagnostics.EventLogPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string PrintingPermissionClass = "System.Drawing.Printing.PrintingPermission, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string SocketPermissionClass = "System.Net.SocketPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string WebPermissionClass = "System.Net.WebPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string PerformanceCounterPermissionClass = "System.Diagnostics.PerformanceCounterPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string DirectoryServicesPermissionClass = "System.DirectoryServices.DirectoryServicesPermission, System.DirectoryServices, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string MessageQueuePermissionClass = "System.Messaging.MessageQueuePermission, System.Messaging, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string ServiceControllerPermissionClass = "System.ServiceProcess.ServiceControllerPermission, System.ServiceProcess, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		private const string OleDbPermissionClass = "System.Data.OleDb.OleDbPermission, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string SqlClientPermissionClass = "System.Data.SqlClient.SqlClientPermission, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
#endif

		private static NamedPermissionSet _fullTrust;
		private static NamedPermissionSet _localIntranet;
		private static NamedPermissionSet _internet;
		private static NamedPermissionSet _skipVerification;
		private static NamedPermissionSet _execution;
		private static NamedPermissionSet _nothing;
		private static NamedPermissionSet _everything;

		public static PermissionSet GetSpecialPermissionSet (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			switch (name) {
			case ReservedNames.FullTrust:
				return FullTrust;
			case ReservedNames.LocalIntranet:
				return LocalIntranet;
			case ReservedNames.Internet:
				return Internet;
			case ReservedNames.SkipVerification:
				return SkipVerification;
			case ReservedNames.Execution:
				return Execution;
			case ReservedNames.Nothing:
				return Nothing;
			case ReservedNames.Everything:
				return Everything;
			default:
				return null;
			}
		}

		public static PermissionSet FullTrust {
			get {
				if (_fullTrust == null)
					_fullTrust = BuildFullTrust ();
				return _fullTrust;
			}
		}

		public static PermissionSet LocalIntranet {
			get {
				if (_localIntranet == null)
					_localIntranet = BuildLocalIntranet ();
				return _localIntranet;
			}
		}

		public static PermissionSet Internet {
			get {
				if (_internet == null)
					_internet = BuildInternet ();
				return _internet;
			}
		}

		public static PermissionSet SkipVerification {
			get {
				if (_skipVerification == null)
					_skipVerification = BuildSkipVerification ();
				return _skipVerification;
			}
		}

		public static PermissionSet Execution {
			get {
				if (_execution == null)
					_execution = BuildExecution ();
				return _execution;
			}
		}


		public static PermissionSet Nothing {
			get {
				if (_nothing == null)
					_nothing = BuildNothing ();
				return _nothing;
			}
		}

		public static PermissionSet Everything {
			get {
				if (_everything == null)
					_everything = BuildEverything ();
				return _everything;
			}
		}

		// internal stuff

		private static NamedPermissionSet BuildFullTrust ()
		{
			return new NamedPermissionSet (ReservedNames.FullTrust, PermissionState.Unrestricted);
		}

		private static NamedPermissionSet BuildLocalIntranet ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (ReservedNames.LocalIntranet, PermissionState.None);

			nps.AddPermission (new EnvironmentPermission (EnvironmentPermissionAccess.Read, "USERNAME;USER"));

			nps.AddPermission (new FileDialogPermission (PermissionState.Unrestricted));

			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			isfp.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
			isfp.UserQuota = Int64.MaxValue;
			nps.AddPermission (isfp);

			nps.AddPermission (new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit));

			SecurityPermissionFlag spf = SecurityPermissionFlag.Execution | SecurityPermissionFlag.Assertion;
			nps.AddPermission (new SecurityPermission (spf));

			nps.AddPermission (new UIPermission (PermissionState.Unrestricted));

			// DnsPermission requires stuff outside corlib (System)
			nps.AddPermission (PermissionBuilder.Create (DnsPermissionClass, PermissionState.Unrestricted));

			// PrintingPermission requires stuff outside corlib (System.Drawing)
			nps.AddPermission (PermissionBuilder.Create (PrintingPermission ("SafePrinting")));
#if !NET_2_0
			// EventLogPermission requires stuff outside corlib (System)
			nps.AddPermission (PermissionBuilder.Create (EventLogPermission (".", "Instrument")));
#endif
			return nps;
		}

		private static NamedPermissionSet BuildInternet ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (ReservedNames.Internet, PermissionState.None);
			nps.AddPermission (new FileDialogPermission (FileDialogPermissionAccess.Open));

			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			isfp.UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser;
			isfp.UserQuota = 10240;
			nps.AddPermission (isfp);

			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Execution));

			nps.AddPermission (new UIPermission (UIPermissionWindow.SafeTopLevelWindows, UIPermissionClipboard.OwnClipboard));

			// PrintingPermission requires stuff outside corlib (System.Drawing)
			nps.AddPermission (PermissionBuilder.Create (PrintingPermission ("SafePrinting")));
			return nps;
		}

		private static NamedPermissionSet BuildSkipVerification ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (ReservedNames.SkipVerification, PermissionState.None);
			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.SkipVerification));
			return nps;
		}

		private static NamedPermissionSet BuildExecution ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (ReservedNames.Execution, PermissionState.None);
			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Execution));
			return nps;
		}

		private static NamedPermissionSet BuildNothing ()
		{
			return new NamedPermissionSet (ReservedNames.Nothing, PermissionState.None);
		}

		private static NamedPermissionSet BuildEverything ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (ReservedNames.Everything, PermissionState.None);

			nps.AddPermission (new EnvironmentPermission (PermissionState.Unrestricted));
			nps.AddPermission (new FileDialogPermission (PermissionState.Unrestricted));
			nps.AddPermission (new FileIOPermission (PermissionState.Unrestricted));
			nps.AddPermission (new IsolatedStorageFilePermission (PermissionState.Unrestricted));
			nps.AddPermission (new ReflectionPermission (PermissionState.Unrestricted));
			nps.AddPermission (new RegistryPermission (PermissionState.Unrestricted));
#if NET_2_0
			nps.AddPermission (new KeyContainerPermission (PermissionState.Unrestricted));
#endif

			// not quite all in this case
			SecurityPermissionFlag spf = SecurityPermissionFlag.AllFlags;
			spf &= ~SecurityPermissionFlag.SkipVerification;
			nps.AddPermission (new SecurityPermission (spf));

			nps.AddPermission (new UIPermission (PermissionState.Unrestricted));

			// others requires stuff outside corlib
			nps.AddPermission (PermissionBuilder.Create (DnsPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (PrintingPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (EventLogPermissionClass, PermissionState.Unrestricted));

			nps.AddPermission (PermissionBuilder.Create (SocketPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (WebPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (PerformanceCounterPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (DirectoryServicesPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (MessageQueuePermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (ServiceControllerPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (OleDbPermissionClass, PermissionState.Unrestricted));
			nps.AddPermission (PermissionBuilder.Create (SqlClientPermissionClass, PermissionState.Unrestricted));
#if NET_2_0
//			nps.AddPermission (PermissionBuilder.Create (DataProtectionPermissionClass, PermissionState.Unrestricted));
//			nps.AddPermission (PermissionBuilder.Create (StorePermissionClass, PermissionState.Unrestricted));
#endif
			return nps;
		}

		private static SecurityElement PrintingPermission (string level)
		{
			SecurityElement se = new SecurityElement ("IPermission");
			se.AddAttribute ("class", PrintingPermissionClass);
			se.AddAttribute ("version", "1");
			se.AddAttribute ("Level", level);
			return se;
		}

		private static SecurityElement EventLogPermission (string name, string access)
		{
			SecurityElement se = new SecurityElement ("IPermission");
			se.AddAttribute ("class", EventLogPermissionClass);
			se.AddAttribute ("version", "1");

			SecurityElement child = new SecurityElement ("Machine");
			child.AddAttribute ("name", name);
			child.AddAttribute ("access", access);

			se.AddChild (child);
			return se;
		}
	}
}
