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
	/* I'm just guessing that this is the correct place for this
	 * attribute, and that the option is correct.  It shuts up
	 * CorCompare for this undocumented class.
	 */
	[EditorBrowsable (EditorBrowsableState.Never)]
	public abstract class RegexRunner {
		// constructor
	
		protected internal RegexRunner () {
			throw new NotImplementedException ("RegexRunner is not supported by Mono.");
		}

		// protected abstract

		protected abstract bool FindFirstChar ();

		protected abstract void Go ();

		protected abstract void InitTrackCount ();

		// protected methods

		protected void Capture (int capnum, int start, int end) {
		}

		protected static bool CharInSet (char ch, string set, string category) {
			return false;
		}

		protected void Crawl (int i) {
		}

		protected int CrawlPos () {
			return 0;
		}

		protected void DoubleCrawl () {
		}

		protected void DoubleStack () {
		}

		protected void DoubleTrack () {
		}

		protected void EnsureStorage () {
		}

		protected bool IsBoundary (int index, int startpos, int endpos) {
			return false;
		}

		protected bool IsECMABoundary (int index, int startpos, int endpos) {
			return false;
		}

		protected bool IsMatched (int cap) {
			return false;
		}

		protected int MatchIndex (int cap) {
			return 0;
		}

		protected int MatchLength (int cap) {
			return 0;
		}

		protected int PopCrawl () {
			return 0;
		}

		protected void TransferCapture (int capnum, int uncapnum, int start, int end) {
		}

		protected void Uncapture () {
		}

		// internal
		
		protected internal Match Scan (Regex regex, string text, int textbeg, int textend, int textstart, int prevlen, bool quick) {
			return null;
		}
	}
}
