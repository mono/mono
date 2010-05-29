//
// System.Runtime.Remoting.Channels.Ipc.Win32.IpcChannelHelper.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// Provides helper services to the IpcChannel implementation.
    /// </summary>
    internal sealed class IpcChannelHelper
    {
        public const string Scheme = "ipc";
        public const string SchemeStart = "ipc://";

        IpcChannelHelper()
        {
        }

        static readonly char[] InvalidPipeNameChars =
            new char[] {'\\', '/'};

        /// <summary>
        /// Validates a pipe name.
        /// </summary>
        /// <param name="pipeName">The pipe name.</param>
        /// <returns></returns>
        public static bool IsValidPipeName(string pipeName) 
        {
            if (pipeName == null || pipeName.Trim() == "")
                return false;

            if (pipeName.IndexOfAny(Path.InvalidPathChars) >= 0)
                return false;

            if (pipeName.IndexOfAny(InvalidPipeNameChars) >= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Parses an url against IpcChannel's rules.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="pipeName">The pipe name.</param>
        /// <param name="objectUri">The uri of the object.</param>
        /// <returns>All but the object uri.</returns>
        public static string Parse(string url, out string pipeName, out string objectUri)
        {
            if (url.StartsWith(SchemeStart)) 
            {
                int i = url.IndexOf('/', SchemeStart.Length);
                if (i >= 0) 
                {
                    pipeName = url.Substring(SchemeStart.Length, i - SchemeStart.Length);
                    objectUri = url.Substring(i);
                    return SchemeStart + pipeName;
                }
                else 
                {
                    pipeName = url.Substring(SchemeStart.Length);
                    objectUri = null;
                    return SchemeStart + pipeName;
                }
            }

            pipeName = null;
            objectUri = null;
            return null;
        }

        /// <summary>
        /// Parses an url against IpcChannel's rules.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="objectUri">The uri of the object.</param>
        /// <returns>All but the object uri.</returns>
        public static string Parse(string url, out string objectUri)
        {
            string pipeName;
            return Parse(url, out pipeName, out objectUri);
        }

        /// <summary>
        /// Copies a stream.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void Copy(Stream input, Stream output) 
        {
            MemoryStream ms = input as MemoryStream;
	    if (ms != null)
	    {
	        ms.WriteTo (output);
		return;
	    }

            // TODO: find out the optimal chunk size.
            const int size = 1024 * 1024;
            byte[] buffer = new byte[size];

            int count;
            while ((count = input.Read(buffer, 0, size)) > 0)
            {
                output.Write(buffer, 0, count);
            }
        }

    }
}

#endif
