//
// IVsaReferenceItem.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaReferenceItem : IVsaItem
	{
		// public property
	
		//[Guid ("")]
		string AssemblyName {
			get;
			set;
		}
	}
}
