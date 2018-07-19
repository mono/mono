// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace System.Workflow.Activities.Rules.Design
{
    #region IntellisenseTextBox

    internal partial class IntellisenseTextBox : TextBox
    {
        #region members and constructors

        private ListView listBoxAutoComplete = new ListView();
        public event EventHandler<AutoCompletionEventArgs> PopulateAutoCompleteList;
        public event EventHandler<AutoCompletionEventArgs> PopulateToolTipList;
        int oldSelectionStart;

        enum memberIcons
        {
            Default = 0,
            Type = 1,
            PublicMethod = 2,
            PrivateMethod = 3,
            InternalMethod = 4,
            ProtectedMethod = 5,
            PublicProperty = 6,
            PrivateProperty = 7,
            InternalProperty = 8,
            ProtectedProperty = 9,
            PublicField = 10,
            PrivateField = 11,
            InternalField = 12,
            ProtectedField = 13,
            Keyword = 14,
            ExtensionMethod = 15
        }

        public IntellisenseTextBox()
        {
            InitializeComponent();

            this.AcceptsReturn = true;

            this.listBoxAutoComplete.FullRowSelect = true;
            this.listBoxAutoComplete.MultiSelect = false;
            this.listBoxAutoComplete.SmallImageList = this.autoCompletionImageList;
            this.listBoxAutoComplete.LargeImageList = this.autoCompletionImageList;
            this.listBoxAutoComplete.View = System.Windows.Forms.View.Details;
            this.listBoxAutoComplete.HeaderStyle = ColumnHeaderStyle.None;
            this.listBoxAutoComplete.Columns.Add(Messages.No, this.listBoxAutoComplete.Size.Width);
            this.listBoxAutoComplete.CausesValidation = false;
            this.listBoxAutoComplete.Sorting = SortOrder.Ascending;

            this.listBoxAutoComplete.Visible = false;
            this.KeyPress += new KeyPressEventHandler(IntellisenseTextBox_KeyPress);
            this.HandleCreated += new EventHandler(IntellisenseTextBox_HandleCreated);

        }


        #endregion

        #region IntellisenseTextBox event handlers

        private void IntellisenseTextBox_HandleCreated(object sender, EventArgs e)
        {
            if (this.TopLevelControl != null)
            {
                this.TopLevelControl.Controls.Add(this.listBoxAutoComplete);
                this.listBoxAutoComplete.DoubleClick += new EventHandler(listBoxAutoComplete_DoubleClick);
                this.listBoxAutoComplete.SelectedIndexChanged += new EventHandler(listBoxAutoComplete_SelectedIndexChanged);
                this.listBoxAutoComplete.Enter += new EventHandler(listBoxAutoComplete_Enter);
            }
        }

        void IntellisenseTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            string currentValue = this.Text;
            int selectionStart = this.SelectionStart;
            int selectionLength = this.SelectionLength;
            StringBuilder projectedValue = new StringBuilder(currentValue.Substring(0, selectionStart));
            projectedValue.Append(currentValue.Substring(selectionStart + selectionLength));

            char c = e.KeyChar;
            if (c == '.')
            {
                if (this.listBoxAutoComplete.Visible)
                {
                    this.SelectItem();
                    HideIntellisenceDropDown();
                    IntellisenseTextBox_KeyPress(sender, e);
                }
                else
                {
                    projectedValue.Insert(selectionStart, '.');
                    UpdateIntellisenceDropDown(projectedValue.ToString().Substring(0, selectionStart + 1));
                    ShowIntellisenceDropDown(selectionStart);
                    IntellisenseTextBox_KeyDown(sender, new KeyEventArgs(Keys.Down)); // fake down arrow to select first item
                }
            }
            else if (c == '(')
            {
                if (listBoxAutoComplete.Visible)
                {
                    this.SelectItem();
                    HideIntellisenceDropDown();
                    IntellisenseTextBox_KeyPress(sender, e);
                }
                else
                {
                    projectedValue.Insert(selectionStart, '(');
                    ShowToolTip(selectionStart, projectedValue.ToString().Substring(0, selectionStart + 1));
                }
            }
            else if (!this.listBoxAutoComplete.Visible
                && CurrentPrefix.Length == 0
                && (c == '_' || char.IsLetter(c) || char.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber))
            {
                projectedValue.Insert(selectionStart, c);
                UpdateIntellisenceDropDown(projectedValue.ToString().Substring(0, selectionStart + 1));
                ShowIntellisenceDropDown(selectionStart);
                if (this.listBoxAutoComplete.Visible)
                    IntellisenseTextBox_KeyDown(sender, new KeyEventArgs(Keys.Down)); // fake down arrow to select first item
            }
            else if (this.listBoxAutoComplete.Visible)
            {
                projectedValue.Insert(selectionStart, c);
                UpdateAutoCompleteSelection(CurrentPrefix + c);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void IntellisenseTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string currentValue = this.Text;
            int selectionStart = this.SelectionStart;
            int selectionLength = this.SelectionLength;

            StringBuilder removedString = new StringBuilder(currentValue.Substring(selectionStart, selectionLength));

            StringBuilder projectedValue = new StringBuilder(currentValue.Substring(0, selectionStart));
            projectedValue.Append(currentValue.Substring(selectionStart + selectionLength));

            System.Diagnostics.Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "KeyCode:{0}, KeyData:{1}, KeyValue:{2}", e.KeyCode, e.KeyData, e.KeyValue));
            this.toolTip.Hide(this);
            if (e.KeyData == (Keys.Control | Keys.Space))
            {
                if (!this.listBoxAutoComplete.Visible)
                {
                    UpdateIntellisenceDropDown(this.Text.Substring(0, selectionStart - CurrentPrefix.Length));
                    ShowIntellisenceDropDown(selectionStart);
                    UpdateAutoCompleteSelection(CurrentPrefix);
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (this.Text.Length > 0)
                {
                    if (removedString.Length == 0 && selectionStart > 0)
                    {
                        removedString.Append(projectedValue[selectionStart - 1]);
                        projectedValue.Length = projectedValue.Length - 1;
                    }

                    if (CurrentPrefix.Length <= 1)
                        HideIntellisenceDropDown();

                    if (removedString.ToString().IndexOfAny(". ()[]\t\n".ToCharArray()) >= 0)
                        HideIntellisenceDropDown();
                    else if (this.listBoxAutoComplete.Visible)
                        UpdateAutoCompleteSelection(CurrentPrefix.Substring(0, CurrentPrefix.Length - 1));
                }

            }
            else if (e.KeyCode == Keys.Up)
            {
                if (this.listBoxAutoComplete.Visible)
                {
                    if (this.listBoxAutoComplete.SelectedIndices.Count > 0 && this.listBoxAutoComplete.SelectedIndices[0] > 0)
                    {
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0] - 1].Selected = true;
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0]].Focused = true;
                    }

                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (this.listBoxAutoComplete.Visible)
                {
                    if (this.listBoxAutoComplete.SelectedIndices.Count == 0)
                    {
                        if (this.listBoxAutoComplete.Items.Count > 0)
                        {
                            this.listBoxAutoComplete.Items[0].Selected = true;
                            this.listBoxAutoComplete.Items[0].Focused = true;
                        }
                    }
                    else if (this.listBoxAutoComplete.SelectedIndices[0] < this.listBoxAutoComplete.Items.Count - 1)
                    {
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0] + 1].Selected = true;
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0]].Focused = true;
                    }
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.ShiftKey
                || e.KeyCode == Keys.ControlKey
                || e.KeyCode == Keys.OemPeriod)
            {
                //DO nothing
            }
            else if ((e.KeyValue < 48 || (e.KeyValue >= 58 && e.KeyValue <= 64) || (e.KeyValue >= 91 && e.KeyValue <= 96) || e.KeyValue > 122) &&
                e.KeyData != (Keys.Shift | Keys.OemMinus))
            {
                if (this.listBoxAutoComplete.Visible)
                {
                    if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Space)
                    {
                        this.SelectItem();
                        e.Handled = true;
                    }

                    HideIntellisenceDropDown();
                }
            }
        }

        private void IntellisenseTextBox_Leave(object sender, EventArgs e)
        {
            // remmember caret position before leaving
            this.oldSelectionStart = this.SelectionStart;
            this.toolTip.Hide(this);


            // make sure to close intellisense dropdown
            if ((this.listBoxAutoComplete.Focused == false) && (this.Focused == false))
                this.listBoxAutoComplete.Visible = false;
        }

        private void IntellisenseTextBox_Enter(object sender, EventArgs e)
        {
            // regain caret position
            if (this.oldSelectionStart >= 0)
                this.SelectionStart = this.oldSelectionStart;
        }

        void IntellisenseTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            HideIntellisenceDropDown();
        }

        #endregion

        #region dropdown event handlers

        void listBoxAutoComplete_Enter(object sender, EventArgs e)
        {
            // we want to make sure the dropdown does not contain the focus at all times
            this.CausesValidation = false;
            this.Focus();
            this.CausesValidation = true;
        }

        private void listBoxAutoComplete_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem listViewItem in this.listBoxAutoComplete.Items)
            {
                // make sure selection looks in focus (note: original
                // selection color for non focused listview is grayed)

                if (listViewItem.Selected)
                {
                    listViewItem.ForeColor = SystemColors.HighlightText;
                    listViewItem.BackColor = SystemColors.Highlight;
                    listViewItem.EnsureVisible();
                }
                else
                {
                    listViewItem.ForeColor = SystemColors.ControlText;
                    listViewItem.BackColor = SystemColors.Window;
                }
            }
        }

        private void listBoxAutoComplete_DoubleClick(object sender, EventArgs e)
        {
            // Item double clicked, select it
            if (this.listBoxAutoComplete.SelectedItems.Count == 1)
            {
                this.SelectItem();
                HideIntellisenceDropDown();
            }
        }

        #endregion

        #region helpers

        private void SelectItem()
        {
            if (this.listBoxAutoComplete.SelectedItems.Count > 0)
            {
                int selectionStart = this.SelectionStart;
                int prefixEnd = selectionStart - CurrentPrefix.Length;
                int suffixStart = selectionStart;

                if (suffixStart >= this.Text.Length)
                    suffixStart = this.Text.Length;

                string prefix = this.Text.Substring(0, prefixEnd);
                string fill = this.listBoxAutoComplete.SelectedItems[0].Text;
                string suffix = this.Text.Substring(suffixStart, this.Text.Length - suffixStart);

                this.Text = prefix + fill + suffix;
                this.SelectionStart = prefix.Length + fill.Length;
                this.ScrollToCaret();
                this.oldSelectionStart = this.SelectionStart;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void PopulateListBox(ICollection list)
        {
            this.listBoxAutoComplete.Items.Clear();
            if (list != null && list.Count > 0)
            {
                foreach (object item in list)
                {
                    ListViewItem listViewItem = null;
                    if (item is string)
                    {
                        listViewItem = new ListViewItem(item as string);
                        listViewItem.ImageIndex = (int)memberIcons.Default;
                    }
                    else if (item is IntellisenseKeyword)
                    {
                        listViewItem = new ListViewItem(((IntellisenseKeyword)item).Name);
                        listViewItem.ImageIndex = (int)memberIcons.Keyword;
                    }
                    else if (item is MemberInfo)
                    {
                        listViewItem = new ListViewItem(((MemberInfo)item).Name as string);
                        if (item is PropertyInfo)
                        {
                            MethodInfo mi = ((PropertyInfo)item).GetGetMethod(true);
                            if (mi == null)
                                mi = ((PropertyInfo)item).GetSetMethod(true);
                            if (mi.IsPublic)
                                listViewItem.ImageIndex = (int)memberIcons.PublicProperty;
                            else if (mi.IsPrivate)
                                listViewItem.ImageIndex = (int)memberIcons.PrivateProperty;
                            else if (mi.IsFamily || mi.IsFamilyAndAssembly || mi.IsFamilyOrAssembly)
                                listViewItem.ImageIndex = (int)memberIcons.ProtectedProperty;
                            else // mi.IsAssembly
                                listViewItem.ImageIndex = (int)memberIcons.InternalProperty;
                        }
                        else if (item is FieldInfo)
                        {
                            FieldInfo fi = (FieldInfo)item;
                            if (fi.IsPublic)
                                listViewItem.ImageIndex = (int)memberIcons.PublicField;
                            else if (fi.IsPrivate)
                                listViewItem.ImageIndex = (int)memberIcons.PrivateField;
                            else if (fi.IsFamily || fi.IsFamilyAndAssembly || fi.IsFamilyOrAssembly)
                                listViewItem.ImageIndex = (int)memberIcons.ProtectedField;
                            else // fi.IsAssembly
                                listViewItem.ImageIndex = (int)memberIcons.InternalField;
                        }
                        else if (item is ExtensionMethodInfo)
                        {
                            listViewItem.ImageIndex = (int)memberIcons.ExtensionMethod;
                        }
                        else if (item is MethodInfo)
                        {
                            MethodInfo mi = (MethodInfo)item;
                            if (mi.IsPublic)
                                listViewItem.ImageIndex = (int)memberIcons.PublicMethod;
                            else if (mi.IsPrivate)
                                listViewItem.ImageIndex = (int)memberIcons.PrivateMethod;
                            else if (mi.IsFamily || mi.IsFamilyAndAssembly || mi.IsFamilyOrAssembly)
                                listViewItem.ImageIndex = (int)memberIcons.ProtectedMethod;
                            else // mi.IsAssembly
                                listViewItem.ImageIndex = (int)memberIcons.InternalMethod;
                        }
                        else if (item is Type)
                            listViewItem.ImageIndex = (int)memberIcons.Type;
                    }
                    this.listBoxAutoComplete.Items.Add(listViewItem);
                }
            }
            this.listBoxAutoComplete.Sort();

            if (this.listBoxAutoComplete.Items.Count > 0)
            {
                this.listBoxAutoComplete.Columns[0].Width = -2; // this will set the column size to the longest value
                this.listBoxAutoComplete.Size = new Size(this.listBoxAutoComplete.Items[0].Bounds.Width + 30, 72);
            }
        }

        internal void HideIntellisenceDropDown()
        {
            this.listBoxAutoComplete.Hide();
            this.toolTip.Hide(this);
        }

        private void ShowIntellisenceDropDown(int charIndex)
        {
            if (this.listBoxAutoComplete.Items.Count > 0)
            {
                // Find the position of the caret          
                Point clientPoint = this.GetPositionFromCharIndex(charIndex - 1);
                clientPoint.Y += (int)Math.Ceiling(this.Font.GetHeight()) + 2;
                clientPoint.X -= 6;
                if (charIndex > 0 && this.Text[charIndex - 1] == '\n')
                {
                    clientPoint.Y += (int)Math.Ceiling(this.Font.GetHeight());
                    clientPoint.X = this.GetPositionFromCharIndex(0).X - 6;
                }

                Point parentScreenLocation = TopLevelControl.PointToScreen(new Point(0, 0));
                Point locationInDialog = PointToScreen(clientPoint);
                locationInDialog.Offset(-parentScreenLocation.X, -parentScreenLocation.Y);

                //Fix location and size to avoid clipping
                Size topLevelControlSize = (TopLevelControl is Form) ? ((Form)TopLevelControl).ClientSize : TopLevelControl.Size;
                Rectangle listboxRectangle = new Rectangle(locationInDialog, this.listBoxAutoComplete.Size);

                if (listboxRectangle.Right > topLevelControlSize.Width)
                {
                    if (this.listBoxAutoComplete.Size.Width > topLevelControlSize.Width)
                        this.listBoxAutoComplete.Size = new Size(topLevelControlSize.Width, this.listBoxAutoComplete.Height);

                    locationInDialog = new Point(topLevelControlSize.Width - this.listBoxAutoComplete.Size.Width, locationInDialog.Y);
                }
                if (listboxRectangle.Bottom > topLevelControlSize.Height)
                    this.listBoxAutoComplete.Size = new Size(this.listBoxAutoComplete.Width, topLevelControlSize.Height - listboxRectangle.Top);

                // set position and show
                this.listBoxAutoComplete.Location = locationInDialog;
                this.listBoxAutoComplete.BringToFront();
                this.listBoxAutoComplete.Show();
            }
        }

        private void UpdateIntellisenceDropDown(string text)
        {
            AutoCompletionEventArgs autoCompletionEventArgs = new AutoCompletionEventArgs();
            autoCompletionEventArgs.Prefix = text;
            if (this.PopulateAutoCompleteList != null)
                this.PopulateAutoCompleteList(this, autoCompletionEventArgs);

            PopulateListBox(autoCompletionEventArgs.AutoCompleteValues);
        }

        private void UpdateAutoCompleteSelection(string currentValue)
        {
            bool wordMatched = false;

            if (string.IsNullOrEmpty(currentValue.Trim()) && this.listBoxAutoComplete.Items.Count > 0)
            {
                wordMatched = true;
                this.listBoxAutoComplete.Items[0].Selected = true;
                this.listBoxAutoComplete.Items[0].Focused = true;
            }
            else
            {
                for (int i = 0; i < this.listBoxAutoComplete.Items.Count; i++)
                {
                    if (this.listBoxAutoComplete.Items[i].Text.StartsWith(currentValue, StringComparison.OrdinalIgnoreCase))
                    {
                        wordMatched = true;
                        this.listBoxAutoComplete.Items[i].Selected = true;
                        this.listBoxAutoComplete.Items[i].Focused = true;
                        break;
                    }
                }
            }
            if (!wordMatched && this.listBoxAutoComplete.SelectedItems.Count == 1)
                this.listBoxAutoComplete.SelectedItems[0].Selected = false;
        }

        private void ShowToolTip(int charIndex, string prefix)
        {
            Point clientPoint = this.GetPositionFromCharIndex(charIndex - 1);
            clientPoint.Y += (int)Math.Ceiling(this.Font.GetHeight()) + 2;
            clientPoint.X -= 6;

            AutoCompletionEventArgs autoCompletionEventArgs = new AutoCompletionEventArgs();
            autoCompletionEventArgs.Prefix = prefix;

            if (this.PopulateToolTipList != null)
            {
                this.PopulateToolTipList(this, autoCompletionEventArgs);

                if (autoCompletionEventArgs.AutoCompleteValues != null)
                {
                    StringBuilder toolTipText = new StringBuilder();
                    bool firstMethod = true;
                    foreach (MemberInfo memberInfo in autoCompletionEventArgs.AutoCompleteValues)
                    {
                        if (firstMethod)
                            firstMethod = false;
                        else
                            toolTipText.Append("\n");

                        ParameterInfo[] parameters = null;

                        MethodInfo methodInfo = memberInfo as MethodInfo;
                        if (methodInfo != null)
                        {
                            toolTipText.Append(RuleDecompiler.DecompileType(methodInfo.ReturnType));
                            toolTipText.Append(" ");
                            toolTipText.Append(methodInfo.Name);
                            toolTipText.Append("(");

                            parameters = methodInfo.GetParameters();
                        }
                        else
                        {
                            // Must be constructor... if not, the best thing to do is let it throw "invalid cast".
                            ConstructorInfo ctorInfo = (ConstructorInfo)memberInfo;

                            toolTipText.Append(RuleDecompiler.DecompileType(ctorInfo.DeclaringType));
                            toolTipText.Append("(");

                            parameters = ctorInfo.GetParameters();
                        }


                        if (parameters != null && parameters.Length > 0)
                        {
                            int lastParamIndex = parameters.Length - 1;

                            // Append the first parameter
                            AppendParameterInfo(toolTipText, parameters[0], 0 == lastParamIndex);
                            for (int i = 1; i < parameters.Length; ++i)
                            {
                                toolTipText.Append(", ");
                                AppendParameterInfo(toolTipText, parameters[i], i == lastParamIndex);
                            }
                        }

                        toolTipText.Append(")");
                    }
                    this.toolTip.Show(toolTipText.ToString(), this, clientPoint);
                }
            }
        }

        private static void AppendParameterInfo(StringBuilder toolTipText, ParameterInfo parameterInfo, bool isLastParameter)
        {
            Type paramType = parameterInfo.ParameterType;
            if (paramType != null)
            {
                if (paramType.IsByRef)
                {
                    if (parameterInfo.IsOut)
                        toolTipText.Append("out ");
                    else
                        toolTipText.Append("ref ");

                    paramType = paramType.GetElementType();
                }
                else if (isLastParameter && paramType.IsArray)
                {
                    object[] attrs = parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                        toolTipText.Append("params ");
                }

                toolTipText.Append(RuleDecompiler.DecompileType(paramType));
                toolTipText.Append(" ");
            }
            toolTipText.Append(parameterInfo.Name);
        }

        private string CurrentPrefix
        {
            get
            {
                string textTillCaret = this.Text.Substring(0, this.SelectionStart);

                int prefixStart = textTillCaret.LastIndexOfAny(" .()[]\t\r\n".ToCharArray());
                if (prefixStart >= 0)
                    return textTillCaret.Substring(prefixStart + 1);
                else
                    return textTillCaret;
            }
        }

        #endregion

        #region override members

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // bail out an editcontrol before giving up on the dialog
            if (this.listBoxAutoComplete.Visible)
            {
                switch (keyData)
                {
                    case Keys.Enter:
                    case Keys.Tab:
                        this.SelectItem();
                        HideIntellisenceDropDown();
                        return true;
                    case Keys.Escape:
                        HideIntellisenceDropDown();
                        return true;
                    default:
                        break;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }

    #endregion

    #region AutoCompletionEventArgs

    internal class AutoCompletionEventArgs : EventArgs
    {
        private string prefix;
        ICollection autoCompleteValues;

        public ICollection AutoCompleteValues
        {
            get
            {
                return autoCompleteValues;
            }
            set
            {
                autoCompleteValues = value;
            }
        }

        public string Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;
            }
        }
    }

    #endregion
}
