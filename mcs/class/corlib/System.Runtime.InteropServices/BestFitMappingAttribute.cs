//
// System.Runtime.InteropServices.BestFitMappingAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Novell, Inc.  http://www.ximian.com
//
using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	[Serializable]
	public sealed class BestFitMappingAttribute : Attribute {
		bool bfm;
		
		public BestFitMappingAttribute (bool bfm)
		{
			this.bfm = bfm;
		}

		public bool ThrowOnUnmappableChar = false;
		
		public bool BestFitMapping {
			get {
				return bfm;
			}
		}
	}
}
