//
// TODOAttribute.cs
//
// Author:
//   Ravi Pratap (ravi@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
using System;

namespace Mono.Data.TdsTypes {

	/// <summary>
	///   The TODO attribute is used to flag all incomplete bits in our class libraries
	/// </summary>
	///
	/// <remarks>
	///   Use this to decorate any element which you think is not complete
	/// </remarks>
	[AttributeUsage (AttributeTargets.All)]
	internal class MonoTODOAttribute : Attribute {

		string comment;

		public MonoTODOAttribute ()
		{}

		public MonoTODOAttribute (string comment)
		{
			this.comment = comment;
		}
	}
}
