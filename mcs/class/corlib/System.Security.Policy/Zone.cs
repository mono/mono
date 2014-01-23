//
// System.Security.Policy.Zone
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Globalization;
using System.Security.Permissions;
using System.Runtime.InteropServices;

using Mono.Security;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class Zone :
#if NET_4_0
		EvidenceBase,
#endif
		IIdentityPermissionFactory, IBuiltInEvidence	{

		private SecurityZone zone;
		
		public Zone (SecurityZone zone)
		{
			if (!Enum.IsDefined (typeof (SecurityZone), zone)) {
				string msg = String.Format (Locale.GetText ("Invalid zone {0}."), zone);
				throw new ArgumentException (msg, "zone");
			}

			this.zone = zone;
		}

		// properties

		public SecurityZone SecurityZone {
			get { return zone; }
		}

		// methods

		public object Copy ()
		{
			return new Zone (zone);
		}

		public IPermission CreateIdentityPermission (Evidence evidence)
		{
			return new ZoneIdentityPermission (zone);
		}

		[MonoTODO ("Not user configurable yet")]
		public static Zone CreateFromUrl (string url)
		{
			if (url == null)
				throw new ArgumentNullException ("url");

			SecurityZone z = SecurityZone.NoZone;
			if (url.Length == 0)
				return new Zone (z);

			Uri uri = new Uri (url);
			// TODO: apply zone configuration
			// this is the only way to use the Trusted and Untrusted zones

			if (z == SecurityZone.NoZone) {
				// not part of configuration, the use default mapping
				if (uri.IsFile) {
					if (File.Exists (uri.LocalPath))
						z = SecurityZone.MyComputer;
					else if (String.Compare ("FILE://", 0, url, 0, 7, true, CultureInfo.InvariantCulture) == 0)
						z = SecurityZone.Intranet;	// non accessible file:// 
					else
						z = SecurityZone.Internet;
				}
				else if (uri.IsLoopback) {			// e.g. http://localhost/x
					z = SecurityZone.Intranet;
				}
				else {
					// all protocols, including unknown ones
					z = SecurityZone.Internet;
				}
			}

			return new Zone (z);
		}

		public override bool Equals (object o)
		{
			Zone z = (o as Zone);
			if (z == null)
				return false;

			return (z.zone == zone);
		}

		public override int GetHashCode ()
		{
			return (int) zone;
		}

		public override string ToString ()
		{
			SecurityElement se = new SecurityElement ("System.Security.Policy.Zone");
			se.AddAttribute ("version", "1");
			se.AddChild (new SecurityElement ("Zone", zone.ToString ()));
			return se.ToString ();
		}

		// interface IBuiltInEvidence

		int IBuiltInEvidence.GetRequiredSize (bool verbose)
		{
			return 3;
		}

		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position)
		{
			int new_zone = (int) buffer [position++];
			new_zone += buffer [position++];
			return position;
		}

		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose)
		{
			buffer [position++] = '\x0003';
			buffer [position++] = (char) (((int) zone) >> 16);
			buffer [position++] = (char) (((int) zone) & 0x0FFFF);
			return position;
		}
	}
}
