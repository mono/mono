//
// System.Version.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System {

	[Serializable]
	[ComVisible (true)]
	public sealed class Version : ICloneable, IComparable, IComparable<Version>, IEquatable<Version> {
		int _Major, _Minor, _Build, _Revision;

		private const int UNDEFINED = -1;

		private void CheckedSet (int defined, int major, int minor, int build, int revision)
		{
			// defined should be 2, 3 or 4

			if (major < 0) {
				throw new ArgumentOutOfRangeException ("major");
			}
			this._Major = major;

			if (minor < 0) {
				throw new ArgumentOutOfRangeException ("minor");
			}
			this._Minor = minor;

			if (defined == 2) {
				this._Build = UNDEFINED;
				this._Revision = UNDEFINED;
				return;
			}

			if (build < 0) {
				throw new ArgumentOutOfRangeException ("build");
			}
			this._Build = build;

			if (defined == 3) {
				this._Revision = UNDEFINED;
				return;
			}

			if (revision < 0) {
				throw new ArgumentOutOfRangeException ("revision");
			}
			this._Revision = revision;
		}

		public Version ()
		{
			CheckedSet (2, 0, 0, -1, -1);
		}

		public Version (string version)
		{
			int n;
			string [] vals;
			int major = -1, minor = -1, build = -1, revision = -1;

			if (version == null) {
				throw new ArgumentNullException ("version");
			}

			vals = version.Split (new Char [] {'.'});
			n = vals.Length;

			if (n < 2 || n > 4) {
				throw new ArgumentException (Locale.GetText
					("There must be 2, 3 or 4 components in the version string."));
			}

			if (n > 0)
				major = int.Parse (vals [0]);
			if (n > 1)
				minor = int.Parse (vals [1]);
			if (n > 2)
				build = int.Parse (vals [2]);
			if (n > 3)
				revision = int.Parse (vals [3]);

			CheckedSet (n, major, minor, build, revision);
		}

		public Version (int major, int minor)
		{
			CheckedSet (2, major, minor, 0, 0);
		}

		public Version (int major, int minor, int build)
		{
			CheckedSet (3, major, minor, build, 0);
		}

		public Version (int major, int minor, int build, int revision)
		{
			CheckedSet (4, major, minor, build, revision);
		}

		public int Build {
			get {
				return _Build;
			}
		}

		public int Major {
			get {
				return _Major;
			}
		}

		public int Minor {
			get {
				return _Minor;
			}
		}

		public int Revision {
			get {
				return _Revision;
			}
		}
		public short MajorRevision {
			get {
				return (short)(_Revision >> 16);
			}
		}

		public short MinorRevision {
			get {
				return (short)_Revision;
			}
		}

		public object Clone ()
		{
			if (_Build == -1)
				return new Version (_Major, _Minor);
			else if (_Revision == -1)
				return new Version (_Major, _Minor, _Build);
			else
				return new Version (_Major, _Minor, _Build, _Revision);
		}

		public int CompareTo (object version)
		{
			if (version == null)
				return 1;

			if (! (version is Version))
				throw new ArgumentException (Locale.GetText ("Argument to Version.CompareTo must be a Version."));

			return this.CompareTo ((Version) version);
		}

		public override bool Equals (object obj)
		{
			return this.Equals (obj as Version);
		}

		public int CompareTo (Version value)
		{
			if (value == null)
				return 1;
			
			if (this._Major > value._Major)
				return 1;
			else if (this._Major < value._Major)
				return -1;

			if (this._Minor > value._Minor)
				return 1;
			else if (this._Minor < value._Minor)
				return -1;

			if (this._Build > value._Build)
				return 1;
			else if (this._Build < value._Build)
				return -1;

			if (this._Revision > value._Revision)
				return 1;
			else if (this._Revision < value._Revision)
				return -1;

			return 0;
		}

		public bool Equals (Version obj)
		{
			return ((obj != null) &&
			    (obj._Major == _Major) &&
			    (obj._Minor == _Minor) &&
			    (obj._Build == _Build) &&
			    (obj._Revision == _Revision));
		}

		public override int GetHashCode ()
		{
			return (_Revision << 24) | (_Build << 16) | (_Minor << 8) | _Major;
		}

		// <summary>
		//   Returns a stringified representation of the version, format:
		//   major.minor[.build[.revision]]
		// </summary>
		public override string ToString ()
		{
			string mm = _Major.ToString () + "." + _Minor.ToString ();
			
			if (_Build != UNDEFINED)
				mm = mm + "." + _Build.ToString ();
			if (_Revision != UNDEFINED)
				mm = mm + "." + _Revision.ToString ();

			return mm;
		}

		// <summary>
		//    LAME: This API is lame, since there is no way of knowing
		//    how many fields a Version object has, it is unfair to throw
		//    an ArgumentException, but this is what the spec claims.
		//
		//    ie, Version a = new Version (1, 2);  a.ToString (3) should
		//    throw the expcetion.
		// </summary>
		public string ToString (int fieldCount)
		{
			if (fieldCount == 0)
				return String.Empty;
			if (fieldCount == 1)
				return _Major.ToString ();
			if (fieldCount == 2)
				return _Major.ToString () + "." + _Minor.ToString ();
			if (fieldCount == 3){
				if (_Build == UNDEFINED)
					throw new ArgumentException (Locale.GetText
					("fieldCount is larger than the number of components defined in this instance."));
				return _Major.ToString () + "." + _Minor.ToString () + "." +
					_Build.ToString ();
			}
			if (fieldCount == 4){
				if (_Build == UNDEFINED || _Revision == UNDEFINED)
					throw new ArgumentException (Locale.GetText
					("fieldCount is larger than the number of components defined in this instance."));
				return _Major.ToString () + "." + _Minor.ToString () + "." +
					_Build.ToString () + "." + _Revision.ToString ();
			}
			throw new ArgumentException (Locale.GetText ("Invalid fieldCount parameter: ") + fieldCount.ToString());	
		}

		public static bool operator== (Version v1, Version v2) 
		{
			return Equals (v1, v2);
		}

		public static bool operator!= (Version v1, Version v2)
		{
			return !Equals (v1, v2);
		}

		public static bool operator> (Version v1, Version v2)
		{
			return v1.CompareTo (v2) > 0;
		}

		public static bool operator>= (Version v1, Version v2)
		{
			return v1.CompareTo (v2) >= 0;
		}

		public static bool operator< (Version v1, Version v2)
		{
			return v1.CompareTo (v2) < 0;
		}

		public static bool operator<= (Version v1, Version v2)
		{
			return v1.CompareTo (v2) <= 0;
		}

#if BOOSTRAP_NET_4_0 || NET_4_0 || MOONLIGHT
		public static Version Parse (string input)
		{
			// Exactly the same as calling Version(string) .ctor
			return new Version (input);
		}

		// Implemented the TryParse separated from the Parse impl due to its
		// small size, and because the Parse one depends on the exceptions thrown by
		// Int32.Parse to match .Net, and detecting such exceptions here is not worth it.
		public static bool TryParse (string input, out Version result)
		{
			int n;
			string [] vals;
			int [] values; // actual parsed values

			result = null;

			if (input == null)
				return false;

			vals = input.Split (new Char [] {'.'});
			n = vals.Length;

			if (n < 2 || n > 4)
				return false;

			values = new int [n];
			for (int i = 0; i < n; i++) {
				int part;
				if (!Int32.TryParse (vals [i], out part) || part < 0)
					return false;

				values [i] = part;
			}

			result = new Version ();
			if (n > 0)
				result._Major = values [0];
			if (n > 1)
				result._Minor = values [1];
			if (n > 2)
				result._Build = values [2];
			if (n > 3)
				result._Revision = values [3];

			return true;
		}
#endif

		// a very gentle way to construct a Version object which takes 
		// the first four numbers in a string as the version
		internal static Version CreateFromString (string info)
		{
			int major = 0;
			int minor = 0;
			int build = 0;
			int revision = 0;
			int state = 1;
			int number = UNDEFINED; // string may not begin with a digit

                        if (info == null)
                                return new Version (0, 0, 0, 0);

			for (int i=0; i < info.Length; i++) {
				char c = info [i];
				if (Char.IsDigit (c)) {
					if (number < 0) {
						number = (c - '0');
					}
					else {
						number = (number * 10) + (c - '0');
					}
				}
				else if (number >= 0) {
					// assign
					switch (state) {
					case 1:
						major = number;
						break;
					case 2:
						minor = number;
						break;
					case 3:
						build = number;
						break;
					case 4:
						revision = number;
						break;
					}
					number = -1;
					state ++;
				}
				// ignore end of string
				if (state == 5)
					break;
			}

			// Last number
			if (number >= 0) {
				switch (state) {
				case 1:
					major = number;
					break;
				case 2:
					minor = number;
					break;
				case 3:
					build = number;
					break;
				case 4:
					revision = number;
					break;
				}
			}
			return new Version (major, minor, build, revision);
		}
	}
}
