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
			get {
				if (!IsDefined)
					return 0;		// capture not completed
				else if (start <= end)
					return start;		// normal capture
				else
					return end;		// reverse capture
			}
		}

		public int Length {
			get {
				if (!IsDefined)
					return 0;
				else if (start <= end)
					return end - start;
				else
					return start - end;
			}
		}

		public string Value {
			get { return IsDefined ? text.Substring (Index, Length) : ""; }
		}

		public override string ToString () {
			return Value;
		}

		// internal members

		internal Capture () {			// empty capture
			this.previous = null;
			this.text = null;
			this.checkpoint = 0;

			this.start = -1;
			this.end = -1;
		}

		internal Capture (Capture cap) {	// copy constructor
			this.previous = cap.previous;
			this.text = cap.text;
			this.checkpoint = cap.checkpoint;

			this.start = cap.start;
			this.end = cap.end;
		}

		internal Capture (string text) {	// first capture
			this.previous = null;
			this.text = text;
			this.checkpoint = 0;

			this.start = -1;
			this.end = -1;
		}
		
		internal Capture (Capture previous, int checkpoint) {
			this.previous = previous;
			this.text = previous.text;
			this.checkpoint = checkpoint;

			this.start = -1;
			this.end = -1;
		}

		internal Capture Previous {
			get { return previous; }
		}

		internal string Text {
			get { return text; }
		}

		internal int Checkpoint {
			get { return checkpoint; }
		}

		internal bool IsDefined {
			get { return start >= 0 && end >= 0; }
		}

		internal Capture GetLastDefined () {
			Capture cap = this;
			while (cap != null && !cap.IsDefined)
				cap = cap.Previous;

			return cap;
		}

		internal void Open (int ptr) {
			this.start = ptr;
		}

		internal void Close (int ptr) {
			this.end = ptr;
		}

		// private

		private int start, end;
		private string text;
		private int checkpoint;
		private Capture previous;
	}

	public class Group : Capture {
		public static Group Synchronized (Group inner) {
			return inner;	// is this enough?
		}

		public CaptureCollection Captures {
			get { return captures; }
		}

		public bool Success {
			get { return GetLastDefined () != null; }
		}

		// internal

		internal Group () : base () {
		}
		
		internal Group (Capture last) : base (last) {
			captures = new CaptureCollection (last);

			// TODO make construction of captures lazy
		}

		private CaptureCollection captures;
	}

	public class Match : Group {
		public static Match Empty {
			get { return empty; }
		}
		
		public static Match Synchronized (Match inner) {
			return inner;	// FIXME need to sync on machine access
		}
		
		public GroupCollection Groups {
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

		internal Match () : base () {
			this.regex = null;
			this.machine = null;
			this.text_length = 0;
			this.groups = new GroupCollection ();

			groups.Add (this);
		}
		
		internal Match (Regex regex, IMachine machine, int text_length, Capture[] captures) : base (captures[0]) {
			this.regex = regex;
			this.machine = machine;
			this.text_length = text_length;
			this.groups = new GroupCollection ();

			groups.Add (this);
			for (int i = 1; i < captures.Length; ++ i)
				groups.Add (new Group (captures[i]));
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
