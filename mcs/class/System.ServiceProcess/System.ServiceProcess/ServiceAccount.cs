//
// System.ServiceProcess.ServiceAccount
//
// Author: Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2002, Cesar Octavio Lopez Nataren 

namespace System.ServiceProcess
{
	[Serializable]
	public enum ServiceAccount {
		LocalService = 0,
		NetworkService = 1,	
		LocalSystem = 2,
		User = 3
	}
}
