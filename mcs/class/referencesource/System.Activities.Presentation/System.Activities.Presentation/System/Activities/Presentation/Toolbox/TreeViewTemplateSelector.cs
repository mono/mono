//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System.Windows;
    using System.Windows.Controls;

    // This class is resposible for selecting proper template for Category and Tool objects 
    // (those entities have different values beeing displayed)

    sealed class TreeViewTemplateSelector : DataTemplateSelector
    {
        ToolboxControl owner;

        public TreeViewTemplateSelector(ToolboxControl owner)
        {
            this.owner = owner;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate result = base.SelectTemplate(item, container);

            if (item is ToolboxItemWrapper && null != this.owner.ToolTemplate)
            {
                result = this.owner.ToolTemplate;
            }
            if (item is ToolboxCategory && null != this.owner.CategoryTemplate)
            {
                result = this.owner.CategoryTemplate;
            }

            return result;
        }
    }
}
