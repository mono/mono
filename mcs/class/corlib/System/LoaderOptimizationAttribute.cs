//
// System.LoaderOptimizationAttribute.cs
//
// Author: 
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class LoaderOptimizationAttribute : Attribute
	{
		private LoaderOptimization lo;
		
		// Constructors
		public LoaderOptimizationAttribute (byte value)
		{
			lo = (LoaderOptimization) value;
		}

		public LoaderOptimizationAttribute (LoaderOptimization value)
		{
			lo = value;
		}

		// Properties
		public LoaderOptimization Value {
			get { return lo; }
		}
	}
}
