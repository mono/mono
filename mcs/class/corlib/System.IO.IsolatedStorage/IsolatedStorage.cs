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
		private IsolatedStorageScope _scope;

		// Constructor
		protected IsolatedStorage ()
			: base ()
		{
		}

		// Properties
		[MonoTODO ("Code Identity is CAS related")]
		public object AssemblyIdentity {
			get {
				throw new NotImplementedException (
					Locale.GetText ("CAS related")); 
			}
		}

		[CLSCompliant (false)]
		public virtual ulong CurrentSize {
			get {
				throw new InvalidOperationException (
					Locale.GetText ("IsolatedStorage does not have a preset CurrentSize."));
			}
		}

		[MonoTODO ("Code Identity is CAS related")]
		public object DomainIdentity {
			get {
				throw new NotImplementedException (
					Locale.GetText ("CAS related")); 
			}
		}

		[CLSCompliant (false)]
		public virtual ulong MaximumSize {
			get {
				throw new InvalidOperationException (
					Locale.GetText ("IsolatedStorage does not have a preset MaximumSize."));
			}
		}

		public IsolatedStorageScope Scope {
			get { return _scope; }
		}

		protected virtual char SeparatorExternal {
			get { return System.IO.Path.DirectorySeparatorChar; }
		}

		protected virtual char SeparatorInternal {
			get { return '.'; }
		}

		// Methods
		protected abstract IsolatedStoragePermission GetPermission (PermissionSet ps);

		[MonoTODO ("Evidences are CAS related")]
		protected void InitStore (
			IsolatedStorageScope scope, Type domainEvidenceType,
			Type assemblyEvidenceType)
		{
			// no exception here because this can work without CAS
			_scope = scope;

		}

		public abstract void Remove ();
	}
}
