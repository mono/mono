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
		CodeObject CodeDOM {
			get;
		}


		//[Guid ("")]
		string SourceText {
			get;
			set;
		}


		//[Guid ("")]
		void AddEventSource (string eventSourceName, string eventSourceType);



		//[Guid ("")]
		void AppendSourceText (string text);

		
		//[Guid("")]
		void RemoveEventSource (string eventSourceName);
	}
}
