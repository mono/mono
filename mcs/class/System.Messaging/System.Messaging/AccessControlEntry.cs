//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
	public class AccessControlEntry 
	{
		#region Constructor
		
		[MonoTODO]
		public AccessControlEntry()
		{
		}
		[MonoTODO]
		public AccessControlEntry(Trustee trustee)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public AccessControlEntry(Trustee trustee,
			GenericAccessRights genericAccessRights,
			StandardAccessRights standardAccessRights,
			AccessControlEntryType entryType)
		{
			throw new NotImplementedException();
		}
		
		#endregion //Constructor
		
		
		#region Properties
		
		public AccessControlEntryType EntryType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public GenericAccessRights GenericAccessRights {
			[MonoTODO]
			get {throw new NotImplementedException(); }
			[MonoTODO]
			set {throw new NotImplementedException(); }
		}
		public StandardAccessRights StandardAccessRights {
			[MonoTODO]
			get { throw new NotImplementedException();}
			[MonoTODO]
			set { throw new NotImplementedException();}
		}
		public Trustee Trustee {
			[MonoTODO]
			get { throw new NotImplementedException();}
			[MonoTODO]
			set { throw new NotImplementedException();}
		}
		protected int CustomAccessRights {
			[MonoTODO]
			get { throw new NotImplementedException(); }
			[MonoTODO]
			set { throw new NotImplementedException(); }
		}
		
		#endregion //Properties
		
		
		[MonoTODO]
		~AccessControlEntry()
		{
		}
	}
}
