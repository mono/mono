// 
// System.Web.Services.Description.MimeTextMatch.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
