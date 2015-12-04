//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Windows;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Runtime;

    public sealed class DynamicArgumentDialog : WorkflowElementDialog
    {
        bool isDictionary;
        Type underlyingArgumentType;
        ModelItem data;

        DynamicArgumentDialog(ModelItem activity, ModelItem data, EditingContext context, DependencyObject owner, DynamicArgumentDesignerOptions options)
        {
            this.MinHeight = 200;
            this.MinWidth = 700;
            this.WindowSizeToContent = SizeToContent.Manual;
            this.ModelItem = activity;
            this.Context = context;
            this.HelpKeyword = HelpKeywords.DynamicArgumentDialog;
            this.Owner = owner;
            this.Title = options.Title;
            this.data = data;
            this.Content = new DynamicArgumentDesigner()
            {
                DynamicArguments = DynamicArgumentDesigner.ModelItemToWrapperCollection(data, out isDictionary, out underlyingArgumentType),
                IsDictionary = isDictionary,
                UnderlyingArgumentType = underlyingArgumentType,
                Context = context,
                OwnerActivity = activity,
                HideDirection = options.HideDirection,
                ArgumentPrefix = options.ArgumentPrefix,
                HintText = options.HintText,
                ParentDialog = this,
            };
        }

        public static bool ShowDialog(ModelItem activity, ModelItem data, EditingContext context, DependencyObject owner, DynamicArgumentDesignerOptions options)
        {
            return new DynamicArgumentDialog(activity, data, context, owner, options).ShowOkCancel();
        }

        protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
        {
            if (dialogResult.Value)
            {
                DynamicArgumentDesigner.WrapperCollectionToModelItem((this.Content as DynamicArgumentDesigner).DynamicArguments, data, isDictionary, underlyingArgumentType);
            }
        }
    }
}
