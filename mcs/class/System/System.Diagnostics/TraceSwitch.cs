//
// System.Diagnostics.TraceSwtich.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
//

namespace System.Diagnostics
{
	/// <summary>
	/// Multi-level switch to provide tracing and debug output without
	/// recompiling.
	/// </summary>
	public class TraceSwitch : Switch
	{
		private TraceLevel level;
		private bool traceError = false;
		private bool traceInfo = false;
		private bool traceVerbose = false;
		private bool traceWarning = false;

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="displayName">Name for the switch</param>
		/// <param name="description">Description of the switch</param>
		public TraceSwitch(string displayName, string description)
			: base(displayName, description)
		{
		}

		/// <summary>
		/// Gets or sets the trace level that specifies the messages to 
		/// output for tracing and debugging.
		/// </summary>
		public TraceLevel Level
		{
			get 
			{
				return level;
			}
			set
			{
				level = value;
			}

		}

		/// <summary>
		/// Gets a value indicating whether the Level is set to Error, 
		/// Warning, Info, or Verbose.
		/// </summary>
		public bool TraceError
		{
			get 
			{
				return traceError;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the Level is set to Info or Verbose.
		/// </summary>
		public bool TraceInfo
		{
			get 
			{
				return traceInfo;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the Level is set to Verbose.
		/// </summary>
		public bool TraceVerbose
		{
			get 
			{
				return traceVerbose;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the Level is set to
		/// Warning, Info, or Verbose.
		/// </summary>
		public bool TraceWarning
		{
			get
			{
				return traceWarning;
			}
		}
	}
}
