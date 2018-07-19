using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Windows.Forms.Design;
using System.Security.Permissions;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System.Workflow.Activities
{
    [ActivityDesignerTheme(typeof(WebServiceResponseDesignerTheme))]
    internal sealed class WebServiceResponseDesigner : ActivityDesigner
    {
        #region Properties and Methods
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            ITypeProvider typeProvider = (ITypeProvider)GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            WebServiceOutputActivity webServiceResponse = this.Activity as WebServiceOutputActivity;
            webServiceResponse.GetParameterPropertyDescriptors(properties);
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null)
            {
                // If the receive activity id changed, clear out parameters.
                if (e.Member.Name == "InputActivityName")
                {
                    (e.Activity as WebServiceOutputActivity).ParameterBindings.Clear();
                    TypeDescriptor.Refresh(e.Activity);
                }
            }
        }
        #endregion
    }
    #region WebServiceResponseDesignerTheme
    internal sealed class WebServiceResponseDesignerTheme : ActivityDesignerTheme
    {
        public WebServiceResponseDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x94, 0xB6, 0xF7);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xDF);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xA5, 0xC3, 0xF7);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion
}
