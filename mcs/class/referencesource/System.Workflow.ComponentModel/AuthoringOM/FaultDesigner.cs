namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Workflow.ComponentModel.Design;

    #region Class ThrowDesigner
    [ActivityDesignerTheme(typeof(ThrowDesignerTheme))]
    internal sealed class ThrowDesigner : ActivityDesigner
    {
    }
    #endregion

    #region ThrowDesignerTheme
    internal sealed class ThrowDesignerTheme : ActivityDesignerTheme
    {
        public ThrowDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xC8, 0x2D, 0x11);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xFB, 0xD7, 0xD0);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xF3, 0x85, 0x72);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
