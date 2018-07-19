namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    #region Class TerminateDesigner
    [ActivityDesignerTheme(typeof(TerminateDesignerTheme))]
    internal sealed class TerminateDesigner : ActivityDesigner
    {
    }
    #endregion

    #region TerminateDesignerTheme
    internal sealed class TerminateDesignerTheme : ActivityDesignerTheme
    {
        public TerminateDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x79, 0x73, 0xA5);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xFF, 0xE8, 0xDF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xFF, 0xB3, 0x95);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
