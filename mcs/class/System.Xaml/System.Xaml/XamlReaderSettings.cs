//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlReaderSettings
	{
		public XamlReaderSettings ()
		{
		}

		public XamlReaderSettings (XamlReaderSettings settings)
		{
			// null settings is allowed (!)
			var s = settings;
			if (s == null)
				return;

			AllowProtectedMembersOnRoot = s.AllowProtectedMembersOnRoot;
			BaseUri = s.BaseUri;
			IgnoreUidsOnPropertyElements = s.IgnoreUidsOnPropertyElements;
			LocalAssembly = s.LocalAssembly;
			ProvideLineInfo = s.ProvideLineInfo;
			ValuesMustBeString = s.ValuesMustBeString;
		}

		public bool AllowProtectedMembersOnRoot { get; set; }
		public Uri BaseUri { get; set; }
		public bool IgnoreUidsOnPropertyElements { get; set; }
		public Assembly LocalAssembly { get; set; }
		public bool ProvideLineInfo { get; set; }
		public bool ValuesMustBeString { get; set; }
	}
}
