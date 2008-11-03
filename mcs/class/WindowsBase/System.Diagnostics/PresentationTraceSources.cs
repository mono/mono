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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Windows;

namespace System.Diagnostics {

	public static class PresentationTraceSources {
		public static readonly DependencyProperty TraceLevelProperty;

		public static TraceSource AnimationSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource DataBindingSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource DependencyPropertySource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource DocumentsSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource FreezableSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource HwndHostSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource MarkupSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource NameScopeSource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource ResourceDictionarySource {
			get { throw new NotImplementedException (); }
		}

		public static TraceSource RoutedEventSource {
			get { throw new NotImplementedException (); }
		}

		public static PresentationTraceLevel GetTraceLevel (object element)
		{
			throw new NotImplementedException ();
		}

		public static void Refresh ()
		{
			throw new NotImplementedException ();
		}

		public static void SetTraceLevel (object element, PresentationTraceLevel traceLevel)
		{
			throw new NotImplementedException ();
		}
	}
}