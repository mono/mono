//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    // This class will be the error view presented when there are exceptions or errors
    // in the designer view, or when we are unable to load the designer.
    sealed partial class ErrorView : UserControl
    {

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ErrorView), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DetailsProperty =
            DependencyProperty.Register("Details", typeof(string), typeof(ErrorView), new UIPropertyMetadata(string.Empty));


        public ErrorView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Details
        {
            get { return (string)GetValue(DetailsProperty); }
            set { SetValue(DetailsProperty, value); }
        }

        public EditingContext Context { get; set; }

        private void OnHelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Context == null)
            {
                return;
            }
            IIntegratedHelpService help = this.Context.Services.GetService<IIntegratedHelpService>();
            if (help != null)
            {
                help.ShowHelpFromKeyword(HelpKeywords.ErrorView);
            }
            else
            {
                System.Diagnostics.Process.Start(SR.DefaultHelpUrl);
            }
        }


    }
}
