//
// System.Diagnostics.Debugger.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	[MonoTODO]
	public sealed class Debugger {

		[MonoTODO]
		public Debugger ()
		{
		}

		public static readonly string DefaultCategory = null;

		[MonoTODO]
		public static bool IsAttached {
			get {return false;}
		}

		[MonoTODO]
		public static void Break ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsLogging ()
		{
			// Return false. DefaultTraceListener invokes this method, so throwing
			// a NotImplementedException wouldn't be appropriate.
      return false;
		}

		[MonoTODO]
		public static bool Launch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Log (int level, string category, string message)
		{
			// Do nothing. DefaultTraceListener invokes this method, so throwing
			// a NotImplementedException wouldn't be appropriate.
		}
	}
}

