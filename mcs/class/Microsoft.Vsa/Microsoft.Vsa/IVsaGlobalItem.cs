//
// IVsaGlobalItem.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{	
	using System;
	using System.Runtime.InteropServices;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaGlobalItem : IVsaItem
	{
		//[Guid ("")]
		bool ExposeMembers {
			get;
			set;
		}


		//[Guid ("")]
		string TypeString { set; }
	}
}

		
