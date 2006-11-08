//
// System.Diagnostics.Debugger.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
//
using System;

namespace System.Diagnostics
{
	/// <summary>
	/// Enables communication with a debugger.
	/// </summary>
	[MonoTODO]
	public sealed class Debugger
	{
		private static bool isAttached;
		
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
				return isAttached;
			}
		}
		
		/// <summary>
		/// Causes a breakpoint to be signaled to an attached debugger.
		/// </summary>
		[MonoTODO]
		public static void Break()
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Checks to see if logging is enabled by an attached debugger.
		/// </summary>
		[MonoTODO]
		public static bool IsLogging()
		{
			// Return false. DefaultTraceListener invokes this method, so throwing
			// a NotImplementedException wouldn't be appropriate.
      return false;

		}
		
		/// <summary>
		/// Launches and attaches a debugger to the process.
		/// </summary>
		[MonoTODO]
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
		[MonoTODO]
		public static void Log(int level, string category, string message)
		{
			// Do nothing. DefaultTraceListener invokes this method, so throwing
			// a NotImplementedException wouldn't be appropriate.
		}
		
		public Debugger()
		{
			isAttached = false;
		}
	}
}
