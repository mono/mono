//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Input;

    partial class SendContentDialog : WorkflowElementDialog
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ContentDialogViewModel<SendMessageContent, SendParametersContent>), typeof(SendContentDialog));

        SendContentDialog()
        {
            InitializeComponent();
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "This values must be set before this constructor complete to ensure ShowOkCancel() can consume that immediately")]
        SendContentDialog(ModelItem activity, EditingContext context, DependencyObject owner)
            : this()
        {
            this.ModelItem = activity;
            this.Context = context;
            this.HelpKeyword = HelpKeywords.MessageContentDialog;
            this.Owner = owner;
            this.ViewModel = new ContentDialogViewModel<SendMessageContent, SendParametersContent>(this.ModelItem);
            if (!this.Context.Items.GetValue<ReadOnlyState>().IsReadOnly)
            {
                this.OnOk = this.ViewModel.OnOk;
            }
        }

        public ContentDialogViewModel<SendMessageContent, SendParametersContent> ViewModel
        {
            get { return (ContentDialogViewModel<SendMessageContent, SendParametersContent>)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static bool ShowDialog(ModelItem activity, EditingContext context, DependencyObject owner)
        {
            return new SendContentDialog(activity, context, owner).ShowOkCancel();
        }

        void OnDynamicArgumentDesignerLoaded(object sender, RoutedEventArgs args)
        {
            ((DynamicArgumentDesigner)sender).ParentDialog = this;
        }

        void OnExpressionTextBoxLoaded(object sender, RoutedEventArgs args)
        {
            ((ExpressionTextBox)sender).IsIndependentExpression = true;
        }
    }
}
