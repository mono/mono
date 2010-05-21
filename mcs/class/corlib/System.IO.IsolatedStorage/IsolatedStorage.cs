//
// System.IO.IsolatedStorage.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOONLIGHT
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.IO.IsolatedStorage {

	[ComVisible (true)]
	public abstract class IsolatedStorage : MarshalByRefObject {

		// Constructor
		protected IsolatedStorage ()
			: base ()
		{
		}

                internal IsolatedStorageScope storage_scope;
		internal object _assemblyIdentity;
		internal object _domainIdentity;
		internal object _applicationIdentity;

		// Properties

		[MonoTODO ("Does not currently use the manifest support")]
		[ComVisible (false)]
		public object ApplicationIdentity {
			[SecurityPermission (SecurityAction.Demand, ControlPolicy=true)]
			get {
				if ((storage_scope & IsolatedStorageScope.Application) == 0) {
					throw new InvalidOperationException (Locale.GetText ("Invalid Isolation Scope.")); 
				}
				if (_applicationIdentity == null)
					throw new InvalidOperationException (Locale.GetText ("Identity unavailable.")); 

				throw new NotImplementedException (Locale.GetText ("CAS related")); 
			}
		}

		public object AssemblyIdentity {
			[SecurityPermission (SecurityAction.Demand, ControlPolicy=true)]
			get {
				if ((storage_scope & IsolatedStorageScope.Assembly) == 0) {
					throw new InvalidOperationException (Locale.GetText ("Invalid Isolation Scope.")); 
				}
				if (_assemblyIdentity == null)
					throw new InvalidOperationException (Locale.GetText ("Identity unavailable.")); 
				return _assemblyIdentity;
			}
		}

		[CLSCompliant (false)]
#if NET_4_0
		[Obsolete]
#endif
		public virtual ulong CurrentSize {
			get {
				throw new InvalidOperationException (
					Locale.GetText ("IsolatedStorage does not have a preset CurrentSize."));
			}
		}

		public object DomainIdentity {
			[SecurityPermission (SecurityAction.Demand, ControlPolicy=true)]
			get {
				if ((storage_scope & IsolatedStorageScope.Domain) == 0) {
					throw new InvalidOperationException (Locale.GetText ("Invalid Isolation Scope.")); 
				}
				if (_domainIdentity == null)
					throw new InvalidOperationException (Locale.GetText ("Identity unavailable.")); 
				return _domainIdentity;
			}
		}

		[CLSCompliant (false)]
#if NET_4_0
		[Obsolete]
#endif
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

		protected void InitStore (IsolatedStorageScope scope, Type domainEvidenceType, Type assemblyEvidenceType)
		{
			// I know it's useless - but it's tested as such...
			switch (scope) {
			case (IsolatedStorageScope.Assembly | IsolatedStorageScope.User):
			case (IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Domain):
				throw new NotImplementedException (scope.ToString ());
			default:
				// invalid (incomplete) scope
				throw new ArgumentException (scope.ToString ());
			}
		}

		[MonoTODO ("requires manifest support")]
		protected void InitStore (IsolatedStorageScope scope, Type appEvidenceType)
		{
			if (AppDomain.CurrentDomain.ApplicationIdentity == null)
				throw new IsolatedStorageException (Locale.GetText ("No ApplicationIdentity available for AppDomain."));

			if (appEvidenceType == null) {
				// TODO - Choose evidence
			}

			// no exception here because this can work without CAS
			storage_scope = scope;
		}
		public abstract void Remove ();
	}
}
/* MOONLIGHT */
#endif 
