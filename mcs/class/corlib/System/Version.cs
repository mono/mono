//
// System.Version.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class Version : ICloneable, IComparable {
		int major, minor, build, revision;

		const int MAXINT = int.MaxValue;
		
		public Version (string version)
		{
			int n;
			string [] vals = version.Split (new Char [] {'.'});
			
			n = vals.Length;
			if (n > 0)
				major = int.Parse (vals [0]);
			if (n > 1)
				minor = int.Parse (vals [1]);
			if (n > 2)
				build = int.Parse (vals [2]);
			if (n > 3)
				build = int.Parse (vals [3]);
		}
		
		public Version (int major, int minor)
		{
			this.major = major;
			this.minor = minor;
			this.build = MAXINT;
			this.revision = MAXINT;
		}

		public Version (int major, int minor, int build)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
			this.revision = MAXINT;
		}

		public Version (int major, int minor, int build, int revision)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
			this.revision = revision;
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
			
			if (version == null)
				throw new ArgumentNullException ("version");
			if (! (version is Version))
				throw new ArgumentException ("version");

			v = version as Version;

			if (this.major > v.major)
				return 1;
			else if (this.major < v.major)
				return -1;

			if (this.minor > v.minor)
				return 1;
			else if (this.minor < this.minor)
				return -1;

			if (this.build > v.build)
				return 1;
			else if (this.build < this.build)
				return -1;

			// FIXME: Compare revision or build first?
			if (this.revision > v.revision)
				return 1;
			else if (this.revision < v.revision)
				return -1;

			return 0;
		}

		public override bool Equals (object obj)
		{
			Version x;
			
			if (obj == null)
				throw new ArgumentNullException ("obj");
			if (!(obj is Version))
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
			
			if (build != MAXINT)
				mm = mm + "." + build.ToString ();
			if (revision != MAXINT)
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
				if (build == MAXINT)
					throw new ArgumentException ("fields is larger than the number of components defined in this instance");
				return major.ToString () + "." + minor.ToString () + "." +
					build.ToString ();
			}
			if (fields == 4){
				if (build == MAXINT || revision == MAXINT)
					throw new ArgumentException ("fields is larger than the number of components defined in this instance");
				return major.ToString () + "." + minor.ToString () + "." +
					build.ToString () + "." + revision.ToString ();
			}
			throw new ArgumentException ("Invalid fields parameter: " + fields.ToString());	
		}
	}
}



