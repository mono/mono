//
// System.Diagnostics.Debugger.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
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
using System.Runtime.CompilerServices;

using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	/// <summary>
	/// Enables communication with a debugger.
	/// </summary>
	[ComVisible (true)]
	[MonoTODO ("The Debugger class is not functional")]
	public sealed class Debugger
	{

		/// <summary>
		/// Represents the default category of a message with a constant.
		/// </summary>
		public static readonly string DefaultCategory = "";

		/// <summary>
		/// Returns a Boolean indicating whether a debugger is attached to a process.
		/// </summary>
		/// <value>
		/// true if debugger is attached; otherwise, false.
		/// </value>
		public static bool IsAttached
		{
			get
			{
				return IsAttached_internal ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool IsAttached_internal ();

		/// <summary>
		/// Causes a breakpoint to be signaled to an attached debugger.
		/// </summary>
		public static void Break()
		{
			// The JIT inserts a breakpoint on the caller.
		}

		/// <summary>
		/// Checks to see if logging is enabled by an attached debugger.
		/// </summary>
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern bool IsLogging();

		/// <summary>
		/// Launches and attaches a debugger to the process.
		/// </summary>
		[MonoTODO ("Not implemented")]
		public static bool Launch()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Posts a message for the attached debugger.
		/// </summary>
		/// <param name="level">
		/// A description of the importance of this message
		/// </param>
		/// <param name="category">
		/// A string describing the category of this message.
		/// </param>
		/// <param name="message">
		/// A string representing the message to show.
		/// </param>
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void Log(int level, string category, string message);

#if NET_4_0
		[ObsoleteAttribute("Call the static methods directly on this type", true)]
#endif
		public Debugger()
		{
		}

#if MONODROID
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void Mono_UnhandledException_internal (Exception ex);

		internal static void Mono_UnhandledException (Exception ex)
		{
			Mono_UnhandledException_internal (ex);
		}
#endif
	}
}
