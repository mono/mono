//------------------------------------------------------------------------------
// 
// System.ComponentModel.IExtenderProvider.
//
// Author:  Asier Llano Palacios, asierllano@infonegocio.com
//
//------------------------------------------------------------------------------

using System;

namespace System.ComponentModel {

	public interface IExtenderProvider {
		bool CanExtend( object extendee );
	}
}


