//
// System.Windows.Forms.PropertyGrid
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
//using System.Drawing.Printing;
using System.ComponentModel;
using System.Collections;
//using System.Windows.Forms.Design;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides a user interface for browsing the properties of an object.

	/// </summary>

	[MonoTODO]
	public class PropertyGrid : ContainerControl {

		#region Fields
		AttributeCollection browsableAttributes;
		Color commandsBackColor;
		Color commandsForeColor;
		bool commandsVisibleIfAvailable;
		Color helpBackColor;
		Color helpForeColor;
		bool helpVisible;
		bool largeButtons;
		Color lineColor;
		PropertySort propertySort;
		bool toolbarVisible;
		Color viewBackColor;
		Color viewForeColor;
		#endregion
		
		#region Constructors
		[MonoTODO]
		public PropertyGrid() 
		{
			browsableAttributes = new AttributeCollection( 
			 new Attribute[] {BrowsableAttribute.Yes} //Attribute[] attributes
			);
			commandsBackColor=SystemColors.Control;
			commandsForeColor=SystemColors.ControlText;
			commandsVisibleIfAvailable=false;
			helpBackColor=SystemColors.Control;
			helpForeColor=SystemColors.ControlText;
			helpVisible=true;
			largeButtons=false;
			lineColor=SystemColors.ScrollBar;
			propertySort=PropertySort.Categorized;// OR PropertySort.Alphabetical;
			toolbarVisible=true;
			viewBackColor=SystemColors.Window;
			viewForeColor=SystemColors.WindowText;
		}
		#endregion
		
		#region Properties
		[MonoTODO]
		public override bool AutoScroll {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Image BackgroundImage {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public AttributeCollection BrowsableAttributes {
			get { return browsableAttributes; }
			set {
				if (value==null || value==AttributeCollection.Empty) {
					browsableAttributes=new AttributeCollection( 
						new Attribute[] {BrowsableAttribute.Yes} //Attribute[] attributes
					);
				}
				else {
					browsableAttributes=value;
				}
			}
		}
		
		[MonoTODO]
		public virtual bool CanShowCommands {
			get { throw new NotImplementedException (); }
		}
		
		public Color CommandsBackColor {
			get { return commandsBackColor; }
			set { commandsBackColor=value; }
		}
		
		public Color CommandsForeColor {
			get { return commandsForeColor; }
			set { commandsForeColor=value; }
		}
		
		[MonoTODO]
		public virtual bool CommandsVisible {
			get { throw new NotImplementedException (); }
		}
		
		public virtual bool CommandsVisibleIfAvailable {
			get { return commandsVisibleIfAvailable; }
			set { commandsVisibleIfAvailable=value; }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new Control.ControlCollection Controls {get;}
		
		[MonoTODO]
		public Point ContextMenuDefaultLocation {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected override Size DefaultSize {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected virtual Type DefaultTabType {
			get { throw new NotImplementedException (); }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// protected bool DrawFlatToolbar {get; set;}
		
		[MonoTODO]
		public override Color ForeColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public Color HelpBackColor {
			get { return helpBackColor; }
			set { helpBackColor=value; }
		}
		
		public Color HelpForeColor {
			get { return helpForeColor; }
			set { helpForeColor=value; }
		}
		
		public virtual bool HelpVisible {
			get { return helpVisible; }
			set { helpVisible=value; }
		}
		
		public bool LargeButtons {
			get { return largeButtons; }
			set { largeButtons=value; }
		}
		
		public Color LineColor {
			get { return lineColor; }
			set { lineColor=value; }
		}
		
		public PropertySort PropertySort {
			get { return propertySort; }
			set { propertySort=value; }
		}
		
		[MonoTODO]
		public PropertyGrid.PropertyTabCollection PropertyTabs {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public GridItem SelectedGridItem {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public object SelectedObject {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public object[] SelectedObjects {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
//		[MonoTODO]
//		public PropertyTab SelectedTab {
//			get { throw new NotImplementedException (); }
//		}
		
		[MonoTODO]
		protected override bool ShowFocusCues {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override ISite Site {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual bool ToolbarVisible {
			get { return toolbarVisible; }
			set { toolbarVisible=value; }
		}
		
		public Color ViewBackColor {
			get { return viewBackColor; }
			set { viewBackColor=value; }
		}
		
		public Color ViewForeColor {
			get { return viewForeColor; }
			set { viewForeColor=value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		public void CollapseAllGridItems() 
		{
			throw new NotImplementedException ();
		}
		
//		[MonoTODO]
//		protected virtual PropertyTab CreatePropertyTab(Type tabType) {
//			throw new NotImplementedException ();
//		}
		
		//inherited
		//protected override void Dispose(bool disposing) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public void ExpandAllGridItems() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnGotFocus(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs me) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseMove(MouseEventArgs me) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs me) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs pevent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnPropertyTabChanged(PropertyTabChangedEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnPropertyValueChanged(PropertyValueChangedEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSelectedGridItemChanged(SelectedGridItemChangedEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSelectedObjectsChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnSystemColorsChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnVisibleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void Refresh() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RefreshTabs(PropertyTabScope tabScope) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetSelectedProperty() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void ScaleCore(float dx,float dy) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Events
		public event EventHandler PropertySortChanged;
		public event PropertyTabChangedEventHandler PropertyTabChanged;
		public event PropertyValueChangedEventHandler PropertyValueChanged;
		public event SelectedGridItemChangedEventHandler SelectedGridItemChanged;
		public event EventHandler SelectedObjectsChanged;
		#endregion
		
		/// sub-class: PropertyGrid.PropertyTabCollection
		/// <summary>
		/// Contains a collection of PropertyTab objects.
		/// </summary>
		public class PropertyTabCollection : ICollection, IEnumerable {
			#region Properties
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
			
//			[MonoTODO]
//			public PropertyTab this[int index] {
//				get { throw new NotImplementedException (); }
//			}
			#endregion
			
			#region Methods
			[MonoTODO]
			public void AddTabType(Type propertyTabType) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddTabType(Type propertyTabType,PropertyTabScope tabScope) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Clear(PropertyTabScope tabScope) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void ICollection.CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void RemoveTabType(Type propertyTabType) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public object SyncRoot {
				// FIXME: should return object that can be used with the C# lock keyword
				get { return this; }
			}
			
			[MonoTODO]
			public bool IsSynchronized {
				// FIXME: should return true if object is synchronized
				get { return false; }
			}
			#endregion
		}
	}
}
