// 
// System.Web.Services.Protocols.MatchAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.All, Inherited = true)]
	public sealed class MatchAttribute : Attribute {

		#region Fields

		int capture;
		int group;
		bool ignoreCase;
		int maxRepeats;
		string pattern;

		#endregion

		#region Constructors

		public MatchAttribute (string pattern) 
		{
			ignoreCase = false;
			maxRepeats = -1;
			this.pattern = pattern;
			group = 1;
		}

		#endregion // Constructors

		#region Properties

		public int Capture {
			get { return capture; }
			set { capture = value; }
		}

		public int Group {
			get { return group; }
			set { group = value; }
		}

		public bool IgnoreCase {
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		public int MaxRepeats {
			get { return maxRepeats; }
			set { maxRepeats = value; }
		}

		public string Pattern {
			get { return pattern; }
			set { pattern = value; }
		}

		#endregion // Properties
	}
}
