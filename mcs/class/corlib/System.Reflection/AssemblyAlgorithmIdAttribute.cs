//
// System.Reflection.AssemblyAlgorithmIdAttribute.cs
//
// Author: Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Configuration.Assemblies;

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyAlgorithmIdAttribute : Attribute
	{
		// Field
		private uint id;
		
		// Constructor
		public AssemblyAlgorithmIdAttribute (AssemblyHashAlgorithm algorithmId)
		{
			id = (uint) algorithmId;
		}
		
		[CLSCompliant (false)]
		public AssemblyAlgorithmIdAttribute (uint algorithmId)
		{
			id = algorithmId;
		}
		
		// Property
		[CLSCompliant (false)]
		public uint Algorithmid
		{
			get { return id; }
		}
	}
}
