thisdir = class/System.Windows.Forms/Gtk
SUBDIRS = 
include ../../../build/rules.make

CSCOMPILE=mcs

# We can't build this by default because it depends on gtk-sharp.

# Skip build/library.make for the moment -- we don't need its more advanced
# features and our build rules are more complicated than it can
# handle.

EXES = demo.exe Demo2.exe Demo3.exe singlewidget.exe
LIBRARY = System.Windows.Forms.dll 

LIB_MCS_FLAGS = /r:Accessibility /r:glib-sharp.dll /r:gdk-sharp.dll /r:gtk-sharp.dll \
    /r:gnome-sharp.dll /r:System.Drawing.dll /r:pango-sharp.dll	\
 	-L `pkg-config --variable=prefix gtk-sharp`/lib

EXE_MCS_FLAGS = /r:$(LIBRARY) /r:System.Drawing.dll /r:System.Data /r:ByteFX.Data.dll 

LIB_SOURCES = \
	AccessibleEvents.cs	\
	AccessibleNavigation.cs	\
	AccessibleObject.cs	\
	AccessibleRole.cs	\
	AccessibleSelection.cs	\
	AccessibleStates.cs	\
	AnchorStyles.cs	\
	ApplicationContext.cs	\
	Application.cs	\
	BaseCollection.cs	\
	BindingContext.cs	\
	Binding.cs	\
	BindingManagerBase.cs	\
	BindingMemberInfo.cs	\
	BindingsCollection.cs	\
	BorderStyle.cs	\
	BoundsSpecified.cs	\
	ButtonBase.cs	\
	Button.cs	\
	CheckBox.cs	\
	ColorDialog.cs	\
	ComboBox.cs	\
	CommonDialog.cs	\
	ComVisible.cs	\
	ContainerControl.cs	\
	ContextMenu.cs	\
	ControlBindingsCollection.cs	\
	Control.cs	\
	ControlEventArgs.cs	\
	ControlEventHandler.cs	\
	ControlStyles.cs	\
	ConvertEventArgs.cs	\
	ConvertEventHandler.cs	\
	CreateParams.cs	\
	CurrencyManager.cs	\
	Cursor.cs	\
	DataGridBoolColumn.cs	\
	DataGridCell.cs	\
	DataGridColumnStyle.cs	\
	DataGrid.cs	\
	DataGridLineStyle.cs	\
	DataGridParentRowsLabelStyle.cs	\
	DataGridPreferredColumnWidthTypeConverter.cs	\
	DataGridTableStyle.cs	\
	DataGridTextBoxColumn.cs	\
	DataGridTextBox.cs	\
	DialogResult.cs	\
	DockStyle.cs	\
	DragAction.cs	\
	DragDropEffects.cs	\
	DragEventArgs.cs	\
	DragEventHandler.cs	\
	DrawItemEventArgs.cs	\
	DrawItemEventHandler.cs	\
	DrawItemState.cs	\
	FileDialog.cs	\
	FlatStyle.cs	\
	FolderBrowserDialog.cs	\
	FontDialog.cs	\
	FormBorderStyle.cs	\
	Form.cs	\
	FormStartPosition.cs	\
	FormWindowState.cs	\
	GiveFeedbackEventArgs.cs	\
	GiveFeedbackEventHandler.cs	\
	GridColumnStylesCollection.cs	\
	GridTableStylesCollection.cs	\
	GroupBox.cs	\
	HelpEventArgs.cs	\
	HelpEventHandler.cs	\
	HorizontalAlignment.cs	\
	HScrollBar.cs	\
	IButtonControl.cs	\
	IContainerControl.cs	\
	IDataGridColumnStyleEditingNotificationService.cs	\
	IDataGridEditingService.cs	\
	IDataObject.cs	\
	ImageList.cs	\
	ImeMode.cs	\
	IMessageFilter.cs	\
	InputLanguageChangedEventArgs.cs	\
	InputLanguageChangedEventHandler.cs	\
	InputLanguageChangingEventArgs.cs	\
	InputLanguageChangingEventHandler.cs	\
	InputLanguageCollection.cs	\
	InputLanguage.cs	\
	InvalidateEventArgs.cs	\
	InvalidateEventHandler.cs	\
	ItemChangedEventArgs.cs	\
	ItemChangedEventHandler.cs	\
	IWin32Window.cs	\
	KeyEventArgs.cs	\
	KeyEventHandler.cs	\
	KeyPressEventArgs.cs	\
	KeyPressEventHandler.cs	\
	Keys.cs	\
	Label.cs	\
	LayoutEventArgs.cs	\
	LayoutEventHandler.cs	\
	LeftRightAlignment.cs	\
	ListBox.cs	\
	ListControl.cs	\
	MainMenu.cs	\
	MdiLayout.cs	\
	MeasureItemEventArgs.cs	\
	MeasureItemEventHandler.cs	\
	Menu.cs	\
	MenuItem.cs	\
	MenuMerge.cs	\
	MessageBoxButtons.cs	\
	MessageBox.cs	\
	Message.cs	\
	MouseButtons.cs	\
	MouseEventArgs.cs	\
	MouseEventHandler.cs	\
	NavigateEventArgs.cs	\
	NavigateEventHandler.cs	\
	OpenFileDialog.cs	\
	PageSetupDialog.cs	\
	PaintEventArgs.cs	\
	PaintEventHandler.cs	\
	PictureBox.cs	\
	PictureBoxSizeMode.cs	\
	PrintDialog.cs	\
	ProgressBar.cs	\
	QueryAccessibilityHelpEventArgs.cs	\
	QueryAccessibilityHelpEventHandler.cs	\
	QueryContinueDragEventArgs.cs	\
	QueryContinueDragEventHandler.cs	\
	RadioButton.cs	\
	RightToLeft.cs	\
	SaveFileDialog.cs	\
	ScrollableControl.cs	\
	ScrollBar.cs	\
	ScrollBars.cs	\
	ScrollEventArgs.cs	\
	ScrollEventHandler.cs	\
	ScrollEventType.cs	\
	Shortcut.cs	\
	SizeGripStyle.cs	\
	StatusBar.cs	\
	StatusBarDrawItemEventArgs.cs	\
	StatusBarDrawItemEventHandler.cs	\
	StatusBarPanelAutoSize.cs	\
	StatusBarPanelBorderStyle.cs	\
	StatusBarPanelClickEventArgs.cs	\
	StatusBarPanelClickEventHandler.cs	\
	StatusBarPanelCollection.cs	\
	StatusBarPanel.cs	\
	StatusBarPanelStyle.cs	\
	SWFGtkConv.cs	\
	TextBoxBase.cs	\
	TextBox.cs	\
	Timer.cs	\
	TODOAttribute.cs	\
	UICues.cs	\
	UICuesEventArgs.cs	\
	UICuesEventHandler.cs	\
	VScrollBar.cs	

all-local: $(LIBRARY) $(EXES)

clean-local:
	rm -f $(LIBRARY) $(EXES)

test-local run-test-local install-local:

DISTFILES = demo.cs Demo2.cs Demo3.cs singlewidget.cs $(LIB_SOURCES)

dist-local: dist-default

$(LIBRARY): $(LIB_SOURCES)
	$(CSCOMPILE) $(LIB_MCS_FLAGS) /target:library /out:$@ $^

%.exe: %.cs $(LIBRARY)
	$(CSCOMPILE) $(EXE_MCS_FLAGS) /target:exe /out:$@ $<
