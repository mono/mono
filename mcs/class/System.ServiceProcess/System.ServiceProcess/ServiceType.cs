//
// System.ServiceProcess.ServiceType
//
// Author: Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2002, Cesar Octavio Lopez Nataren 
//

namespace System.ServiceProcess
{	
	[Flags]
	[Serializable]
        public enum ServiceType {
		KernelDriver = 1,
		FileSystemDriver = 2,
		Adapter = 4,
		RecognizerDriver = 8,
		Win32OwnProcess = 16,
		Win32ShareProcess = 32,
		InteractiveProcess = 256
	}
}
