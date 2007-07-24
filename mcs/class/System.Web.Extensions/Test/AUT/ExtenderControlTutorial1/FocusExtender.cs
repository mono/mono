using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Samples.CS
{
    [TargetControlType(typeof(Control))]
    public class FocusExtender : ExtenderControl
    {
        private string _highlightCssClass;
        private string _noHighlightCssClass;

        public string HighlightCssClass
        {
            get { return _highlightCssClass; }
            set { _highlightCssClass = value; }
        }

        public string NoHighlightCssClass
        {
            get { return _noHighlightCssClass; }
            set { _noHighlightCssClass = value; }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            ScriptReference reference = new ScriptReference();
            reference.Path = ResolveClientUrl("FocusBehavior.js");

            return new ScriptReference[] { reference };
        }
        
        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors(Control targetControl)
        {
            ScriptBehaviorDescriptor descriptor = new ScriptBehaviorDescriptor("Samples.FocusBehavior", targetControl.ClientID);
            descriptor.AddProperty("highlightCssClass", this.HighlightCssClass);
            descriptor.AddProperty("nohighlightCssClass", this.NoHighlightCssClass);

            return new ScriptDescriptor[] { descriptor };
        }
    }
}
