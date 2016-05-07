//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Expressions
{
    using System;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;

    public abstract class TextualExpressionEditor : ExpressionActivityEditor
    {
        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register("MaxLines", typeof(int), typeof(TextualExpressionEditor),
                new FrameworkPropertyMetadata(Int32.MaxValue));

        public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register("MinLines", typeof(int), typeof(TextualExpressionEditor),
                new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(string), typeof(TextualExpressionEditor),
                new FrameworkPropertyMetadata(null));

        public int MaxLines
        {
            get { return (int)GetValue(MaxLinesProperty); }
            set { SetValue(MaxLinesProperty, value); }
        }

        public int MinLines
        {
            get { return (int)GetValue(MinLinesProperty); }
            set { SetValue(MinLinesProperty, value); }
        }

        public string DefaultValue
        {
            get { return (string)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        public virtual IExpressionEditorService ExpressionEditorService
        {
            get { return null; }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnCompleteWordCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnCompleteWordCommandExecute(ExecutedRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnGlobalIntellisenseCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnGlobalIntellisenseCommandExecute(ExecutedRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnParameterInfoCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnParameterInfoCommandExecute(ExecutedRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnQuickInfoCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnQuickInfoCommandExecute(ExecutedRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnIncreaseFilterLevelCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnDecreaseFilterLevelCommandExecute(ExecutedRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnDecreaseFilterLevelCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.Handled = false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "This is virtual method, the overriden method may need to access the members of derived type")]
        public virtual void OnIncreaseFilterLevelCommandExecute(ExecutedRoutedEventArgs e)
        {
            e.Handled = false;
        }
    }
}
