//
// System.Windows.Forms.Design.DocumentDesigner
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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.Windows.Forms.Design.Behavior;

namespace System.Windows.Forms.Design
{
	[ToolboxItemFilter ("System.Windows.Forms")]
	public class DocumentDesigner : ScrollableControlDesigner, IRootDesigner, IToolboxUser
	{

		// This is what you *see*
		/*
			.-------------------------------------.
			| Panel to host the designed Control  |
			|--------------Splitter---------------|
			|	  Panel with a ComponentTray	 |
			|_____________________________________|

		*/
		//
#region DesignerViewFrame
		public class DesignerViewFrame : System.Windows.Forms.UserControl
		{
			private System.Windows.Forms.Panel DesignerPanel;
			private System.Windows.Forms.Splitter splitter1;
			private System.Windows.Forms.Panel ComponentTrayPanel;
			private ComponentTray _componentTray;
			private Control _designedControl;

			public DesignerViewFrame (Control designedControl, ComponentTray tray)
			{
				if (designedControl == null) {
					throw new ArgumentNullException ("designedControl");
				}
				if (tray == null) {
					throw new ArgumentNullException ("tray");
				}
				//
				// The InitializeComponent() call is required for Windows Forms designer support.
				//
				InitializeComponent();

 				_designedControl = designedControl;
 				this.SuspendLayout ();
 				this.DesignerPanel.Controls.Add (designedControl);
 				this.ResumeLayout ();

				this.ComponentTray = tray;
			}

#region Windows Forms Designer generated code
			/// <summary>
			/// This method is required for Windows Forms designer support.
			/// Do not change the method contents inside the source code editor. The Forms designer might
			/// not be able to load this method if it was changed manually.
			/// </summary>
			private void InitializeComponent() {
				this.ComponentTrayPanel = new System.Windows.Forms.Panel();
				this.splitter1 = new System.Windows.Forms.Splitter();
				this.DesignerPanel = new System.Windows.Forms.Panel();
				this.SuspendLayout();
				// 
				// ComponentTrayPanel
				// 
				this.ComponentTrayPanel.BackColor = System.Drawing.Color.LemonChiffon;
				this.ComponentTrayPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
				this.ComponentTrayPanel.Location = new System.Drawing.Point(0, 194);
				this.ComponentTrayPanel.Name = "ComponentTrayPanel";
				this.ComponentTrayPanel.Size = new System.Drawing.Size(292, 72);
				this.ComponentTrayPanel.TabIndex = 1;
				this.ComponentTrayPanel.Visible = false;
				// 
				// splitter1
				// 
				this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
				this.splitter1.Location = new System.Drawing.Point(0, 186);
				this.splitter1.Name = "splitter1";
				this.splitter1.Size = new System.Drawing.Size(292, 8);
				this.splitter1.TabIndex = 2;
				this.splitter1.TabStop = false;
				this.splitter1.Visible = false;
				// 
				// DesignerPanel
				// 
				this.DesignerPanel.AutoScroll = true;
				this.DesignerPanel.BackColor = System.Drawing.Color.White;
				this.DesignerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				this.DesignerPanel.Location = new System.Drawing.Point(0, 0);
				this.DesignerPanel.Name = "DesignerPanel";
				this.DesignerPanel.Size = new System.Drawing.Size(292, 266);
				this.DesignerPanel.TabIndex = 0;
				this.DesignerPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DesignerPanel_MouseUp);
				this.DesignerPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DesignerPanel_MouseMove);
				this.DesignerPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DesignerPanel_MouseDown);
				this.DesignerPanel.Paint += new PaintEventHandler (DesignerPanel_Paint);
				// 
				// DesignerViewFrame
				// 
				this.Controls.Add(this.splitter1);
				this.Controls.Add(this.ComponentTrayPanel);
				this.Controls.Add(this.DesignerPanel);
				this.Name = "UserControl1";
				this.Size = new System.Drawing.Size(292, 266);
				this.Dock = DockStyle.Fill;
				this.ResumeLayout(false);
			}

#endregion
			
			private bool _mouseDown = false;
			private bool _firstMove = false;

			void DesignerPanel_Paint (object sender, PaintEventArgs e)
			{
				IUISelectionService selectionServ = this.DesignedControl.Site.GetService (typeof (IUISelectionService)) as IUISelectionService;
				if (selectionServ != null)
					selectionServ.PaintAdornments (this.DesignerPanel, e.Graphics);
			}

			void DesignerPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
			{
				_mouseDown = true;
				_firstMove = true;
			}

			void DesignerPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
			{
				IUISelectionService selectionServ = this.DesignedControl.Site.GetService (typeof (IUISelectionService)) as IUISelectionService;
				if (selectionServ == null)
					return;
				
				selectionServ.SetCursor (e.X, e.Y);
				if (_mouseDown) {
					if (_firstMove) {
						selectionServ.MouseDragBegin (this.DesignerPanel, e.X, e.Y);
						_firstMove = false;
					}
					else {
						selectionServ.MouseDragMove (e.X, e.Y);
					}
				}
				else if (selectionServ.SelectionInProgress) {
					selectionServ.MouseDragMove (e.X, e.Y);
				}
			}

			void DesignerPanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
			{
				IUISelectionService selectionServ = this.DesignedControl.Site.GetService (typeof (IUISelectionService)) as IUISelectionService;
				if (_mouseDown) {
					if (selectionServ != null)
						selectionServ.MouseDragEnd (false);
					_mouseDown = false;
				}
				else if (selectionServ.SelectionInProgress) {
					selectionServ.MouseDragEnd (false);
				}
			}

			// by default the component tray is hidden and essentially should be shown once there
			// is a component added to it
			//
			public void ShowComponentTray ()
			{
				if (!this.ComponentTray.Visible) {
					this.ComponentTrayPanel.Visible = true;
					this.ComponentTray.Visible = true;
					this.splitter1.Visible = true;
				}
			}
			
			public void HideComponentTray ()
			{
				if (!this.ComponentTray.Visible) {
					this.ComponentTrayPanel.Visible = true;
					this.ComponentTray.Visible = true;
					this.splitter1.Visible = true;
				}
			}

			public ComponentTray ComponentTray {
				get { return _componentTray; }
				set {
					this.SuspendLayout ();
					this.ComponentTrayPanel.Controls.Remove (_componentTray);
					this.ComponentTrayPanel.Controls.Add (value);
					this.ResumeLayout ();
					_componentTray = value;
					_componentTray.Visible = false;
				}
			}

			public Control DesignedControl {
				get { return _designedControl; }
				set { 
				}
			}

			protected override void Dispose (bool disposing)
			{
				if (_designedControl != null) {
					this.DesignerPanel.Controls.Remove (_designedControl);
					_designedControl = null;
				}
				
				if (_componentTray != null) {
					this.ComponentTrayPanel.Controls.Remove (_componentTray);
					_componentTray.Dispose ();
					_componentTray = null;
				}
				
				base.Dispose (disposing);
			}
		}		
#endregion	   
		
		
		
		
		private DesignerViewFrame _designerViewFrame;

		public DocumentDesigner ()
		{
		}

		private DesignerViewFrame View {
			get { return _designerViewFrame; }
		}

#region Initialization
		public override void Initialize (IComponent component)
		{
			base.Initialize (component);

			_designerViewFrame = new DesignerViewFrame (this.Control, new ComponentTray (this, component.Site));
			_designerViewFrame.DesignedControl.Location = new Point (15, 15);
			SetValue (this.Component,  "Location", new Point (0, 0));

			IComponentChangeService componentChangeSvc = GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (componentChangeSvc != null) {
				componentChangeSvc.ComponentAdded += new ComponentEventHandler (OnComponentAdded);
				componentChangeSvc.ComponentRemoved += new ComponentEventHandler (OnComponentRemoved);
			}

			IMenuCommandService menuCommands = GetService (typeof (IMenuCommandService)) as IMenuCommandService;
			IServiceContainer serviceContainer = this.GetService (typeof (IServiceContainer)) as IServiceContainer;
			if (menuCommands != null && serviceContainer != null)
				new DefaultMenuCommands (serviceContainer).AddTo (menuCommands);
			InitializeSelectionService ();
		}

		private void InitializeSelectionService ()
		{
			IUISelectionService guiSelectionService = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (guiSelectionService == null) {
				IServiceContainer serviceContainer = this.GetService (typeof (IServiceContainer)) as IServiceContainer;
				serviceContainer.AddService (typeof (IUISelectionService), (IUISelectionService) new UISelectionService (serviceContainer));
			}

			ISelectionService selectionService = this.GetService (typeof (ISelectionService)) as ISelectionService;
			selectionService.SetSelectedComponents (new IComponent[] { this.Component });
		}


		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (_designerViewFrame != null) {
					_designerViewFrame.Dispose ();
					_designerViewFrame = null;
					
				}
				IComponentChangeService componentChangeSvc = GetService (typeof (IComponentChangeService)) as IComponentChangeService;
				if (componentChangeSvc != null) {
					componentChangeSvc.ComponentAdded -= new ComponentEventHandler (OnComponentAdded);
					componentChangeSvc.ComponentRemoved -= new ComponentEventHandler (OnComponentRemoved);
				}
			}
			base.Dispose (disposing);
		}
#endregion


#region MSDN says overriden

		public override GlyphCollection GetGlyphs (GlyphSelectionType selectionType)
		{
			return base.GetGlyphs (selectionType);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		protected override void OnContextMenu (int x, int y)
		{
			base.OnContextMenu (x, y);
		}

		protected override void OnCreateHandle ()
		{
			base.OnCreateHandle ();
		}
		
#endregion


#region Components and ComponentTray

		private void OnComponentAdded (object sender, ComponentEventArgs args)
		{
			if (!(args.Component is Control)) {
				this.View.ComponentTray.AddComponent (args.Component);
				if (this.View.ComponentTray.ComponentCount > 0) {
					if (!this.View.ComponentTray.Visible)
						this.View.ShowComponentTray ();
				}
			}
		}

		private void OnComponentRemoved (object sender, ComponentEventArgs args)
		{
			if (!(args.Component is Control)) {
				this.View.ComponentTray.RemoveComponent (args.Component);
				if (this.View.ComponentTray.ComponentCount == 0) {
					if (this.View.ComponentTray.Visible)
						this.View.HideComponentTray ();
				}
			}
		}
#endregion


#region IRootDesigner

		object IRootDesigner.GetView (ViewTechnology technology)
		{
			if (technology != ViewTechnology.Default)
				throw new ArgumentException ("Only ViewTechnology.WindowsForms is supported.");
			return _designerViewFrame;
		}

		ViewTechnology[] IRootDesigner.SupportedTechnologies {
			get {
				return new ViewTechnology[] { ViewTechnology.Default };
			}
		}
#endregion


#region IToolBoxUser

		// Indicates whether the specified tool is supported by the designer.
		// If it is not the tool is disabled in the toolbox.
		//
		// Used for subclasses, e.g the FormDocumentDesigner won't accept a Form?
		//
		bool IToolboxUser.GetToolSupported (ToolboxItem tool)
		{
			return this.GetToolSupported (tool);
		}

		protected virtual bool GetToolSupported (ToolboxItem tool)
		{
			return true;
		}


		// Handles the behavior that occurs when a user double-clicks a toolbox item.
		//
		void IToolboxUser.ToolPicked (ToolboxItem tool)
		{
			this.ToolPicked (tool);
		}

		// ToolPicked is called when the user double-clicks on a toolbox item. 
		// The document designer should create a component for the specified tool. 
		// Only tools that are enabled in the toolbox will be passed to this method.
		//
		// I create the component in the parent container of the primary selection.
		// If not available I create it in the rootcomponent (this essentially :-) )
		//
		protected virtual void ToolPicked (ToolboxItem tool)
		{
			ISelectionService selectionSvc = GetService (typeof (ISelectionService)) as ISelectionService;
			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (selectionSvc != null && host != null) {
				IDesigner designer = host.GetDesigner ((IComponent) selectionSvc.PrimarySelection);
				if (designer is ParentControlDesigner)
					ParentControlDesigner.InvokeCreateTool ((ParentControlDesigner) designer, tool);
				else
					this.CreateTool (tool);
			}
			else {
				this.CreateTool (tool);
			}
			IToolboxService tbServ = this.GetService (typeof (IToolboxService)) as IToolboxService;
			tbServ.SelectedToolboxItemUsed ();
		}
#endregion


#region Properties
		// A root designer can be resized to the bottom and to the right.
		//
		public override SelectionRules SelectionRules {
			get {
				return (SelectionRules.RightSizeable | SelectionRules.BottomSizeable | SelectionRules.Visible);
			}
		}
#endregion


#region Metadata filtering and Design-Time properties

		// MSDN says that this adds the "BackColor" and "Location" browsable design-time propeties.
		// 
		// The reason for overwriting the Location property created by the ControDesigner is that
		// the root component is not draggable (e.g a form has a static location in the DesignerViewFrame)
		//
		protected override void PreFilterProperties (IDictionary properties)
		{
			base.PreFilterProperties (properties);

			PropertyDescriptor propertyDescriptor = properties["BackColor"] as PropertyDescriptor;
			if (propertyDescriptor != null) {
				properties["BackColor"] = TypeDescriptor.CreateProperty (typeof (DocumentDesigner),
						propertyDescriptor,
						new Attribute[] { new DefaultValueAttribute (System.Drawing.SystemColors.Control) });
			}
			
			propertyDescriptor = properties["Location"] as PropertyDescriptor;
			if (propertyDescriptor != null) {
				properties["Location"] = TypeDescriptor.CreateProperty (typeof (DocumentDesigner),
						propertyDescriptor,
						new Attribute[] { new DefaultValueAttribute (typeof (Point), "0, 0") });
			}
		}

		private Color BackColor {
			get { return (Color) ShadowProperties["BackColor"]; }
			set {
				ShadowProperties["BackColor"] = value;
				this.Control.BackColor = value;
			}
		}
		
		private Point Location {
			get { return (Point) ShadowProperties["Location"]; }
			set { ShadowProperties["Location"] = value; }
		}
#endregion


#region Misc
		protected IMenuEditorService menuEditorService;

		// Checks for the existence of a menu editor service and creates one if one does not already exist.
		// component - The IComponent to ensure has a context menu service.
		// XXX: Not sure exactly what this should do...
		//
		protected virtual void EnsureMenuEditorService (IComponent c)
		{
			if (this.menuEditorService == null && c is ContextMenu)
				menuEditorService = (IMenuEditorService) GetService (typeof (IMenuEditorService));
		}
#endregion

	}
}
