//
// MapAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) Novell, Inc.  
//
using System;
namespace Mono.Posix {

	[AttributeUsage (AttributeTargets.Enum)]
	public class MapAttribute : Attribute {
	}
}
