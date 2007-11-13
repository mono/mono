//
// System.Reflection/ExceptionHandlingClause.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class ExceptionHandlingClause {
		#region Sync with reflection.h
		internal Type catch_type;
		internal int filter_offset;
		internal ExceptionHandlingClauseOptions flags;
		internal int try_offset;
		internal int try_length;
		internal int handler_offset;
		internal int handler_length;
		#endregion

		internal ExceptionHandlingClause () {
		}

		public Type CatchType {
			get {
				return catch_type;
			}
		}

		public int FilterOffset {
			get {
				return filter_offset;
			}
		}

		public ExceptionHandlingClauseOptions Flags {
			get {
				return flags;
			}
		}

		public int HandlerLength {
			get {
				return handler_length;
			}
		}

		public int HandlerOffset {
			get {
				return handler_offset;
			}
		}

		public int TryLength {
			get {
				return try_length;
			}
		}

		public int TryOffset {
			get {
				return try_offset;
			}
		}

		public override string ToString ()
		{
			string ret = String.Format ("Flags={0}, TryOffset={1}, TryLength={2}, HandlerOffset={3}, HandlerLength={4}",
						    flags, try_offset, try_length, handler_offset, handler_length);
			if (catch_type != null)
				ret = String.Format ("{0}, CatchType={1}", ret, catch_type);
			if (flags == ExceptionHandlingClauseOptions.Filter)
				ret = String.Format (CultureInfo.InvariantCulture, "{0}, FilterOffset={1}", ret, filter_offset);
			return ret;
		}
	}

}

#endif
