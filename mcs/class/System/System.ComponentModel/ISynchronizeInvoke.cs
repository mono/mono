//
// System.ComponentModel.ISynchronizeInvoke.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.ComponentModel 
{
	public interface ISynchronizeInvoke 
	{
		bool InvokeRequired {
			get;
		}

		IAsyncResult BeginInvoke(Delegate method, object[] args);

		object EndInvoke(IAsyncResult result);

		object Invoke(Delegate method, object[] args);
	}
}
