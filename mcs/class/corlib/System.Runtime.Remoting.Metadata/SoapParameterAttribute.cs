//
// System.Runtime.Remoting.Metadata.SoapParameterAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Parameter)]
	public sealed class SoapParameterAttribute : SoapAttribute
	{
		public SoapParameterAttribute ()
		{
		}
	}
}
