//
// System.Windows.Forms.GroupBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public class GroupBox : Control {

		//
		//  --- Constructor
		//

		[MonoTODO]
		public GroupBox() {
			
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public override bool AllowDrop {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Rectangle DisplayRectangle {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public FlatStyle FlatStyle {
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
		
		//inherited
		//public IAsyncResult BeginInvoke(Delegate d) {
		//	throw new NotImplementedException ();
		//}

		//public IAsyncResult BeginInvoke(Delegate d, object[] objs) {
		//	throw new NotImplementedException ();
		//}
		//public void Dispose() {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		//public static bool Equals(object o1, object o2) {
		//	throw new NotImplementedException ();
		//}
		//
		//public void Invalidate() {
		//	throw new NotImplementedException ();
		//}
		//
		//public object Invoke(Delegate d) {
		//	throw new NotImplementedException ();
		//}
		//
		//public object Invoke(Delegate d, object[] objs) {
		//	throw new NotImplementedException ();
		//}
		//
		//public void PerformLayout() {
		//	throw new NotImplementedException ();
		//}
		//
		//public void ResumeLayout() {
		//	throw new NotImplementedException ();
		//}
		//
		//public void Scale(float val) {
		//	throw new NotImplementedException ();
		//}
		//
		//public void Scale(float val1, float val2) {
		//	throw new NotImplementedException ();
		//}
		//
		//public void Select() {
		//	throw new NotImplementedException ();
		//}
		//
		//public void SetBounds(int b1, int b2, int b3, int b4) {
		//	throw new NotImplementedException ();
		//}
		//
		//public void SetBounds(int b1, int b2, int b3, int b4, BoundsSpecified bounds) {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}


		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);
 
				createParams.Caption = Text;
				createParams.ClassName = "GROUP";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//		createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILD | 
					(int)WindowStyles.WS_VISIBLE | (int)SS_Static_Control_Types.SS_LEFT );
				window.CreateHandle (createParams);
				return createParams;
			}
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//

		//[MonoTODO]
		//protected override void Dispose(bool disposing) {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnPaint(PaintEventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) {
			throw new NotImplementedException ();
		}

		//[MonoTODO]
		//protected override virtual void Select(bool b1, bool b2)
		//{
		//	throw new NotImplementedException ();
		//}


		//protected override void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}

		// Inherited
		//protected override void UpdateBounds(int b1, int b2, int b3, int b4)
		//{
		//	throw new NotImplementedException ();
		//}

		// Inherited
		//protected override void UpdateBounds(int b1, int b2, int b3, int b4, int b5, int b6)
		//{
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		protected override void WndProc(ref Message m) {
			throw new NotImplementedException ();
		}

	}
}
