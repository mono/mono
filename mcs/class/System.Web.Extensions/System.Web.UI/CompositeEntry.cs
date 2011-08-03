//
// Authors:
//      Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com)
//
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
using System.IO;
using System.Reflection;
using System.Web.Hosting;

namespace System.Web.UI
{	  
	sealed class CompositeEntry
	{
		public Assembly Assembly;
		public string NameOrPath;
		public WebResourceAttribute Attribute;

		string GetFilePath ()
		{
			if (Assembly != null)
				return Assembly.Location;
			else if (!String.IsNullOrEmpty (NameOrPath))
				return HostingEnvironment.MapPath (NameOrPath);
			else
				return String.Empty;
		}
		
		public bool IsModifiedSince (DateTime since)
		{
			return File.GetLastWriteTimeUtc (GetFilePath ()) > since;
		}

		public bool IsModifiedSince (long atime)
		{
			return File.GetLastWriteTimeUtc (GetFilePath ()).Ticks > atime;
		}
		
		public override int GetHashCode ()
		{
			int ret = 0;

			if (Assembly != null)
				ret ^= Assembly.GetHashCode ();
			if (NameOrPath != null)
				ret ^= NameOrPath.GetHashCode ();

			return ret;
		}
	}
}
