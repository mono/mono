namespace System.Workflow.Activities
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

    #region Class DelayDesigner
    [ActivityDesignerTheme(typeof(DelayDesignerTheme))]
    internal sealed class DelayDesigner : ActivityDesigner
    {
    }
    #endregion

    #region DelayDesignerTheme
    internal sealed class DelayDesignerTheme : ActivityDesignerTheme
    {
        public DelayDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x80, 0x40, 0x40);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0x80, 0x40, 0x40);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xF1, 0xE4, 0xE4);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
