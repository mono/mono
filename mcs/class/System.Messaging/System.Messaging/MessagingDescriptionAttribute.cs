//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.ComponentModel;

namespace System.Messaging 
{
	[AttributeUsage(AttributeTargets.All)]
	public class MessagingDescriptionAttribute:  DescriptionAttribute 
	{
		[MonoTODO ("localization")]
		public MessagingDescriptionAttribute(string description) : base(description) 
		{
		}

		[MonoTODO ("localization")]
		public override string Description
		{
			get
			{
				return base.Description;
			}
		}
	}
}
