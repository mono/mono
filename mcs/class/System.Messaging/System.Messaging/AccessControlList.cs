
//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.Collections;

namespace System.Messaging 
{
	public class AccessControlList: CollectionBase 
	{
		[MonoTODO]
		public AccessControlList()
		{
		}
		[MonoTODO]
		public int Add(AccessControlEntry entry) {
			throw new NotImplementedException();	
		}
		[MonoTODO]
		public bool Contains(AccessControlEntry entry) {
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void CopyTo(AccessControlEntry[] array, int index) {
			if (array == null) throw new ArgumentNullException();
			if (index < 0) throw new ArgumentOutOfRangeException();			
			throw new NotImplementedException();		
		}
		[MonoTODO]
		public int IndexOf(AccessControlEntry entry) {
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Insert(int index, AccessControlEntry entry) {
			throw new NotImplementedException();		
		}
		[MonoTODO]
		public void Remove(AccessControlEntry entry) {
			throw new NotImplementedException();
		}
		[MonoTODO]
		~AccessControlList()
		{
		}		
	}
}
