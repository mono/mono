//
// System.Windows.Forms.FileDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public abstract class FileDialog : CommonDialog {

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public bool AddExtension {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual bool CheckFileExists {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool CheckPathExists {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string DefaultExt {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool DereferenceLinks {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string FileName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string[] FileNames {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string Filter {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int FilterIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string InitialDirectory {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool RestoreDirectory {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowHelp {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string Title {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ValidateNames {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static bool Equals(object o1, object o2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override void Reset()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public DialogResult ShowDialog()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public DialogResult ShowDialog(IWin32Window owner)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		// --- Public Events
		//
		[MonoTODO]
		public event CancelEventHandler FileOk {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}

		//
		// --- Protected Methods
		//
		//[MonoTODO]
		//protected  void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected  override IntPtr HookProc( IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam )
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected  void OnFileOk( CancelEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected  override bool RunDialog( IntPtr hWndOwner)
		{
			throw new NotImplementedException ();
		}
	 }
}
