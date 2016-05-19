namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Security;
    using System.Security.Permissions;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    #region Class SetStateDesigner

    [ActivityDesignerTheme(typeof(SetStateDesignerTheme))]
    [System.Runtime.InteropServices.ComVisible(false)]
    internal sealed class SetStateDesigner : ActivityDesigner
    {
        #region Fields

        private string previousTargetState = String.Empty;
        private Size targetStateSize = Size.Empty;

        #endregion Fields

        #region Properties

        #region Protected Properties


        protected override Rectangle TextRectangle
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle textRectangle = base.TextRectangle;
                textRectangle.Offset(0, (-targetStateSize.Height - margin.Height) / 2);
                return textRectangle;
            }
        }

        protected override Rectangle ImageRectangle
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle imageRectangle = base.ImageRectangle;
                imageRectangle.Offset(0, (-targetStateSize.Height - margin.Height) / 2);
                return imageRectangle;
            }
        }

        #endregion

        #region Private Properties

        private string TargetState
        {
            get
            {
                SetStateActivity setState = this.Activity as SetStateActivity;
                if (setState == null)
                    return String.Empty;

                string targetState = setState.TargetStateName;
                if (targetState == null)
                    return String.Empty;

                return targetState;
            }
        }

        /// <summary>
        /// Gets the value of text rectangle in logical coordinates.
        /// </summary>
        internal Rectangle TargetStateRectangle
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                Rectangle bounds = this.Bounds;
                Rectangle textRectangle = this.TextRectangle;
                Point location = new Point(
                    bounds.Left + margin.Width,
                    textRectangle.Bottom + (margin.Height / 2));
                Size size = new Size(
                    bounds.Width - margin.Width * 2,
                    targetStateSize.Height);
                return new Rectangle(location, size);
            }
        }
        #endregion Private Properties

        #endregion Properties

        #region Methods

        #region Public Methods

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivityDesigner");

            CompositeActivity parentActivity = parentActivityDesigner.Activity as CompositeActivity;
            if (parentActivity == null)
                return false;

            bool result = ValidateParent(parentActivity);
            if (!result)
                return false;

            return base.CanBeParentedTo(parentActivityDesigner);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (previousTargetState != this.TargetState)
                PerformLayout();
        }

        /// <summary>
        /// Called to set the size of the visual cues or designers contained within the designer.
        /// </summary>
        /// <param name="e">ActivityDesignerLayoutEventArgs holding layout arguments</param>
        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);

            string targetState = this.TargetState;
            if (String.IsNullOrEmpty(targetState))
            {
                // We use a dummy string so we don't 
                // calculate an empty rectangle
                targetState = "M";
            }

            Font font = e.DesignerTheme.Font;

            this.targetStateSize = StateMachineDesignerPaint.MeasureString(e.Graphics,
                font,
                targetState,
                StringAlignment.Near,
                Size.Empty);

            size.Height += targetStateSize.Height;
            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            string targetState = this.TargetState;

            ActivityDesignerPaint.DrawText(e.Graphics,
                e.DesignerTheme.Font,
                targetState,
                this.TargetStateRectangle,
                StringAlignment.Center,
                e.AmbientTheme.TextQuality,
                e.DesignerTheme.ForegroundBrush);
        }

        #endregion Protected Methods

        #region Static Private Methods

        static private bool ValidateParent(CompositeActivity parentActivity)
        {
            if (parentActivity == null)
                return false;

            if (SetStateValidator.IsValidContainer(parentActivity))
                return true;

            return ValidateParent(parentActivity.Parent);
        }

        #endregion Static Private Methods

        #endregion Methods

    }

    #endregion

    #region SetStateDesignerTheme
    internal sealed class SetStateDesignerTheme : ActivityDesignerTheme
    {
        public SetStateDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion

    #region Class StateDropDownEditor

    internal sealed class StateDropDownEditor : UITypeEditor
    {
        #region Fields
        private IWindowsFormsEditorService _editorService;
        private ITypeDescriptorContext _context;
        private object _selectedObject;
        #endregion Fields

        #region Constructors/Destructors
        public StateDropDownEditor()
        {
        }
        #endregion Constructors/Destructors

        #region Methods

        #region Public Methods
        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object value)
        {
            if (typeDescriptorContext == null)
                throw new ArgumentNullException("typeDescriptorContext");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");
            _editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            _context = typeDescriptorContext;

            // Initialize the dropdown control
            ListBox dropDownList = new ListBox();
            dropDownList.BorderStyle = BorderStyle.None;

            Activity activity = _context.Instance as Activity;
            if (activity == null)
            {
                // this could happen when there are multiple 
                // SetState activities selected
                object[] activities = _context.Instance as object[];
                if (activities != null && activities.Length > 0)
                    activity = (Activity)activities[0];
            }
            Debug.Assert(activity != null);

            // Add the items from the typeconverter, followed by the datasource choices
            PopulateDropDownList(dropDownList, activity);

            dropDownList.SelectedIndexChanged += new EventHandler(dataSourceDropDown_SelectedIndexChanged);

            // Display the control
            _editorService.DropDownControl(dropDownList);

            // If a value was selected, read the selected value from the control and return it
            if (dropDownList.SelectedIndex != -1 && _selectedObject != null)
                return _selectedObject;

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        #endregion Public Methods

        #region Private Methods

        private void dataSourceDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _editorService.CloseDropDown();
            _selectedObject = null;

            ListBox dropDownList = sender as ListBox;
            if (dropDownList == null)
                throw new ArgumentNullException("sender");

            if (dropDownList.SelectedIndex < 0)
                return;

            _selectedObject = dropDownList.Items[dropDownList.SelectedIndex];
        }

        private void PopulateDropDownList(ListBox dropDownList, Activity activity)
        {
            Debug.Assert(dropDownList != null);
            Debug.Assert(activity != null);

            StateActivity enclosingState = StateMachineHelpers.FindEnclosingState(activity);
            if (enclosingState == null)
                return;

            StateActivity rootState = StateMachineHelpers.GetRootState(enclosingState);

            FindStates(dropDownList, rootState);
        }

        private void FindStates(ListBox dropDownList, StateActivity parent)
        {
            foreach (Activity activity in parent.EnabledActivities)
            {
                StateActivity state = activity as StateActivity;
                if (state != null)
                {
                    if (StateMachineHelpers.IsLeafState(state))
                    {
                        dropDownList.Items.Add(state.QualifiedName);
                    }
                    else
                    {
                        FindStates(dropDownList, state);
                    }
                }
            }
        }

        #endregion Private Methods

        #endregion Methods
    }

    #endregion Class StateDropDownEditor
}
