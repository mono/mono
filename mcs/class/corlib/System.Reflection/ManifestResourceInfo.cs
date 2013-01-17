// System.Reflection.ManifestResourceInfo
//
// Sean MacIsaac (macisaac@ximian.com)
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. 2001

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

using System.Runtime.InteropServices;

namespace System.Reflection
{
	[ComVisible (true)]
	[StructLayout (LayoutKind.Sequential)]
	public class ManifestResourceInfo
	{
		private Assembly _assembly;
		private string _filename;
		private ResourceLocation _location;

		internal ManifestResourceInfo ()
		{
		}

#if NET_4_0
		public
#else
		internal
#endif
		ManifestResourceInfo (Assembly containingAssembly, string containingFileName, ResourceLocation resourceLocation)
		{
			_assembly = containingAssembly;
			_filename = containingFileName;
			_location = resourceLocation;
		}
		public virtual string FileName {
			get { return _filename; }
		}

		public virtual Assembly ReferencedAssembly {
			get { return _assembly; }
		}

		public virtual ResourceLocation ResourceLocation {
			get { return _location; }
		}
	}
}
