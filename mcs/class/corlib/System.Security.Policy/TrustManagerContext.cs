//
// System.Security.Policy.TrustManagerContext class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;

namespace System.Security.Policy {

	public class TrustManagerContext {

		private bool _debug;
		private bool _ignorePersistedDecision;
		private bool _noPrompt;
		private bool _keepAlive;
		private bool _persist;
		private TrustManagerUIContext _ui;

		[MonoTODO]
		public TrustManagerContext ()
			: this (TrustManagerUIContext.Run)
		{
		}

		public TrustManagerContext (TrustManagerUIContext uiContext)
		{
			_debug = false;
			_ignorePersistedDecision = false;
			_noPrompt = false;
			_keepAlive = false;
			_persist = false;
			_ui = uiContext;
		}

		public bool Debug {
			get { return _debug; }
			set { _debug = value; }
		}

		public bool IgnorePersistedDecision {
			get { return _ignorePersistedDecision; }
			set { _ignorePersistedDecision = value; }
		}

		public bool NoPrompt {
			get { return _noPrompt; }
			set { _noPrompt = value; }
		}

		public bool KeepAlive {
			get { return _keepAlive; }
			set { _keepAlive = value; }
		}

		public bool Persist {
			get { return _persist; }
			set { _persist = value; }
		}

		public TrustManagerUIContext UIContext {
			get { return _ui; }
			set { _ui = value; }
		}
	}
}

#endif
