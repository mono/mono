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
		public object AssemblyIdentity
		{
			get;
		}

		[CLSCompliant (false)]
		public virtual ulong CurrentSize
		{
			get;
		}

		public object DomainIdentity
		{
			get;
		}

		[CLSCompliant (false)]
		public virtual ulong MaximumSize
		{
			get;
		}

		public IsolatedStorageScope Scope
		{
			get;
		}

		protected virtual char SeperatorExternal
		{
			get;
		}

		protected virtual char SerperatorInternal
		{
			get;
		}

		// Methods
		protected abstract IsolatedStoragePermission GetPermission (PermissionSet ps);

		[MonoTODO]
		protected void InitStore (IsolatedStorageScope scope, Type domainEvidenceType,
					  Type assemblyEvidenceType)
		{
		}

		protected abstract void Remove ();
	}
}
