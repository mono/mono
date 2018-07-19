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

    #region Class CodeDesigner
    [ActivityDesignerTheme(typeof(CodeDesignerTheme))]
    internal sealed class CodeDesigner : ActivityDesigner
    {
    }
    #endregion

    #region CodeDesignerTheme
    internal sealed class CodeDesignerTheme : ActivityDesignerTheme
    {
        public CodeDesignerTheme(WorkflowTheme theme)
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
}
