//------------------------------------------------------------------------------
// 
// System.EventArgs.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Mon 07/16/2001 
//
//------------------------------------------------------------------------------

using System;

namespace System {

	[Serializable]
	public class EventArgs
	{
		public static readonly EventArgs Empty = new EventArgs();
	
		public EventArgs() { }
	}

} // System


