//------------------------------------------------------------------------------
// 
// System.ResolveEventArgs.cs 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// 
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-06 
//
//------------------------------------------------------------------------------

using System;

namespace System {

	public class ResolveEventArgs
	{
		private string m_Name;
		public string Name {get{return m_Name;}}
	
		public ResolveEventArgs(string name){
			m_Name = name;
		}

	} // ResolveEventArgs

} // System


