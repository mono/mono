//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Hosting;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowElementDialog : ContentControl
    {
        public static readonly DependencyProperty ModelItemProperty =
            DependencyProperty.Register("ModelItem",
            typeof(ModelItem),
            typeof(WorkflowElementDialog),
            new PropertyMetadata(OnModelItemChanged));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
            typeof(string),
            typeof(WorkflowElementDialog));

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context",
            typeof(EditingContext),
            typeof(WorkflowElementDialog));

        public static readonly DependencyProperty WindowResizeModeProperty =
            DependencyProperty.Register("WindowResizeMode", 
            typeof(ResizeMode), 
            typeof(WorkflowElementDialog),
            new UIPropertyMetadata(ResizeMode.CanResize));

        public static readonly DependencyProperty WindowSizeToContentProperty =
            DependencyProperty.Register("WindowSizeToContent", 
            typeof(SizeToContent),
            typeof(WorkflowElementDialog), 
            new UIPropertyMetadata(SizeToContent.WidthAndHeight));

        protected WorkflowElementDialog()
        {
        }

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public DependencyObject Owner
        {
            get;
            set;
        }

        public ResizeMode WindowResizeMode
        {
            get { return (ResizeMode)GetValue(WindowResizeModeProperty); }
            set { SetValue(WindowResizeModeProperty, value); }
        }

        public SizeToContent WindowSizeToContent
        {
            get { return (SizeToContent)GetValue(WindowSizeToContentProperty); }
            set { SetValue(WindowSizeToContentProperty, value); }
        }

        public bool EnableMinimizeButton
        {
            get;
            set;
        }

        public bool EnableMaximizeButton
        {
            get;
            set;
        }

        public string HelpKeyword
        {
            get;
            set;
        }

        internal Func<bool> OnOk
        {
            get;
            set;
        }

        public void Show()
        {
            Show(false);
        }

        public bool ShowOkCancel()
        {
            bool? result = Show(true);
            return result.HasValue && result.Value;
        }

        internal void CloseDialog(bool commitChanges)
        {
            this.Window.DialogResult = commitChanges;
        }

        internal WorkflowElementDialogWindow Window
        {
            get;
            set;
        }

        bool? Show(bool okCancel)
        {
            WorkflowElementDialogWindow wnd = new WorkflowElementDialogWindow(this, okCancel, this.EnableMinimizeButton, this.EnableMaximizeButton, this.OnOk) 
            { 
                Title = this.Title 
            };
            if (null != this.Context)
            {
                WindowHelperService srv = this.Context.Services.GetService<WindowHelperService>();
                if (null != srv)
                {
                    srv.TrySetWindowOwner(this.Owner, wnd);
                }
            }
            wnd.Closed += (s, e) => { this.OnWorkflowElementDialogClosed(((Window)s).DialogResult); };
            return wnd.ShowDialog();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            // This is necessary for WPF data bindings to work.
            // It needs to be done explicitly, probably because
            // this ContentControl doesn't define its own 
            // Template.VisualTree (maybe it should).
            this.DataContext = this;
        }

        protected void EnableOk(bool enabled)
        {
            if (this.Window != null)
            {
                this.Window.okButton.IsEnabled = enabled;
            }
        }

        protected virtual void OnModelItemChanged(object newItem)
        {
        }

        protected virtual void OnWorkflowElementDialogClosed(bool? dialogResult)
        {
        }

        static void OnModelItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WorkflowElementDialog dialog = (WorkflowElementDialog)dependencyObject;
            dialog.OnModelItemChanged(e.NewValue);
        }
    }
}
