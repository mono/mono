thisdir = class/System.Windows.Forms/Gtk
SUBDIRS = 
include ../../../build/rules.make

CSCOMPILE=mcs

# We can't build this by default because it depends on gtk-sharp.

# Skip build/library.make for the moment -- we don't need its more advanced
# features and our build rules are more complicated than it can
# handle.

EXES = Samples/demo.exe \
		Samples/Demo2.exe \
		Samples/Demo3.exe \
		Samples/singlewidget.exe

LIBRARY = System.Windows.Forms.dll 

LIB_MCS_FLAGS = /r:Accessibility /r:glib-sharp.dll /r:gdk-sharp.dll /r:gtk-sharp.dll \
    /r:gnome-sharp.dll /r:System.Drawing.dll /r:pango-sharp.dll \
		-L `pkg-config --variable=prefix gtk-sharp`/lib

EXE_MCS_FLAGS = /r:$(LIBRARY) /r:System.Drawing.dll /r:System.Data /r:ByteFX.Data.dll 

LIB_SOURCES = \
	ConvertEventHandler.cs	\
	ApplicationContext.cs	\
	IDataObject.cs	\
	FontDialog.cs	\
	DragDropEffects.cs	\
	AccessibleEvents.cs	\
	AnchorStyles.cs	\
	StatusBarPanel.cs	\
	Button.cs	\
	MouseButtons.cs	\
	DataGridLineStyle.cs	\
	FileDialog.cs	\
	MainMenu.cs	\
	GiveFeedbackEventArgs.cs	\
	KeyPressEventHandler.cs	\
	ControlStyles.cs	\
	RadioButton.cs	\
	InputLanguageChangingEventHandler.cs	\
	ScrollEventType.cs	\
	CurrencyManager.cs	\
	VScrollBar.cs	\
	DrawItemState.cs	\
	DataGridPreferredColumnWidthTypeConverter.cs	\
	FormBorderStyle.cs	\
	FormWindowState.cs	\
	PaintEventHandler.cs	\
	IDataGridColumnStyleEditingNotificationService.cs	\
	KeyPressEventArgs.cs	\
	ScrollableControl.cs	\
	NavigateEventHandler.cs	\
	DataGridBoolColumn.cs	\
	InvalidateEventHandler.cs	\
	InputLanguageChangedEventArgs.cs	\
	ScrollBar.cs	\
	UICues.cs	\
	NavigateEventArgs.cs	\
	ScrollEventArgs.cs	\
	DataGridTextBox.cs	\
	ComVisible.cs	\
	StatusBarPanelBorderStyle.cs	\
	PictureBox.cs	\
	KeyEventHandler.cs	\
	AccessibleObject.cs	\
	PaintEventArgs.cs	\
	BoundsSpecified.cs	\
	GroupBox.cs	\
	DataGridTableStyle.cs	\
	ListControl.cs	\
	DataGrid.cs	\
	Cursor.cs	\
	DrawItemEventArgs.cs	\
	ScrollBars.cs	\
	DragEventHandler.cs	\
	ScrollEventHandler.cs	\
	InvalidateEventArgs.cs	\
	StatusBarDrawItemEventHandler.cs	\
	QueryAccessibilityHelpEventArgs.cs	\
	GiveFeedbackEventHandler.cs	\
	Label.cs	\
	AccessibleStates.cs	\
	Binding.cs	\
	MdiLayout.cs	\
	ButtonBase.cs	\
	ProgressBar.cs	\
	StatusBarPanelClickEventArgs.cs	\
	GridColumnStylesCollection.cs	\
	Keys.cs	\
	MessageBox.cs	\
	LeftRightAlignment.cs	\
	QueryContinueDragEventArgs.cs	\
	FlatStyle.cs	\
	AccessibleNavigation.cs	\
	IWin32Window.cs	\
	BindingMemberInfo.cs	\
	ConvertEventArgs.cs	\
	Control.cs	\
	StatusBarPanelAutoSize.cs	\
	StatusBarPanelCollection.cs	\
	CheckBox.cs	\
	DragEventArgs.cs	\
	MouseEventArgs.cs	\
	StatusBarPanelClickEventHandler.cs	\
	IButtonControl.cs	\
	MessageBoxButtons.cs	\
	KeyEventArgs.cs	\
	TextBox.cs	\
	DialogResult.cs	\
	Application.cs	\
	ControlEventHandler.cs	\
	StatusBarPanelStyle.cs	\
	RightToLeft.cs	\
	UICuesEventArgs.cs	\
	Shortcut.cs	\
	TODOAttribute.cs	\
	AccessibleSelection.cs	\
	MeasureItemEventHandler.cs	\
	Menu.cs	\
	ImeMode.cs	\
	DockStyle.cs	\
	HScrollBar.cs	\
	ControlBindingsCollection.cs	\
	DataGridColumnStyle.cs	\
	StatusBar.cs	\
	PictureBoxSizeMode.cs	\
	SWFGtkConv.cs	\
	ContainerControl.cs	\
	CreateParams.cs	\
	ContextMenu.cs	\
	ListBox.cs	\
	ComboBox.cs	\
	IMessageFilter.cs	\
	InputLanguageCollection.cs	\
	LayoutEventHandler.cs	\
	DrawItemEventHandler.cs	\
	MenuItem.cs	\
	InputLanguage.cs	\
	DataGridTextBoxColumn.cs	\
	HorizontalAlignment.cs	\
	ColorDialog.cs	\
	UICuesEventHandler.cs	\
	Form.cs	\
	DragAction.cs	\
	QueryContinueDragEventHandler.cs	\
	IDataGridEditingService.cs	\
	BindingManagerBase.cs	\
	ImageList.cs	\
	FormStartPosition.cs	\
	QueryAccessibilityHelpEventHandler.cs	\
	LayoutEventArgs.cs	\
	BaseCollection.cs	\
	BorderStyle.cs	\
	ControlEventArgs.cs	\
	ItemChangedEventHandler.cs	\
	AccessibleRole.cs	\
	MouseEventHandler.cs	\
	StatusBarDrawItemEventArgs.cs	\
	IContainerControl.cs	\
	InputLanguageChangingEventArgs.cs	\
	MenuMerge.cs	\
	TextBoxBase.cs	\
	ItemChangedEventArgs.cs	\
	BindingContext.cs	\
	DataGridParentRowsLabelStyle.cs	\
	GridTableStylesCollection.cs	\
	MeasureItemEventArgs.cs	\
	HelpEventHandler.cs	\
	SizeGripStyle.cs	\
	DataGridCell.cs	\
	BindingsCollection.cs	\
	HelpEventArgs.cs	\
	Message.cs	\
	InputLanguageChangedEventHandler.cs	\
	CommonDialog.cs	\
	OpenFileDialog.cs	\
	SaveFileDialog.cs	\
	FolderBrowserDialog.cs	\
	PageSetupDialog.cs	\
	PrintDialog.cs	\
	Timer.cs	\
	DrawMode.cs	\
	ComboBoxStyle.cs	\
	Appearance.cs	\
	CheckState.cs	\
	Panel.cs	\
	TabPage.cs	\
	TabControl.cs	\
	TabAlignment.cs	\
	TabAppearance.cs	\
	TabDrawMode.cs	\
	TabSizeMode.cs	\
	Cursors.cs	\
	MessageBoxDefaultButton.cs	\
	MessageBoxIcon.cs	\
	MessageBoxOptions.cs \
	AssemblyInfo.cs \
	SelectionMode.cs \
	CheckedListBox.cs \
	ItemCheckEventHandler.cs \
	ItemCheckEventArgs.cs



all-local: $(LIBRARY) $(EXES)

clean-local:
	rm -f $(LIBRARY) $(EXES)

test-local run-test-local install-local:

DISTFILES = Samples/demo.cs Samples/Demo2.cs Samples/Demo3.cs Samples/singlewidget.cs $(LIB_SOURCES)

dist-local: dist-default

$(LIBRARY): $(LIB_SOURCES)
	$(CSCOMPILE) $(LIB_MCS_FLAGS) /target:library /out:$@ $^

%.exe: %.cs $(LIBRARY)
	$(CSCOMPILE) $(EXE_MCS_FLAGS) /target:exe /out:$@ $<
