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

			v = version;

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

		
	}
}

