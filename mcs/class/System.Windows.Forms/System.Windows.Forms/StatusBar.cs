//
// System.Windows.Forms.StatusBar.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//	Represents a Windows status bar control.
	// </summary>
	public class StatusBar : Control {

		private bool sizingGrip;
		private bool showPanels;
		private StatusBarPanelCollection panels;
		private string  stext;
		private const int GripSize = 16;   // FIXME: get size from SystemMetrics
		private const int PanelGap = 2;    // FIXME: get size from StatusBar
		private const int TextOffset = 3;
		internal DockStyle dockstyle;

		public StatusBar() : base()
		{
			Dock = DockStyle.Bottom;
			showPanels = false;
			sizingGrip = true;
			Size = DefaultSize;
		}

		public override string ToString()
		{
			string str = "System.Windows.Forms.StatusBar, Panels.Count: ";
			str += Panels.Count;
			for ( int i = 0; i < Panels.Count ; i++ ) {
				
				str += ", Panels[" + i + "]: " + Panels[i].ToString ( );
			}
			return str;
		}

		protected override void CreateHandle()
		{
			initCommonControlsLibrary ( );
			base.CreateHandle();
		}

		protected virtual void OnDrawItem(StatusBarDrawItemEventArgs e)
		{
			if( DrawItem != null)
				DrawItem ( this, e );
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			SetPanelsImpl ( );
		}

		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}

		[MonoTODO]
		protected override void OnLayout(LayoutEventArgs e)
		{
			//FIXME:
			base.OnLayout(e);
		}

		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e)
		{
			//FIXME:
			base.OnMouseDown(e);
		}

		protected virtual void OnPanelClick(StatusBarPanelClickEventArgs e)
		{
			if ( PanelClick != null )
				PanelClick ( this , e );
		}

		[MonoTODO]
		protected override void OnResize(EventArgs e)
		{
			UpdatePanels( true, false, null );
			base.OnResize(e);
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			switch ( m.Msg ) {
			case Msg.WM_DRAWITEM:
				DRAWITEMSTRUCT dis = new DRAWITEMSTRUCT();
				dis = (DRAWITEMSTRUCT)Marshal.PtrToStructure( m.LParam, dis.GetType() );
				
				if ( dis.itemID < Panels.Count ) {
					OnDrawItem (
						new StatusBarDrawItemEventArgs (
						Graphics.FromHdc ( dis.hDC ),
						Font,
						new Rectangle(  dis.rcItem.left,
						dis.rcItem.top,
						dis.rcItem.right - dis.rcItem.left,
						dis.rcItem.bottom - dis.rcItem.top),
						dis.itemID,
						(DrawItemState)dis.itemState,
						Panels[dis.itemID] ) );
				}
				m.Result = (IntPtr)1;
			break;
			case Msg.WM_NOTIFY:
				// FIXME
			break;
			default:
				base.WndProc(ref m);
			break;
			}
		}

		public event StatusBarDrawItemEventHandler DrawItem;
		public event StatusBarPanelClickEventHandler PanelClick;

		public override Color BackColor {
			get {	return base.BackColor;	}
			set {	base.BackColor = value;	}
		}

		public override Image BackgroundImage {
			get {	return base.BackgroundImage;	}
			set {	base.BackgroundImage = value;	}
		}

		//FIXME:
		[MonoTODO]
		public override DockStyle Dock {
			get {
				return dockstyle;
			}
			set {
				dockstyle =  value;
			}
		}

		public override Font Font {

			get { return base.Font; }
			set { base.Font = value; }
		}

		public override Color ForeColor {

			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		public new ImeMode ImeMode {

			get { return DefaultImeMode; }
			set {  }
		}

		public StatusBar.StatusBarPanelCollection Panels {
			get { 
				if( panels == null )
					panels = new StatusBar.StatusBarPanelCollection( this );
				return panels;
			}
		}

		public bool ShowPanels {
			get { return showPanels; }
			set {
				showPanels = value; 
				SetPanelsImpl ( );
			}
		}

		[MonoTODO]
		public bool SizingGrip
		{
			get { return sizingGrip; }
			set { 
				// the only way to get rid of the grip dynamically
				// is to recreate window
				bool recreate = sizingGrip != value;
				sizingGrip = value;
				if ( IsHandleCreated && recreate )
					RecreateHandle();
			}
		}

		[MonoTODO]
		public new bool TabStop {
			get { return false; }
			set {  } 
		}

		[MonoTODO]
		public override string Text {
			get {   // should reuse base.Text ?
				return stext;
			}
			set {
				stext = value;
				if ( IsHandleCreated )
					UpdateStatusText ( );
			}
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();

				createParams.Caption = Text;
				createParams.ClassName = "msctls_statusbar32";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE |
					WindowStyles.WS_OVERLAPPED |
					WindowStyles.WS_CLIPCHILDREN |
					WindowStyles.WS_CLIPCHILDREN );

				if( SizingGrip )
					createParams.Style |= (int)StatusbarControlStyles.SBARS_SIZEGRIP;

				createParams.Style |= (int)StatusbarControlStyles.SBT_TOOLTIPS;

				return createParams;
			}		
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return new Size ( 100, 22 ); }
		}
		
		internal  void UpdateParts ( ) 	{
			if ( Panels.Count > 0) {
				int[] array = new int[ panels.Count ];

				CalculatePanelWidths ( array );
				int size = array.Length;

				IntPtr buffer = Marshal.AllocCoTaskMem( Marshal.SizeOf( size ) * size );
				Marshal.Copy( array, 0, buffer, size );
				Win32.SendMessage( Handle, (int)StatusbarMessages.SB_SETPARTS, size, buffer.ToInt32() );
				Win32.SendMessage( Handle, (int)StatusbarMessages.SB_SIMPLE, 0, 0 );
				Marshal.FreeCoTaskMem( buffer );
			}
			else {
				Win32.SendMessage( Handle, (int)StatusbarMessages.SB_SIMPLE, 1, 0 );
				UpdateStatusText ( );
			}
		}

		internal  void UpdateText ( StatusBarPanel p ) {
			// if p is not null then this call is request to 
			// update text in some specific panel
			for (int i = 0; i < panels.Count; i++ ) {
				if ( p != null && p != panels[i] )
					continue;

				int DrawStyle = i;
						
				if ( panels[i].Style == StatusBarPanelStyle.OwnerDraw )
					DrawStyle |= (int)StatusbarDrawType.SBT_OWNERDRAW;

				switch ( panels[i].BorderStyle ) 
				{
					case StatusBarPanelBorderStyle.None:
						DrawStyle |= (int)StatusbarDrawType.SBT_NOBORDERS;
						break;
					case StatusBarPanelBorderStyle.Raised:
						DrawStyle |= (int)StatusbarDrawType.SBT_POPOUT;
						break;
				}

				string TextToSet;
				
				switch ( panels[i].Alignment ) {
				case HorizontalAlignment.Center:
					TextToSet = panels[i].Text.Insert( 0, "\t" );
				break;
				case HorizontalAlignment.Right:
					TextToSet = panels[i].Text.Insert( 0, "\t\t" );
				break;
				default:
					TextToSet = panels[i].Text;
				break;
				}

				Win32.SendMessage( Handle, (int)StatusbarMessages.SB_SETTEXT, DrawStyle,
							TextToSet );
			}
		}

		internal  void UpdateToolTips ( StatusBarPanel p ) {
			// if p == null set tooltips for each panel
			for (int i = 0; i < panels.Count; i++ ) {
				if ( p != null && p != panels[i] )
					continue;

				Win32.SendMessage ( Handle, (int)StatusbarMessages.SB_SETTIPTEXT, i ,
							panels[i].ToolTipText );
			}
		}

		internal  void UpdatePanels ( bool updateParts, bool updateText, StatusBarPanel p ) {
			if ( IsHandleCreated ) {
				if ( updateParts )
					UpdateParts ( );

				if ( updateText )
					UpdateText( p );

				Invalidate( );
			}
		}

		protected void CalculatePanelWidths ( int[] array ) {
			int[] WidthArray = new int[panels.Count];

			int FixedWidth = ClientSize.Width - (SizingGrip == true ? GripSize : 0);
			int NumSpringPanels = 0;

			for (int i = 0; i < panels.Count; i++ )	{
				switch ( panels[i].AutoSize ) {
				case StatusBarPanelAutoSize.None: 
					WidthArray[i] = panels[i].Width + (PanelGap + TextOffset)*2;
				break;
				case StatusBarPanelAutoSize.Contents:
					WidthArray[i] = panels[i].GetContentWidth( ) + (PanelGap + TextOffset)*2;
				break;
				default:
					WidthArray[i] = 0;
					NumSpringPanels++;
				break;
				}
				FixedWidth   -= WidthArray[i];
			}

			int SpringPanelLength = 0;
			if ( NumSpringPanels > 0 && FixedWidth > 0)
				SpringPanelLength = FixedWidth / NumSpringPanels;

			for (int i = 0; i < panels.Count; i++ )	{
				if ( panels[i].AutoSize == StatusBarPanelAutoSize.Spring) 
					WidthArray[i] = SpringPanelLength > panels[i].MinWidth ? 
							SpringPanelLength : panels[i].MinWidth;
			}

			for (int i = 0; i < panels.Count; i++ )
				array[i] = WidthArray[i] + (i == 0 ? 0 : array[i - 1]);
		}

		internal  void UpdateStatusText ( ){
			Win32.SendMessage( Handle, (int)StatusbarMessages.SB_SETTEXT,
						255 | (int)StatusbarDrawType.SBT_NOBORDERS, Text );
		}

		internal  void SetPanelsImpl ( ) {
			if( IsHandleCreated ) {
				if ( base.Font.ToHfont ( ) != IntPtr.Zero )
					Win32.SendMessage ( Handle, Msg.WM_SETFONT, base.Font.ToHfont().ToInt32(), 0 );

				if( panels == null || panels.Count == 0 || showPanels == false) {
					Win32.SendMessage( Handle, (int)StatusbarMessages.SB_SIMPLE, 1, 0 );
					UpdateStatusText ( );
				}
				else {
					UpdatePanels ( true, true, null );
					UpdateToolTips ( null );
				}
			}
		}

		private void initCommonControlsLibrary ( ) {
			if ( !RecreatingHandle ) {
				INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
				initEx.dwICC = CommonControlInitFlags.ICC_BAR_CLASSES;
				Win32.InitCommonControlsEx(initEx);
			}
		}

		//
		// System.Windows.Forms.StatusBar.StatusBarPanelCollection
		//
		// Author:
		//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
		//   stub ammended by Jaak Simm (jaaksimm@firm.ee)
		//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
		// (C) Ximian, Inc., 2002
		//
		// <summary>
		//	Represents the collection of panels in a StatusBar control.
		// </summary>
		public class StatusBarPanelCollection : IList, ICollection, IEnumerable {
			private ArrayList list;
			private StatusBar owner;

			public StatusBarPanelCollection( StatusBar owner ) : base() {
				list = new ArrayList();
				this.owner = owner;
			}

			public virtual int Add( StatusBarPanel value ) {
				if (value == null)
					throw new ArgumentNullException("value");

				if (value.Parent != null)
					throw new ArgumentException("Object already has a parent.", "value");

				value.SetParent( owner );
				int Index = list.Add( value );

				owner.UpdatePanels ( true, true, null );
				return Index;
			}

			public virtual StatusBarPanel Add( string text ) {
				StatusBarPanel panel = new StatusBarPanel();
				panel.Text = text;
				this.Add ( panel );
				return panel;
			}

			public virtual void AddRange(StatusBarPanel[] panels) {
				if (panels == null)
					throw new ArgumentNullException("panels");

				// do we need to check for panel.Parent
				// like it is done in Add(StatusBarPanel) ?

				for (int i = 0; i < panels.Length; i++)
					panels[i].SetParent( owner );

				list.AddRange(panels);
				owner.UpdatePanels ( true, true, null );
			}

			public virtual void Clear() {
				for (int i = 0; i < list.Count; i++ )
					((StatusBarPanel)list[i]).SetParent ( null );

				list.Clear();
				owner.UpdatePanels ( true, true, null );
			}

			public bool Contains(StatusBarPanel panel) {
				return list.Contains(panel);
			}

			public IEnumerator GetEnumerator() {
				return list.GetEnumerator();
			}

			public int IndexOf(StatusBarPanel panel) {
				return list.IndexOf(panel);
			}

			public virtual void Insert(int index, StatusBarPanel value) {
				if (value == null)
					throw new ArgumentNullException ( "value" );

				if (value.Parent != null)
					throw new ArgumentException ( "Object already has a parent.", "value" );

				if (index < 0 || index > Count )
					throw new ArgumentOutOfRangeException( "index" );

				// very strange place to check autosize property :-))
				if ( !Enum.IsDefined ( typeof(StatusBarPanelAutoSize), value.AutoSize ) )
					throw new InvalidEnumArgumentException( "AutoSize",
						(int)value.AutoSize,
						typeof(StatusBarPanelAutoSize));

				list.Insert(index, value);
				value.SetParent ( owner ); 
				owner.UpdatePanels ( true, true , null );
			}

			public virtual void Remove(StatusBarPanel value) {
				if (value == null)
					throw new ArgumentNullException( "value" );

				list.Remove( value );
				value.SetParent ( null );
			}

			public virtual void RemoveAt(int index)	{
				if (index < 0 || index > Count )
					throw new ArgumentOutOfRangeException( "index" );

				StatusBarPanel p = (StatusBarPanel)list[index];
				list.RemoveAt(index);
				p.SetParent ( null );
				owner.UpdatePanels( true, true, null );
			}

			[MonoTODO]
			// This member supports the .NET Framework 
			void ICollection.CopyTo(Array array, int index)	{
				if (array == null)
					throw new ArgumentNullException ( "array" );

				if (index < 0)
					throw new ArgumentOutOfRangeException ( "index" );

				if (array.Rank != 1 || index >= array.Length || Count+index >= array.Length)
					throw new ArgumentException ( ); // FIXME: messages

				// easier/quicker to let the runtime throw the invalid cast exception if necessary
				for (int i = 0; index < array.Length; i++, index++)
					array.SetValue(list[i], index);
			}
			
			[MonoTODO]
			int IList.Add(object panel)
			{
				if (!(panel is StatusBarPanel))
					throw new ArgumentException();//FIXME: message
				return Add((StatusBarPanel) panel);
			}

			bool IList.Contains(object panel)
			{
				if (!(panel is StatusBarPanel))
					return false;
				return Contains((StatusBarPanel) panel);
			}

			int IList.IndexOf(object panel)	{
				if (!(panel is StatusBarPanel))
					return -1;
				return IndexOf((StatusBarPanel) panel);
			}

			[MonoTODO]
			void IList.Insert(int index, object panel)
			{
				if (!(panel is StatusBarPanel))
					throw new ArgumentException();//FIXME: message

				Insert(index, (StatusBarPanel) panel);
			}

			[MonoTODO]
			void IList.Remove(object panel)
			{
				if (!(panel is StatusBarPanel))
					throw new ArgumentException(); //FIXME: message

				Remove((StatusBarPanel) panel);
			}

			
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			object IList.this[int index] {
				get { return this[index]; }
				set { this[index]= (StatusBarPanel)value; }
			}

			public virtual StatusBarPanel this[int index] {
				get
				{
					// The same checks are done by the list, so this is redundant
					// This is left here in case you prefer better exception messages over performance
					//string method_string = "get_Item(int) ";
					//if (index < 0)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index < 0");
					//}
					//if (index >= Count)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index >= Count");
					//}
					return (StatusBarPanel)list[index];
				}
				set
				{
					// The same checks are done by the list, so this is redundant
					// This is left here in case you prefer better exception messages over performance
					//string method_string = "set_Item(int,StatusBarPanel) ";
					//if (index < 0)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index < 0");
					//}
					//if (index >= Count)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index >= Count");
					//}
					//if (value == null)
					//{
					//	throw new ArgumentNullException(class_string + method_string + "panel == null");
					//}
					list[index] = value;
				}
			}

			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}

			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException (); }
			}

			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			private bool IsFixedSize { get { return false; } }
		}
	}
}

