//
// System.Runtime.CompilerServices.AccessedThroughPropertyAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	public sealed class AccessedThroughPropertyAttribute : Attribute
	{
		string name;
		public AccessedThroughPropertyAttribute (string propertyName)
		{
			name = propertyName;
		}

		public string PropertyName {
			get { return name; }
		}
	}
}
