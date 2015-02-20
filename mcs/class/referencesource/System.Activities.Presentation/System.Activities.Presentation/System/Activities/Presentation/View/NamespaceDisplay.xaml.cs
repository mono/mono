using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Automation.Peers;

namespace System.Activities.Presentation.View
{
    /// <summary>
    /// Interaction logic for NamespaceDisplay.xaml
    /// </summary>
    partial class NamespaceDisplay : UserControl
    {
        public static readonly DependencyProperty NamespaceProperty = DependencyProperty.Register(
            "Namespace",
            typeof(string),
            typeof(NamespaceDisplay));

        public static readonly DependencyProperty IsInvalidProperty = DependencyProperty.Register(
            "IsInvalid",
            typeof(bool),
            typeof(NamespaceDisplay));

        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            "ErrorMessage",
            typeof(string),
            typeof(NamespaceDisplay));

        public NamespaceDisplay()
        {
            InitializeComponent();
        }

        public string Namespace
        {
            get { return (string)GetValue(NamespaceProperty); }
            set { SetValue(NamespaceProperty, value); }
        }

        public bool IsInvalid
        {
            get { return (bool)GetValue(IsInvalidProperty); }
            set { SetValue(IsInvalidProperty, value); }
        }

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new NamespaceDisplayAutomationPeer(this);
        }
    }
}
