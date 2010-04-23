//
// ObjectCache.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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

namespace System.Runtime.Caching
{
	static class Helpers
	{
		public static bool CaseInsensitive {
			get;
			private set;
		}

		public static bool Is64Bit {
			get;
			private set;
		}
		
		public static IEqualityComparer <string> StringEqualityComparer {
                        get; private set;
                }

		public static IComparer <string> StringComparer {
			get; private set;
		}
		
		public static StringComparison StringComparison {
                        get; private set;
                }

		static Helpers ()
		{
			PlatformID pid = Environment.OSVersion.Platform;
                        bool runningOnWindows = ((int) pid != 128 && pid != PlatformID.Unix && pid != PlatformID.MacOSX);

			Is64Bit = IntPtr.Size == 8;
                        if (runningOnWindows)
                                CaseInsensitive = true;
                        else {
                                string mono_iomap = Environment.GetEnvironmentVariable ("MONO_IOMAP");
                                if (!String.IsNullOrEmpty (mono_iomap)) {
                                        if (mono_iomap == "all")
                                                CaseInsensitive = true;
                                        else {
                                                string[] parts = mono_iomap.Split (':');
                                                foreach (string p in parts) {
                                                        if (p == "all" || p == "case") {
                                                                CaseInsensitive = true;
                                                                break;
                                                        }
                                                }
                                        }
                                }
                        }

                        if (CaseInsensitive) {
                                StringEqualityComparer = global::System.StringComparer.OrdinalIgnoreCase;
				StringComparer = global::System.StringComparer.OrdinalIgnoreCase;
                                StringComparison = global::System.StringComparison.OrdinalIgnoreCase;
                        } else {
                                StringEqualityComparer = global::System.StringComparer.Ordinal;
				StringComparer = global::System.StringComparer.Ordinal;
                                StringComparison = global::System.StringComparison.Ordinal;

                        }	
		}
	}
}
