//
// IVsaCodeItem.cs
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;
	using System.CodeDom;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
        public interface IVsaCodeItem : IVsaItem
	{
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		CodeObject CodeDOM {
			get;
		}


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		string SourceText {
			get;
			set;
		}


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void AddEventSource (string eventSourceName, string eventSourceType);



		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void AppendSourceText (string text);

		
		//[Guid("")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		void RemoveEventSource (string eventSourceName);
	}
}
