//
// System.Windows.Forms.ListControl.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public abstract class ListControl : Control {

		//ControlStyles controlStyles;
		//
		//  --- Public Properties
		//
		[MonoTODO]
		public object DataSource {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string DisplayMember {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public abstract int SelectedIndex {get;set;}
//			get {
//				//throw new NotImplementedException ();
//			}
//			set {
//				//throw new NotImplementedException ();
//			}
//		}
		[MonoTODO]
		public object SelectedValue {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string ValueMember {
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
		//public IAsyncResult BeginInvoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public IAsyncResult BeginInvoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		[MonoTODO]
		public string GetItemText(object Item)
		{
			throw new NotImplementedException ();
		}
		//inherited
		//public void Invalidate()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout(Control ctl, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val1, float val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected override void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int val1, int val2, int val3, int val4, BoundsSpecified bounds)
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event EventHandler DataSourceChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public event EventHandler DisplayMemberChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}

		//
		// --- Protected Constructor
		//
		[MonoTODO]
		protected ListControl()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected CurrencyManager DataManager {
			get {
				throw new NotImplementedException ();
			}
		}


		//
		//  --- Protected Methods
		//
		//inherited
		//protected override void Dispose(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnDataSourceChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnDisplayMemberChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSelectedIndexChanged(EventArgs e) {
			throw new NotImplementedException ();
		}		
		
		[MonoTODO]
		protected virtual void OnSelectedValueChanged(EventArgs e){
			throw new NotImplementedException ();
		}

		
		[MonoTODO]
		protected override void OnBindingContextChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected abstract void RefreshItem(int index);
		//inherited
		//protected ContentAlignment RtlTranslateAlignment(ContentAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4, int val5, int val6)
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
