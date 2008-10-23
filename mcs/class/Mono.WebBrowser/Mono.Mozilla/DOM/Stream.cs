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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using io=System.IO;
using Mono.Mozilla.DOM;

namespace Mono.Mozilla
{
	internal class Stream : nsIInputStream, nsIOutputStream {
		
		io.Stream back;
		
		public Stream (io.Stream stream) {
			this.back = stream;
		}


		public io.Stream BaseStream {
			get { return back; }
		}
		
		public int close ()
		{
			back.Close ();
			return 0;
		}



		public int flush ()
		{
			back.Flush ();
			return 0;
		}



		public int write ([MarshalAs (UnmanagedType.LPStr) ]  string str,
				 uint count,
				out uint ret)
		{
			ret = count;
			if (count <= 0) {
				return 0;
			}
			
			byte [] buffer = Encoding.ASCII.GetBytes (str);
			back.Write(buffer, 0, (int)count);
			return 0;
		}



		public int writeFrom ([MarshalAs (UnmanagedType.Interface) ]  nsIInputStream aFromStream,
				 uint aCount,
				out uint ret)
		{
			ret = 0;
			return 0;
		}



		public int writeSegments ( nsIReadSegmentFunDelegate aReader,
				 IntPtr aClosure,
				 uint aCount,
				out uint ret)
		{
			ret = 0;
			return 0;
		}

		public int isNonBlocking (out bool ret)
		{
			ret = false;
			return 0;
		}


		public int available ( out uint ret) {
			ret = 0;
			return 0;
		}

		public int read (
				   HandleRef str,
				   uint count, out uint ret) {
			byte[] buffer = new byte[count];
			ret = (uint) back.Read (buffer, 0, (int)count);
			string res = Encoding.ASCII.GetString (buffer);
			Base.StringSet (str, res);
			return 0;
		}


		public int readSegments (
				   nsIWriteSegmentFunDelegate aWriter,
				   IntPtr aClosure,
				   uint aCount, out uint ret) {
			ret = 0;
			return 0;
		}	
	}
}
		