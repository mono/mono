//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using System.Activities.Presentation.Model;
    using System.Windows;
    using System.Runtime;
    using System.Windows.Automation.Peers;
    using System.Diagnostics.CodeAnalysis;
    using System.Xaml;
    using System.Globalization;

    public abstract class ExpressionActivityEditor : UserControl
    {
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(ExpressionActivityEditor));

        public static readonly DependencyProperty ExpressionProperty = DependencyProperty.Register("Expression", typeof(ModelItem), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ExpressionTypeProperty = DependencyProperty.Register("ExpressionType", typeof(Type), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OwnerActivityProperty = DependencyProperty.Register("OwnerActivity", typeof(ModelItem), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty UseLocationExpressionProperty = DependencyProperty.Register("UseLocationExpression", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty PathToArgumentProperty = DependencyProperty.Register("PathToArgument", typeof(string), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty IsSupportedExpressionProperty = DependencyProperty.Register("IsSupportedExpression", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

        public static readonly DependencyProperty ExplicitCommitProperty = DependencyProperty.Register("ExplicitCommit", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register("AcceptsTab", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsIndependentExpressionProperty = DependencyProperty.Register("IsIndependentExpression", typeof(bool), typeof(ExpressionActivityEditor),
                new FrameworkPropertyMetadata(false));

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ModelItem Expression
        {
            get { return (ModelItem)GetValue(ExpressionProperty); }
            set { SetValue(ExpressionProperty, value); }
        }

        public Type ExpressionType
        {
            get { return (Type)GetValue(ExpressionTypeProperty); }
            set { SetValue(ExpressionTypeProperty, value); }
        }

        public bool UseLocationExpression
        {
            get { return (bool)GetValue(UseLocationExpressionProperty); }
            set { SetValue(UseLocationExpressionProperty, value); }
        }

        public bool IsIndependentExpression
        {
            get { return (bool)GetValue(IsIndependentExpressionProperty); }
            set { SetValue(IsIndependentExpressionProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ModelItem OwnerActivity
        {
            get { return (ModelItem)GetValue(OwnerActivityProperty); }
            set { SetValue(OwnerActivityProperty, value); }
        }

        public string PathToArgument
        {
            get { return (string)GetValue(PathToArgumentProperty); }
            set { SetValue(PathToArgumentProperty, value); }
        }

        public bool IsSupportedExpression
        {
            get { return (bool)GetValue(IsSupportedExpressionProperty); }
            set { SetValue(IsSupportedExpressionProperty, value); }
        }

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public bool AcceptsTab
        {
            get { return (bool)GetValue(AcceptsTabProperty); }
            set { SetValue(AcceptsTabProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        public bool ExplicitCommit
        {
            get { return (bool)GetValue(ExplicitCommitProperty); }
            set { SetValue(ExplicitCommitProperty, value); }
        }

        static AttachableMemberIdentifier editorPropertyID = new AttachableMemberIdentifier(typeof(ExpressionActivityEditor), "ExpressionActivityEditor");

        public static void SetExpressionActivityEditor(object target, string name)
        {
            ValidateExpressionActivityEditorName(name);
            AttachablePropertyServices.SetProperty(target, editorPropertyID, name);
        }

        public static string GetExpressionActivityEditor(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, editorPropertyID, out value) ? value : null;
        }

        internal static void ValidateExpressionActivityEditorName(string name)
        {
            if (name != null && name.Trim().Length == 0)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentUICulture, SR.InvalidExpressionEditorNameToSet,
                    name)));
            }
        }

        internal string ItemStatus
        {
            get
            {
                return this.OnCreateAutomationPeer().GetItemStatus();
            }
        }

        protected EditingContext Context
        {
            get
            {
                if (this.OwnerActivity != null)
                {
                    return this.OwnerActivity.GetEditingContext();
                }
                else
                {
                    return null;
                }
            }
        }

        protected ExpressionActivityEditor()
        { }
        
        public virtual void BeginEdit() { }
        public virtual bool CanCommit() { return true; }
        public abstract bool Commit(bool isExplicitCommit);
    }
}
