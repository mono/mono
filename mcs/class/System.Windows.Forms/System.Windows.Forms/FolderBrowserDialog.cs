//
// System.Windows.Forms.FolderBrowserDialog.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public class FolderBrowserDialog {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public FolderBrowserDialog() {
			throw new NotImplementedException ();
		}

		//
		//	 --- Public Fields
		//
		
		[MonoTODO]
		public virtual ObjRef CreateObjRef(Type requstedType){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual object GetLifetimeService(){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object GetService(Type service){
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public Type GetType(){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object InitializeLifetimeService(){
			throw new NotImplementedException ();
		}

		
		public virtual void Dispose(){
			base.Dispose();
		}

		public virtual void Dispose(){
			base.Dispose();
		}
		
		[MonoTODO]
		~FolderBrowserDialog(){
		}
		
		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this FolderBrowserDialog and another object.
		/// </remarks>
		
		public override bool Equals (object obj) {
			if (!(obj is FolderBrowserDialog))
				return false;

			return (this == (FolderBrowserDialog) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode () {
			unchecked{//FIXME Add our proprities to the hash
				return base.GetHashCode();
			}
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the FolderBrowserDialog as a string.
		/// </remarks>
		
		public override string ToString () {
			//FIXME do a better tostring
			return base.ToString() + "FolderBrowserDialog";
		}

		
		public virtual void Dispose(bool disposing){
			base.Dispose(disposing);
		}

		//
		//  --- Public Properties
		//

		public event EventHandler Disposed;
		public event EventHandler HelpRequest;

	}
}