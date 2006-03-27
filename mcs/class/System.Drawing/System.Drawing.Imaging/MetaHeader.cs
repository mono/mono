//
// System.Drawing.Imaging.MetaHeader.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

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

using System;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[StructLayout(LayoutKind.Sequential)]
	public sealed class MetaHeader {

		private short headerSize;
		private int maxRecord;
		private short noObjects;
		private short noParameters;
		private int size;
		private short type;
		private short version;

		// constructors
		public MetaHeader()
		{
		}

		// properties
		public short HeaderSize {
			get { return headerSize; }
			set { headerSize = value; }
		}
		
		public int MaxRecord {
			get { return maxRecord; }
			set { maxRecord = value; }
		}
		
		public short NoObjects {
			get { return noObjects; }
			set { noObjects = value; }
		}
		
		public short NoParameters {
			get { return noParameters; }
			set { noParameters = value; }
		}
		
		public int Size {
			get { return size; }
			set { size = value; }
		}

		public short Type {
			get { return type; }
			set { type = value; }
		}

		public short Version {
			get { return version; }
			set { version = value; }
		}
		
	}

}
