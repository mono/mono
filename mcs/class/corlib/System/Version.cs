//
// System.Version.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	[Serializable]
	public sealed class Version : ICloneable, IComparable {

		int major, minor, build, revision;

		private const int UNDEFINED = -1;
		
		private void CheckedSet (int defined, int major, int minor, int build, int revision)
		{
			// defined should be 2, 3 or 4

			if (major < 0) {
				throw new ArgumentOutOfRangeException ("major");
			}
			this.major = major;

			if (minor < 0) {
				throw new ArgumentOutOfRangeException ("minor");
			}
			this.minor = minor;

			if (defined == 2) {
				this.build = UNDEFINED;
				this.revision = UNDEFINED;
				return;
			}

			if (build < 0) {
				throw new ArgumentOutOfRangeException ("build");
			}
			this.build = build;

			if (defined == 3) {
				this.revision = UNDEFINED;
				return;
			}

			if (revision < 0) {
				throw new ArgumentOutOfRangeException ("revision");
			}
			this.revision = revision;
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
				throw new ArgumentException (Locale.GetText ("There must be 2, 3 or 4 components in the version string"));
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
				return build;
			}
		}

		public int Major {
			get {
				return major;
			}
		}

		public int Minor {
			get {
				return minor;
			}
		}

		public int Revision {
			get {
				return revision;
			}
		}

		public object Clone ()
		{
			return new Version (major, minor, build, revision);
		}

		public int CompareTo (object version)
		{
			Version v;
			
			// LAMESPEC: Docs are unclear whether an 
			// ArgumentNullException should be thrown are 
			// that a value > 0 should be returned.
			if (version == null)
				return 1;
			//	throw new ArgumentNullException ("version");
			
			if (! (version is Version))
				throw new ArgumentException (Locale.GetText ("Argument to Version.CompareTo must be a Version"));

			v = version as Version;

			if (this.major > v.major)
				return 1;
			else if (this.major < v.major)
				return -1;

			if (this.minor > v.minor)
				return 1;
			else if (this.minor < v.minor)
				return -1;

			if (this.build > v.build)
				return 1;
			else if (this.build < v.build)
				return -1;

			if (this.revision > v.revision)
				return 1;
			else if (this.revision < v.revision)
				return -1;

			return 0;
		}

		public override bool Equals (object obj)
		{
			Version x;
			
			if (obj == null || !(obj is Version))
				return false;

			x = (Version) obj;
			
			if ((x.major == major) &&
			    (x.minor == minor) &&
			    (x.build == build) &&
			    (x.revision == revision))
				return true;
			return false;
		}

		public override int GetHashCode ()
		{
			return (revision << 24) | (build << 16) | (minor << 8) | major;
		}

		// <summary>
		//   Returns a stringified representation of the version, format:
		//   major.minor[.build[.revision]]
		// </summary>
		public override string ToString ()
		{
			string mm = major.ToString () + "." + minor.ToString ();
			
			if (build != UNDEFINED)
				mm = mm + "." + build.ToString ();
			if (revision != UNDEFINED)
				mm = mm + "." + revision.ToString ();

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
		public string ToString (int fields)
		{
			if (fields == 0)
				return "";
			if (fields == 1)
				return major.ToString ();
			if (fields == 2)
				return major.ToString () + "." + minor.ToString ();
			if (fields == 3){
				if (build == UNDEFINED)
					throw new ArgumentException (Locale.GetText ("fields is larger than the number of components defined in this instance"));
				return major.ToString () + "." + minor.ToString () + "." +
					build.ToString ();
			}
			if (fields == 4){
				if (build == UNDEFINED || revision == UNDEFINED)
					throw new ArgumentException (Locale.GetText ("fields is larger than the number of components defined in this instance"));
				return major.ToString () + "." + minor.ToString () + "." +
					build.ToString () + "." + revision.ToString ();
			}
			throw new ArgumentException (Locale.GetText ("Invalid fields parameter: ") + fields.ToString());	
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

	}
}



