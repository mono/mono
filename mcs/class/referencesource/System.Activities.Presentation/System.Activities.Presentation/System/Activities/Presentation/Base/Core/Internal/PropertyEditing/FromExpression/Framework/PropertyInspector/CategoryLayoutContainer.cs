// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Controls;
    using System.Activities.Presentation.PropertyEditing;

    internal class CategoryLayoutContainer : ItemsControl
    {
        public CategoryLayoutContainer()
        {
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ContentPresenter contentPresenter = element as ContentPresenter;
            CategoryEditor categoryEditor = item as CategoryEditor;
            if (contentPresenter != null && categoryEditor != null)
            {
                Binding contentBinding = new Binding("DataContext.Category");
                contentBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(CategoryLayoutContainer), 1);
                contentPresenter.SetBinding(ContentPresenter.ContentProperty, contentBinding);
                contentPresenter.ContentTemplate = categoryEditor.EditorTemplate;
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
