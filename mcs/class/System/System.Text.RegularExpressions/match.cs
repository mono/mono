//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	match.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;

namespace System.Text.RegularExpressions {

	[Serializable]
	public class Capture {
		public int Index {
			get { return index; }
		}

		public int Length {
			get { return length; }
		}

		public string Value {
			get { 
				if (text!= null)
					return text.Substring (index, length); 
				else
					return String.Empty;
			}
		}

		public override string ToString () {
			return Value;
		}

		// internal members

		internal Capture (string text) : this (text, 0, 0) { }

		internal Capture (string text, int index, int length) {
			this.text = text;
			this.index = index;
			this.length = length;
		}
		
		internal string Text {
			get { return text; }
		}

		// private

		internal int index, length;
		internal string text;
	}

	[Serializable]
	public class Group : Capture {
		public static Group Synchronized (Group inner) {
			return inner;	// is this enough?
		}

		public CaptureCollection Captures {
			get { return captures; }
		}

		public bool Success {
			get { return success; }
		}

		// internal

		internal Group (string text, int[] caps) : base (text) {
			this.captures = new CaptureCollection ();

			if (caps == null || caps.Length == 0) {
				this.success = false;
				return;
			}

			this.success = true;
			this.index = caps[0];
			this.length = caps[1];
			captures.Add (this);
			for (int i = 2; i < caps.Length; i += 2)
				captures.Add (new Capture (text, caps[i], caps[i + 1]));
			captures.Reverse ();
		}
		
		internal Group (): base ("")
		{
			captures = new CaptureCollection ();
		}

		private bool success;
		private CaptureCollection captures;
	}

	[Serializable]
	public class Match : Group {
		public static Match Empty {
			get { return empty; }
		}
		
		public static Match Synchronized (Match inner) {
			return inner;	// FIXME need to sync on machine access
		}
		
		public virtual GroupCollection Groups {
			get { return groups; }
		}

		public Match NextMatch () {
			if (this == Empty)
				return Empty;

			int scan_ptr = regex.RightToLeft ? Index : Index + Length;

			// next match after an empty match: make sure scan ptr makes progress
			
			if (Length == 0)
				scan_ptr += regex.RightToLeft ? -1 : +1;

			return machine.Scan (regex, Text, scan_ptr, text_length);
		}

		public virtual string Result (string replacement) {
			return ReplacementEvaluator.Evaluate (replacement, this);
		}

		// internal

		internal Match () : base (null, null) {
			this.regex = null;
			this.machine = null;
			this.text_length = 0;
			this.groups = new GroupCollection ();

			groups.Add (this);
		}
		
		internal Match (Regex regex, IMachine machine, string text, int text_length, int[][] grps) :
			base (text, grps[0])
		{
			this.regex = regex;
			this.machine = machine;
			this.text_length = text_length;

			this.groups = new GroupCollection ();
			groups.Add (this);
			for (int i = 1; i < grps.Length; ++ i)
				groups.Add (new Group (text, grps[i]));
		}

		internal Regex Regex {
			get { return regex; }
		}

		// private

		private Regex regex;
		private IMachine machine;
		private int text_length;
		private GroupCollection groups;

		private static Match empty = new Match ();
	}
}
