//------------------------------------------------------------------------------
// 
// System.IAsyncResult.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Mon 08/24/2001 
//
//------------------------------------------------------------------------------

using System;
using System.Threading;

namespace System {

	public interface IAsyncResult
	{
		object AsyncState
		{
			get;
		}

		WaitHandle AsyncWaitHandle
		{
			get;
		}

		bool CompletedSynchronously
		{
			get;
		}

		bool IsCompleted
		{
			get;
		}
	}

} // Namespace System


