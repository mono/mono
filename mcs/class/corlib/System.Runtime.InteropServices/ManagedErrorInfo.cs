// ManagedErrorInfo class
//
// Eberhard Beilharz (eb1@sil.org)
//
// Copyright (C) 2012 SIL International

#if FEATURE_COMINTEROP

using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Runtime.InteropServices
{
	/// <summary>
	/// Helper class that allows to pass an exception as an IErrorInfo object. This is useful
	/// when we get an exception in managed code that is called from unmanaged code that is called
	/// from managed code and we want to get to the exception in the outer managed code.
	/// </summary>
	internal class ManagedErrorInfo: IErrorInfo
	{
		private Exception m_Exception;
		public ManagedErrorInfo (Exception e)
		{
			m_Exception = e;
		}

		public Exception Exception {
			get { return m_Exception; }
		}

		#region IErrorInfo
		public int GetGUID (out Guid guid)
		{
			// not supported
			guid = Guid.Empty;
			return 0;
		}

		public int GetSource (out string source)
		{
			source = m_Exception.Source;
			return 0;
		}

		public int GetDescription (out string description)
		{
			description = m_Exception.Message;
			return 0;
		}

		public int GetHelpFile (out string helpFile)
		{
			helpFile = m_Exception.HelpLink;
			return 0;
		}

		public int GetHelpContext(out uint helpContext)
		{
			// not supported
			helpContext = 0;
			return 0;
		}
		#endregion
	}
}

#endif
