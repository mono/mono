//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.Collections;

namespace System.Messaging 
{
	public class MessageQueueEnumerator: MarshalByRefObject, IEnumerator, IDisposable 
	{
        // To avoid multiple disposals
        private bool disposed = false;
		private ArrayList queueList;
		private int currentIndex;
		
		internal MessageQueueEnumerator(ArrayList queueList)
		{
			this.queueList = queueList;
			this.currentIndex = -1;
		}

		public MessageQueue Current 
		{
			get 
			{
				if (currentIndex < 0 ||
					currentIndex >= queueList.Count)
					return null;
				return (MessageQueue)queueList[currentIndex];
			}
		}
		
		object IEnumerator.Current 
		{
			get 
			{
				if (currentIndex < 0 ||
					currentIndex >= queueList.Count)
					return null;
				return queueList[currentIndex];
			}
		}
		
		public IntPtr LocatorHandle 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		
		public void Close()
		{
			Dispose();
		}
		
		public void Dispose()
		{
            // Do this only at the first time
            if (!this.disposed)
				Dispose(true);
            disposed = true;         
            // Take this object off the finalization queue 
            //GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			// do nothing
		}
		
		public bool MoveNext()
		{
			return (++currentIndex) < queueList.Count;			
		}
		
		public void Reset()
		{
			currentIndex = -1;
		}
		
		~MessageQueueEnumerator()
		{
           	if (!this.disposed)
				Dispose(false);		
		}
	}
}
