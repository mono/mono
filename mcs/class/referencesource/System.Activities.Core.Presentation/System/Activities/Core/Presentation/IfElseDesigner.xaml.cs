//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for IfElseDesigner.xaml
    /// </summary>
    partial class IfElseDesigner
    {
        public IfElseDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(If);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(IfElseDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Then"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Else"), BrowsableAttribute.No);

            builder.AddCustomAttributes(type, type.GetProperty("Condition"), new HidePropertyInOutlineViewAttribute());
        }

        void OnExpressionEditorLoaded(object sender, RoutedEventArgs e)
        {
            ExpressionTextBox expressionTextBox = sender as ExpressionTextBox;
            Fx.Assert(expressionTextBox != null, "sender should be userControl");
            //bind ExpressionProperty of Expression textbox to ModelItem.Condition
            Binding b = new Binding();
            ArgumentToExpressionConverter argumentToExpressionConverter = new ArgumentToExpressionConverter();
            b.Converter = argumentToExpressionConverter;
            b.Mode = BindingMode.TwoWay;

            b.Source = this.ModelItem;
            b.Path = new PropertyPath("Condition");
            if (BindingOperations.GetBinding(expressionTextBox, ExpressionTextBox.ExpressionProperty) != null)
            {
                BindingOperations.ClearBinding(expressionTextBox, ExpressionTextBox.ExpressionProperty);
            }
            expressionTextBox.SetBinding(ExpressionTextBox.ExpressionProperty, b);
            //bind OwnerActivityProperty of Expression textbox to ModelItem
            Binding b1 = new Binding();
            b1.Source = this.ModelItem;
            if (BindingOperations.GetBinding(expressionTextBox, ExpressionTextBox.OwnerActivityProperty) != null)
            {
                BindingOperations.ClearBinding(expressionTextBox, ExpressionTextBox.OwnerActivityProperty);
            }
            expressionTextBox.SetBinding(ExpressionTextBox.OwnerActivityProperty, b1);
        }
    }
}
