//
// System.Management.ManagementOperationObserver
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
namespace System.Management
{
	public class ManagementOperationObserver
	{
		[MonoTODO]
		public ManagementOperationObserver ()
		{
		}

		[MonoTODO]
		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		public event CompletedEventHandler Completed;
		public event ObjectPutEventHandler ObjectPut;
		public event ObjectReadyEventHandler ObjectReady;
		public event ProgressEventHandler Progress;
	}
}

