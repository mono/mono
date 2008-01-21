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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using Mono.WebBrowser;

namespace Mono.WebBrowser.DOM
{
	public interface INavigation
	{
		bool CanGoBack { get; }
		bool CanGoForward { get; }
		bool Back ();
		bool Forward ();
		void Home ();
		void Reload ();
		void Reload (ReloadOption option);
		void Stop ();
		void Go (string url);
		void Go (string url, LoadFlags flags);
	}
	
	[Flags]
	public enum LoadFlags : uint {
		None = 0x0000,
		AsMetaRefresh = 0x0010,
		AsLinkClick = 0x0020,
		BypassHistory = 0x0040,
		ReplaceHistory = 0x0080,
		BypassLocalCache = 0x0100,
		BypassProxy = 0x0200
	}
}
