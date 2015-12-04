//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Runtime;
    using System.Collections.Generic;
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation.View;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.ObjectModel;
    using System.Globalization;

    sealed class FlowSwitchCaseEditorDialog : WorkflowElementDialog
    {
        static DependencyProperty caseProperty = DependencyProperty.Register("Case", typeof(object), typeof(FlowSwitchCaseEditorDialog), new UIPropertyMetadata(null));
        Type genericType;
        CaseKeyBox caseKeyBox;

        public FlowSwitchCaseEditorDialog(ModelItem activity, EditingContext context, DependencyObject owner, string title, Type genericType)
        {
            this.WindowSizeToContent = SizeToContent.Manual;
            this.ModelItem = activity;
            this.Context = context;
            this.Owner = owner;
            this.Title = title;
            this.genericType = genericType;
            this.WindowResizeMode = ResizeMode.NoResize;
            this.MinWidth = 300;
            this.MaxWidth = 300;
            this.MinHeight = 120;
            this.MaxHeight = 120;

            caseKeyBox = new CaseKeyBox()
            {
                DisplayHintText = true,
                Visibility = Visibility.Visible,
                ValueType = genericType,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Width = Double.NaN,
                CommitExplicitly = true,
                LabelText = StringResourceDictionary.Instance.GetString("addCaseLabel"),
            };
            caseKeyBox.ViewModel.DataTemplateName = CaseKeyBoxViewModel.BoxesTemplate;
            caseKeyBox.ViewModel.IsBoxOnly = true;
            caseKeyBox.SetBinding(CaseKeyBox.ValueProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(FlowSwitchCaseEditorDialog.caseProperty),
                Mode = BindingMode.TwoWay
            });
            caseKeyBox.CaseKeyValidationCallback = this.ValidateCaseKey;
            caseKeyBox.ValueCommitted += (sender, e) =>
                {
                    this.CloseDialog(true);
                };
            caseKeyBox.EditCancelled += (sender, e) =>
                {
                    this.CloseDialog(false);
                };

            this.Content = caseKeyBox;

            this.OnOk = () =>
                {
                    caseKeyBox.CommitChanges();
                    return false; // ValueCommitted event handler will handle CloseDialog
                };
        }

        bool ValidateCaseKey(object obj, out string reason)
        {
            return GenericFlowSwitchHelper.ValidateCaseKey(obj,
                this.ModelItem.Properties["Cases"],
                this.genericType,
                out reason);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                caseKeyBox.CommitChanges();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                caseKeyBox.CancelChanges();
                e.Handled = true;
            }
        }

        public string CaseName
        {
            get
            {
                return GenericFlowSwitchHelper.GetString(GetValue(caseProperty), this.genericType);
            }
        }

        public object Case
        {
            get
            {
                return GetValue(caseProperty);
            }
        }
    }
}
