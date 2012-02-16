//
// System.Messaging.MessageQueuePermission.cs
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Peter Van Isacker
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

using System.Globalization;
using System.Security;
using System.Security.Permissions;

namespace System.Messaging {

	[Serializable]
	public sealed class MessageQueuePermission: CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		private MessageQueuePermissionEntryCollection _list;
		private bool _unrestricted;

		public MessageQueuePermission ()
		{
			_list = new MessageQueuePermissionEntryCollection (this);
		}
		
		public MessageQueuePermission (MessageQueuePermissionEntry[] permissionAccessEntries)
			: this ()
		{
			foreach (MessageQueuePermissionEntry entry in permissionAccessEntries)
				_list.Add (entry);
		}
		
		public MessageQueuePermission (PermissionState state)
			: this ()
		{
			_unrestricted = (state == PermissionState.Unrestricted);
		}
		
		public MessageQueuePermission (MessageQueuePermissionAccess permissionAccess, string path)
			: this ()
		{
			MessageQueuePermissionEntry entry = new MessageQueuePermissionEntry (permissionAccess, path);
			_list.Add (entry);
		}
		
		public MessageQueuePermission (MessageQueuePermissionAccess permissionAccess,
			string machineName, string label, string category) : this ()
		{
			MessageQueuePermissionEntry entry = new MessageQueuePermissionEntry (permissionAccess, machineName, label, category);
			_list.Add (entry);
		}
		
		public MessageQueuePermissionEntryCollection PermissionEntries {
			get { return _list; }
		}
		
		public override IPermission Copy ()
		{
			if (_unrestricted)
				return new MessageQueuePermission (PermissionState.Unrestricted);
			else {
				MessageQueuePermission copy = new MessageQueuePermission (PermissionState.None);
				foreach (MessageQueuePermissionEntry entry in _list)
					copy._list.Add (entry);
				return copy;
			}
		}

		public bool IsUnrestricted () 
		{
			return _unrestricted;
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
		{
			CheckSecurityElement (securityElement, "securityElement", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			_unrestricted = (IsUnrestricted (securityElement));

			// TODO read elements
		}
		
		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			Cast (target);
			return null;
		}
		
		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			Cast (target);
			return false;
		}
		
		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);
			if (_unrestricted)
				se.AddAttribute ("Unrestricted", "true");
			else {
				// TODO
			}
			return se;
		}
		
		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			Cast (target);
			return null;
		}

		// helpers

		private bool IsEmpty ()
		{
			return (!_unrestricted && (_list.Count == 0));
		}

		private MessageQueuePermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			MessageQueuePermission mqp = (target as MessageQueuePermission);
			if (mqp == null) {
				ThrowInvalidPermission (target, typeof (MessageQueuePermission));
			}

			return mqp;
		}

		// static helpers

		private static char[] invalidChars = new char[] { '\t', '\n', '\v', '\f', '\r', ' ', '\\', '\x160' };

		internal static void ValidateMachineName (string name)
		{
			// FIXME: maybe other checks are required (but not documented)
			if ((name == null) || (name.Length == 0) || (name.IndexOfAny (invalidChars) != -1)) {
				string msg = Locale.GetText ("Invalid machine name '{0}'.");
				if (name == null)
					name = "(null)";
				msg = String.Format (msg, name);
				throw new ArgumentException (msg, "MachineName");
			}
		}

		internal static void ValidatePath (string path)
		{
			// FIXME: maybe other checks are required (but not documented)
			if ((path.Length > 0) && (path [0] != '\\')) {
				string msg = Locale.GetText ("Invalid path '{0}'.");
				throw new ArgumentException (String.Format (msg, path), "Path");
			}
		}

		// NOTE: The following static methods should be moved out to a (static?) class 
		// if (ever) System.Drawing.dll gets more than one permission in it's assembly.

		// snippet moved from FileIOPermission (nickd) to be reused in all derived classes
		internal SecurityElement Element (int version) 
		{
			SecurityElement se = new SecurityElement ("IPermission");
			Type type = this.GetType ();
			se.AddAttribute ("class", type.FullName + ", " + type.Assembly.ToString ().Replace ('\"', '\''));
			se.AddAttribute ("version", version.ToString ());
			return se;
		}

		internal static PermissionState CheckPermissionState (PermissionState state, bool allowUnrestricted)
		{
			string msg;
			switch (state) {
			case PermissionState.None:
				break;
			case PermissionState.Unrestricted:
				if (!allowUnrestricted) {
					msg = Locale.GetText ("Unrestricted isn't not allowed for identity permissions.");
					throw new ArgumentException (msg, "state");
				}
				break;
			default:
				msg = String.Format (Locale.GetText ("Invalid enum {0}"), state);
				throw new ArgumentException (msg, "state");
			}
			return state;
		}

		// logic isn't identical to CodeAccessPermission.CheckSecurityElement - see unit tests
		internal static int CheckSecurityElement (SecurityElement se, string parameterName, int minimumVersion, int maximumVersion) 
		{
			if (se == null)
				throw new ArgumentNullException (parameterName);

			if (se.Attribute ("class") == null) {
				string msg = Locale.GetText ("Missing 'class' attribute.");
				throw new ArgumentException (msg, parameterName);
			}

			// we assume minimum version if no version number is supplied
			int version = minimumVersion;
			string v = se.Attribute ("version");
			if (v != null) {
				try {
					version = Int32.Parse (v);
				}
				catch (Exception e) {
					string msg = Locale.GetText ("Couldn't parse version from '{0}'.");
					msg = String.Format (msg, v);
					throw new ArgumentException (msg, parameterName, e);
				}
			}

			if ((version < minimumVersion) || (version > maximumVersion)) {
				string msg = Locale.GetText ("Unknown version '{0}', expected versions between ['{1}','{2}'].");
				msg = String.Format (msg, version, minimumVersion, maximumVersion);
				throw new ArgumentException (msg, parameterName);
			}
			return version;
		}

		// must be called after CheckSecurityElement (i.e. se != null)
		internal static bool IsUnrestricted (SecurityElement se) 
		{
			string value = se.Attribute ("Unrestricted");
			if (value == null)
				return false;
			return (String.Compare (value, Boolean.TrueString, true, CultureInfo.InvariantCulture) == 0);
		}

		internal static void ThrowInvalidPermission (IPermission target, Type expected) 
		{
			string msg = Locale.GetText ("Invalid permission type '{0}', expected type '{1}'.");
			msg = String.Format (msg, target.GetType (), expected);
			throw new ArgumentException (msg, "target");
		}
	}
}
