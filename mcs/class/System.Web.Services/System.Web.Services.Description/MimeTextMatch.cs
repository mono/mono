// 
// System.Web.Services.Description.MimeTextMatch.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class MimeTextMatch {

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

		[DefaultValue (0)]
		[XmlAttribute ("capture")]
		public int Capture {
			get { return capture; }
			set {
				if (value < 0)
					throw new ArgumentException ();
				capture = value; 
			}
		}
	
		[DefaultValue (1)]	
		[XmlAttribute ("group")]
		public int Group {
			get { return group; }
			set {
				if (value < 0)
					throw new ArgumentException ();
				group = value; 
			}
		}

		[XmlAttribute ("ignoreCase")]
		public bool IgnoreCase {
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		[XmlElement ("match")]
		public MimeTextMatchCollection Matches {
			get { return matches; }
		}

		[XmlAttribute ("name")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlAttribute ("pattern")]
		public string Pattern {
			get { return pattern; }
			set { pattern = value; }
		}

		[XmlIgnore]
		public int Repeats {
			get { return repeats; }
			set {
				if (value < 0)
					throw new ArgumentException ();
				repeats = value; 
			}
		}

		[DefaultValue ("1")]
		[XmlAttribute ("repeats")]
		public string RepeatsString {
			get { return Repeats.ToString (); }
			set { Repeats = Int32.Parse (value); }
		}

		[XmlAttribute ("type")]
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
