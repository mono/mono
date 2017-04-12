using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRDescriptionAttribute : DescriptionAttribute
    {
        // Fields
        private bool replaced;

        // Methods
        public SRDescriptionAttribute(string description)
            : base(description)
        {
        }

        // Properties
        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = SR.Keys.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }

 

}
