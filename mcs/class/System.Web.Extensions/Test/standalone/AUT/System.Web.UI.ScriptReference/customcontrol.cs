using System;
using System.Drawing;
using System.Web.UI;
using System.Web;
using System.Globalization;

namespace SampleControl
{
    public class UpdatePanelAnimationWithClientResource : Control
    {
        private string _updatePanelID;
        private Color _borderColor;
        private Boolean _animate;
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                _borderColor = value;
            }
        }

        public string UpdatePanelID
        {
            get
            {
                return _updatePanelID;
            }
            set
            {
                _updatePanelID = value;
            }
        }

        public Boolean Animate
        {
            get
            {
                return _animate;
            }
            set
            {
                _animate = value;
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Animate)
            {

                UpdatePanel updatePanel = (UpdatePanel)FindControl(UpdatePanelID);

                string script = String.Format(
                   CultureInfo.InvariantCulture,
                   @"
Sys.Application.add_load(function(sender, args) {{
var {0}_borderAnimation = new BorderAnimation('{1}');    
var panelElement = document.getElementById('{0}');
     if (args.get_isPartialLoad()) {{
        {0}_borderAnimation.animate(panelElement);
    }}
}})
",
                   updatePanel.ClientID,
                   ColorTranslator.ToHtml(BorderColor));


                ScriptManager.RegisterStartupScript(
                    this,
                    typeof(UpdatePanelAnimationWithClientResource),
                    ClientID,
                    script,
                    true);
            }
        }
    }
}
