// 
// System.Web.Services.Description.MimeTextMatch.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class MimeTextMatch : ServiceDescriptionFormatExtension {

		#region Fields

		int capture;
		int group;
		bool ignoreCase;
		MimeTextMatchCollection matches;
		string name;
		string pattern;
		int repeats;
		string type;

		#endregion // Fields

		#region Constructors
		
		public MimeTextMatch ()
		{
			capture = 0;
			group = 1;
			ignoreCase = false;
			matches = null;
			name = String.Empty;
			pattern = String.Empty;
			repeats = 1;
			type = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public int Capture {
			get { return capture; }
			set {
				if (value < 0)
					throw new ArgumentException ();
				capture = value; 
			}
		}
		
		public int Group {
			get { return group; }
			set {
				if (value < 0)
					throw new ArgumentException ();
				group = value; 
			}
		}

		public bool IgnoreCase {
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		public MimeTextMatchCollection Matches {
			get { return matches; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Pattern {
			get { return pattern; }
			set { pattern = value; }
		}

		public int Repeats {
			get { return repeats; }
			set {
				if (value < 0)
					throw new ArgumentException ();
				repeats = value; 
			}
		}

		public string RepeatsString {
			get { return Repeats.ToString (); }
			set { Repeats = Int32.Parse (value); }
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (MimeTextMatchCollection matches) 
		{
			this.matches = matches;
		}

		#endregion // Methods
	}
}
