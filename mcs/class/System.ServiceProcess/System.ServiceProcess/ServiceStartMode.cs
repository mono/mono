//
// System.ServiceProcess.ServiceStartMode
//
// Author: Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2002, Cesar Octavio Lopez Nataren 
//

namespace System.ServiceProcess
{	
	[Serializable]
        public enum ServiceStartMode {
		Automatic = 2,
		Manual = 3,
		Disabled = 4
	}
}
