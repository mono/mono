//
// System.Drawing.PrintingPermission.cs
//
// Authors:
//	Dennis Hayes (dennish@Raytek.com)
//	Herve Poussineau (hpoussineau@fr.st)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc
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

namespace System.Drawing.Printing {

	[Serializable]
	public sealed class PrintingPermission : CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		private PrintingPermissionLevel _Level;
		
		public PrintingPermission (PermissionState state) 
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted)
				_Level = PrintingPermissionLevel.AllPrinting;
		}

		public PrintingPermission (PrintingPermissionLevel printingLevel) 
		{
			Level = printingLevel;
		}
		
		// properties

		public PrintingPermissionLevel Level{
			get { return _Level; }
			set {
				if (!Enum.IsDefined (typeof (PrintingPermissionLevel), value)) {
					string msg = Locale.GetText ("Invalid enum {0}");
					throw new ArgumentException (String.Format (msg, value), "Level");
				}
				 _Level = value;
			}
		}

		// methods

		public override IPermission Copy ()
		{
			return new PrintingPermission (this.Level);
		}
		
		public override void FromXml (SecurityElement esd)
		{
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd))
				_Level = PrintingPermissionLevel.AllPrinting;
			else {
				string level = esd.Attribute ("Level");
				if (level != null) {
					_Level = (PrintingPermissionLevel) Enum.Parse (
						typeof (PrintingPermissionLevel), level);
				}
				else
					_Level = PrintingPermissionLevel.NoPrinting;
			}
		}
		
		public override IPermission Intersect (IPermission target)
		{
			PrintingPermission pp = Cast (target);
			if ((pp == null) || IsEmpty () || pp.IsEmpty ())
				return null;

			PrintingPermissionLevel level = (_Level <= pp.Level) ? _Level : pp.Level;
			return new PrintingPermission (level);
		}
		
		public override bool IsSubsetOf (IPermission target)
		{
			PrintingPermission pp = Cast (target);
			if (pp == null)
				return IsEmpty ();
			
			return (_Level <= pp.Level);
		}
		
		public bool IsUnrestricted ()
		{
			return (_Level == PrintingPermissionLevel.AllPrinting);
		}
		
		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);
			if (IsUnrestricted ())
				se.AddAttribute ("Unrestricted", "true");
			else
				se.AddAttribute ("Level", _Level.ToString ());
			return se;
		}
		
		public override IPermission Union (IPermission target)
		{
			PrintingPermission pp = Cast (target);
			if (pp == null)
				return new PrintingPermission (_Level);
			if (IsUnrestricted () || pp.IsUnrestricted ())
				return new PrintingPermission (PermissionState.Unrestricted);
			if (IsEmpty () && pp.IsEmpty ())
				return null;

			PrintingPermissionLevel level = (_Level > pp.Level) ? _Level : pp.Level;
			return new PrintingPermission (level);
		}

		// Internal helpers methods

		private bool IsEmpty ()
		{
			return (_Level == PrintingPermissionLevel.NoPrinting);
		}

		private PrintingPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			PrintingPermission pp = (target as PrintingPermission);
			if (pp == null) {
				ThrowInvalidPermission (target, typeof (PrintingPermission));
			}

			return pp;
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

			string c = se.Attribute ("class");
#if NET_2_0
			if (c == null) {
				string msg = Locale.GetText ("Missing 'class' attribute.");
				throw new ArgumentException (msg, parameterName);
			}
#else
			if ((c == null) || (String.Compare (c, 0, "System.Drawing.Printing.PrintingPermission", 0, 42) != 0)) {
				string msg = Locale.GetText ("Wrong 'class' attribute.");
				throw new ArgumentException (msg, parameterName);
			}
#endif
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
#if !NET_2_0
			else {
				string msg = Locale.GetText ("Missing 'version' attribute.");
				throw new ArgumentException (msg, parameterName);
			}
#endif

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
