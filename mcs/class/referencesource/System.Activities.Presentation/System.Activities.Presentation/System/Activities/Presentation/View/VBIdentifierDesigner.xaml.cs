//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
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
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.Windows.Automation.Peers;
    using System.Xml.Linq;
    using System.Reflection;

    partial class VBIdentifierDesigner : UserControl
    {
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(VBIdentifierName), typeof(VBIdentifierDesigner), new UIPropertyMetadata(OnIdentifierChanged));

        public static readonly DependencyProperty NameStringProperty =
            DependencyProperty.Register("NameString", typeof(string), typeof(VBIdentifierDesigner), new UIPropertyMetadata(null, null, OnNameChanging));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(VBIdentifierDesigner));

        public event PropertyChangedEventHandler TextBoxPropertyChanged;

        TextBox textBox;
        bool isInternalChange;
        bool checkAgainstXaml;

        public VBIdentifierName Identifier
        {
            get { return (VBIdentifierName)GetValue(IdentifierProperty); }
            set { SetValue(IdentifierProperty, value); }
        }

        public string NameString
        {
            get { return (string)GetValue(NameStringProperty); }
            set { SetValue(NameStringProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        internal TextBox IdentifierTextBox
        {
            get
            {
                return this.textBox;
            }
            set
            {
                if (value != this.textBox)
                {
                    this.textBox = value;
                    if (this.TextBoxPropertyChanged != null)
                    {
                        this.TextBoxPropertyChanged(this, new PropertyChangedEventArgs("IdentifierTextBox"));
                    }
                }
            }
        }

        public VBIdentifierDesigner()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
                {
                    //when designer gets loaded, get its identifier binding expression with parent control
                    var binding = this.GetBindingExpression(VBIdentifierDesigner.IdentifierProperty);
                    if (null != binding && null != binding.ParentBinding)
                    {
                        //if one exists - define update source exception filter
                        binding.ParentBinding.UpdateSourceExceptionFilter = this.OnBindingException;
                    }
                };
        }


        void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.IdentifierTextBox = sender as TextBox;
            this.IdentifierTextBox.LostKeyboardFocus += this.OnTextBoxLostKeyboardFocus;
        }

        void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.IdentifierTextBox.IsFocused)
            {
                DependencyObject focusScope = FocusManager.GetFocusScope(this.IdentifierTextBox);
                FocusManager.SetFocusedElement(focusScope, this);
            }
        }

        void OnTextBoxUnloaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.LostKeyboardFocus -= this.OnTextBoxLostKeyboardFocus;
            this.IdentifierTextBox = null;
        }

        object OnBindingException(object sender, Exception err)
        {
            //whenever exception occurs, allow ValidationException to be rethrown - this exception is needed to revert invalid value
            if (err is TargetInvocationException && err.InnerException is ValidationException || err is ValidationException)
            {
                throw FxTrace.Exception.AsError( err.InnerException ?? err );
            }
            //simply return null - we don't use error template for this control
            return null;
        }

        static object OnNameChanging(DependencyObject sender, object newValue)
        {
            var ctrl = (VBIdentifierDesigner)sender;
            //before allowing new value to be assigned to property, check if it passes validation
            return ctrl.OnNameChanging(ctrl.NameString, (string)newValue);
        }

        object OnNameChanging(string oldName, string newName)
        {
            string result = newName;
            if (!this.isInternalChange)
            {
                try
                {
                    this.isInternalChange = true;
                    //try to create new Identifier - if this call succeds, set the new value to a property
                    this.Identifier = new VBIdentifierName(this.checkAgainstXaml) { IdentifierName = newName };
                }
                catch (ValidationException)
                {
                    //in case of validation exception - do not allow new invalid value to be set
                    result = oldName;
                    //if text box is still visible - refresh its Text property to old value
                    if (null != this.textBox)
                    {
                        var binding = this.textBox.GetBindingExpression(TextBox.TextProperty);
                        binding.UpdateTarget();
                    }
                }
                finally
                {
                    this.isInternalChange = false;
                }
            }
            return result;
        }

        static void OnIdentifierChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as VBIdentifierDesigner).OnIdentifierChanged();
        }

        void OnIdentifierChanged()
        {
            if (!this.isInternalChange && null != this.Identifier)
            {
                this.isInternalChange = true;
                this.NameString = this.Identifier.IdentifierName;
                this.isInternalChange = false;
                this.checkAgainstXaml = this.Identifier.CheckAgainstXaml;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new VBIdentiferDesignerAutomationPeer(this);
        }
    }

    class VBIdentiferDesignerAutomationPeer : UIElementAutomationPeer
    {
        public VBIdentiferDesignerAutomationPeer(VBIdentifierDesigner owner)
            : base(owner)
        {
        }
        
        protected override string GetItemStatusCore()
        {
            VBIdentifierDesigner vbIdentifierDesigner = this.Owner as VBIdentifierDesigner;
            if (vbIdentifierDesigner != null)
            {
                VBIdentifierName vbIdentifier = vbIdentifierDesigner.Identifier;
                if (vbIdentifier != null)
                {
                    XElement itemStatus = new XElement("VBIdentifierStatus",
                        new XAttribute("Status", vbIdentifier.IsValid ? "Valid" : "Invalid"),
                        new XAttribute("WarningMessage", vbIdentifier.ErrorMessage));
                    return itemStatus.ToString();
                }
            }
            return base.GetItemStatusCore();
        }
    }
}
