//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Activities.Debugger;
    using System.Activities.Expressions;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Xaml;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Microsoft.Activities.Presentation;

    // The main class for search. This class will walkthrough the model item tree to build a TextImage.
    // And it will access ObjectToSourceLocationMapping with a specific SourceLocation to get a ModelItem
    // and highlight.
    class ModelSearchServiceImpl : ModelSearchService
    {
        const int StartIndexUnchangeMark = -1;
        const string DisplayNamePropertyName = "DisplayName";
        EditingContext editingContext;
        ModelService modelService;
        WorkflowDesigner designer;
        List<SearchableEntry> entries = new List<SearchableEntry>();
        Dictionary<int, SearchableEntry> textImageIndexEntryMapping = new Dictionary<int, SearchableEntry>();
        TextImage textImage;
        HashSet<Object> alreadyVisitedObjects = new HashSet<Object>();
        HashSet<object> objectsOnDesinger = new HashSet<object>();
        int index;
        ModelItem lastNavigatedItem;
        bool isModelTreeChanged;
        ModelItem itemToFocus;
        AdornerLayer adornerLayer;
        SearchToolTipAdorner toolTipAdorner;
        WorkflowViewElement lastWorkflowViewElement;

        public ModelSearchServiceImpl(WorkflowDesigner designer)
        {
            if (designer == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("designer"));
            }
            this.designer = designer;
            this.editingContext = this.designer.Context;

            this.editingContext.Services.Subscribe<ModelService>(new SubscribeServiceCallback<ModelService>(this.OnModelServiceAvailable));
            this.editingContext.Services.Subscribe<DesignerView>(new SubscribeServiceCallback<DesignerView>(this.OnDesignerViewAvailable));
            this.editingContext.Services.Subscribe<ModelTreeManager>(new SubscribeServiceCallback<ModelTreeManager>(this.OnModelTreeManagerAvailable));
            this.editingContext.Items.Subscribe<Selection>(this.OnSelectionChanged);

            // At the first time, we should generate the TextImage.
            this.isModelTreeChanged = true;
        }

        void OnEditingScopeCompleted(object sender, EditingScopeEventArgs e)
        {
            this.isModelTreeChanged = true;
        }

        void OnSelectionChanged(Selection selection)
        {
            if (selection.PrimarySelection != this.lastNavigatedItem)
            {
                this.isModelTreeChanged = true;
            }
        }

        // Listen to the mouse down in designer and close tooltip.
        void OnDesignerSurfaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveToolTipAdorner();
        }

        bool ShouldIgnore(ModelProperty property)
        {
            // Since we have searched each variable. We can strip out "Variables" property here.
            // It's valid to hardcode "Variables" property. That's the way how variable designer get variables.
            // We should strip out 'DisplayName', since it is searched at the beginning.
            // We strip out 'Id', since it's a property from the Activity Base class, but never used in design time. 
            return string.Equals(property.Name, "Variables", StringComparison.Ordinal)
                        || string.Equals(property.Name, DisplayNamePropertyName, StringComparison.Ordinal)
                        || string.Equals(property.Name, "Id", StringComparison.Ordinal);
        }

        public override TextImage GenerateTextImage()
        {
            RemoveToolTipAdorner();

            // If the modelitem tree was not changed since last time we generated the text image,
            // return the original TextImage and set the StartIndex to StartIndexUnchangeMark
            // means VS should use their own index.
            if (!this.isModelTreeChanged)
            {
                textImage.StartLineIndex = StartIndexUnchangeMark;
                return textImage;
            }
            this.entries.Clear();
            this.textImageIndexEntryMapping.Clear();
            this.index = 0;
            IEnumerable<ModelItem> itemsToSearch = this.GetItemsOnDesigner(preOrder: true, excludeRoot: true, excludeErrorActivity: true, excludeExpression: true, includeOtherObjects: false);
            foreach (ModelItem item in itemsToSearch)
            {
                this.objectsOnDesinger.Add(item.GetCurrentValue());
            }

            Selection selection = this.editingContext.Items.GetValue<Selection>();
            int startIndex = StartIndexUnchangeMark;

            // If and only if root is selected, start search from the beginning.
            if (selection.SelectionCount == 1 && selection.PrimarySelection == modelService.Root)
            {
                startIndex = 0;
            }

            AddEntriesForArguments(selection, ref startIndex);
            foreach (ModelItem modelItem in itemsToSearch)
            {
                // Do this check to make sure we start from the topmost selected item.
                if (startIndex == StartIndexUnchangeMark)
                {
                    if (selection.SelectedObjects.Contains(modelItem) && modelItem != this.lastNavigatedItem)
                    {
                        // set the search start index to the next location of the current focus.
                        startIndex = index;
                    }
                }

                // Add the DisplayName property first.
                ModelProperty displayNameProperty = modelItem.Properties[DisplayNamePropertyName];
                if (displayNameProperty != null)
                {
                    AddEntriesForProperty(displayNameProperty, modelItem, null);
                }
                foreach (ModelProperty modelProperty in modelItem.Properties)
                {
                    if (!ShouldIgnore(modelProperty))
                    {
                        AddEntriesForProperty(modelProperty, modelItem, null);
                    }
                }
                AddEntriesForVariables(modelItem);
            }

            AddBrowsableProperties(this.modelService.Root);

            List<string> searchableTexts = new List<string>();
            int textImageIndex = 0;
            foreach (SearchableEntry entry in entries)
            {
                string text = entry.Text;
                if (text == null)
                {
                    text = string.Empty;
                }

                foreach (string line in text.Split(new string[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    this.textImageIndexEntryMapping.Add(textImageIndex, entry);
                    searchableTexts.Add(line);
                    textImageIndex++;
                }
            }

            textImage = new TextImage()
            {
                StartLineIndex = startIndex,
                Lines = searchableTexts
            };

            this.isModelTreeChanged = false;
            return textImage;
        }

        private void OnModelServiceAvailable(ModelService modelService)
        {
            this.modelService = modelService;
        }

        private void OnDesignerViewAvailable(DesignerView designerView)
        {
            designerView.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnDesignerSurfaceMouseLeftButtonDown), true);
        }

        private void OnModelTreeManagerAvailable(ModelTreeManager modelTreeManager)
        {
            modelTreeManager.EditingScopeCompleted += new EventHandler<EditingScopeEventArgs>(OnEditingScopeCompleted);
        }

        private void RemoveToolTipAdorner()
        {
            if (this.toolTipAdorner != null)
            {
                // remove the adorner on the previous hit item.
                this.adornerLayer.Remove(this.toolTipAdorner);
                this.toolTipAdorner = null;
                this.lastWorkflowViewElement.CustomItemStatus = null;
            }
        }

        internal IEnumerable<ModelItem> GetItemsOnDesigner(bool preOrder, bool excludeRoot, bool excludeErrorActivity, bool excludeExpression, bool includeOtherObjects)
        {
            WorkflowViewService viewService = this.WorkflowViewService;
            IList<ModelItem> items =
                ModelTreeManager.DepthFirstSearch(modelService.Root,
                delegate(Type type)
                {
                    // Only find items on the designer surface.
                    return includeOtherObjects || (typeof(WorkflowViewElement).IsAssignableFrom(viewService.GetDesignerType(type)));
                },
                delegate(ModelItem modelItem)
                {
                    return !(excludeExpression && modelItem != null && typeof(ITextExpression).IsAssignableFrom(modelItem.ItemType));
                },
                preOrder);

            // ModelItemKeyValuePair is associated with CaseDesigner. 
            // So ModelItemKeyValuePair will be returned even if they are not really Cases.
            // Those ModelItemKeyValuePairs need to be excluded.
            IEnumerable<ModelItem> itemsToSearch = null;
            if (!excludeErrorActivity)
            {
                itemsToSearch = items.Where<ModelItem>(item => !ModelUtilities.IsModelItemKeyValuePair(item.ItemType)
                            || ModelUtilities.IsSwitchCase(item));
            }
            else
            {
                itemsToSearch = items.Where<ModelItem>(item =>
                    (!ModelUtilities.IsModelItemKeyValuePair(item.ItemType) || ModelUtilities.IsSwitchCase(item))
                    && !IsErrorActivity(item));
            }
            if (excludeRoot)
            {
                itemsToSearch = itemsToSearch.Except<ModelItem>(new ModelItem[] { modelService.Root });
            }
            return itemsToSearch;
        }

        static private bool IsErrorActivity(ModelItem item)
        {
            Type type = item.ItemType;
            if (type.IsGenericType)
            {
                return (typeof(ErrorActivity<>) == type.GetGenericTypeDefinition());
            }

            return (type == typeof(ErrorActivity));
        }

        internal static string ExpressionToString(object expression)
        {
            ITextExpression expr = expression as ITextExpression;
            return (expr != null) ? expr.ExpressionText : expression.ToString();
        }

        SearchableEntry CreateSearchableEntry(SearchableEntryOption entryType,
            ModelItem item, ModelProperty property, string text, string propertyPath)
        {
            return new SearchableEntry()
            {
                LineNumber = index++,
                SearchableEntryType = entryType,
                ModelItem = item,
                ModelProperty = property,
                Text = text,
                PropertyPath = propertyPath
            };
        }

        void AddEntriesForVariables(ModelItem modelItem)
        {
            ModelItemCollection variables = VariableHelper.GetVariableCollection(modelItem);
            if (variables != null)
            {
                foreach (ModelItem variable in variables)
                {
                    entries.Add(CreateSearchableEntry(SearchableEntryOption.Variable, variable, null,
                        TypeNameHelper.GetDisplayName(variable.Properties[DesignTimeVariable.VariableTypeProperty].ComputedValue as Type, false), null));

                    entries.Add(CreateSearchableEntry(SearchableEntryOption.Variable, variable, null,
                        variable.Properties[DesignTimeVariable.VariableNameProperty].ComputedValue.ToString(), null));

                    object propertyValue = variable.Properties[DesignTimeVariable.VariableDefaultProperty].ComputedValue;

                    if (propertyValue != null)
                    {
                        entries.Add(CreateSearchableEntry(SearchableEntryOption.Variable, variable, null,
                            ExpressionToString(propertyValue), null));
                    }

                    if (this.editingContext.Services.GetService<DesignerConfigurationService>().AnnotationEnabled)
                    {
                        string annotationText = (string)variable.Properties[Annotation.AnnotationTextPropertyName].ComputedValue;
                        if (!string.IsNullOrEmpty(annotationText))
                        {
                            entries.Add(CreateSearchableEntry(SearchableEntryOption.Variable, variable, null, annotationText, null));
                        }
                    }
                }
            }
        }

        private void AddEntriesForPropertyReference(string valueText, ModelItem modelItem,
            ModelProperty property, SearchableEntryOption entryType, string propertyPath)
        {
            entries.Add(CreateSearchableEntry(entryType, modelItem, property, valueText, propertyPath));
        }

        private void AddEntriesForPropertyValue(object value, ModelItem modelItem,
            ModelProperty property, SearchableEntryOption entryType, string propertyPath)
        {
            // be ready for recursively visit all sub properties.
            alreadyVisitedObjects.Clear();
            IList<string> texts = GetSearchableStrings(value);
            if (texts != null)
            {
                foreach (string valueText in texts)
                {
                    entries.Add(CreateSearchableEntry(entryType, modelItem, property, valueText, propertyPath));
                }
            }
        }

        void AddBrowsableProperties(ModelItem modelItem)
        {
            foreach (ModelProperty property in modelItem.Properties)
            {
                if (property.IsBrowsable)
                {
                    this.AddEntriesForProperty(property, modelItem, null);
                }
            }
        }


        void AddEntriesForArguments(Selection selection, ref int startIndex)
        {
            ModelProperty argumentsProperty = this.modelService.Root.Properties["Properties"];
            if (argumentsProperty == null)
            {
                return;
            }
            ModelItemCollection arguments = argumentsProperty.Collection;
            if (arguments != null)
            {
                ModelItem selectedArgument = this.GetTopmostSelectedArgument(selection, arguments);
                foreach (ModelItem argument in arguments)
                {
                    // Do this check to make sure we start from the topmost selected item.
                    if (startIndex == StartIndexUnchangeMark && argument == selectedArgument && argument != lastNavigatedItem)
                    {
                        startIndex = index;
                    }
                    entries.Add(CreateSearchableEntry(SearchableEntryOption.Argument, argument, null,
                        TypeNameHelper.GetDisplayName(argument.Properties["Type"].ComputedValue as Type, false), null));

                    entries.Add(CreateSearchableEntry(SearchableEntryOption.Argument, argument, null,
                        argument.Properties[DesignTimeArgument.ArgumentNameProperty].ComputedValue.ToString(), null));

                    IList<string> argumentValues = GetSearchableStrings(argument.Properties[DesignTimeArgument.ArgumentDefaultValueProperty].ComputedValue);
                    if (argumentValues.Count == 1)
                    {
                        AddEntriesForPropertyValue(argumentValues[0],
                            argument, null, SearchableEntryOption.Argument, null);
                    }

                    if (this.editingContext.Services.GetService<DesignerConfigurationService>().AnnotationEnabled)
                    {
                        string annotationText = (string)argument.Properties[Annotation.AnnotationTextPropertyName].ComputedValue;
                        if (!string.IsNullOrEmpty(annotationText))
                        {
                            entries.Add(CreateSearchableEntry(SearchableEntryOption.Argument, argument, null, annotationText, null));
                        }
                    }
                }
            }
        }

        private ModelItem GetTopmostSelectedArgument(Selection selection, ModelItemCollection arguments)
        {
            foreach (ModelItem argument in arguments)
            {
                foreach (ModelItem candidateArgument in selection.SelectedObjects)
                {
                    if (candidateArgument.ItemType == typeof(DesignTimeArgument))
                    {
                        // since for arguments, the selection is not the modelitem, it is the fakemodelitem, we cannot do a
                        // simple reference comparing to find the selected argument.
                        DesignTimeArgument designTimeArgument = candidateArgument.GetCurrentValue() as DesignTimeArgument;
                        if (designTimeArgument.ReflectedObject == argument)
                        {
                            return argument;
                        }
                    }
                }
            }
            return null;
        }

        IList<string> GetSearchableStrings(object computedValue)
        {
            List<string> results = new List<string>();
            if (computedValue == null || this.objectsOnDesinger.Contains(computedValue))
            {
                return results;
            }

            Type type = computedValue.GetType();
            if (type.IsPrimitive || computedValue is string || type.IsEnum || computedValue is Uri)
            {
                return new List<string>() { computedValue.ToString() };
            }

            SearchableStringConverterAttribute attribute =
                ExtensibilityAccessor.GetAttribute<SearchableStringConverterAttribute>(type);

            if (attribute == null)
            {
                // try its generic type.
                if (type.IsGenericType)
                {
                    Type generictype = type.GetGenericTypeDefinition();
                    attribute = ExtensibilityAccessor.GetAttribute<SearchableStringConverterAttribute>(generictype);
                }
            }

            if (attribute != null)
            {
                Type converterType = Type.GetType(attribute.ConverterTypeName);
                if (converterType.IsGenericTypeDefinition)
                {
                    converterType = converterType.MakeGenericType(computedValue.GetType().GetGenericArguments());
                }
                SearchableStringConverter converter = Activator.CreateInstance(converterType) as SearchableStringConverter;
                return converter.Convert(computedValue);
            }

            // don't have an direct converter? and is a collection, then let's try convert each member.
            if (computedValue is IEnumerable)
            {
                foreach (object value in computedValue as IEnumerable)
                {
                    results.AddRange(GetSearchableStrings(value));
                }
                return results;
            }

            // Already tried all the options, let's do a recursive search.
            alreadyVisitedObjects.Add(computedValue);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(computedValue, null);
                if (!alreadyVisitedObjects.Contains(propertyValue))
                {
                    results.AddRange(GetSearchableStrings(propertyValue));
                }
            }
            return results;
        }

        void AddEntriesForProperty(ModelProperty property, ModelItem modelItem, string propertyPath)
        {
            if (!string.IsNullOrEmpty(propertyPath))
            {
                propertyPath += ",";
                propertyPath += property.Name;
            }
            else
            {
                propertyPath = property.Name;
            }

            entries.Add(CreateSearchableEntry(
                SearchableEntryOption.Property, modelItem, property, TypeNameHelper.GetDisplayName(property.PropertyType, false), propertyPath));

            entries.Add(CreateSearchableEntry(
                SearchableEntryOption.Property, modelItem, property, property.Name, propertyPath));

            if (property.ComputedValue != null)
            {
                PropertyValueEditor propertyValueEditor = null;
                try
                {
                    propertyValueEditor = ExtensibilityAccessor.GetSubPropertyEditor(property);
                }
                catch (TargetInvocationException exception)
                {
                    // To workaround 181412.If the current property's property type is a generic type and the activity 
                    // is also a generic type Calling to ExtensibilityAccessor.GetSubPropertyEditor will get this exception. 
                    if (exception.InnerException is ArgumentException)
                    {
                        propertyValueEditor = null;
                    }
                }
                if (propertyValueEditor != null)
                {
                    IList<ModelProperty> properties = ExtensibilityAccessor.GetSubProperties(property);
                    foreach (ModelProperty propertyItem in properties)
                    {
                        AddEntriesForProperty(propertyItem, modelItem, propertyPath);
                    }
                }
                else
                {
                    // We don't search the value of an expandable property.
                    AddEntriesForPropertyValue(property.ComputedValue, modelItem, property, SearchableEntryOption.Property, propertyPath);
                }
            }
            else if (property.Reference != null)
            {
                AddEntriesForPropertyReference(property.Reference, modelItem, property, SearchableEntryOption.Property, propertyPath);
            }
        }

        public ModelItem FindModelItem(int startLine, int startColumn, int endLine, int endColumn)
        {
            SourceLocation sourceLocation = new SourceLocation(/* fileName = */ null, startLine, startColumn, endLine, endColumn);
            return designer.ObjectToSourceLocationMapping.FindModelItem(sourceLocation);
        }

        public ModelItem FindModelItemOfViewState(int startLine, int startColumn, int endLine, int endColumn)
        {
            SourceLocation sourceLocation = new SourceLocation(/* fileName = */ null, startLine, startColumn, endLine, endColumn);
            return designer.ObjectToSourceLocationMapping.FindModelItemOfViewState(sourceLocation);
        }

        public SourceLocation FindSourceLocation(ModelItem modelItem)
        {
            return designer.ObjectToSourceLocationMapping.FindSourceLocation(modelItem);
        }

        public IEnumerable<object> GetObjectsWithSourceLocation()
        {
            return designer.ObjectToSourceLocationMapping.GetObjectsWithSourceLocation();
        }

        private ModelItem FindModelItemForNavigate(int startLine, int startColumn, int endLine, int endColumn)
        {
            // If we search ModelItem first, we will not have a chance to search ViewState because
            // we will always get an ModelItem, at least the out-most Activity.
            ModelItem modelItem = this.FindModelItemOfViewState(startLine, startColumn, endLine, endColumn);
            if (modelItem != null)
            {
                return modelItem;
            }

            return this.FindModelItem(startLine, startColumn, endLine, endColumn);
        }

        public override bool NavigateTo(int startLine, int startColumn, int endLine, int endColumn)
        {
            ModelItem itemToFocus = this.FindModelItemForNavigate(startLine, startColumn, endLine, endColumn);

            return this.NavigateTo(itemToFocus);
        }

        // Navigate to a ModelItem with the specified location in TextImage. This is for Find Next.
        public override bool NavigateTo(int location)
        {
            if (location < 0 || location >= this.textImageIndexEntryMapping.Count)
            {
                return false;
            }

            SearchableEntry entry = this.textImageIndexEntryMapping[location];
            return NavigateTo(entry);
        }

        public bool NavigateTo(ModelItem itemToFocus)
        {
            if (itemToFocus == null)
            {
                return false;
            }

            SearchableEntry entry = CreateSearchableEntryForArgumentOrVariable(itemToFocus);
            if (entry != null)
            {
                return this.NavigateTo(entry);
            }

            itemToFocus = this.FindModelItemToFocus(itemToFocus);

            itemToFocus.Focus();

            return true;
        }

        private static SearchableEntry CreateSearchableEntryNoRecursive(ModelItem modelItem)
        {
            if (typeof(DynamicActivityProperty).IsAssignableFrom(modelItem.ItemType))
            {
                return new SearchableEntry
                {
                    SearchableEntryType = SearchableEntryOption.Argument,
                    ModelItem = modelItem
                };
            }
            else if (typeof(Variable).IsAssignableFrom(modelItem.ItemType))
            {
                return new SearchableEntry
                {
                    SearchableEntryType = SearchableEntryOption.Variable,
                    ModelItem = modelItem
                };
            }

            return null;
        }

        private static SearchableEntry CreateSearchableEntryForArgumentOrVariable(ModelItem itemToFocus)
        {
            SearchableEntry entry = null;
            ModelUtilities.ReverseTraverse(itemToFocus, (ModelItem modelItem) =>
            {
                entry = CreateSearchableEntryNoRecursive(modelItem);
                return (entry == null);
            });
            return entry;
        }

        private bool NavigateTo(SearchableEntry entry)
        {
            if (entry.SearchableEntryType == SearchableEntryOption.Variable)
            {
                itemToFocus = entry.ModelItem.Parent.Parent;
                HighlightModelItem(itemToFocus);
                this.lastNavigatedItem = itemToFocus;
                var designerView = this.editingContext.Services.GetService<DesignerView>();
                // Open the variable designer.
                designerView.CheckButtonVariables();
                designerView.variables1.SelectVariable(entry.ModelItem);
            }
            else if (entry.SearchableEntryType == SearchableEntryOption.Argument)
            {
                itemToFocus = this.modelService.Root;
                HighlightModelItem(itemToFocus);
                var designerView = this.editingContext.Services.GetService<DesignerView>();
                // Open the argument designer.
                designerView.CheckButtonArguments();
                designerView.arguments1.SelectArgument(entry.ModelItem);
                this.lastNavigatedItem = entry.ModelItem;
            }
            else
            {
                itemToFocus = entry.ModelItem;
                HighlightModelItem(itemToFocus);
                this.lastNavigatedItem = itemToFocus;
                ICommandService commandService = this.editingContext.Services.GetService<ICommandService>();
                if (commandService != null)
                {
                    commandService.ExecuteCommand(CommandValues.ShowProperties, null);
                }

                PropertyInspector propertiesGrid = this.designer.PropertyInspectorView as PropertyInspector;
                propertiesGrid.SelectPropertyByPath(entry.PropertyPath);
                if (ShouldShowSearchToolTip(itemToFocus))
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        WorkflowViewElement viewElement = itemToFocus.View as WorkflowViewElement;
                        if (viewElement != null)
                        {
                            this.adornerLayer = AdornerLayer.GetAdornerLayer(viewElement as WorkflowViewElement);
                            if (this.adornerLayer != null)
                            {
                                DesignerView designerView = this.editingContext.Services.GetService<DesignerView>();
                                string toolTipText = string.Format(CultureInfo.CurrentUICulture, SR.SearchHintText, entry.ModelProperty.Name);
                                this.toolTipAdorner = new SearchToolTipAdorner(viewElement, designerView, toolTipText);

                                viewElement.CustomItemStatus = "SearchToolTip=" + toolTipText;
                                this.lastWorkflowViewElement = viewElement;

                                this.adornerLayer.Add(this.toolTipAdorner);
                            }
                        }
                    }), DispatcherPriority.ApplicationIdle);
                }
            }

            return true;
        }

        private bool ShouldShowSearchToolTip(ModelItem item)
        {
            return !typeof(WorkflowService).IsAssignableFrom(item.ItemType)
                && !typeof(ActivityBuilder).IsAssignableFrom(item.ItemType);
        }

        private void HighlightModelItem(ModelItem itemToFocus)
        {
            DesignerView designerView = this.editingContext.Services.GetService<DesignerView>();
            double width = 0.0, height = 0.0;
            Rect rectToBringIntoView;
            FrameworkElement fe = (FrameworkElement)itemToFocus.View;
            if (fe != null)
            {
                width = Math.Min(fe.RenderSize.Width, designerView.ScrollViewer.ViewportWidth);
                height = Math.Min(fe.RenderSize.Height, designerView.ScrollViewer.ViewportHeight);
                rectToBringIntoView = new Rect(0, 0, width, height);
            }
            else
            {
                rectToBringIntoView = Rect.Empty;
            }
            itemToFocus.Highlight(rectToBringIntoView);
        }

        private ModelItem FindModelItemToFocus(ModelItem itemToFocus)
        {
            WorkflowViewService viewService = this.WorkflowViewService;
            if (viewService == null || itemToFocus == null)
            {
                return itemToFocus;
            }

            ModelUtilities.ReverseTraverse(itemToFocus, (ModelItem modelItem) =>
            {
                if (modelItem == null)
                {
                    // continue;
                    return true;
                }

                // if the item has Designer, we assume it can get focus.
                if (CanFocusOnModelItem(modelItem, viewService))
                {
                    itemToFocus = modelItem;
                    // break;
                    return false;
                }

                // continue
                return true;
            });

            return itemToFocus;
        }

        private WorkflowViewService WorkflowViewService
        {
            get
            {
                return (WorkflowViewService)this.editingContext.Services.GetService<ViewService>();
            }
        }

        private static bool CanFocusOnModelItem(ModelItem itemToFocus, WorkflowViewService viewService)
        {
            Fx.Assert(itemToFocus != null && viewService != null, "null argument");

            if (typeof(ITextExpression).IsAssignableFrom(itemToFocus.ItemType))
            {
                return false;
            }

            Type designerType = viewService.GetDesignerType(itemToFocus.ItemType);
            return typeof(WorkflowViewElement).IsAssignableFrom(designerType);
        }
    }
}
