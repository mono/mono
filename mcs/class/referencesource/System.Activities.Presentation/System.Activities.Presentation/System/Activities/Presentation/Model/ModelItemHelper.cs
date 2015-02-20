//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Runtime;

    internal static class ModelItemHelper
    {
        internal static ModelEditingScope ModelItemBeginEdit(ModelTreeManager modelTreeManager, string description, bool shouldApplyChangesImmediately)
        {
            if (shouldApplyChangesImmediately && modelTreeManager.Context.Services.GetService<UndoEngine>().IsBookmarkInPlace)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidNestedModelItemBeginEditExceptionMessage));
            }

            EditingScope editingScope = modelTreeManager.CreateEditingScope(description, shouldApplyChangesImmediately);

            if (shouldApplyChangesImmediately && editingScope == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidNestedModelItemBeginEditExceptionMessage));
            }

            return editingScope;
        }

        internal static bool CanCreateImmediateEditingScope(ModelItem modelItem)
        {
            Fx.Assert(modelItem is IModelTreeItem, "modelItem must implement IModelTreeItem");

            return ((IModelTreeItem)modelItem).ModelTreeManager.CanCreateImmediateEditingScope();
        }

        internal static void TryCreateImmediateEditingScopeAndExecute(EditingContext context, string editingScopeDescription, Action<EditingScope> modelEditingWork)
        {
            Fx.Assert(context != null, "context should not be null.");
            Fx.Assert(modelEditingWork != null, "modelEditingWork should not be null.");

            ModelTreeManager manager = context.Services.GetRequiredService<ModelTreeManager>();

            if (manager.CanCreateImmediateEditingScope())
            {
                using (EditingScope editingScope = manager.CreateEditingScope(editingScopeDescription, true))
                {
                    modelEditingWork(editingScope);
                }
            }
            else
            {
                modelEditingWork(null);
            }
        }

        internal static EditingScope TryCreateImmediateEditingScope(ModelTreeManager manager, string editingScopeDescription)
        {
            if (manager.CanCreateImmediateEditingScope())
            {
                return manager.CreateEditingScope(editingScopeDescription, true);
            }

            return null;
        }
    }
}
