//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Diagnostics;
    using System.Runtime;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    internal static class ErrorReporting
    {
        private static WeakReference activeDesignerViewReference;

        internal static DesignerView ActiveDesignerView
        {
            get
            {
                if (activeDesignerViewReference != null)
                {
                    return activeDesignerViewReference.Target as DesignerView;
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    activeDesignerViewReference = null;
                }
                else
                {
                    activeDesignerViewReference = new WeakReference(value);
                }
            }
        }

        public static void ShowErrorMessage(string message)
        {
            ShowErrorMessage(message, false);
        }

        public static void ShowAlertMessage(string message)
        {
            ShowAlertMessage(message, false);
        }

        public static void ShowErrorMessage(string message, string details)
        {
            if (string.IsNullOrEmpty(details))
            {
                ShowErrorMessage(message);
            }
            else
            {
                ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, "{0}\n\n\"{1}\"", message, details));
            }
        }

        public static void ShowErrorMessage(string message, bool includeStackTrace)
        {
            string stackTrace = null;
            if (includeStackTrace)
            {
                //generate stack trace
                stackTrace = new StackTrace().ToString();
                //remove top frame from the trace (which is a call to ShowErrorMessage)
                stackTrace = stackTrace.Remove(0, stackTrace.IndexOf(Environment.NewLine, StringComparison.Ordinal) + 1);
            }
            ShowMessageBox(message, MessageBoxImage.Error, stackTrace);
        }

        public static void ShowAlertMessage(string message, bool includeStackTrace)
        {
            string stackTrace = null;
            if (includeStackTrace)
            {
                //generate stack trace
                stackTrace = new StackTrace().ToString();
                //remove top frame from the trace (which is a call to ShowAlertMessage)
                stackTrace = stackTrace.Remove(0, stackTrace.IndexOf(Environment.NewLine, StringComparison.Ordinal) + 1);
            }
            ShowMessageBox(message, MessageBoxImage.Warning, stackTrace);
        }

        public static void ShowErrorMessage(Exception err)
        {
            if (null != err)
            {
                ShowMessageBox(string.Format(CultureInfo.InvariantCulture, "{0}:\r\n{1}", err.GetType().Name, err.Message), MessageBoxImage.Error, err.StackTrace);
            }
        }

        static void ShowMessageBox(string message, MessageBoxImage icon, string stackTrace)
        {
            //determine an icon
            string iconName = icon == MessageBoxImage.Error ? "TextBoxErrorIcon" :
                icon == MessageBoxImage.Warning ? "WarningValidationIcon" : string.Empty;

            //set properties
            var dlg = new ErrorDialog()
            {
                ErrorDescription = message ?? "<null>",
                Icon = EditorResources.GetIcons()[iconName],
                StackTrace = stackTrace,
                StackTraceVisibility = string.IsNullOrEmpty(stackTrace) ? Visibility.Collapsed : Visibility.Visible,
                Context = null != ActiveDesignerView ? ActiveDesignerView.Context : null,
                Owner = ActiveDesignerView,
            };
            //show error window
            dlg.Show();
        }

        sealed class ErrorDialog : WorkflowElementDialog
        {
            public string ErrorDescription { get; set; }

            public Visibility StackTraceVisibility { get; set; }

            public string StackTrace { get; set; }

            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "This property is accessed by XAML")]
            public object Icon { get; set; }

            protected override void OnInitialized(EventArgs e)
            {
                this.Title = SR.WorkflowDesignerErrorPresenterTitle;
                this.Content = new ContentPresenter()
                {
                    ContentTemplate = (DataTemplate)EditorResources.GetResources()["ErrorPresenterDialogTemplate"],
                    Content = this,
                };
                this.MinWidth = 365;
                this.WindowResizeMode = ResizeMode.NoResize;
                this.WindowSizeToContent = SizeToContent.WidthAndHeight;
                //handle loaded event
                this.Loaded += this.OnDialogWindowLoaded;
                base.OnInitialized(e);
            }

            void OnDialogWindowLoaded(object s, RoutedEventArgs e)
            {
                //get the containing window
                var parentWindow = VisualTreeUtils.FindVisualAncestor<Window>(this);
                //and handle KeyDown event - in case of Esc, we should close the dialog
                parentWindow.KeyDown += OnWindowKeyDown;

                //add Copy command support - when user presses Ctrl+C, copy content of the error into clipboard
                var copyBinding = new CommandBinding(ApplicationCommands.Copy);
                copyBinding.PreviewCanExecute += OnCopyCanExecute;
                copyBinding.Executed += OnCopyExecuted;
                parentWindow.CommandBindings.Add(copyBinding);
            }

            void OnWindowKeyDown(object s, KeyEventArgs e)
            {
                //Esc - close the dialog box
                if (e.Key == Key.Escape)
                {
                    ((Window)s).DialogResult = false;
                    e.Handled = true;
                }
            }

            void OnCopyCanExecute(object s, CanExecuteRoutedEventArgs e)
            {
                //do not allow text boxes to handle the ApplicationCommand.Copy, i will handle it myself
                e.CanExecute = true;
                e.ContinueRouting = false;
                e.Handled = true;
            }

            void OnCopyExecuted(object s, ExecutedRoutedEventArgs e)
            {
                //build a string with detailed error description
                StringBuilder error = new StringBuilder();
                error.Append('-', 25);
                error.Append(Environment.NewLine);
                error.Append(this.Title);
                error.Append(Environment.NewLine);
                error.Append('-', 25);
                error.Append(Environment.NewLine);
                error.Append(this.ErrorDescription);
                error.Append(Environment.NewLine);
                if (this.StackTraceVisibility == Visibility.Visible)
                {
                    error.Append('-', 25);
                    error.Append(Environment.NewLine);
                    error.Append(this.StackTrace);
                    error.Append(Environment.NewLine);
                }
                error.Append('-', 25);
                error.Append(Environment.NewLine);
                string result = error.ToString();

                //attempt to set the value into clipboard - according to MSDN - if a call fails, it means some other process is accessing clipboard
                //so sleep and retry
                for (int i = 0; i < 10; ++i)
                {
                    try
                    {
                        Clipboard.SetText(result);
                        break;
                    }
                    catch (Exception err)
                    {
                        if (Fx.IsFatal(err))
                        {
                            throw;
                        }
                        Thread.Sleep(50);
                    }
                }
                e.Handled = true;
            }
        }
    }
}
