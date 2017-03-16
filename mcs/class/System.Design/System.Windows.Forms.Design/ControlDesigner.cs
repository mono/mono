//
// System.Windows.Forms.Design.ControlDesigner
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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


using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.Windows.Forms.Design.Behavior;

namespace System.Windows.Forms.Design
{
	public class ControlDesigner : ComponentDesigner, IMessageReceiver
	{
		

		private WndProcRouter _messageRouter;
		private bool _locked = false;
		private bool _mouseDown = false;
		private bool _mouseMoveAfterMouseDown = false;
		private bool _mouseDownFirstMove = false;
		private bool _firstMouseMoveInClient = true;

		public ControlDesigner ()
		{
		}

#region Initialization
		public override void Initialize (IComponent component)
		{
			base.Initialize (component);

			if (!(component is Control))
				throw new ArgumentException ("Component is not a Control.");

			Control.Text = component.Site.Name;
			_messageRouter = new WndProcRouter ((Control) component, (IMessageReceiver) this);
			Control.WindowTarget = _messageRouter;

			// DT properties
			//
			this.Visible = true;
			this.Enabled = true;
			this.Locked = false;
			this.AllowDrop = false;
			//
			// The control properties
			//
			Control.Enabled = true;
			Control.Visible = true;
			Control.AllowDrop = false;

			this.Control.DragDrop += new DragEventHandler (OnDragDrop);
			this.Control.DragEnter += new DragEventHandler (OnDragEnter);
			this.Control.DragLeave += new EventHandler (OnDragLeave);
			this.Control.DragOver += new DragEventHandler (OnDragOver);

			// XXX: The control already has a handle?
			//
			if (Control.IsHandleCreated)
				OnCreateHandle ();

		}

		// The default implementation of this method sets the component's Text property to
		// its name (Component.Site.Name), if the property field is of type string.
		//
		public override void OnSetComponentDefaults ()
		{
			if (this.Component != null && this.Component.Site != null) {
				PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties (this.Component)["Text"];
				if (propertyDescriptor != null && !propertyDescriptor.IsReadOnly &&
					propertyDescriptor.PropertyType == typeof (string)) {
					propertyDescriptor.SetValue (Component, Component.Site.Name);
				}
			}
		}
#endregion


#region Properties and Fields - AccessabilityObject Left
		protected static readonly Point InvalidPoint = new Point (int.MinValue, int.MinValue);

		protected internal BehaviorService BehaviorService {
			get { throw new NotImplementedException (); }
		}

		public virtual Control Control {
			get { return (Control) base.Component; }
		}

		protected virtual bool EnableDragRect {
			get { return true; }
		}

		public virtual SelectionRules SelectionRules {
			get {
				if (this.Control == null)
					return SelectionRules.None;

				// all controls on the surface are visible
				//
				SelectionRules selectionRules = SelectionRules.Visible;

				if ((bool)GetValue (this.Component, "Locked") == true) {
					selectionRules |= SelectionRules.Locked;
				}
				else {
					DockStyle dockStyle = (DockStyle) this.GetValue (base.Component, "Dock", typeof (DockStyle));

					switch (dockStyle) {
						case DockStyle.Top:
							selectionRules |= SelectionRules.BottomSizeable;
							break;
						case DockStyle.Left:
							selectionRules |= SelectionRules.RightSizeable;
							break;
						case DockStyle.Right:
							selectionRules |= SelectionRules.LeftSizeable;
							break;
						case DockStyle.Bottom:
							selectionRules |= SelectionRules.TopSizeable;
							break;
						case DockStyle.Fill:
							break;
						default:
							selectionRules |= SelectionRules.Moveable;
							selectionRules |= SelectionRules.AllSizeable;
							break;
					}
				}

				return selectionRules;
			}
		}

		public override ICollection AssociatedComponents {
			get {
				ArrayList components = new ArrayList ();
				foreach (Control c in this.Control.Controls)
					if (c.Site != null)
						components.Add (c);
				return components;
			}
		}

		protected override IComponent ParentComponent {
			get { return this.GetValue (this.Control,  "Parent") as Control;}
		}
		// TODO: implement ControlDesigner.ControlAccessabilityObject
		//
		public virtual AccessibleObject AccessibilityObject {
			get {
				if (accessibilityObj == null)
					accessibilityObj = new AccessibleObject ();

				return accessibilityObj;
			}
		}
		protected AccessibleObject accessibilityObj;

#endregion


#region WndProc
	
		protected void DefWndProc (ref Message m)
		{
			_messageRouter.ToControl (ref m);
		}
		
		protected void BaseWndProc (ref Message m)
		{
			_messageRouter.ToSystem (ref m);
		}
		
		void IMessageReceiver.WndProc (ref Message m)
		{
			this.WndProc (ref m);
		}
		
		// Keep in mind that messages are recieved for the child controls if routed
		//
		protected virtual void WndProc (ref Message m)
		{
			// Filter out kb input
			//
			if ((Native.Msg) m.Msg >= Native.Msg.WM_KEYFIRST &&  (Native.Msg) m.Msg <= Native.Msg.WM_KEYLAST)
				return;

			// Mouse messages should be routed the control, if GetHitTest (virtual) returns true.
			//
			if (IsMouseMessage ((Native.Msg) m.Msg) &&
			    this.GetHitTest (new Point (Native.LoWord((int) m.LParam), Native.HiWord (((int) m.LParam))))) {
				
				this.DefWndProc (ref m);
				return;
			}

			switch ((Native.Msg) m.Msg) {
				case Native.Msg.WM_CREATE:
					this.DefWndProc (ref m);
					if (m.HWnd == this.Control.Handle)
						OnCreateHandle ();
					break;

				case Native.Msg.WM_CONTEXTMENU:
					OnContextMenu (Native.LoWord ((int) m.LParam), Native.HiWord ((int) m.LParam));
					break;

				case Native.Msg.WM_SETCURSOR:
					if (this.GetHitTest (new Point (Native.LoWord ((int) m.LParam), Native.HiWord ((int) m.LParam))))
						this.DefWndProc (ref m);
					else
						OnSetCursor ();
					break;

				case Native.Msg.WM_SETFOCUS:
					this.DefWndProc (ref m);
					break;
				
				case Native.Msg.WM_PAINT:
					// Wait for control's WM_PAINT to complete first.
					//
					this.DefWndProc (ref m);

					Graphics gfx = Graphics.FromHwnd (m.HWnd);
					PaintEventArgs args = new PaintEventArgs (gfx, this.Control.Bounds);
					OnPaintAdornments (args);
					gfx.Dispose ();
					args.Dispose ();
					break;

				case Native.Msg.WM_NCRBUTTONDOWN:
				case Native.Msg.WM_NCLBUTTONDOWN:
				case Native.Msg.WM_NCMBUTTONDOWN:
				case Native.Msg.WM_NCLBUTTONDBLCLK:
				case Native.Msg.WM_NCRBUTTONDBLCLK:
					break;

				case Native.Msg.WM_LBUTTONDBLCLK:
				case Native.Msg.WM_RBUTTONDBLCLK:
				case Native.Msg.WM_MBUTTONDBLCLK:
					if ((Native.Msg)m.Msg == Native.Msg.WM_LBUTTONDBLCLK)
						_mouseButtonDown = MouseButtons.Left;
					else if ((Native.Msg)m.Msg == Native.Msg.WM_RBUTTONDBLCLK)
						_mouseButtonDown = MouseButtons.Right;
					else if ((Native.Msg)m.Msg == Native.Msg.WM_MBUTTONDBLCLK)
						_mouseButtonDown = MouseButtons.Middle;
					OnMouseDoubleClick ();
					this.BaseWndProc (ref m);
					break;

				case Native.Msg.WM_MOUSEHOVER:
					OnMouseHover ();
					break;
				
				case Native.Msg.WM_LBUTTONDOWN:
				case Native.Msg.WM_RBUTTONDOWN:		 
				case Native.Msg.WM_MBUTTONDOWN:
					_mouseMoveAfterMouseDown = true;
					if ((Native.Msg)m.Msg == Native.Msg.WM_LBUTTONDOWN)
						_mouseButtonDown = MouseButtons.Left;
					else if ((Native.Msg)m.Msg == Native.Msg.WM_RBUTTONDOWN)
						_mouseButtonDown = MouseButtons.Right;
					else if ((Native.Msg)m.Msg == Native.Msg.WM_MBUTTONDOWN)
						_mouseButtonDown = MouseButtons.Middle;
				
					if (_firstMouseMoveInClient) {
						OnMouseEnter ();
						_firstMouseMoveInClient = false;
					}
					this.OnMouseDown (Native.LoWord ((int)m.LParam), Native.HiWord ((int)m.LParam));
					this.BaseWndProc (ref m);
					break;

				case Native.Msg.WM_MOUSELEAVE:
					_firstMouseMoveInClient = false;
					OnMouseLeave ();
					this.BaseWndProc (ref m);
					break;

				// The WM_CANCELMODE message is sent to cancel certain modes, such as mouse capture.
				// For example, the system sends this message to the active window when a dialog box
				// or message box is displayed. Certain functions also send this message explicitly to
				// the specified window regardless of whether it is the active window. For example,
				// the EnableWindow function sends this message when disabling the specified window.
				//
				case Native.Msg.WM_CANCELMODE:
					OnMouseDragEnd (true);
					this.DefWndProc (ref m);
					break;

				case Native.Msg.WM_LBUTTONUP:
				case Native.Msg.WM_RBUTTONUP:
				case Native.Msg.WM_NCLBUTTONUP:
				case Native.Msg.WM_NCRBUTTONUP:
				case Native.Msg.WM_MBUTTONUP:  
				case Native.Msg.WM_NCMBUTTONUP:
					_mouseMoveAfterMouseDown = false; // just in case
					this.OnMouseUp ();
					this.BaseWndProc (ref m);
					break;

				// // MWF Specific msg! - must reach control
				// //
				// case Native.Msg.WM_MOUSE_ENTER:
				// 	_firstMouseMoveInClient = false; // just so that nothing will get fired in WM_MOUSEMOVE
				// 	OnMouseEnter ();
				// 	this.DefWndProc (ref m);
				// 	break;

				// FIXME: The first MOUSEMOVE after WM_MOUSEDOWN should be ingored
				//
				case Native.Msg.WM_MOUSEMOVE:
					if (_mouseMoveAfterMouseDown) { // mousemove is send after each mousedown so ignore that
						_mouseMoveAfterMouseDown = false;
						this.BaseWndProc (ref m);
						return;
					}
					// If selection is in progress pass the mouse move msg to the primary selection.
					// If resizing is in progress pass to the parent of the primary selection (remmember that the selection
					// frame is not a control and is drawn in the parent of the primary selection).
					//
					// Required in order for those 2 operations to continue when the mouse is moving over a control covering
					// the one where the action takes place.
					// 
					IUISelectionService uiSelectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
					ISelectionService selectionServ = this.GetService (typeof (ISelectionService)) as ISelectionService;
					IDesignerHost host = this.GetService (typeof (IDesignerHost)) as IDesignerHost;
					

					if (uiSelectionServ != null && selectionServ != null && host != null) {
						Control primarySelection = selectionServ.PrimarySelection as Control;
						Point location = new Point (Native.LoWord ((int)m.LParam), Native.HiWord ((int)m.LParam));

						if (uiSelectionServ.SelectionInProgress &&
							this.Component != host.RootComponent && 
							this.Component != selectionServ.PrimarySelection) {

							location = primarySelection.PointToClient (this.Control.PointToScreen (location));
							Native.SendMessage (primarySelection.Handle, (Native.Msg)m.Msg, m.WParam, Native.LParam (location.X, location.Y));
						}
						else if (uiSelectionServ.ResizeInProgress && 
						   // this.Component != host.RootComponent && 
							this.Control.Parent == ((Control)selectionServ.PrimarySelection).Parent) {

							location = this.Control.Parent.PointToClient (this.Control.PointToScreen (location));
							Native.SendMessage (this.Control.Parent.Handle, (Native.Msg)m.Msg, m.WParam, Native.LParam (location.X, location.Y));
						}
						else {
							this.OnMouseMove (location.X, location.Y);
						}
					}
					else {
						this.OnMouseMove (Native.LoWord ((int)m.LParam), Native.HiWord ((int)m.LParam));
					}
					this.BaseWndProc (ref m);
					break;

				default:
					// Pass everything else to the control and return
					//
					this.DefWndProc (ref m);
					break;
			}
		}
		
		// Indicates whether a mouse click at the specified point should be handled by the control.
		//
		protected virtual bool GetHitTest (Point point)
		{
			return false;
		}

		private bool IsMouseMessage (Native.Msg msg)
		{
			if (msg >= Native.Msg.WM_MOUSEFIRST && msg <= Native.Msg.WM_MOUSELAST)
				return true;
			else if (msg >= Native.Msg.WM_NCLBUTTONDOWN && msg <= Native.Msg.WM_NCMBUTTONDBLCLK)
				return true;
			else if (msg == Native.Msg.WM_MOUSEHOVER || msg == Native.Msg.WM_MOUSELEAVE)
				return true;
			else
				return false;
		}
#endregion


#region WndProc Message Handlers

		protected virtual void OnSetCursor ()
		{
		}

		// Raises the DoDefaultAction.
		//
		private void OnMouseDoubleClick ()
		{
			try {
				base.DoDefaultAction ();
			}
			catch (Exception e) {
				this.DisplayError (e);
			}
		}

		internal virtual void OnMouseDown (int x, int y)
		{
			_mouseDown = true;
			_mouseDownFirstMove = true;
			IUISelectionService uiSelection = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (uiSelection != null && uiSelection.AdornmentsHitTest (this.Control, x, y)) {
				// 1) prevent primary selection from being changed at this point.
				// 2) delegate behaviour in the future to the IUISelectionService
			}
			else {
				ISelectionService selectionService = this.GetService (typeof (ISelectionService)) as ISelectionService;
				if (selectionService != null) {
					selectionService.SetSelectedComponents (new IComponent[] { this.Component });
				}
			}
		}

		// Note that this is a pure WM_MOUSEMOVE acceptor
		//
		internal virtual void OnMouseMove (int x, int y)
		{
			if (_mouseDown) {
				if (_mouseDownFirstMove) {
					OnMouseDragBegin (x, y);
					_mouseDownFirstMove = false;
				}
				else {
					OnMouseDragMove (x, y);
				}
			}

		}

		internal virtual void OnMouseUp ()
		{
			IUISelectionService uiSelection = this.GetService (typeof (IUISelectionService)) as IUISelectionService;

			if (_mouseDown) {
				this.OnMouseDragEnd (false);
				if (uiSelection != null && (uiSelection.SelectionInProgress || uiSelection.ResizeInProgress)) {
					uiSelection.MouseDragEnd (false);
				}
				_mouseDown = false;
			}
			else {			  
				if (uiSelection != null && (uiSelection.SelectionInProgress || uiSelection.ResizeInProgress)) { 
					// If the mouse up happens over the a control which is not defacto participating in 
					// the selection or resizing in progress, then inform the IUISelectionService of that event
					//
					uiSelection.MouseDragEnd (false);
				}
			}
		}

		protected virtual void OnContextMenu (int x, int y)
		{
			IMenuCommandService service = this.GetService (typeof(IMenuCommandService)) as IMenuCommandService;
			if (service != null) {
				service.ShowContextMenu (MenuCommands.SelectionMenu, x, y);
			}
		}

		protected virtual void OnMouseEnter ()
		{
		}

		protected virtual void OnMouseHover ()
		{
		}

		protected virtual void OnMouseLeave ()
		{
		}

		// Provides an opportunity to perform additional processing immediately
		// after the control handle has been created.
		//
		protected virtual void OnCreateHandle ()
		{
		}

		// Called after the control is done with the painting so that the designer host
		// can paint stuff over it.
		//
		protected virtual void OnPaintAdornments (PaintEventArgs pe)
		{
		}
#endregion


#region Mouse Dragging

		MouseButtons _mouseButtonDown;

		internal MouseButtons MouseButtonDown {
			get { return _mouseButtonDown;  }
		}

		protected virtual void OnMouseDragBegin (int x, int y)
		{
			IUISelectionService selectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selectionServ != null && ((this.SelectionRules & SelectionRules.Moveable) == SelectionRules.Moveable)) {
				// once this is fired the parent control (parentcontroldesigner) will start getting dragover events.
				//
				selectionServ.DragBegin ();
			}
		}

		protected virtual void OnMouseDragMove (int x, int y)
		{
		}
		
		protected virtual void OnMouseDragEnd (bool cancel)
		{
		}
#endregion

		
#region Parenting
		protected void HookChildControls (Control firstChild)
		{
			if (firstChild != null) {
				foreach (Control control in firstChild.Controls) {
					control.WindowTarget = (IWindowTarget) new WndProcRouter (control, (IMessageReceiver) this);
				}
			}
		}

		protected void UnhookChildControls (Control firstChild)
		{
			if (firstChild != null) {
				foreach (Control control in firstChild.Controls) {
					if (control.WindowTarget is WndProcRouter)
						((WndProcRouter) control.WindowTarget).Dispose ();
				}
			}
		}

		// Someone please tell me why the hell is this method here?
		// What about having ParentControlDesigner.CanParent(...) ?
		// 
		public virtual bool CanBeParentedTo (IDesigner parentDesigner)
		{
			IDesignerHost host = this.GetService (typeof (IDesignerHost)) as IDesignerHost;

			if (parentDesigner is ParentControlDesigner &&
				this.Component != host.RootComponent &&
				!this.Control.Controls.Contains (((ParentControlDesigner)parentDesigner).Control)) {
					return true;
			} else {
				return false;
			}
		}
#endregion

		protected void DisplayError (Exception e)
		{
			if (e != null) {
				IUIService uiService = GetService (typeof (IUIService)) as IUIService;
				if (uiService != null) {
					uiService.ShowError (e);
				}
				else {
					string errorText = e.Message;
					if (errorText == null ||  errorText == String.Empty)
						errorText = e.ToString ();
					MessageBox.Show (Control, errorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

#region Drag and Drop handling

		// Enables or disables Drag and Drop
		//
		protected void EnableDragDrop(bool value)
		{
			if (this.Control != null) {
				if (value) {
					Control.DragDrop += new DragEventHandler (OnDragDrop);
					Control.DragOver += new DragEventHandler (OnDragOver);
					Control.DragEnter += new DragEventHandler (OnDragEnter);
					Control.DragLeave += new EventHandler (OnDragLeave);
					Control.GiveFeedback += new GiveFeedbackEventHandler (OnGiveFeedback);
					Control.AllowDrop = true;
				}
				else {
					Control.DragDrop -= new DragEventHandler (OnDragDrop);
					Control.DragOver -= new DragEventHandler (OnDragOver);
					Control.DragEnter -= new DragEventHandler (OnDragEnter);
					Control.DragLeave -= new EventHandler (OnDragLeave);
					Control.GiveFeedback -= new GiveFeedbackEventHandler (OnGiveFeedback);
					Control.AllowDrop = false;
				}
			}
		}

		private void OnGiveFeedback (object sender, GiveFeedbackEventArgs e)
		{
			OnGiveFeedback (e);
		}

		private void OnDragDrop (object sender, DragEventArgs e)
		{
			OnDragDrop (e);
		}

		private void OnDragEnter (object sender, DragEventArgs e)
		{
			OnDragEnter (e);
		}

		private void OnDragLeave (object sender, EventArgs e)
		{
			OnDragLeave (e);
		}

		private void OnDragOver (object sender, DragEventArgs e)
		{
			OnDragOver (e);
		}

		protected virtual void OnGiveFeedback (GiveFeedbackEventArgs e)
		{
			e.UseDefaultCursors = false;
		}

		protected virtual void OnDragDrop (DragEventArgs de)
		{
		}

		protected virtual void OnDragEnter (DragEventArgs de)
		{
		}

		protected virtual void OnDragLeave (EventArgs e)
		{
		}

		protected virtual void OnDragOver (DragEventArgs de)
		{
		}
#endregion


#region Redirected Properties

		// This IDesignerFilter interface method override adds a set of properties
		// to this designer's component at design time. This method adds the following
		// browsable properties: "Visible", "Enabled", "ContextMenu", "AllowDrop", "Location",
		// "Name", "Controls", and "Locked".
		//
		// XXX: We aren't redirecting Controls
		// 
		protected override void PreFilterProperties (IDictionary properties)
		{
			base.PreFilterProperties (properties);

			string[] newProperties = {
				"Visible", "Enabled", "ContextMenu", "AllowDrop", "Location", "Name",
			};

			Attribute[][] attributes = { 
				new Attribute[] { new DefaultValueAttribute (true) },
				new Attribute[] { new DefaultValueAttribute (true) },
				new Attribute[] { new DefaultValueAttribute (null) },
				new Attribute[] { new DefaultValueAttribute (false) },
				new Attribute[] { new DefaultValueAttribute (typeof (Point), "0, 0") },
				new Attribute[] {}
			 };
			
			PropertyDescriptor propertyDescriptor = null;

			// If existing redirect each property to the ControlDesigner.
			//
			for (int i=0; i < newProperties.Length; i++) {
				propertyDescriptor = properties[newProperties[i]] as PropertyDescriptor;
				if (propertyDescriptor != null)
					properties[newProperties[i]] = TypeDescriptor.CreateProperty (typeof (ControlDesigner),
												      propertyDescriptor,
												      attributes[i]);
			}

			// This one is a must to have.
			//
			properties["Locked"] = TypeDescriptor.CreateProperty (typeof (ControlDesigner), "Locked",
										  typeof(bool),
										  new Attribute[] {
											  DesignOnlyAttribute.Yes,
											  BrowsableAttribute.Yes,
											  CategoryAttribute.Design,
											  new DefaultValueAttribute (false),
											  new DescriptionAttribute("The Locked property determines if we can move or resize the control.")
										  });

		}

		// ShadowProperties returns the real property value if there is no "shadow" one set
		// Welcome to the land of shadows... :-)
		//
		private bool Visible {
			get { return (bool) base.ShadowProperties["Visible"]; }
			set { base.ShadowProperties["Visible"] = value; }
		}

		private bool Enabled {
			get { return (bool) base.ShadowProperties["Enabled"]; }
			set { base.ShadowProperties["Enabled"] = value; }
		}

		private bool Locked {
			get { return _locked; }
			set { _locked = value; }
		}

		private bool AllowDrop {
			get { return (bool)base.ShadowProperties["AllowDrop"]; }
			set { base.ShadowProperties["AllowDrop"] = value; }
		}

		private string Name {
			get { return base.Component.Site.Name; }
			set { base.Component.Site.Name = value; }
		}

		private ContextMenu ContextMenu {
			get { return (ContextMenu) base.ShadowProperties["ContextMenu"]; }
			set { base.ShadowProperties["ContextMenu"] = value; }
		}

		private Point Location {
			get { return this.Control.Location; }
			set { this.Control.Location = value; }
		}
#endregion


#region Utility methods
		internal object GetValue (object component, string propertyName)
		{
		   return this.GetValue (component, propertyName, null);
		}
		
		internal object GetValue (object component, string propertyName, Type propertyType)
		{
			PropertyDescriptor prop = TypeDescriptor.GetProperties (component)[propertyName] as PropertyDescriptor;
			if (prop == null)
				throw new InvalidOperationException ("Property \"" + propertyName + "\" is missing on " + 
													 component.GetType().AssemblyQualifiedName);
			if (propertyType != null && !propertyType.IsAssignableFrom (prop.PropertyType))
					throw new InvalidOperationException ("Types do not match: " + prop.PropertyType.AssemblyQualifiedName +
														 " : " + propertyType.AssemblyQualifiedName);
			return prop.GetValue (component);
		}

		internal void SetValue (object component, string propertyName, object value)
		{
			PropertyDescriptor prop = TypeDescriptor.GetProperties (component)[propertyName] as PropertyDescriptor;

			if (prop == null)
					throw new InvalidOperationException ("Property \"" + propertyName + "\" is missing on " + 
														 component.GetType().AssemblyQualifiedName);
			if (!prop.PropertyType.IsAssignableFrom (value.GetType ()))
					throw new InvalidOperationException ("Types do not match: " + value.GetType ().AssemblyQualifiedName +
														 " : " + prop.PropertyType.AssemblyQualifiedName);
			if (!prop.IsReadOnly)
				prop.SetValue (component, value);
		}
#endregion

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (this.Control != null) {
					UnhookChildControls (Control);
					OnMouseDragEnd (true);
					_messageRouter.Dispose ();
					this.Control.DragDrop -= new DragEventHandler (OnDragDrop);
					this.Control.DragEnter -= new DragEventHandler (OnDragEnter);
					this.Control.DragLeave -= new EventHandler (OnDragLeave);
					this.Control.DragOver -= new DragEventHandler (OnDragOver);
				}
			}
			base.Dispose (true);
		}



		public virtual ControlDesigner InternalControlDesigner (int internalControlIndex)
		{
			return null;
		}

		public virtual int NumberOfInternalControlDesigners ()
		{
			return 0;
		}

		protected bool EnableDesignMode (Control child, string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (child == null)
				throw new ArgumentNullException ("child");

			bool success = false;
			INestedContainer nestedContainer = this.GetService (typeof (INestedContainer)) as INestedContainer;
			if (nestedContainer != null) {
				nestedContainer.Add (child, name);
				success = true;
			}
			return success;
		}

#region NET_2_0 Stubs

		[ComVisible (true)]
		public class ControlDesignerAccessibleObject : AccessibleObject
		{
			[MonoTODO]
			public ControlDesignerAccessibleObject (ControlDesigner designer, Control control)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetChild (int index)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override int GetChildCount ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetFocused ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetSelected ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject HitTest (int x, int y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override Rectangle Bounds {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string DefaultAction {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string Description {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string Name {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override AccessibleObject Parent {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override AccessibleRole Role {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override AccessibleStates State {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string Value {
				get { throw new NotImplementedException (); }
			}
		}

		[MonoTODO]
		protected virtual ControlBodyGlyph GetControlGlyph (GlyphSelectionType selectionType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual GlyphCollection GetGlyphs (GlyphSelectionType selectionType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void InitializeExistingComponent (IDictionary defaultValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void InitializeNewComponent (IDictionary defaultValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragComplete (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override InheritanceAttribute InheritanceAttribute {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual IList SnapLines {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual bool ParticipatesWithSnapLines {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool AutoResizeHandles {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endregion


	}
}
