//
// UsageComplementAttribute.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//


using System;

namespace Mono
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class UsageComplementAttribute : System.Attribute
	{
		public string Details;

		public UsageComplementAttribute(string details)
		{
			Details = details;
		}

		public override string ToString()
		{
			return Details;
		}
	}
}
