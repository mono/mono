//
// AuthorAttribute.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//
using System;

namespace Mono
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
	public class AuthorAttribute : System.Attribute
	{
		public string Name;
		public string SubProject;

		public AuthorAttribute(string name)
		{
			Name = name;
			SubProject = null;
		}

		public AuthorAttribute(string name, string subProject)
		{
			Name = name;
			SubProject = subProject;
		}

		public override string ToString()
		{
			if (SubProject == null)
				return Name;
			else
				return Name + " (" + SubProject + ")"; 
		}
	}
}
