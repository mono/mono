//
// AboutAttribute.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//


using System;

namespace Mono
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AboutAttribute : System.Attribute
	{
		public string Details;

		public AboutAttribute(string details)
		{
			Details = details;
		}

		public override string ToString()
		{
			return Details;
		}
	}
}
