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
using System.Runtime.CompilerServices;

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

	internal class MonoAsyncResult : IAsyncResult {

		object async_state;
		WaitHandle handle;
		IntPtr data;
		bool sync_completed;
		bool completed;
		
		public object AsyncState
		{
			get {
				return async_state;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get {
				return handle;
			}
		}

		public bool CompletedSynchronously
		{
			get {
				return sync_completed;
			}
		}

		public bool IsCompleted
		{
			get {
				return completed;
			}
		}
		

	}
	
} // Namespace System


