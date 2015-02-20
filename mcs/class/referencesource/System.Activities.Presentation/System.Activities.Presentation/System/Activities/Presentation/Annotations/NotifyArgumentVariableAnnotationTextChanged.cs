//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;

    // This is to workaround a bug that updating ModelItem from outside of ArgumentDesigner/VariableDesigner will not update it.
    internal class NotifyArgumentVariableAnnotationTextChanged : Change
    {
        public ArgumentDesigner ArgumentDesigner { get; set; }

        public VariableDesigner VariableDesigner { get; set; }

        public override string Description
        {
            get { return SR.NotifyAnnotationTextChangedDescription; }
        }

        public override bool Apply()
        {
            this.VariableDesigner.Dispatcher.BeginInvoke(
                new Action(() =>
                    {
                        this.VariableDesigner.NotifyAnnotationTextChanged();
                        this.ArgumentDesigner.NotifyAnnotationTextChanged();
                    }),
                Windows.Threading.DispatcherPriority.ApplicationIdle,
                null);
            return true;
        }

        public override Change GetInverse()
        {
            return new NotifyArgumentVariableAnnotationTextChanged()
            {
                VariableDesigner = this.VariableDesigner,
                ArgumentDesigner = this.ArgumentDesigner,
            };
        }
    }
}
