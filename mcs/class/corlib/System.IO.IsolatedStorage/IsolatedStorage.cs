//
// System.IO.IsolatedStorage.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Security;
using System.Security.Permissions;

namespace System.IO.IsolatedStorage
{
	public abstract class IsolatedStorage : MarshalByRefObject
	{
		// Constructor
		protected IsolatedStorage ()
			: base ()
		{
		}

		// Properties
		[MonoTODO]
		public object AssemblyIdentity {
			get {return null;}
		}

		[CLSCompliant (false)]
		[MonoTODO]
		public virtual ulong CurrentSize {
			get {return 0;}
		}

		[MonoTODO]
		public object DomainIdentity {
			get {return null;}
		}

		[CLSCompliant (false)]
		[MonoTODO]
		public virtual ulong MaximumSize {
			get {return 0;}
		}

		[MonoTODO]
		public IsolatedStorageScope Scope {
			get {return 0;}
		}

		[MonoTODO]
		protected virtual char SeparatorExternal {
			get {return Char.MinValue;}
		}

		[MonoTODO]
		protected virtual char SeparatorInternal {
			get {return Char.MinValue;}
		}

		// Methods
		protected abstract IsolatedStoragePermission GetPermission (PermissionSet ps);

		[MonoTODO]
		protected void InitStore (IsolatedStorageScope scope, Type domainEvidenceType,
					  Type assemblyEvidenceType)
		{
		}

		public abstract void Remove ();
	}
}
