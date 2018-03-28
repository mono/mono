//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Linq;
    using System.ComponentModel;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Activities.Presentation.Model;

    class CaseKeyBoxViewModel : DependencyObject
    {
        static readonly string Null = "(null)";
        static readonly string Empty = "(empty)";

        public static readonly DependencyProperty ComboBoxIsEditableProperty =
            DependencyProperty.Register("ComboBoxIsEditable", typeof(bool), typeof(CaseKeyBoxViewModel), new UIPropertyMetadata(false));

        public static readonly DependencyProperty ComboBoxVisibilityProperty =
            DependencyProperty.Register("ComboBoxVisibility", typeof(Visibility), typeof(CaseKeyBoxViewModel), new UIPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ComboBoxItemsProperty =
            DependencyProperty.Register("ComboBoxItems", typeof(ObservableCollection<string>), typeof(CaseKeyBoxViewModel));

        public static readonly DependencyProperty DataTemplateNameProperty =
            DependencyProperty.Register("DataTemplateName", typeof(string), typeof(CaseKeyBoxViewModel), new UIPropertyMetadata("Label"));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CaseKeyBoxViewModel), new UIPropertyMetadata(String.Empty));

        public static readonly DependencyProperty TextBoxVisibilityProperty =
            DependencyProperty.Register("TextBoxVisibility", typeof(Visibility), typeof(CaseKeyBoxViewModel), new UIPropertyMetadata(Visibility.Visible));

        public const string BoxesTemplate = "Boxes";
        public const string LabelTemplate = "Label";

        string oldText = String.Empty;

        public CaseKeyBoxViewModel(ICaseKeyBoxView view)
        {
            this.View = view;
        }

        public bool ComboBoxIsEditable
        {
            get { return (bool)GetValue(ComboBoxIsEditableProperty); }
            set { SetValue(ComboBoxIsEditableProperty, value); }
        }

        public ObservableCollection<string> ComboBoxItems
        {
            get { return (ObservableCollection<string>)GetValue(ComboBoxItemsProperty); }
            set { SetValue(ComboBoxItemsProperty, value); }
        }

        public Visibility ComboBoxVisibility
        {
            get { return (Visibility)GetValue(ComboBoxVisibilityProperty); }
            set { SetValue(ComboBoxVisibilityProperty, value); }
        }

        public string DataTemplateName
        {
            get { return (string)GetValue(DataTemplateNameProperty); }
            set { SetValue(DataTemplateNameProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Visibility TextBoxVisibility
        {
            get { return (Visibility)GetValue(TextBoxVisibilityProperty); }
            set { SetValue(TextBoxVisibilityProperty, value); }
        }

        public bool IsBoxOnly
        {
            get;
            set;
        }

        public bool OnEnterPressed()
        {
            return this.CommitChanges();
        }

        public void OnEscapePressed()
        {
            this.Text = oldText;
            if (!this.IsBoxOnly)
            {
                this.DataTemplateName = CaseKeyBoxViewModel.LabelTemplate;
            }
            this.View.OnEditCancelled();
        }

        public void OnLabelGotFocus()
        {
            this.DataTemplateName = CaseKeyBoxViewModel.BoxesTemplate;
        }

        public bool OnLostFocus()
        {
            return CommitChanges();
        }

        public void OnValueChanged()
        {
            if (this.Value is ModelItem)
            {
                // Since Value is a DP, this code will trigger OnValueChanged once more.
                this.Value = ((ModelItem)this.Value).GetCurrentValue();
                return;
            }

            if (this.DataTemplateName != LabelTemplate && !this.IsBoxOnly)
            {
                this.DataTemplateName = LabelTemplate;
            }

            if (this.DisplayHintText)
            {
                this.Text = string.Empty;
                return;
            }
            if (this.ValueType == null)
            {
                return;
            }
            if (this.ValueType.IsValueType)
            {
                if (this.Value == null)
                {
                    this.Value = Activator.CreateInstance(this.ValueType);
                }
            }
            if (this.Value == null)
            {
                this.Text = Null;
            }
            else if ((this.ValueType == typeof(string)) && string.Equals(this.Value, String.Empty))
            {
                this.Text = Empty;
            }
            else
            {
                TypeConverter converter = XamlUtilities.GetConverter(this.ValueType);
                Fx.Assert(converter != null, "TypeConverter is not available");
                try
                {
                    this.Text = converter.ConvertToString(this.Value);
                }
                catch (ArgumentException)
                {
                    this.Text = this.Value.ToString();
                }
            }
        }

        public void OnValueTypeChanged()
        {
            if (this.ValueType == null)
            {
                return;
            }
            bool isBool = this.ValueType == typeof(bool);
            bool isEnum = this.ValueType.IsEnum;
            if (isBool || isEnum)
            {
                this.ComboBoxVisibility = Visibility.Visible;
                this.TextBoxVisibility = Visibility.Collapsed;
                this.ComboBoxIsEditable = false;
                if (isBool)
                {
                    this.ComboBoxItems = new ObservableCollection<string> { "True", "False" };
                }
                else
                {
                    this.ComboBoxItems = new ObservableCollection<string>(Enum.GetNames(this.ValueType).ToList());
                }
            }
            else if (this.ValueType.IsValueType)
            {
                this.ComboBoxVisibility = Visibility.Collapsed;
                this.TextBoxVisibility = Visibility.Visible;
                this.ComboBoxIsEditable = false;
            }
            else
            {
                this.ComboBoxVisibility = Visibility.Visible;
                this.TextBoxVisibility = Visibility.Collapsed;
                this.ComboBoxIsEditable = true;
                this.ComboBoxItems = new ObservableCollection<string> { Null };
                if (this.ValueType == typeof(string))
                {
                    this.ComboBoxItems.Add(Empty);
                }
            }
            OnValueChanged();
        }

        public void SaveOldText()
        {
            this.oldText = this.Text;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "If conversion fails, the exception type is System.Exception.So we must catch all types of exceptions here.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Catch all exceptions to prevent crash.")]
        bool CommitChanges()
        {
            object result = null;

            try
            {
                result = ResolveInputText();
            }
            catch
            {
                // ---- all
                Fx.Assert(false, "Result should have been valid. Preview event handler should have handled the validation.");
                return false;
            }

            this.Value = result;
            if (this.DataTemplateName != CaseKeyBoxViewModel.LabelTemplate && !this.IsBoxOnly)
            {
                // this is for the case when setting this.Value to null. It looks like
                // OnValueChanged won't get called because NULL is a default value for
                // the CaseKeyBox instance in SwitchDesigner.
                this.DataTemplateName = CaseKeyBoxViewModel.LabelTemplate;
            }
            this.View.OnValueCommitted();

            return true;
        }

        object ResolveInputText()
        {
            object result = null;
            if (this.ValueType == typeof(string))
            {
                if (this.Text.Equals(Null))
                {
                    result = null;
                }
                else if (this.Text.Equals(Empty))
                {
                    result = string.Empty;
                }
                else
                {
                    result = this.Text;
                }
            }
            else if (!this.ValueType.IsValueType && this.Text.Equals(Null))
            {
                result = null;
            }
            else
            {
                TypeConverter converter = XamlUtilities.GetConverter(this.ValueType);
                Fx.Assert(converter != null, "TypeConverter is not available");

                if (!converter.CanConvertFrom(typeof(string)) || !converter.CanConvertTo(typeof(string)))
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.NotSupportedCaseKeyStringConversion));
                }

                result = converter.ConvertFromString(this.Text);
                // See if the result can be converted back to a string.
                // For example, we have a enum Color {Black, White}.
                // String "3" can be converted to integer 3, but integer 3
                // cannot be converted back to a valid string for enum Color.
                // In this case, we disallow string "3".
                converter.ConvertToString(result);
            }

            string reason;
            if (this.CaseKeyValidationCallback != null && !this.CaseKeyValidationCallback(result, out reason))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(reason));
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "If conversion fails, the exception type is System.Exception.So we must catch all types of exceptions here.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Catch all exceptions to prevent crash.")]
        public bool CanResolveInputText(out string reason)
        {
            reason = string.Empty;
            try
            {
                ResolveInputText();
                return true;
            }
            catch (Exception e)
            {
                reason = e.Message;
                return false;
            }
        }

        public bool TextHasBeenChanged()
        {
            string normalizedOldText = this.oldText;
            string normalizedNewText = this.Text;
            
            // Tricky: this.DisplayHintText = false => This CaseKeyBox is in CaseDesigner
            // Here, when changing value of string value type from "(empty)" to "", we must
            // consider the text hasn't been changed, such that we don't do commit-change.
            // We normalize the strings for empty-string situation before we do comparison.
            if (this.ValueType == typeof(string) && !this.DisplayHintText)
            {
                normalizedOldText = normalizedOldText == Empty ? string.Empty : normalizedOldText;
                normalizedNewText = normalizedNewText == Empty ? string.Empty : normalizedNewText;
            }

            return normalizedOldText != normalizedNewText;
        }

        ICaseKeyBoxView View { get; set; }

        bool DisplayHintText
        {
            get { return this.View.DisplayHintText; }
        }
        
        object Value
        {
            get { return this.View.Value; }
            set { this.View.Value = value; }
        }

        Type ValueType
        {
            get { return this.View.ValueType; }
        }

        CaseKeyValidationCallbackDelegate CaseKeyValidationCallback
        {
            get { return this.View.CaseKeyValidationCallback;  }
        }

        public void ResetText()
        {
            this.Text = string.Empty;
            this.oldText = string.Empty;
        }
    }
}
