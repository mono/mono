#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DbLinq.Factory;
using DbLinq.Vendor;
using DbLinq.Schema.Dbml;
using DbMetal;
using DbMetal.Generator;
using DbMetal.Generator.Implementation.CodeDomGenerator;

namespace VisualMetal
{
    public partial class MainWindow : Window
    {
        public IProcessor Program = ObjectFactory.Get<IProcessor>();
        public Parameters Parameters = new Parameters();
        public ISchemaLoader Loader;

        Database database;
        public Database Database
        {
            get
            {
                return database;
            }
            set
            {
                database = value;

                TableList.ItemsSource = Database.Table;
                SchemaPropertyGrid.SelectedObject = Database;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                if (!String.IsNullOrEmpty(Properties.Settings.Default.Params))
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Settings.Default.Params)))
                        Parameters = (Parameters)XamlReader.Load(stream);
            }
            catch { } // throw away any errors from parsing parameters
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (!Properties.Settings.Default.SavePassword)
                Parameters.Password = null; // clear password for security.
            Properties.Settings.Default.Params = XamlWriter.Save(Parameters);
            Properties.Settings.Default.Save();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(this).ShowDialog();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Database markup files (*.dbml)|*.dbml|All files (*.*)|*.*";
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                Database = Program.ReadSchema(Parameters, dialog.FileName);
            }
        }

        private void GenerateCSharp_Click(object sender, RoutedEventArgs e)
        {
            if (Database == null)
            {
                MessageBox.Show("No database schema loaded.");
                return;
            }

            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "C# source files (*.cs)|*.cs|All files (*.*)|*.*";
            dialog.FileName = Parameters.Database;
            // TODO: use common way with DbMetal instead of hardcoded generators.
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                new CSharpCodeDomGenerator().GenerateCSharp(Database, dialog.FileName);
            //Program.GenerateCSharp(Parameters, Database, Loader, dialog.FileName);
        }

        private void GenerateVisualBasic_Click(object sender, RoutedEventArgs e)
        {
            if (Database == null)
            {
                MessageBox.Show("No database schema loaded.");
                return;
            }

            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "Visual Basic source files (*.vb)|*.vb|All files (*.*)|*.*";
            dialog.FileName = Parameters.Database;
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                new CSharpCodeDomGenerator().GenerateVisualBasic(Database, dialog.FileName);
        }

        private void SaveDbml_Click(object sender, RoutedEventArgs e)
        {
            if (Database == null)
            {
                MessageBox.Show("No database schema loaded.");
                return;
            }

            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "Database markup files (*.dbml)|*.dbml|All files (*.*)|*.*";
            dialog.FileName = Parameters.Database;
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                using (Stream dbmlFile = File.OpenWrite(dialog.FileName))
                    DbmlSerializer.Write(dbmlFile, Database);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                ColumnList.ItemsSource = null;
            else
            {
                DbLinq.Schema.Dbml.Table selected = (DbLinq.Schema.Dbml.Table)e.AddedItems[0];
                ColumnList.ItemsSource = selected.Type.Items;
                TablePropertyGrid.SelectedObject = selected;
            }
        }

        private void ColumnList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                ColumnPropertyGrid.SelectedObject = e.AddedItems[0];
            else
                ColumnPropertyGrid.SelectedObject = null;
        }

        private void RebindTableList(object sender, RoutedEventArgs e)
        {
            // this is ugly, it's needed to refresh the Table.ToString calls
            // Alternates to doing this might be implementing INotifyProvider on the table class, or not using an ItemsSource binding
            // or maybe use a data template to display the item, instead of using ToString
            // ideally we need a datagrid instead of a listbox

            var temp = TableList.ItemsSource;
            TableList.ItemsSource = null;
            TableList.ItemsSource = temp;
        }

        private void RebindColumnList(object sender, RoutedEventArgs e)
        {
            // this is ugly, it's needed to refresh the Column.ToString calls
            var temp = ColumnList.ItemsSource;
            ColumnList.ItemsSource = null;
            ColumnList.ItemsSource = temp;
        }

        public bool LoadSchema()
        {
            try
            {
                ISchemaLoader loader;
                Database = Program.ReadSchema(Parameters, out loader);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                return false;
            }
            return true;
        }
    }
}