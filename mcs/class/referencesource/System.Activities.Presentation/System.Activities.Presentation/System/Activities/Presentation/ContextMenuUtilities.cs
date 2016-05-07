//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    internal static class ContextMenuUtilities
    {
        public static void OnAddAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext)
        {
            OnAddAnnotationCommandCanExecute(e, editingContext, EditingContextUtilities.GetSingleSelectedModelItem(editingContext));
        }

        public static void OnAddAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext, DataGrid dataGrid)
        {
            OnAddAnnotationCommandCanExecute(e, editingContext, DataGridHelper.GetSingleSelectedObject(dataGrid));
        }

        public static void OnAddAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext, ModelItem modelItem)
        {
            e.Handled = true;

            if (modelItem == null || !DesignerConfigurationServiceUtilities.IsAnnotationEnabled(editingContext) || EditingContextUtilities.IsReadOnly(editingContext))
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = !modelItem.HasAnnotation();
        }

        public static void OnDeleteAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext)
        {
            OnDeleteAnnotationCommandCanExecute(e, editingContext, EditingContextUtilities.GetSingleSelectedModelItem(editingContext));
        }

        public static void OnDeleteAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext, DataGrid dataGrid)
        {
            OnDeleteAnnotationCommandCanExecute(e, editingContext, DataGridHelper.GetSingleSelectedObject(dataGrid));
        }

        public static void OnDeleteAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext, ModelItem modelItem)
        {
            e.Handled = true;

            if (modelItem == null || !DesignerConfigurationServiceUtilities.IsAnnotationEnabled(editingContext) || EditingContextUtilities.IsReadOnly(editingContext))
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = modelItem.HasAnnotation();
        }

        public static void OnDeleteAllAnnotationCommandCanExecute(CanExecuteRoutedEventArgs e, EditingContext editingContext)
        {
            e.Handled = true;

            if (!DesignerConfigurationServiceUtilities.IsAnnotationEnabled(editingContext) || EditingContextUtilities.IsReadOnly(editingContext))
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }
        
        // var/arg designer
        public static void OnDeleteCommandCanExecute(CanExecuteRoutedEventArgs e, DataGrid dataGrid)
        {
            e.Handled = true;

            if (dataGrid == null || dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count == 0)
            {
                e.CanExecute = false;
            }

            if (dataGrid.SelectedItems.Count == 1 && dataGrid.SelectedItems[0] == CollectionView.NewItemPlaceholder)
            {
                e.CanExecute = false;
            }

            e.CanExecute = true;
        }

        public static void OnAddAnnotationCommandExecuted(ExecutedRoutedEventArgs e, ModelItem modelItem)
        {
            ModelProperty property = modelItem.Properties.Find(Annotation.AnnotationTextPropertyName);
            if (property != null)
            {
                using (ModelEditingScope editingScope = modelItem.BeginEdit(SR.AddAnnotationDescription))
                {
                    property.SetValue(string.Empty);
                    ViewStateService viewStateService = modelItem.GetEditingContext().Services.GetService<ViewStateService>();
                    viewStateService.StoreViewStateWithUndo(modelItem, Annotation.IsAnnotationDockedViewStateName, false);
                    editingScope.Complete();
                }

                if (modelItem.View != null)
                {
                    WorkflowViewElement element = modelItem.View as WorkflowViewElement;
                    if (element != null)
                    {
                        element.OnEditAnnotation();
                    }
                }
            }

            e.Handled = true;
        }

        public static void OnEditAnnotationCommandExecuted(ExecutedRoutedEventArgs e, ModelItem modelItem)
        {
            WorkflowViewElement element = modelItem.View as WorkflowViewElement;
            if (element != null)
            {
                element.OnEditAnnotation();
            }

            e.Handled = true;
        }

        public static void OnDeleteAnnotationCommandExecuted(ExecutedRoutedEventArgs e, ModelItem modelItem)
        {
            using (ModelEditingScope editingScope = modelItem.BeginEdit(SR.DeleteAnnotationDescription))
            {
                modelItem.Properties[Annotation.AnnotationTextPropertyName].SetValue(null);
                ViewStateService viewStateService = modelItem.GetEditingContext().Services.GetService<ViewStateService>();
                viewStateService.StoreViewStateWithUndo(modelItem, Annotation.IsAnnotationDockedViewStateName, null);
                editingScope.Complete();
            }

            e.Handled = true;
        }

        public static void OnAnnotationMenuLoaded(EditingContext editingContext, Control control, RoutedEventArgs e)
        {
            if (DesignerConfigurationServiceUtilities.IsAnnotationEnabled(editingContext))
            {
                control.Visibility = Visibility.Visible;
            }
            else
            {
                control.Visibility = Visibility.Collapsed;
            }

            e.Handled = true;
        }
    }
}
