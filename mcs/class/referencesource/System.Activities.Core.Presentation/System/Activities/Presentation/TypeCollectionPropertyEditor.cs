//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Windows;
    using System.Runtime;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;
    using System.Collections;

    sealed class TypeCollectionPropertyEditor : DialogPropertyValueEditor 
    {
        public const string AllowDuplicate = "AllowDuplicate";

        public const string Filter = "Filter";

        public const string DefaultType = "DefaultType";

        public TypeCollectionPropertyEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("TypeCollection_InlineTemplate");
        }

        internal static T GetOptionValueOrUseDefault<T>(IEnumerable attributes, string optionName, T defaultValue)
        {
            object optionValue;

            if (EditorOptionAttribute.TryGetOptionValue(attributes, optionName, out optionValue))
            {
                return (T)optionValue;
            }

            return defaultValue;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ModelPropertyEntryToOwnerActivityConverter propertyEntryConverter = 
                new ModelPropertyEntryToOwnerActivityConverter();

            ModelItem activityModelItem = 
                (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null);

            ModelItem parentModelItem =
                (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), true, null);

            EditingContext context = ((IModelTreeItem)activityModelItem).ModelTreeManager.Context;

            ModelItemCollection inputData = parentModelItem.Properties[propertyValue.ParentProperty.PropertyName].Collection;
            IEnumerable<Type> rawInputData = inputData.GetCurrentValue() as IEnumerable<Type>;
            Fx.Assert(rawInputData != null, "rawInputData is null or is not IEnumerable<Type>.");

            ModelProperty editingProperty = activityModelItem.Properties[propertyValue.ParentProperty.PropertyName];
            bool allowDuplication = GetOptionValueOrUseDefault(editingProperty.Attributes, TypeCollectionPropertyEditor.AllowDuplicate, true);
            Func<Type, bool> filter = GetOptionValueOrUseDefault<Func<Type, bool>>(editingProperty.Attributes, TypeCollectionPropertyEditor.Filter, null);
            Type defaultType = GetOptionValueOrUseDefault<Type>(editingProperty.Attributes, TypeCollectionPropertyEditor.DefaultType, typeof(Object));
            EditorWindow editorWindow = new EditorWindow(activityModelItem, rawInputData, context, activityModelItem.View, allowDuplication, filter, defaultType);
            if (editorWindow.ShowOkCancel())
            {
                using (var commitEditingScope = inputData.BeginEdit(System.Activities.Core.Presentation.SR.ChangeTypeCollectionEditingScopeDesc))
                {
                    inputData.Clear();
                    foreach (Type i in ((TypeCollectionDesigner)editorWindow.Content).UpdatedTypeCollection)
                    {
                        inputData.Add(i);
                    }
                    commitEditingScope.Complete();
                }
            }
        }

        sealed class EditorWindow : WorkflowElementDialog
        {
            public EditorWindow(ModelItem activity, IEnumerable<Type> data, EditingContext context, DependencyObject owner, bool allowDuplicate, Func<Type, bool> filter, Type defaultType)
            {
                this.ModelItem = activity;
                this.Context = context;
                this.Owner = owner;
                this.EnableMaximizeButton = false;
                this.EnableMinimizeButton = false;
                this.MinWidth = 450;
                this.MinHeight = 260;
                this.WindowResizeMode = ResizeMode.CanResize;
                this.WindowSizeToContent = SizeToContent.Manual;
                TypeCollectionDesigner content = new TypeCollectionDesigner()
                {
                    Context = context,
                    InitialTypeCollection = data,
                    AllowDuplicate = allowDuplicate,
                    Filter = filter,
                    DefaultType = defaultType,
                    ParentDialog = this,
                };
                this.Title = (string)content.Resources["controlTitle"];
                this.Content = content;
                this.OnOk = content.OnOK;
                this.HelpKeyword = HelpKeywords.TypeCollectionEditor;
            }
        }
    }
}
