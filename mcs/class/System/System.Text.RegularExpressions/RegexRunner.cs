//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	RegexRunner.cs
//
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(c) 2002
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Text.RegularExpressions {

	[EditorBrowsable (EditorBrowsableState.Never)]
	[MonoTODO ("RegexRunner is not supported by Mono.")]
	public abstract class RegexRunner {
		// constructor

		[MonoTODO]
		protected internal RegexRunner ()
		{
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
		protected static bool CharInClass (char ch, string charClass)
		{
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
