//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Runtime;

    internal static class EditingContextUtilities
    {
        public static bool IsReadOnly(EditingContext editingContext)
        {
            Fx.Assert(editingContext != null, "editingContext should not be null");

            return editingContext.Items.GetValue<ReadOnlyState>().IsReadOnly;
        }

        public static ModelItem GetSingleSelectedModelItem(EditingContext editingContext)
        {
            Fx.Assert(editingContext != null, "editingContext should not be null");

            Selection selection = editingContext.Items.GetValue<Selection>();
            if (selection.SelectionCount == 1)
            {
                return selection.PrimarySelection;
            }
            else
            {
                return null;
            }
        }
    }
}
