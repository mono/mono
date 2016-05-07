using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Globalization;

namespace System.Activities.Presentation.View
{
    [System.Runtime.Fx.Tag.XamlVisible(false)]
    class VisualBasicEditorAutomationPeer : UIElementAutomationPeer
    {
        public VisualBasicEditorAutomationPeer(VisualBasicEditor owner)
            : base(owner)
        {
        }

        protected override string GetItemStatusCore()
        {
            VisualBasicEditor textBox = this.Owner as VisualBasicEditor;
            if (textBox != null)
            {
                XElement itemStatus = new XElement("VisualBasicEditorItemStatus",
                    new XAttribute("Status", textBox.HasErrors ? "Invalid" : "Valid"),
                    new XAttribute("EditingState", textBox.EditingState.ToString()),
                    new XAttribute("ErrorMessage", String.IsNullOrEmpty(textBox.ErrorMessage) ? String.Empty : textBox.ErrorMessage));
                return itemStatus.ToString();
            }
            return base.GetItemStatusCore();
        }
    }
}
