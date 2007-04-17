//
// System.CStreamWriter
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
//   Dick Porter (dick@ximian.com)
//
// (c) 2006 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
using System;
using System.Text;

namespace System.IO {
	class CStreamWriter : StreamWriter {
		TermInfoDriver driver;

		public CStreamWriter (Stream stream, Encoding encoding)
			: base (stream, encoding)
		{
			driver = (TermInfoDriver) ConsoleDriver.driver;
		}

		public override void Write (char [] buffer, int index, int count)
		{
			if (count <= 0)
				return;
			
			if (!driver.Initialized) {
				try {
					base.Write (buffer, index, count);
				} catch (IOException) {
				}
				
				return;
			}
			
			lock (this) {
				int i = 0, j = 0;
				char []buf;
				char c;
				
				// The idea here is that we want to limit our temp
				// buffer size - I picked 1k because the underlying
				// stream implementation seems to break writes into
				// 1k byte chunks.
				if (count > 1024)
					buf = new char[1024];
				else
					buf = new char[count];
				
				do {
					do {
						c = buffer [index + i++];
						
						if (driver.IsSpecialKey (c)) {
							// flush what we have
							if (j > 0) {
								try {
									base.Write (buf, 0, j);
								} catch (IOException) {
								}

								j = 0;
							}

							// write the special key
							driver.WriteSpecialKey (c);
						} else {
							buf[j++] = c;
							break;
						}
					} while (i < count);
					
					if (j > 0 && (j == buf.Length || i == count)) {
						// buffer is full or no more data to buffer
						// write it out
						try {
							base.Write (buf, 0, j);
						} catch (IOException) {
						}
						
						j = 0;
					}
				} while (i < count);
			}
		}

		public override void Write (char val)
		{
			lock (this) {
				try {
					if (driver.IsSpecialKey (val))
						driver.WriteSpecialKey (val);
					else
						InternalWriteChar (val);
				} catch (IOException) {
				}
			}
		}

		public void WriteKey (ConsoleKeyInfo key)
		{
			lock (this) {
				ConsoleKeyInfo copy = new ConsoleKeyInfo (key);
				if (driver.IsSpecialKey (copy))
					driver.WriteSpecialKey (copy);
				else
					InternalWriteChar (copy.KeyChar);
			}
		}

		public void InternalWriteString (string val)
		{
			try {
				base.Write (val);
			} catch (IOException) {
			}
		}

		public void InternalWriteChar (char val)
		{
			try {
				base.Write (val);
			} catch (IOException) {
			}
		}

		public override void Write (char [] val)
		{
			Write (val, 0, val.Length);
		}

		public override void Write (string val)
		{
			if (val == null)
				return;
			
			if (driver.Initialized)
				Write (val.ToCharArray ());
			else {
				try {
					base.Write (val);
				} catch (IOException){
					
				}
			}
		}
	}
}
#endif

