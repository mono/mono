//
// System.ServiceProcess.ServiceControllerPermissionAccess
//
// Author: Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2002, Cesar Octavio Lopez Nataren 
//

namespace System.ServiceProcess
{	
	[Flags]
	[Serializable]
        public enum ServiceControllerPermissionAccess {
		None = 0,
		Browse = 2, 
		Control = 6,

	}
}
