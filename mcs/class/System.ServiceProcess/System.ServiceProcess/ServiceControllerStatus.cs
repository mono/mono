//
// System.ServiceProcess.ServiceControllerStatus
//
// Author: Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2002, Cesar Octavio Lopez Nataren 
//

namespace System.ServiceProcess
{	
	[Serializable]
        public enum ServiceControllerStatus {
		Stopped = 1,
		StartPending = 2,
		StopPending = 3,
		Running = 4,
		ContinuePending = 5,
		PausePending = 6,
		Paused = 7
	}
}
		
