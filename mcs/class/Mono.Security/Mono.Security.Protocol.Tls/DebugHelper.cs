// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using System.Diagnostics;

namespace Mono.Security.Protocol.Tls
{
	internal class DebugHelper
	{
		private static bool isInitialized;

		[Conditional("DEBUG")]
		public static void Initialize()
		{
			if (!isInitialized)
			{
#if !NET_2_1
				Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
				// Debug.Listeners.Add(new TextWriterTraceListener(@"c:\ssl.log"));
				Debug.AutoFlush = true;
				Debug.Indent();
#endif

				isInitialized = true;
			}
		}

		[Conditional("DEBUG")]
		public static void WriteLine(string format, params object[] args)
		{
			Initialize();
			Debug.WriteLine(String.Format(format, args));
		}

		[Conditional("DEBUG")]
		public static void WriteLine(string message)
		{
			Initialize();
			Debug.WriteLine(message);
		}

		[Conditional("DEBUG")]
		public static void WriteLine(string message, byte[] buffer)
		{
			Initialize();
			DebugHelper.WriteLine(String.Format("{0} ({1} bytes))", message, buffer.Length));
			DebugHelper.WriteBuffer(buffer);
		}

		[Conditional("DEBUG")]
		public static void WriteBuffer(byte[] buffer)
		{
			Initialize();
			DebugHelper.WriteBuffer(buffer, 0, buffer.Length);
		}

		[Conditional("DEBUG")]
		public static void WriteBuffer(byte[] buffer, int index, int length)
		{
			Initialize();
			for (int i = index; i < length; i += 16)
			{
				int count = (length - i) >= 16 ? 16 : (length - i);
				string buf = "";
				for (int j = 0; j < count; j++)
				{
					buf += buffer[i + j].ToString("x2") + " ";
				}
				Debug.WriteLine(buf);
			}
		}
	}
}
