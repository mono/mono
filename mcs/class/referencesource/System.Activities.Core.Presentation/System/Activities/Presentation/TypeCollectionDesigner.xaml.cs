//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.ComponentModel;
    using System.Windows.Threading;
    using System.Windows.Data;
    using Microsoft.Activities.Presentation;

    [Fx.Tag.XamlVisible(false)]
    partial class TypeCollectionDesigner
    {
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
            "Context",
            typeof(EditingContext),
            typeof(TypeCollectionDesigner),
            new UIPropertyMetadata(null));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "Type RoutedCommand is immutable.")]
        public static readonly ICommand AddNewTypeCommand = new RoutedCommand("AddNewType", typeof(TypeCollectionDesigner));
        public static readonly ICommand DeleteTypeCommand = new RoutedCommand("DeleteType", typeof(TypeCollectionDesigner));

        DataGridHelper dgHelper;
        ObservableCollection<TypeWrapper> wrapperCollection;

        public TypeCollectionDesigner()
        {
            this.DefaultType = typeof(Object);
            InitializeComponent();
        }

        // The collection of Type objects to display when type collection designer is opened.
        internal IEnumerable<Type> InitialTypeCollection
        {
            set
            {
                this.wrapperCollection = new ObservableCollection<TypeWrapper>(value.Select(type => new TypeWrapper(type)));
                this.typesDataGrid.ItemsSource = this.wrapperCollection;
            }
        }

        // The collction of Type objects in the type collection designer when user clicks OK.
        public IEnumerable<Type> UpdatedTypeCollection
        {
            get
            {
                return wrapperCollection.Where(wrapper => wrapper.Type != null).Select(wrapper => wrapper.Type);
            }
        }

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public bool AllowDuplicate
        {
            get;
            set;
        }

        public Func<Type, bool> Filter
        {
            get;
            set;
        }

        public Type DefaultType
        {
            get;
            set;
        }

        internal WorkflowElementDialog ParentDialog
        {
            get;
            set;
        }

        internal bool OnOK()
        {   
            if (!this.AllowDuplicate)
            {
                List<TypeWrapper> list = new List<TypeWrapper>();
                foreach (TypeWrapper tw in this.wrapperCollection)
                {
                    if (tw.Type != null && list.Any<TypeWrapper>(entry => Type.Equals(entry.Type, tw.Type)))
                    {
                        ErrorReporting.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, (string)this.FindResource("duplicateEntryErrorMessage"), TypeNameHelper.GetDisplayName(tw.Type, true)));
                        return false;
                    }
                    list.Add(tw);
                }
            }
            return true;
        }

        // The DataGrid does not bubble up KeyDown event and we expect the upper window to be closed when ESC key is down.
        // Thus we added an event handler in DataGrid to handle ESC key and closes the uppper window.
        void OnTypesDataGridRowKeyDown(object sender, KeyEventArgs args)
        {
            DataGridRow row = (DataGridRow)sender;
            if (args.Key == Key.Escape && !row.IsEditing && this.ParentDialog != null)
            {
                this.ParentDialog.CloseDialog(false);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            this.dgHelper = new DataGridHelper(this.typesDataGrid, this);
            this.dgHelper.AddNewRowContent = this.FindResource("addNewRowLabel");
            this.dgHelper.AddNewRowCommand = AddNewTypeCommand;

            base.OnInitialized(e);
        }

        void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ButtonDelete.IsEnabled =
                   (this.typesDataGrid.SelectedItems != null)
                && (this.typesDataGrid.SelectedItems.Count != 0);
        }

        void OnAddTypeExecuted(object sender, ExecutedRoutedEventArgs e)
        {   
            var newEntry = new TypeWrapper(this.DefaultType);
            this.wrapperCollection.Add(newEntry);
            this.dgHelper.BeginRowEdit(newEntry);            
            e.Handled = true;
        }

        void OnDeleteTypeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DataGridHelper.OnDeleteSelectedItems(this.typesDataGrid);
        }

        void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }

        sealed class TypeWrapper : DependencyObject
        {
            public static readonly DependencyProperty TypeProperty =
                DependencyProperty.Register("Type", typeof(Type), typeof(TypeWrapper));

            //Default constructor is required by DataGrid to load NewItemPlaceHolder row and this constructor will never be called.
            //Since we've already customized the new row template and hooked over creating new object event.
            public TypeWrapper()
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            public TypeWrapper(Type type)
            {
                this.Type = type;
            }

            public Type Type
            {
                get { return (Type)GetValue(TypeProperty); }
                set { SetValue(TypeProperty, value); }
            }

            // For screen reader to read the DataGrid row.
            public override string ToString()
            {
                if (this.Type != null && !string.IsNullOrEmpty(this.Type.Name))
                {
                    return this.Type.Name;
                }
                return "null";
            }
        }
    }
}
