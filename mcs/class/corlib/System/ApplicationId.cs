//
// System.ApplicationId class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System.Text;

namespace System {

	[Serializable]
	public sealed class ApplicationId {

		private byte[] _token;
		private string _name;
		private Version _version;
		private string _proc;
		private string _culture;

		public ApplicationId (byte[] publicKeyToken, string name, Version version, string processorArchitecture, string culture)
		{
			if (publicKeyToken == null)
				throw new ArgumentNullException ("publicKeyToken");
			if (name == null)
				throw new ArgumentNullException ("name");
			if (version == null)
				throw new ArgumentNullException ("version");

			_token = (byte[]) publicKeyToken.Clone ();
			_name = name;
			_version = version;
			_proc = processorArchitecture;
			_culture = culture;
		}

		// properties

		public string Culture {
			get { return _culture; }
		}

		public string Name {
			get { return _name; }
		}

		public string ProcessorArchitecture {
			get { return _proc; }
		}

		public byte[] PublicKeyToken {
			get { return (byte[]) _token.Clone (); }
		}

		public Version Version {
			get { return _version; }
		}

		// methods

		public ApplicationId Copy () 
		{
			return new ApplicationId (_token, _name, _version, _proc, _culture);
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;
			ApplicationId appid = (o as ApplicationId);
			if (appid == null)
				return false;
			if (_name != appid._name)
				return false;
			if (_proc != appid._proc)
				return false;
			if (_culture != appid._culture)
				return false;
			if (!_version.Equals (appid._version))
				return false;
			if (_token.Length != appid._token.Length)
				return false;
			for (int i=0; i < _token.Length; i++)
				if (_token [i] != appid._token [i])
					return false;
			return true;
		}

		public override int GetHashCode ()
		{
			int code = _name.GetHashCode () ^ _version.GetHashCode ();
			for (int i=0; i < _token.Length; i++)
				code ^= _token [i];
/* not used in MS implementation (bug?)
			if (_proc != null)
				code ^= _proc.GetHashCode ();
			if (_culture != null)
				code ^= _culture.GetHashCode ();*/
			return code;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (_name);
			if (_culture != null)
				sb.AppendFormat (", culture=\"{0}\"", _culture);
			sb.AppendFormat (", version=\"{0}\", publicKeyToken=\"", _version);
			for (int i=0; i < _token.Length; i++)
				sb.Append (_token [i].ToString ("X2"));
			if (_proc != null)
				sb.AppendFormat ("\", processorArchitecture =\"{0}\"", _proc);
			else
				sb.Append ("\"");
			return sb.ToString ();
		}
	}
}

#endif
