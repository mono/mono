// 
// PeerSecuritySettings.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;

namespace System.ServiceModel
{
	public class PeerSecuritySettings
	{
		SecurityMode mode;
		
		public PeerSecuritySettings()
		{
		}
		
		public SecurityMode Mode {
			get { return mode; }
			set { mode = value; }
		}
	}
}
