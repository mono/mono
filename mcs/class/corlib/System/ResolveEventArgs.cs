//
// System.ResolveEventArgs.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak, All Rights Reserved
//

namespace System
{
	public class ResolveEventArgs : EventArgs
	{
		private string m_Name;

		public ResolveEventArgs (string name)
		{
			m_Name = name;
		}

		public string Name {
			get {
				return m_Name;
			}
		}
	}
}
