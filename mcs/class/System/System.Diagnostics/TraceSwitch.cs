//
// System.Diagnostics.TraceSwtich.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
//

namespace System.Diagnostics
{
	public class TraceSwitch : Switch
	{
		private TraceLevel level;
		private bool traceError = false;
		private bool traceInfo = false;
		private bool traceVerbose = false;
		private bool traceWarning = false;

		public TraceSwitch(string displayName, string description)
			: base(displayName, description)
		{
		}

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

		public bool TraceError
		{
			get 
			{
				return traceError;
			}
		}

		public bool TraceInfo
		{
			get 
			{
				return traceInfo;
			}
		}

		public bool TraceVerbose
		{
			get 
			{
				return traceVerbose;
			}
		}

		public bool TraceWarning
		{
			get
			{
				return traceWarning;
			}
		}
	}
}

