namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.Generic;

    #region StateMachineWorkflowDesigner
    [ActivityDesignerTheme(typeof(StateMachineWorkflowDesignerTheme))]
    [System.Runtime.InteropServices.ComVisible(false)]
    internal sealed class StateMachineWorkflowDesigner : StateDesigner
    {
        #region Fields
        private static readonly Size MinSize = new Size(240, 240);
        private string text;
        private string helpText;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for the StateDesignerBase
        /// </summary>
        public StateMachineWorkflowDesigner()
        {
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            text = DR.GetString(DR.EventBasedWorkFlow);
        }

        #endregion

        #region Properties

        #region Public Properties

        public override string Text
        {
            get
            {
                return text;
            }
        }

        #endregion

        #region Private Properties

        internal override string HelpText
        {
            get
            {
                if (helpText == null)
                {
                    helpText = DR.GetString(DR.StateMachineWorkflowHelpText);
                }
                return helpText;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;

                minimumSize.Width = Math.Max(minimumSize.Width, MinSize.Width);
                minimumSize.Height = Math.Max(minimumSize.Height, MinSize.Height);
                if (IsRootDesigner && InvokingDesigner == null)
                {
                    minimumSize.Width = Math.Max(minimumSize.Width, ParentView.ViewPortSize.Width - StateDesigner.Separator.Width * 2);
                    minimumSize.Height = Math.Max(minimumSize.Height, ParentView.ViewPortSize.Height - StateDesigner.Separator.Height * 2);
                }

                return minimumSize;
            }
        }
        #endregion Private Properties

        #endregion

        #region Methods

        #region Public Methods

        #endregion

        #region Protected Methods

        protected override bool IsSupportedActivityType(Type activityType)
        {
            //we specifically, do not support state machine related activities.
            if (typeof(ListenActivity).IsAssignableFrom(activityType))
                return false;

            return base.IsSupportedActivityType(activityType);
        }

        #endregion

        #region Private Methods

        #endregion Private Methods

        #endregion Methods
    }
    #endregion

    #region StateMachineWorkflowDesignerTheme
    internal sealed class StateMachineWorkflowDesignerTheme : StateMachineTheme
    {
        public StateMachineWorkflowDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = true;
            this.ConnectorStartCap = LineAnchor.DiamondAnchor;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x49, 0x77, 0xB4);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }

        [Browsable(false)]
        public override string InitialStateDesignerImagePath
        {
            get
            {
                return base.InitialStateDesignerImagePath;
            }
            set
            {
                base.InitialStateDesignerImagePath = value;
            }
        }

        [Browsable(false)]
        public override string CompletedStateDesignerImagePath
        {
            get
            {
                return base.CompletedStateDesignerImagePath;
            }
            set
            {
                base.CompletedStateDesignerImagePath = value;
            }
        }
    }
    #endregion
}
