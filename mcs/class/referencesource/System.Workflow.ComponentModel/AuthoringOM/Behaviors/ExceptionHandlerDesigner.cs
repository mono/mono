namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Design;

    #region Class FaultHandlerDesigner
    [ActivityDesignerTheme(typeof(FaultHandlerActivityDesignerTheme))]
    internal sealed class FaultHandlerActivityDesigner : SequentialActivityDesigner
    {
        #region Members, Constructor and Destructor
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (properties["InitializeField"] == null)
                properties["InitializeField"] = TypeDescriptor.CreateProperty(GetType(), "InitializeField", typeof(bool), new Attribute[] { DesignerSerializationVisibilityAttribute.Hidden, BrowsableAttribute.No });
        }
        public override ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                List<DesignerView> views = new List<DesignerView>();
                foreach (DesignerView view in base.Views)
                {
                    // disable the fault handlers, cancellation handler and compensation handler
                    if ((view.ViewId != 2) &&
                            (view.ViewId != 3) &&
                            (view.ViewId != 4)
                        )
                        views.Add(view);
                }
                return new ReadOnlyCollection<DesignerView>(views);
            }
        }
        #endregion

        #region Properties and Methods
        private bool InitializeField
        {
            get
            {
                return false;
            }
        }


        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null && string.Equals(e.Member.Name, "FaultType", StringComparison.Ordinal))
                TypeDescriptor.Refresh(e.Activity);
        }

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivityDesigner");

            if (!(parentActivityDesigner.Activity is FaultHandlersActivity))
                return false;

            return base.CanBeParentedTo(parentActivityDesigner);
        }
        #endregion
    }
    #endregion

    #region FaultHandlerActivityDesignerTheme
    internal sealed class FaultHandlerActivityDesignerTheme : CompositeDesignerTheme
    {
        public FaultHandlerActivityDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
