//
// DefaultParserAttribute.cs: Marks a parser as the default one
//    						  for file extensions not matched against the map
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2003 Ximian, Inc.
//

namespace Mono.Languages
{
	using System;
	using System.Reflection;

	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultParserAttribute : System.Attribute 
	{
		// just a boolean marker
	}
}
