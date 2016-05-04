using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Globalization;
using System.Runtime;

namespace System.Activities.Presentation.View
{
    [Fx.Tag.XamlVisible(false)]
    class NamespaceDisplayAutomationPeer : UIElementAutomationPeer
    {
        public NamespaceDisplayAutomationPeer(NamespaceDisplay owner)
            : base(owner)
        {
        }

        protected override string GetItemStatusCore()
        {
            NamespaceDisplay namespaceDisplay = this.Owner as NamespaceDisplay;
            if (namespaceDisplay != null)
            {
                XElement itemStatus = new XElement("NamespaceStatus",
                    new XAttribute("Status", namespaceDisplay.IsInvalid ? "Invalid" : "Valid"),
                    new XAttribute("ErrorMessage", namespaceDisplay.ErrorMessage));
                return itemStatus.ToString();
            }
            return base.GetItemStatusCore();
        }
    }
}
