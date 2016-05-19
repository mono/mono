//------------------------------------------------------------------------------
// <copyright file="StylesEditorDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Web.UI;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Util;

    using AttributeCollection = System.ComponentModel.AttributeCollection;
    using Control   = System.Windows.Forms.Control;

    using Button    = System.Windows.Forms.Button;
    using Label     = System.Windows.Forms.Label;
    using TextBox   = System.Windows.Forms.TextBox;
    using ListView  = System.Windows.Forms.ListView;
    using ListBox   = System.Windows.Forms.ListBox;

    using FontSize  = System.Web.UI.MobileControls.FontSize;
    using Style     = System.Web.UI.MobileControls.Style;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class StylesEditorDialog : DesignerForm
    {
        private StyleSheet         _styleSheet;
        private StyleSheet         _tempStyleSheet;
        private StyleSheetDesigner _styleSheetDesigner;
        private Style              _previewStyle;
        private Type               _currentNewStyleType;
        private bool               _firstActivate = true;

        private Button             _btnOK;
        private Button             _btnCancel;
        private Button             _btnUp;
        private Button             _btnDown;
        private Button             _btnAdd;
        private Button             _btnRemove;
        private TextBox            _txtType;
        private TreeView           _tvDefinedStyles;
        private ListView           _lvAvailableStyles;
        private PropertyGrid       _propertyBrowser;
        private MSHTMLHost         _samplePreview;
        private ContextMenu        _cntxtMenu;
        private MenuItem           _cntxtMenuItem;
        private TreeNode           _editCandidateNode = null;

        private StyleNode SelectedStyle
        {
            get 
            {
                Debug.Assert(_tvDefinedStyles != null);
                return _tvDefinedStyles.SelectedNode as StyleNode;
            }
            set
            {
                Debug.Assert(_tvDefinedStyles != null);
                _tvDefinedStyles.SelectedNode = value;
            }
        }

        protected override string HelpTopic {
            get { return "net.Mobile.StylesEditorDialog"; }
        }

        /// <summary>
        ///    Create a new StylesEditorDialog instance
        /// </summary>
        /// <internalonly/>
        internal StylesEditorDialog(StyleSheet stylesheet, 
            StyleSheetDesigner styleSheetDesigner,
            String initialStyleName) : base (stylesheet.Site)
        {
            if(stylesheet.DuplicateStyles.Count > 0)
            {
                GenericUI.ShowErrorMessage(
                    SR.GetString(SR.StylesEditorDialog_Title),
                    SR.GetString(SR.StylesEditorDialog_DuplicateStyleNames)
                );
                throw new ArgumentException(
                    SR.GetString(SR.StylesEditorDialog_DuplicateStyleException)
                );
            }
        
            _tempStyleSheet = new StyleSheet();
            _previewStyle   = new Style();

            _styleSheet         = stylesheet;
            _styleSheetDesigner = styleSheetDesigner;

            _tempStyleSheet.Site = _styleSheet.Site;

            InitializeComponent();

            InitAvailableStyles();
            LoadStyleItems();

            if (_tvDefinedStyles.Nodes.Count > 0)
            {
                int initialIndex = 0;
                if (initialStyleName != null)
                {
                    initialIndex = StyleIndex(initialStyleName);
                }
                SelectedStyle = (StyleNode)_tvDefinedStyles.Nodes[initialIndex];
                _tvDefinedStyles.Enabled = true;
                UpdateTypeText();
                UpdatePropertyGrid();
            }

            UpdateButtonsEnabling();
            UpdateFieldsEnabling();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (_tvDefinedStyles != null)
                {
                    foreach (StyleNode item in _tvDefinedStyles.Nodes)
                    {
                        item.Dispose();
                    }
                    _tvDefinedStyles = null;
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _btnOK             = new Button();
            _btnCancel         = new Button();
            _btnUp             = new Button();
            _btnDown           = new Button();
            _btnAdd            = new Button();
            _btnRemove         = new Button();

            _txtType           = new TextBox();
            _tvDefinedStyles   = new TreeView();
            _lvAvailableStyles = new ListView();
            _samplePreview     = new MSHTMLHost();
            _propertyBrowser   = new PropertyGrid();
            _cntxtMenuItem     = new MenuItem();
            _cntxtMenu         = new ContextMenu();

            GroupLabel grplblStyleList = new GroupLabel();
            grplblStyleList.SetBounds(6, 5, 432, 16);
            grplblStyleList.Text = SR.GetString(SR.StylesEditorDialog_StyleListGroupLabel);
            grplblStyleList.TabStop = false;
            grplblStyleList.TabIndex = 0;

            Label lblAvailableStyles = new Label();
            lblAvailableStyles.SetBounds(14, 25, 180, 16);
            lblAvailableStyles.Text = SR.GetString(SR.StylesEditorDialog_AvailableStylesCaption);
            lblAvailableStyles.TabStop = false;
            lblAvailableStyles.TabIndex = 1;

            ColumnHeader chStyleType = new System.Windows.Forms.ColumnHeader();
            ColumnHeader chStyleNamespace = new System.Windows.Forms.ColumnHeader();

            chStyleType.Width = 16;
            chStyleType.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            chStyleNamespace.Width = 16;
            chStyleNamespace.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;

            _lvAvailableStyles.SetBounds(14, 41, 180, 95);
            _lvAvailableStyles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            _lvAvailableStyles.MultiSelect = false;
            _lvAvailableStyles.HideSelection = false;
            _lvAvailableStyles.FullRowSelect = true;
            _lvAvailableStyles.View = System.Windows.Forms.View.Details;
            _lvAvailableStyles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] {chStyleType, chStyleNamespace});
            _lvAvailableStyles.SelectedIndexChanged += new EventHandler(this.OnNewStyleTypeChanged);
            _lvAvailableStyles.DoubleClick += new EventHandler(this.OnDoubleClick);
            _lvAvailableStyles.Sorting = SortOrder.Ascending;
            _lvAvailableStyles.TabIndex = 2;
            _lvAvailableStyles.TabStop = true;

            _btnAdd.AccessibleName = SR.GetString(SR.EditableTreeList_AddName);
            _btnAdd.AccessibleDescription = SR.GetString(SR.EditableTreeList_AddDescription);
            _btnAdd.Name = SR.GetString(SR.EditableTreeList_AddName);
            _btnAdd.SetBounds(198, 77, 32, 25);
            _btnAdd.Text = SR.GetString(SR.StylesEditorDialog_AddBtnCation);
            _btnAdd.Click += new EventHandler(this.OnClickAddButton);
            _btnAdd.TabIndex = 3;
            _btnAdd.TabStop = true;

            Label lblDefinedStyles = new Label();
            lblDefinedStyles.SetBounds(234, 25, 166, 16);
            lblDefinedStyles.Text = SR.GetString(SR.StylesEditorDialog_DefinedStylesCaption);
            lblDefinedStyles.TabStop = false;
            lblDefinedStyles.TabIndex = 4;;

            _tvDefinedStyles.SetBounds(234, 41, 166, 95);
            _tvDefinedStyles.AfterSelect += new TreeViewEventHandler(OnStylesSelected);
            _tvDefinedStyles.AfterLabelEdit += new NodeLabelEditEventHandler(OnAfterLabelEdit);
            _tvDefinedStyles.LabelEdit = true;
            _tvDefinedStyles.ShowPlusMinus = false;
            _tvDefinedStyles.HideSelection = false;
            _tvDefinedStyles.Indent = 15;
            _tvDefinedStyles.ShowRootLines = false;
            _tvDefinedStyles.ShowLines = false;
            _tvDefinedStyles.ContextMenu = _cntxtMenu;
            _tvDefinedStyles.TabIndex = 5;
            _tvDefinedStyles.TabStop = true;
            _tvDefinedStyles.KeyDown += new KeyEventHandler(OnKeyDown);
            _tvDefinedStyles.MouseUp += new MouseEventHandler(OnListMouseUp);
            _tvDefinedStyles.MouseDown += new MouseEventHandler(OnListMouseDown);

            _btnUp.AccessibleName = SR.GetString(SR.EditableTreeList_MoveUpName);
            _btnUp.AccessibleDescription = SR.GetString(SR.EditableTreeList_MoveUpDescription);
            _btnUp.Name = SR.GetString(SR.EditableTreeList_MoveUpName);
            _btnUp.SetBounds(404, 41, 28, 27);
            _btnUp.Click += new EventHandler(this.OnClickUpButton);
            _btnUp.Image = GenericUI.SortUpIcon; 
            _btnUp.TabIndex = 6;
            _btnUp.TabStop = true;

            _btnDown.AccessibleName = SR.GetString(SR.EditableTreeList_MoveDownName);
            _btnDown.AccessibleDescription = SR.GetString(SR.EditableTreeList_MoveDownDescription);
            _btnDown.Name = SR.GetString(SR.EditableTreeList_MoveDownName);
            _btnDown.SetBounds(404, 72, 28, 27);
            _btnDown.Click += new EventHandler(this.OnClickDownButton);
            _btnDown.Image = GenericUI.SortDownIcon;
            _btnDown.TabIndex = 7;
            _btnDown.TabStop = true;

            _btnRemove.AccessibleName = SR.GetString(SR.EditableTreeList_DeleteName);
            _btnRemove.AccessibleDescription = SR.GetString(SR.EditableTreeList_DeleteDescription);
            _btnRemove.Name = SR.GetString(SR.EditableTreeList_DeleteName);
            _btnRemove.SetBounds(404, 109, 28, 27);
            _btnRemove.Click += new EventHandler(this.OnClickRemoveButton);
            _btnRemove.Image = GenericUI.DeleteIcon;
            _btnRemove.TabIndex = 8;
            _btnRemove.TabStop = true;

            GroupLabel grplblStyleProperties = new GroupLabel();
            grplblStyleProperties.SetBounds(6, 145, 432, 16);
            grplblStyleProperties.Text = SR.GetString(SR.StylesEditorDialog_StylePropertiesGroupLabel);
            grplblStyleProperties.TabStop = false;
            grplblStyleProperties.TabIndex = 9;

            Label lblType = new Label();
            lblType.SetBounds(14, 165, 180, 16);
            lblType.Text = SR.GetString(SR.StylesEditorDialog_TypeCaption);
            lblType.TabIndex = 10;
            lblType.TabStop = false;

            _txtType.SetBounds(14, 181, 180, 16);
            _txtType.ReadOnly = true;
            _txtType.TabIndex = 11;
            _txtType.TabStop = true;

            Label lblSample = new Label();
            lblSample.SetBounds(14, 213, 180, 16);
            lblSample.Text = SR.GetString(SR.StylesEditorDialog_SampleCaption);
            lblSample.TabStop = false;
            lblSample.TabIndex = 12;

            _samplePreview.SetBounds(14, 229, 180, 76);
            _samplePreview.TabStop = false;
            _samplePreview.TabIndex = 13;

            Label lblProperties = new Label();
            lblProperties.SetBounds(234, 165, 198, 16);
            lblProperties.Text = SR.GetString(SR.StylesEditorDialog_PropertiesCaption);
            lblProperties.TabIndex = 14;
            lblProperties.TabStop = false;

            _propertyBrowser.SetBounds(234, 181, 198, 178);
            _propertyBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            _propertyBrowser.ToolbarVisible = false;
            _propertyBrowser.HelpVisible = false;
            _propertyBrowser.TabIndex = 15;
            _propertyBrowser.TabStop = true;
            _propertyBrowser.PropertySort = PropertySort.Alphabetical;
            _propertyBrowser.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnPropertyValueChanged);

            _btnOK.DialogResult = DialogResult.OK;
            _btnOK.Location = new System.Drawing.Point(282, 370);
            _btnOK.Size = new System.Drawing.Size(75, 23);
            _btnOK.TabIndex = 16;
            _btnOK.Text = SR.GetString(SR.GenericDialog_OKBtnCaption);
            _btnOK.Click += new EventHandler(this.OnClickOKButton);

            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new System.Drawing.Point(363, 370);
            _btnCancel.Size = new System.Drawing.Size(75, 23);
            _btnCancel.TabIndex = 17;
            _btnCancel.Text = SR.GetString(SR.GenericDialog_CancelBtnCaption);

            _cntxtMenuItem.Text = SR.GetString(SR.EditableTreeList_Rename);
            _cntxtMenu.MenuItems.Add(_cntxtMenuItem);
            _cntxtMenu.Popup += new EventHandler(OnPopup);
            _cntxtMenuItem.Click += new EventHandler(OnContextMenuItemClick);

            GenericUI.InitDialog(this, _styleSheet.Site);

            this.Text = _styleSheet.ID + " - " + SR.GetString(SR.StylesEditorDialog_Title);
            this.ClientSize = new Size(444, 401);
            this.AcceptButton = _btnOK;
            this.CancelButton = _btnCancel;
            this.Activated += new System.EventHandler(StylesEditorDialog_Activated);
            this.Controls.AddRange(new Control[]
                           {
                               grplblStyleList,
                               lblAvailableStyles,
                               _lvAvailableStyles,
                               _btnAdd,
                               lblDefinedStyles,
                               _tvDefinedStyles,
                               _btnUp,
                               _btnDown,
                               _btnRemove,
                               grplblStyleProperties,
                               lblType,
                               _txtType,
                               lblSample,
                               _samplePreview,
                               lblProperties,
                               _propertyBrowser,
                               _btnOK,
                               _btnCancel,
                           });
        }

        private void InitAvailableStyles() 
        {
            //int[] colMaxWidth = { _lvAvailableStyles.Columns[0].Width, _lvAvailableStyles.Columns[1].Width };
            int[] colMaxWidth = { 68, 202 };
            int[] colReqWidth = { 0, 0 };

            // NOTE: Currently no way for third party extenders to add their
            // own styles.  They'll need to specify complete name with tagprefix included.
            StringCollection mobileStyles = new StringCollection();
            mobileStyles.AddRange(
                new String[2]{"System.Web.UI.MobileControls.PagerStyle",
                                 "System.Web.UI.MobileControls.Style"});

            foreach (String mobileStyle in mobileStyles)
            {
                Type type = Type.GetType(mobileStyle, true);
                String[] subItems = {type.Name, type.Namespace};
                ListViewItem item = new ListViewItem(subItems);
                _lvAvailableStyles.Items.Add(item);
            }

            ICollection styles = _styleSheet.Styles;
            foreach (String key in styles)
            {
                Style style = (Style) _styleSheet[key];
                Type type = style.GetType();
                if (!mobileStyles.Contains(type.FullName))
                {
                    String[] subItems = {type.Name, type.Namespace};
                    ListViewItem item = new ListViewItem(subItems);
                    _lvAvailableStyles.Items.Add(item);

                    // Rectangle rcLvi = lvi.GetBounds((int) ItemBoundsPortion.Label);
                    // use a method like GetExtendPoint32
                    colReqWidth[0] = 68;
                    if (colReqWidth[0] > colMaxWidth[0])
                    {
                        colMaxWidth[0] = colReqWidth[0];
                    }
                    // use a method like GetExtendPoint32
                    colReqWidth[1] = 202;
                    if (colReqWidth[1] > colMaxWidth[1])
                    {
                        colMaxWidth[1] = colReqWidth[1];
                    }
                }
            }
            _lvAvailableStyles.Columns[0].Width = colMaxWidth[0] + 4;
            _lvAvailableStyles.Columns[1].Width = colMaxWidth[1] + 4;

            Debug.Assert(_lvAvailableStyles.Items.Count > 0);
            _lvAvailableStyles.Sort();
            _lvAvailableStyles.Items[0].Selected = true;
            _currentNewStyleType = Type.GetType((String) _lvAvailableStyles.Items[0].SubItems[1].Text + "." + 
                _lvAvailableStyles.Items[0].Text, true);
        }

        private void SaveComponent()
        {
            // Clear old styles
            _styleSheet.Clear();

            foreach (StyleNode styleNode in _tvDefinedStyles.Nodes)
            {
                _styleSheet[styleNode.RuntimeStyle.Name] = styleNode.RuntimeStyle;
                styleNode.RuntimeStyle.SetControl(_styleSheet);
            }

            // Delete CurrentStyle if it does not exist any more.
            if (_styleSheetDesigner.CurrentStyle != null && 
                null == _styleSheet[_styleSheetDesigner.CurrentStyle.Name])
            {
                _styleSheetDesigner.CurrentStyle = null;
                _styleSheetDesigner.CurrentChoice = null;
            }

            _styleSheetDesigner.OnStylesChanged();
        }

        private void LoadStyleItems() 
        {
            ICollection styles = _styleSheet.Styles;

            foreach (String key in styles)
            {
                Style style = (Style) _styleSheet[key];
                Style newStyle = (Style) Activator.CreateInstance(style.GetType());
                
                PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(style);

                for (int i = 0; i < propDescs.Count; i++) 
                {
                    if (propDescs[i].Name.Equals("Font"))
                    {
                        foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(style.Font))
                        {
                            desc.SetValue(newStyle.Font, desc.GetValue(style.Font));
                        }
                    }
                    else if (!propDescs[i].IsReadOnly)
                    {
                        propDescs[i].SetValue(newStyle, propDescs[i].GetValue(style));
                    }
                }

                _tempStyleSheet[newStyle.Name] = newStyle;
                newStyle.SetControl(_tempStyleSheet);

                StyleNode newStyleItem = new StyleNode(newStyle);
                _tvDefinedStyles.Nodes.Add(newStyleItem);
            }
        }

        private void UpdateButtonsEnabling()
        {
            if (SelectedStyle == null)
            {
                _btnUp.Enabled = false;
                _btnDown.Enabled = false;
            }
            else
            {
                _btnUp.Enabled = (SelectedStyle.Index > 0);
                _btnDown.Enabled = (SelectedStyle.Index < _tvDefinedStyles.Nodes.Count - 1);
            }

            _btnRemove.Enabled = (SelectedStyle != null);
        }

        private void UpdateFieldsEnabling()
        {
            _propertyBrowser.Enabled = 
                _tvDefinedStyles.Enabled = (SelectedStyle != null);
        }

        private String AutoIDStyle()
        {
            String newStyleID = _currentNewStyleType.Name;

            int i = 1;
            while (StyleIndex(newStyleID + i.ToString(CultureInfo.InvariantCulture)) >= 0)
            {
                i++;
            }
            return newStyleID + i.ToString(CultureInfo.InvariantCulture);
        }

        private int StyleIndex(String name)
        {
            int index = 0;
            foreach (StyleNode styleNode in _tvDefinedStyles.Nodes)
            {
                if (String.Compare(name, styleNode.RuntimeStyle.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        private void UpdatePropertyGrid()
        {
            _propertyBrowser.SelectedObject = (SelectedStyle == null) ? 
                null : ((StyleNode)SelectedStyle).RuntimeStyle;
        }

        private void UpdateTypeText()
        {
            if (SelectedStyle == null)
            {
                _txtType.Text = String.Empty;
            }
            else
            {
                _txtType.Text = ((StyleNode)SelectedStyle).FullName;
            }
        }

        /// <summary>
        ///    Update scheme preview
        /// </summary>
        /// <internalonly/>
        private void UpdateSamplePreview()
        {
            if (_firstActivate)
            {
                return;
            }

            NativeMethods.IHTMLDocument2 tridentDocument = _samplePreview.GetDocument();
            NativeMethods.IHTMLElement documentElement = tridentDocument.GetBody();
            NativeMethods.IHTMLBodyElement bodyElement;

            bodyElement = (NativeMethods.IHTMLBodyElement) documentElement;
            bodyElement.SetScroll("no");

            if (SelectedStyle == null)
            {
                documentElement.SetInnerHTML(String.Empty);
                tridentDocument.SetBgColor("buttonface");
                return;
            }
            else
            {
                tridentDocument.SetBgColor(String.Empty);
            }

            bool cycle = ReferencesContainCycle(SelectedStyle);
            if (cycle)
            {
                documentElement.SetInnerHTML(String.Empty);
                return;
            }

            // apply the current Style to label
            ApplyStyle();

            DesignerTextWriter tw = new DesignerTextWriter();

            //ToolTip
            tw.AddAttribute("title", ((StyleNode)SelectedStyle).RuntimeStyle.Name);

            // ForeColor
            Color c = _previewStyle.ForeColor;
            if (!c.Equals(Color.Empty))
            {
                tw.AddStyleAttribute("color", ColorTranslator.ToHtml(c));
            }

            // BackColor
            c = _previewStyle.BackColor;
            if (!c.Equals(Color.Empty))
            {
                tw.AddStyleAttribute("background-color", ColorTranslator.ToHtml(c));
            }

            // Font Name
            String name = _previewStyle.Font.Name;
            if (name.Length > 0)
            {
                tw.AddStyleAttribute("font-family", name);
            }

            // Font Size
            switch (_previewStyle.Font.Size)
            {
                case FontSize.Large :
                    tw.AddStyleAttribute("font-size", "Medium");
                    break;
                case FontSize.Small :
                    tw.AddStyleAttribute("font-size", "X-Small");
                    break;
                default:
                    tw.AddStyleAttribute("font-size", "Small");
                    break;
            }

            // Font Style
            if (_previewStyle.Font.Bold == BooleanOption.True)
            {
                tw.AddStyleAttribute("font-weight", "bold");
            }
            if (_previewStyle.Font.Italic == BooleanOption.True)
            {
                tw.AddStyleAttribute("font-style", "italic");
            }

            tw.RenderBeginTag("span");
            tw.Write(SR.GetString(SR.StylesEditorDialog_PreviewText));
            tw.RenderEndTag();

            // and show it!
            String finalHTML = "<div align='center'><table width='100%' height='100%'><tr><td><p align='center'>" +
                tw.ToString() + "</p></td></tr></table></div>";
            documentElement.SetInnerHTML(finalHTML);
        }

        /*
         *  BEGIN EVENT HANDLING
         */
        Timer _delayTimer;
        private void StylesEditorDialog_Activated(Object sender, System.EventArgs e)
        {
            if (!_firstActivate)
            {
                return;
            }
            _firstActivate = false;

            _samplePreview.CreateTrident();
            _samplePreview.ActivateTrident();

            UpdateSamplePreview();

            _delayTimer = new Timer();
            _delayTimer.Interval = 100;
            _delayTimer.Tick += new EventHandler(this.OnActivateDefinedStyles);
            _delayTimer.Start();
        }

        private void OnActivateDefinedStyles(Object sender, System.EventArgs e)
        {
            _delayTimer.Stop();
            _delayTimer.Tick -= new EventHandler(this.OnActivateDefinedStyles);
            
            _lvAvailableStyles.Focus();
        }

        internal delegate void StyleRenamedEventHandler(
            Object source, StyleRenamedEventArgs e);
        
        internal event StyleRenamedEventHandler StyleRenamed;

        private void OnStyleRenamed(StyleRenamedEventArgs e)
        {
            if(StyleRenamed != null)
            {
                StyleRenamed(this, e);
            }
        }

        private void OnAfterLabelEdit(Object source, NodeLabelEditEventArgs e)
        {
            Debug.Assert(null != e);
            Debug.Assert(e.CancelEdit == false);

            // this happens when the label is unchanged after entering and exiting
            // label editing mode - bizarre behavior. this may be a bug in treeview
            if (null == e.Label)
            {
                return;
            }

            String oldValue = e.Node.Text;
            String newValue = e.Label;

            String messageTitle = SR.GetString(SR.Style_ErrorMessageTitle);

            // can't accept a style name that already exists
            if (String.Compare(oldValue, newValue , StringComparison.OrdinalIgnoreCase) != 0 && StyleIndex(newValue) >= 0)
            {
                MessageBox.Show(
                    SR.GetString(SR.Style_DuplicateName),
                    messageTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                e.CancelEdit = true;
                return;
            }

            // can't accept an empty style name
            if (newValue.Length == 0)
            {
                MessageBox.Show(
                    SR.GetString(SR.StylesEditorDialog_EmptyName), 
                    messageTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                e.CancelEdit = true;
                return;
            }

            /* Removed for DCR 4240
            // can't accept an illegal style name
            if (!DesignerUtility.IsValidName(newValue))
            {
                MessageBox.Show(
                    SR.GetString(SR.Style_InvalidName, newValue),
                    messageTitle, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                e.CancelEdit = true;
                return;
            }
            */

            SelectedStyle.RuntimeStyle.Name = newValue;
            _tempStyleSheet.Remove(oldValue);
            _tempStyleSheet[newValue] = SelectedStyle.RuntimeStyle;

            if (ReferencesContainCycle(SelectedStyle))
            {
                // Restore original settings
                SelectedStyle.RuntimeStyle.Name = oldValue;
                _tempStyleSheet.Remove(newValue);
                _tempStyleSheet[oldValue] = SelectedStyle.RuntimeStyle;

                MessageBox.Show(
                    SR.GetString(SR.Style_NameChangeCauseCircularLoop),
                    messageTitle, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                e.CancelEdit = true;
                return;
            }

            // Raise StyleRenamed event for any styles which vere
            // renamed.
            OnStyleRenamed(
                new StyleRenamedEventArgs(
                    oldValue,
                    newValue
                )
            );
        }

        private void OnCreateNewStyle()
        {
            String newStyleAutoID = AutoIDStyle();

            Style newStyle = (Style)Activator.CreateInstance(_currentNewStyleType);
            Debug.Assert(newStyle != null);
            newStyle.Name = newStyleAutoID;

            // Add this style to StyleSheet
            _tempStyleSheet[newStyle.Name] = newStyle;
            newStyle.SetControl(_tempStyleSheet);

            StyleNode newStyleItem = new StyleNode(newStyle);

            _tvDefinedStyles.Enabled = true;
            _propertyBrowser.Enabled = true;
            _tvDefinedStyles.Nodes.Add(newStyleItem);
            SelectedStyle = newStyleItem;

            UpdateSamplePreview();
            UpdateButtonsEnabling();
        }

        private void OnClickOKButton(Object sender, EventArgs e)
        {
            SaveComponent();
            Close();
            DialogResult = DialogResult.OK;
        }

        private void OnStylesSelected(Object sender, TreeViewEventArgs e)
        {
            UpdateTypeText();
            UpdatePropertyGrid();
            UpdateSamplePreview();
            UpdateButtonsEnabling();
            UpdateFieldsEnabling();
        }

        private void OnNewStyleTypeChanged(Object sender, EventArgs e)
        {
            if (_lvAvailableStyles.SelectedItems.Count != 0)
            {
                _currentNewStyleType = Type.GetType((String) _lvAvailableStyles.SelectedItems[0].SubItems[1].Text + "." + 
                                                _lvAvailableStyles.SelectedItems[0].Text, true);
                //Debug.Assert(typeof(Style).IsAssignableFrom(_currentNewStyleType), "Non style object passed in.");
            }
        }

        private void MoveSelectedNode(int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);
            
            StyleNode node = SelectedStyle;
            Debug.Assert(node != null);

            int index = node.Index;
            _tvDefinedStyles.Nodes.RemoveAt(index);
            _tvDefinedStyles.Nodes.Insert(index + direction, node);
            SelectedStyle = node;
        }

        private void OnClickUpButton(Object source, EventArgs e)
        {
            MoveSelectedNode(-1);
            UpdateButtonsEnabling();
        }

        private void OnClickDownButton(Object source, EventArgs e)
        {
            MoveSelectedNode(1);
            UpdateButtonsEnabling();
         }

        private void OnClickAddButton(Object sender, EventArgs e)
        {
            OnCreateNewStyle();
        }

        internal delegate void StyleDeletedEventHandler(
            Object source, StyleDeletedEventArgs e);
        
        internal event StyleDeletedEventHandler StyleDeleted;

        private void OnStyleDeleted(StyleDeletedEventArgs e)
        {
            if(StyleDeleted != null)
            {
                StyleDeleted(this, e);
            }
        }

        private void OnClickRemoveButton(Object source, EventArgs e)
        {
            Debug.Assert(SelectedStyle != null);

            String message = SR.GetString(SR.StylesEditorDialog_DeleteStyleMessage);
            String caption = SR.GetString(SR.StylesEditorDialog_DeleteStyleCaption);

            if (System.Windows.Forms.MessageBox.Show(message,
                caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                return;
            }

            String deletedStyle = ((StyleNode)SelectedStyle).RuntimeStyle.Name;

            // Remove this style from temporary StyleSheet
            _tempStyleSheet.Remove(deletedStyle);
            ((StyleNode)SelectedStyle).Dispose();

            int selectedIndex = SelectedStyle.Index;
            int stylesCount = _tvDefinedStyles.Nodes.Count;

            _tvDefinedStyles.Nodes.RemoveAt(selectedIndex);
            OnStyleDeleted(new StyleDeletedEventArgs(deletedStyle));

            if (selectedIndex < stylesCount-1)
            {
                SelectedStyle = (StyleNode) _tvDefinedStyles.Nodes[selectedIndex];
            }
            else if (selectedIndex >= 1)
            {
                SelectedStyle = (StyleNode) _tvDefinedStyles.Nodes[selectedIndex-1];
            }
            else if (stylesCount == 1)
            {
                SelectedStyle = null;
                UpdateTypeText();
                UpdatePropertyGrid();
                UpdateSamplePreview();
                UpdateButtonsEnabling();
                UpdateFieldsEnabling();
            }        
        }

        private void OnDoubleClick(Object sender, EventArgs e)
        {
            OnCreateNewStyle();
        }

        private void OnKeyDown(Object sender, KeyEventArgs e)
        {
            switch(e.KeyData)
            {
                case Keys.F2:
                {
                    if(SelectedStyle != null)
                    {
                        SelectedStyle.BeginEdit();
                    }
                    break;
                }
                case (Keys.Control | Keys.Home):
                {
                    if(_tvDefinedStyles.Nodes.Count > 0)
                    {
                        SelectedStyle = (StyleNode)_tvDefinedStyles.Nodes[0];
                    }
                    break;
                }
                case (Keys.Control | Keys.End):
                {
                    int numNodes = _tvDefinedStyles.Nodes.Count;
                    if(numNodes > 0)
                    {
                        SelectedStyle = (StyleNode)_tvDefinedStyles.Nodes[numNodes - 1];
                    }
                    break;
                }
            }
        }

        private void OnListMouseUp(Object sender, MouseEventArgs e)
        {
            _editCandidateNode= null;
            if (e.Button == MouseButtons.Right)
            {
                _editCandidateNode = (TreeNode)_tvDefinedStyles.GetNodeAt (e.X, e.Y);
            }
        }

        private void OnListMouseDown(Object sender, MouseEventArgs e)
        {
            _editCandidateNode = null;
            if (e.Button == MouseButtons.Right)
            {
                _editCandidateNode = (TreeNode)_tvDefinedStyles.GetNodeAt (e.X, e.Y);
            }
        }
        
        private void OnPopup(Object sender, EventArgs e)
        {
            _cntxtMenuItem.Enabled = (_editCandidateNode != null ||
                                                    _tvDefinedStyles.SelectedNode != null);
        }

        private void OnContextMenuItemClick(Object sender, EventArgs e)
        {
            if(_editCandidateNode == null)
            {
                // context menu key pressed
                if (_tvDefinedStyles.SelectedNode != null)
                {
                    _tvDefinedStyles.SelectedNode.BeginEdit();
                }
            }
            else
            {
                // right mouseclick
                _editCandidateNode.BeginEdit();
            }
            _editCandidateNode = null;
        }

        private void OnPropertyValueChanged(Object sender, PropertyValueChangedEventArgs e)
        {
            if (SelectedStyle == null)
            {
                return;
            }

            UpdateSamplePreview();
        }

        /*
         *  END EVENT HANDLING
         */

        private bool ReferencesContainCycle(StyleNode startingStyleItem)
        {
            StyleNode currentStyleItem = startingStyleItem;
            Style currentStyle = currentStyleItem.RuntimeStyle;
            String reference = currentStyle.StyleReference;
            bool found = true;
            bool cycle = false;

            // Clear referenced boolean
            foreach (StyleNode styleNode in _tvDefinedStyles.Nodes)
            {
                styleNode.Referenced = false;
            }

            // Set current style as referenced.
            currentStyleItem.Referenced = true;

            while ((reference != null && reference.Length > 0) && found && !cycle)
            {
                found = false;
                foreach (StyleNode styleNode in _tvDefinedStyles.Nodes)
                {
                    Style style = styleNode.RuntimeStyle;
                    if (0 == String.Compare(style.Name, reference, StringComparison.OrdinalIgnoreCase))
                    {
                        reference = style.StyleReference;
                        found = true;
                        if (styleNode.Referenced)
                        {
                            cycle = true;
                        }
                        else
                        {
                            styleNode.Referenced = true;
                        }
                        break;
                    }
                }

                // keep on looking. 
                // It depends on whether a style defined in web.config can have a reference or not.

/*              if we do need to keep on looking we need to store the Referenced flag
                // for those styles as well.
                // If not found, check default styles
                if (!found)
                {
                    if (null != StyleSheet.Default[reference])
                    {
                        Style style = StyleSheet.Default[reference];
                        reference = style.Reference;
                        found = true;
                        // get styleNode from other list
                        if (styleNode.Referenced)
                        {
                            cycle = true;
                        }
                        else
                        {
                            styleNode.Referenced = true;
                        }
                        break;
                    }
                }
*/
            }

            return cycle;
        }

        /// <summary>
        ///   Apply the currently selected style to the preview label.
        ///   This function should only be called after making sure that there is no
        ///   cycle that starts with _tvDefinedStyles.SelectedItem
        /// </summary>
        private void ApplyStyle()
        {
            StyleNode     currentStyleItem = (StyleNode)SelectedStyle;
            Style         currentStyle     = currentStyleItem.RuntimeStyle;

            Color         foreColor        = currentStyle.ForeColor;
            Color         backColor        = currentStyle.BackColor;
            BooleanOption fontBold         = currentStyle.Font.Bold;
            BooleanOption fontItalic       = currentStyle.Font.Italic;
            FontSize      fontSize         = currentStyle.Font.Size;
            String        fontName         = currentStyle.Font.Name;
            String        reference        = currentStyle.StyleReference;

            bool found = true;

            while ((reference != null && reference.Length > 0) && found)
            {
                found = false;
                foreach (StyleNode styleNode in _tvDefinedStyles.Nodes)
                {
                    Style style = styleNode.RuntimeStyle;
                    if (0 == String.Compare(style.Name, reference, StringComparison.OrdinalIgnoreCase))
                    {
                        if (foreColor == Color.Empty)
                        {
                            foreColor = style.ForeColor;
                        }
                        if (backColor == Color.Empty)
                        {
                            backColor = style.BackColor;
                        }
                        if (fontBold == BooleanOption.NotSet)
                        {
                            fontBold = style.Font.Bold;
                        }
                        if (fontItalic == BooleanOption.NotSet)
                        {
                            fontItalic = style.Font.Italic;
                        }
                        if (fontSize == FontSize.NotSet)
                        {
                            fontSize = style.Font.Size;
                        }
                        if (fontName.Length == 0)
                        {
                            fontName = style.Font.Name;
                        }
                        reference = style.StyleReference;
                        found = true;
                        break;
                    }
                }

                // If not found, check default styles
                if (!found)
                {
                    if (null != StyleSheet.Default[reference])
                    {
                        Style style = StyleSheet.Default[reference];
                        if (foreColor == Color.Empty)
                        {
                            foreColor = style.ForeColor;
                        }
                        if (backColor == Color.Empty)
                        {
                            backColor = style.BackColor;
                        }
                        if (fontBold == BooleanOption.NotSet)
                        {
                            fontBold = style.Font.Bold;
                        }
                        if (fontItalic == BooleanOption.NotSet)
                        {
                            fontItalic = style.Font.Italic;
                        }
                        if (fontSize == FontSize.NotSet)
                        {
                            fontSize = style.Font.Size;
                        }
                        if (fontName.Length == 0)
                        {
                            fontName = style.Font.Name;
                        }
                        reference = null;
                        found = true;
                        break;
                    }
                }
            }

            _previewStyle.ForeColor = foreColor;
            _previewStyle.BackColor = backColor;
            _previewStyle.Font.Name  = fontName;
            _previewStyle.Font.Size  = fontSize;
            _previewStyle.Font.Bold = fontBold;
            _previewStyle.Font.Italic = fontItalic;
        }

        /*
         *   BEGIN INTERNAL CLASS
         */

        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class StyleNode : TreeNode
        {
            private String          _fullName;
            private bool            _referenced;
            private Style           _runtimeStyle;
            private EventHandler    _styleReferenceChanged;
            private String          _styleReference;
   
            internal StyleNode(Style style)
            {
                _runtimeStyle = style;
                _fullName = style.GetType().FullName;
                _styleReference = RuntimeStyle.StyleReference;
                _styleReferenceChanged = new EventHandler(this.OnStyleReferenceChanged);
                base.Text = RuntimeStyle.Name;


                PropertyDescriptor property;
                property = TypeDescriptor.GetProperties(typeof(Style))["StyleReference"];
                Debug.Assert(property != null);
                property.AddValueChanged(RuntimeStyle, _styleReferenceChanged);
            }

            internal Style RuntimeStyle
            {
                get
                {
                    return _runtimeStyle;
                }
            }

            internal bool Referenced
            {
                get
                {
                    return _referenced;
                }

                set
                {
                    _referenced = value;
                }
            }

            internal String FullName
            {
                get
                {
                    return _fullName;
                }
            }

            internal void Dispose()
            {
                PropertyDescriptor property;
                property = TypeDescriptor.GetProperties(typeof(Style))["StyleReference"];
                Debug.Assert(property != null);
                property.RemoveValueChanged(RuntimeStyle, _styleReferenceChanged);
            }

            // Note that it return false if any of the referenced styles are already in a loop
            // ie. it returns true if and only if current style is in a loop now.
            private bool InCircularLoop()
            {
                StyleSheet styleSheet = (StyleSheet)RuntimeStyle.Control;
                Debug.Assert(styleSheet != null);

                String reference = RuntimeStyle.StyleReference;
                int count = styleSheet.Styles.Count + 1;

                while ((reference != null && reference.Length > 0) && count > 0)
                {
                    if (0 == String.Compare(RuntimeStyle.Name, reference, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        Style style = styleSheet[reference];
                        if (null != style)
                        {
                            reference = style.StyleReference;
                            count --;
                        }
                        else
                        {
                            reference = null;
                        }
                    }
                }

                return false;
            }

            private void OnStyleReferenceChanged(Object sender, EventArgs e)
            {
                if (InCircularLoop())
                {
                    RestoreStyleReference();
                    // new style reference creates a cycle
                    throw new Exception(SR.GetString(SR.Style_ReferenceCauseCircularLoop));
                }

                CacheStyleReference();
            }

            private void RestoreStyleReference()
            {
                RuntimeStyle.StyleReference = _styleReference;
            }

            private void CacheStyleReference()
            {
                _styleReference = RuntimeStyle.StyleReference;
            }
        }
    }

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class StyleRenamedEventArgs : EventArgs
    {
        private String _oldName;
        private String _newName;
        
        internal StyleRenamedEventArgs(
            String oldName,
            String newName)
        {
            _oldName = oldName;
            _newName = newName;
        }

        internal String OldName
        {
            get
            {
                return _oldName;
            }
        }

        internal String NewName
        {
            get
            {
                return _newName;
            }
        }

    }

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class StyleDeletedEventArgs : EventArgs
    {
        private String _name;
        
        internal StyleDeletedEventArgs(String name)
        {
            _name = name;
        }

        internal String Name
        {
            get
            {
                return _name;
            }
        }
    }
}
