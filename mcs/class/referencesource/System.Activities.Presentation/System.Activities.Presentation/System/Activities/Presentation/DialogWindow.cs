//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows;
    using System.Activities.Presentation.View;
    using System.Windows.Interop;
    using System.Windows.Input;
    using System.ComponentModel;
    using Microsoft.Tools.Common;

    internal class DialogWindow : Window
    {        
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context",
            typeof(EditingContext),
            typeof(DialogWindow));

        string helpKeyword = HelpKeywords.HomePage;

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        protected string HelpKeyword
        {
            get
            {
                return this.helpKeyword;
            }
            set
            {
                this.helpKeyword = value;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.HideMinMaxButton();
            this.ShowContextHelpButton();
            this.HideIcon();
            this.AddWindowsHook(OnHookedWindowMessage);
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, new ExecutedRoutedEventHandler(OnHelpExecuted)));
            this.Closing += new CancelEventHandler(OnWindowClosing);
        }

        static IntPtr OnHookedWindowMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Interop.WM_SYSCOMMAND && wParam.ToInt64() == (long)Win32Interop.SC_CONTEXTHELP)
            {
                var rootVisual = HwndSource.FromHwnd(hwnd).RootVisual;
                var focusedElement = FocusManager.GetFocusedElement(rootVisual);
                if (focusedElement == null)
                {
                    focusedElement = rootVisual as IInputElement;
                }
                ApplicationCommands.Help.Execute(null, focusedElement);
                handled = true;
            }

            // According to MSDN, zero should be returned after handling WM_SYSCOMMAND.
            // If this message is unhandled, it's still safe to return zero
            // because WPF framework (HwndSource) will return zero anyway if the
            // message is unhandled.
            return IntPtr.Zero;
        }       
        
        void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Note: Do NOT remove the hook if the close operation needs to be canceled.
            this.RemoveWindowsHook(OnHookedWindowMessage);
        }

        void OnHelpExecuted(Object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Context != null)            
            {
                IIntegratedHelpService help = this.Context.Services.GetService<IIntegratedHelpService>();
                if (help != null)
                {
                    help.ShowHelpFromKeyword(this.helpKeyword);
                    return;
                }
            }
            System.Diagnostics.Process.Start(SR.DefaultHelpUrl);
        }
    }
}
