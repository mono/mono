//
// System.IO.IsolatedStorage.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Security;
using System.Globalization;
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

                protected static IsolatedStorageScope storage_scope;

		// Properties
                [MonoTODO]
		public object AssemblyIdentity {
			get {return null;}                        
		}

		[CLSCompliant (false)]
		public virtual ulong CurrentSize {
			get {
                                throw new InvalidOperationException (
                                        Locale.GetText ("IsolatedStorage does not have a preset CurrentSize."));
                        }
		}

		[MonoTODO]
		public object DomainIdentity {
			get {return null;}
		}

		[CLSCompliant (false)]
		public virtual ulong MaximumSize {
                        get {
                                throw new InvalidOperationException (
                                        Locale.GetText ("IsolatedStorage does not have a preset MaximumSize."));
                        }
                }

		public IsolatedStorageScope Scope {
			get { return storage_scope; }
		}

		protected virtual char SeparatorExternal {
			get { return System.IO.Path.DirectorySeparatorChar; }
		}

		protected virtual char SeparatorInternal {
			get { return '.'; }
		}

		// Methods
		protected abstract IsolatedStoragePermission GetPermission (PermissionSet ps);

		[MonoTODO]
		protected void InitStore (
                        IsolatedStorageScope scope, Type domainEvidenceType,
                        Type assemblyEvidenceType)
		{
		}

		public abstract void Remove ();
	}
}
