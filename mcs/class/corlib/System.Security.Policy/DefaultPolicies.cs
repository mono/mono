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

		public enum Key {
			Ecma,
			MsFinal,
		}

		private const string DnsPermissionClass = "System.Net.DnsPermission, " + Consts.AssemblySystem;
		private const string EventLogPermissionClass = "System.Diagnostics.EventLogPermission, " + Consts.AssemblySystem;
		private const string PrintingPermissionClass = "System.Drawing.Printing.PrintingPermission, " + Consts.AssemblySystem_Drawing;
		private const string SocketPermissionClass = "System.Net.SocketPermission, " + Consts.AssemblySystem;
		private const string WebPermissionClass = "System.Net.WebPermission, " + Consts.AssemblySystem;
		private const string PerformanceCounterPermissionClass = "System.Diagnostics.PerformanceCounterPermission, " + Consts.AssemblySystem;
		private const string DirectoryServicesPermissionClass = "System.DirectoryServices.DirectoryServicesPermission, " + Consts.AssemblySystem_DirectoryServices;
		private const string MessageQueuePermissionClass = "System.Messaging.MessageQueuePermission, " + Consts.AssemblySystem_Messaging;
		private const string ServiceControllerPermissionClass = "System.ServiceProcess.ServiceControllerPermission, " + Consts.AssemblySystem_ServiceProcess;
		private const string OleDbPermissionClass = "System.Data.OleDb.OleDbPermission, " + Consts.AssemblySystem_Data;
		private const string SqlClientPermissionClass = "System.Data.SqlClient.SqlClientPermission, " + Consts.AssemblySystem_Data;
#if NET_2_0
		private const string DataProtectionPermissionClass = "System.Security.Permissions.DataProtectionPermission, " + Consts.AssemblySystem_Security;
		private const string StorePermissionClass = "System.Security.Permissions.StorePermission, " + Consts.AssemblySystem_Security;
#endif

		private static Version _fxVersion;
		private static byte[] _ecmaKey = new byte [16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static StrongNamePublicKeyBlob _ecma;
		private static byte[] _msFinalKey = new byte [160] { 
			0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00,
			0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
			0x07, 0xD1, 0xFA, 0x57, 0xC4, 0xAE, 0xD9, 0xF0, 0xA3, 0x2E, 0x84, 0xAA, 0x0F, 0xAE, 0xFD, 0x0D, 
			0xE9, 0xE8, 0xFD, 0x6A, 0xEC, 0x8F, 0x87, 0xFB, 0x03, 0x76, 0x6C, 0x83, 0x4C, 0x99, 0x92, 0x1E, 
			0xB2, 0x3B, 0xE7, 0x9A, 0xD9, 0xD5, 0xDC, 0xC1, 0xDD, 0x9A, 0xD2, 0x36, 0x13, 0x21, 0x02, 0x90, 
			0x0B, 0x72, 0x3C, 0xF9, 0x80, 0x95, 0x7F, 0xC4, 0xE1, 0x77, 0x10, 0x8F, 0xC6, 0x07, 0x77, 0x4F, 
			0x29, 0xE8, 0x32, 0x0E, 0x92, 0xEA, 0x05, 0xEC, 0xE4, 0xE8, 0x21, 0xC0, 0xA5, 0xEF, 0xE8, 0xF1, 
			0x64, 0x5C, 0x4C, 0x0C, 0x93, 0xC1, 0xAB, 0x99, 0x28, 0x5D, 0x62, 0x2C, 0xAA, 0x65, 0x2C, 0x1D, 
			0xFA, 0xD6, 0x3D, 0x74, 0x5D, 0x6F, 0x2D, 0xE5, 0xF1, 0x7E, 0x5E, 0xAF, 0x0F, 0xC4, 0x96, 0x3D, 
			0x26, 0x1C, 0x8A, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6D, 0xC0, 0x93, 0x34, 0x4D, 0x5A, 0xD2, 0x93 };
		private static StrongNamePublicKeyBlob _msFinal;

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

		public static StrongNameMembershipCondition FullTrustMembership (string name, Key key)
		{
			StrongNamePublicKeyBlob snkb = null;

			switch (key) {
			case Key.Ecma:
				if (_ecma == null) {
					_ecma = new StrongNamePublicKeyBlob (_ecmaKey);
				}
				snkb = _ecma;
				break;
			case Key.MsFinal:
				if (_msFinal == null) {
					_msFinal = new StrongNamePublicKeyBlob (_msFinalKey);
				}
				snkb = _msFinal;
				break;
			}

			if (_fxVersion == null)
			{
				_fxVersion = new Version (Consts.FxVersion);
			}

			return new StrongNameMembershipCondition (snkb, name, _fxVersion);
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
#if NET_2_0
			isfp.UserQuota = 512000;
#else
			isfp.UserQuota = 10240;
#endif
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

#if !NET_2_0
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
#endif		
	}
}
