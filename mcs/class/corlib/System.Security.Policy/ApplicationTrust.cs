//
// System.Security.Policy.ApplicationTrust.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;

using Mono.Security.Cryptography;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible (true)]
	public sealed class ApplicationTrust : ISecurityEncodable {

		private ApplicationIdentity _appid;
		private PolicyStatement _defaultPolicy;
		private object _xtranfo;
		private bool _trustrun;
		private bool _persist;

		public ApplicationTrust ()
		{
		}

		public ApplicationTrust (ApplicationIdentity applicationIdentity)
		{
			if (applicationIdentity == null)
				throw new ArgumentNullException ("applicationIdentity");
			_appid = applicationIdentity;
		}

		public ApplicationIdentity ApplicationIdentity {
			get { return _appid; }
			set {
				if (value == null)
					throw new ArgumentNullException ("ApplicationIdentity");
				_appid = value;
			}
		}

		public PolicyStatement DefaultGrantSet {
			get {
				if (_defaultPolicy == null)
					_defaultPolicy = GetDefaultGrantSet ();

				return _defaultPolicy;
			}
			set { _defaultPolicy = value; }
		}

		public object ExtraInfo {
			get { return _xtranfo; }
			set { _xtranfo = value; }
		}

		public bool IsApplicationTrustedToRun {
			get { return _trustrun; }
			set { _trustrun = value; }
		}

		public bool Persist {
			get { return _persist; }
			set { _persist = value; }
		}

		public void FromXml (SecurityElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			if (element.Tag != "ApplicationTrust")
				throw new ArgumentException ("element");

			string s = element.Attribute ("FullName");
			if (s != null)
				_appid = new ApplicationIdentity (s);
			else
				_appid = null;

			_defaultPolicy = null;
			SecurityElement defaultGrant = element.SearchForChildByTag ("DefaultGrant");
			if (defaultGrant != null) {
				for (int i=0; i < defaultGrant.Children.Count; i++) {
					SecurityElement se = (defaultGrant.Children [i] as SecurityElement);
					if (se.Tag == "PolicyStatement") {
						DefaultGrantSet.FromXml (se, null);
						break;
					}
				}
			}

			if (!Boolean.TryParse (element.Attribute ("TrustedToRun"), out _trustrun))
				_trustrun = false;

			if (!Boolean.TryParse (element.Attribute ("Persist"), out _persist))
				_persist = false;

			_xtranfo = null;
			SecurityElement xtra = element.SearchForChildByTag ("ExtraInfo");
			if (xtra != null) {
				s = xtra.Attribute ("Data");
				if (s != null) {
					byte[] data = CryptoConvert.FromHex (s);
					using (MemoryStream ms = new MemoryStream (data)) {
						BinaryFormatter bf = new BinaryFormatter ();
						_xtranfo = bf.Deserialize (ms);
					}
				}
			}
		}

		public SecurityElement ToXml () 
		{
			SecurityElement se = new SecurityElement ("ApplicationTrust");
			se.AddAttribute ("version", "1");

			if (_appid != null) {
				se.AddAttribute ("FullName", _appid.FullName);
			}

			if (_trustrun) {
				se.AddAttribute ("TrustedToRun", "true");
			}

			if (_persist) {
				se.AddAttribute ("Persist", "true");
			}

			SecurityElement defaultGrant = new SecurityElement ("DefaultGrant");
			defaultGrant.AddChild (DefaultGrantSet.ToXml ());
			se.AddChild (defaultGrant);

			if (_xtranfo != null) {
				byte[] data = null;
				using (MemoryStream ms = new MemoryStream ()) {
					BinaryFormatter bf = new BinaryFormatter ();
					bf.Serialize (ms, _xtranfo);
					data = ms.ToArray ();
				}
				SecurityElement xtra = new SecurityElement ("ExtraInfo");
				xtra.AddAttribute ("Data", CryptoConvert.ToHex (data));
				se.AddChild (xtra);
			}

			return se;
		}

		// internal stuff

		private PolicyStatement GetDefaultGrantSet ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			return new PolicyStatement (ps);
		}
	}
}

#endif
