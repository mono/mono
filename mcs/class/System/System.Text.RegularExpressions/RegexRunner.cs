//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	RegexRunner.cs
//
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(c) 2002

using System;
using System.ComponentModel;

namespace System.Text.RegularExpressions {
	[EditorBrowsable (EditorBrowsableState.Never)]
	public abstract class RegexRunner {
		// constructor

		[MonoTODO]
		protected internal RegexRunner () {
			throw new NotImplementedException ("RegexRunner is not supported by Mono.");
		}

		// protected abstract

		protected abstract bool FindFirstChar ();

		protected abstract void Go ();

		protected abstract void InitTrackCount ();

		// protected methods

		[MonoTODO]
		protected void Capture (int capnum, int start, int end) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool CharInSet (char ch, string set, string category) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void Crawl (int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected int Crawlpos () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DoubleCrawl () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DoubleStack () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DoubleTrack () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void EnsureStorage () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool IsBoundary (int index, int startpos, int endpos) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool IsECMABoundary (int index, int startpos, int endpos) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool IsMatched (int cap) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected int MatchIndex (int cap) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected int MatchLength (int cap) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected int Popcrawl () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void TransferCapture (int capnum, int uncapnum, int start, int end) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void Uncapture () {
			throw new NotImplementedException ();
		}

		// internal
		
		protected internal Match Scan (Regex regex, string text, int textbeg, int textend, int textstart, int prevlen, bool quick) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal int[] runcrawl;
		[MonoTODO]
		protected internal int runcrawlpos;
		[MonoTODO]
		protected internal Match runmatch;
		[MonoTODO]
		protected internal Regex runregex;
		[MonoTODO]
		protected internal int[] runstack;
		[MonoTODO]
		protected internal int runstackpos;
		[MonoTODO]
		protected internal string runtext;
		[MonoTODO]
		protected internal int runtextbeg;
		[MonoTODO]
		protected internal int runtextend;
		[MonoTODO]
		protected internal int runtextpos;
		[MonoTODO]
		protected internal int runtextstart;
		[MonoTODO]
		protected internal int[] runtrack;
		[MonoTODO]
		protected internal int runtrackcount;
		[MonoTODO]
		protected internal int runtrackpos;
	}
}
