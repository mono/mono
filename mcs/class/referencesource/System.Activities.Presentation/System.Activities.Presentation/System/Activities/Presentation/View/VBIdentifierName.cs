//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Windows;
    using System.Xaml;
    using System.Xml;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;

    /// <summary>
    /// The class is not only used for VB(Dev10), but also for C# (Dev11).
    /// </summary>
    internal class VBIdentifierName : DependencyObject
    {
        public static readonly DependencyProperty IdentifierNameProperty =
            DependencyProperty.Register("IdentifierName", typeof(string), typeof(VBIdentifierName), new UIPropertyMetadata(OnIdentifierNameChanged));

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(VBIdentifierName));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(VBIdentifierName));

        static VBCodeProvider vbProvider;
        static CSharpCodeProvider csProvider;
        static XamlSchemaContext xamlContext = new XamlSchemaContext();
        static XamlType xamlType = new XamlType(typeof(string), xamlContext);

        bool checkAgainstXaml;

        VBCodeProvider VBProvider
        {
            get
            {
                if (vbProvider == null)
                {
                    vbProvider = CodeDomProvider.CreateProvider("VisualBasic") as VBCodeProvider;
                }
                return vbProvider;
            }
        }

        CSharpCodeProvider CSProvider
        {
            get
            {
                if (csProvider == null)
                {
                    csProvider = CodeDomProvider.CreateProvider("C#") as CSharpCodeProvider;
                }

                return csProvider;
            }
        }

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        public string IdentifierName
        {
            get { return (string)GetValue(IdentifierNameProperty); }
            set { SetValue(IdentifierNameProperty, value); }
        }

        public bool CheckAgainstXaml
        {
            get
            {
                return this.checkAgainstXaml;
            }
        }

        public VBIdentifierName()
        {
            this.checkAgainstXaml = false;
        }

        public VBIdentifierName(bool checkAgainstXaml)
        {
            this.checkAgainstXaml = checkAgainstXaml;
        }

        static void OnIdentifierNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((VBIdentifierName)sender).OnIdentifierNameChanged();
        }

        internal static bool IsValidXamlName(string name)
        {
            bool isValid = new XamlMember(name, xamlType, false).IsNameValid;

            if (isValid)
            {
                //Work around TFS bug #825815, in some cases, XamlMember.IsNameValid returns true but it's not valid Xml Name.
                try
                {
                    XmlConvert.VerifyName(name);
                }
                catch (XmlException)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        void OnIdentifierNameChanged()
        {
            string trimedName = this.IdentifierName;
            if (this.CheckAgainstXaml && !VBIdentifierName.IsValidXamlName(trimedName))
            {
                this.IsValid = false;
                this.ErrorMessage = string.Format(CultureInfo.CurrentUICulture, SR.InvalidXamlMemberName, trimedName);
            }
            else if (!this.VBProvider.IsValidIdentifier(trimedName) || !this.CSProvider.IsValidIdentifier(trimedName))
            {
                this.IsValid = false;
                this.ErrorMessage = string.Format(CultureInfo.CurrentUICulture, SR.InvalidIdentifier, trimedName);
            }
            else if (trimedName.StartsWith("[", StringComparison.Ordinal) && trimedName.EndsWith("]", StringComparison.Ordinal))
            {
                this.IsValid = false;
                this.ErrorMessage = string.Format(CultureInfo.CurrentUICulture, SR.InvalidIdentifier, trimedName);
            }
            else
            {
                this.IsValid = true;
                this.ErrorMessage = string.Empty;
            }
        }
    }
}
