//
// System.UnhandledExceptionEventArgs.cs 
//
// Author:
//   Chris Hynes (chrish@assistedsolutions.com)
//
// (C) 2001 Chris Hynes
//

namespace System 
{
	[Serializable]
	public class UnhandledExceptionEventArgs : EventArgs
	{
		private object exception;
		private bool m_isTerminating;

		public UnhandledExceptionEventArgs (object exception, bool isTerminating)
		{
			this.exception = exception;
			this.m_isTerminating = isTerminating;
		}

		public object ExceptionObject {
			get {
				return exception;
			}
		}

		public bool IsTerminating {
			get {
				return m_isTerminating;
			}
		}
	}
}
