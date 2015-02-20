//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Linq;

    static class DeleteHelper
    {
        public static bool CanDelete(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            bool canExecute = false;
            Selection selection = context.Items.GetValue<Selection>();
            if (null != selection && selection.SelectionCount > 0)
            {
                DesignerView designerView = context.Services.GetService<DesignerView>();
                canExecute = selection.SelectedObjects.All(
                    p => (null != p.View && p.View is WorkflowViewElement && !p.View.Equals(designerView.RootDesigner)));
            }
            return canExecute;
        }

        public static void Delete(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            Selection selection = context.Items.GetValue<Selection>();
            if (null != selection)
            {
                bool selectRoot = false;

                DesignerView designerView = context.Services.GetService<DesignerView>();
                var toDelete = selection.SelectedObjects.Where(p => null != p.View && p.View is WorkflowViewElement && !p.View.Equals(designerView.RootDesigner));
                if (toDelete.Count() > 0)
                {
                    using (EditingScope es = (EditingScope)toDelete.FirstOrDefault().BeginEdit(SR.DeleteOperationEditingScopeDescription))
                    {
                        Dictionary<ICompositeView, List<ModelItem>> containerToModelItemsDict = new Dictionary<ICompositeView, List<ModelItem>>();
                        List<ModelItem> modelItemsPerContainer;
                        foreach (var item in toDelete)
                        {
                            ICompositeView container = (ICompositeView)DragDropHelper.GetCompositeView((WorkflowViewElement)item.View);
                            if (null != container)
                            {
                                if (!containerToModelItemsDict.TryGetValue(container, out modelItemsPerContainer))
                                {
                                    modelItemsPerContainer = new List<ModelItem>();
                                    containerToModelItemsDict.Add(container, modelItemsPerContainer);
                                }
                                modelItemsPerContainer.Add(item);
                            }
                        }
                        foreach (ICompositeView container in containerToModelItemsDict.Keys)
                        {
                            container.OnItemsDelete(containerToModelItemsDict[container]);
                            selectRoot = true;
                        }

                        if (selectRoot)
                        {
                            DesignerView view = context.Services.GetService<DesignerView>();
                            if (null != view)
                            {
                                WorkflowViewElement rootView = view.RootDesigner as WorkflowViewElement;
                                if (rootView != null)
                                {
                                    Selection.SelectOnly(context, rootView.ModelItem);
                                }
                            }
                        }
                        es.Complete();
                    }
                }
            }
        }
    }
}
