//
// QueryEventArgs.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

#if NET_2_0
namespace System.Xml.Query
{
	public abstract class QueryEventArgs : EventArgs
	{
		[MonoTODO]
		protected QueryEventArgs ()
		{
		}

		public abstract string Message { get; }
	}
}

#endif
