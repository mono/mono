// System.Configuration.Install.Installer.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Collections;

namespace System.Configuration.Install
{
	public class InstallerCollection : CollectionBase
	{
		[MonoTODO]
		public Installer this[int index] {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int Add (Installer value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange (Installer[] value) {
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public void AddRange (InstallerCollection value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (Installer value) {
			throw new NotImplementedException ();
		}		

		[MonoTODO]
		public void CopyTo (Installer[] array, int index) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int IndexOf (Installer value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, Installer value) {
			throw new NotImplementedException ();			
		}

		[MonoTODO]
		protected override void OnInsert (int index, object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSet (int index, object oldValue, object newValue) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (Installer value) {
			throw new NotImplementedException ();
		}
	}
}
