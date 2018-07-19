//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Activities.Presentation;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Model;

    partial class StartSymbol
    {

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(StartSymbol));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static StartSymbol CreateStartSymbol(EditingContext context)
        {
            StartSymbol start = new StartSymbol();
            FakeRoot fakeRoot = new FakeRoot { StartNode = new StartNode() };
            ModelTreeManager manager = context.Services.GetService<ModelTreeManager>();
            start.ModelItem = new FakeModelItemImpl(manager, typeof(FakeRoot), fakeRoot, null).Properties["StartNode"].Value;
            start.Name = "StartSymbol";
            start.Focusable = true;
            start.Context = context;
            start.DataContext = start;
            return start;
        }

        StartSymbol()
        {
            InitializeComponent();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Selection selection = this.Context.Items.GetValue<Selection>();

                if (selection.SelectionCount == 1)
                {
                    Fx.Assert(selection.PrimarySelection.Parent.ItemType == typeof(FakeRoot), "StartNode should have a fakeroot.");
                    // Avoid calling the delete command, if only the start node is selected.
                    e.Handled = true;
                }
            }

            base.OnPreviewKeyDown(e);
        }
    }


}
