thisdir = class/System.Windows.Forms/Gtk
SUBDIRS = 
include ../../../build/rules.make

# We can't build this by default because it depends on gtk-sharp.

# Skip build/library.make for the moment -- we don't need its more advanced
# features and our build rules are more complicated than it can
# handle.

EXES = demo.exe Demo2.exe singlewidget.exe
LIBRARY = System.Windows.Forms.dll 

LIB_MCS_FLAGS = /r:glib-sharp.dll /r:gdk-sharp.dll /r:gtk-sharp.dll \
    /r:gnome-sharp.dll /r:System.Drawing.dll -L `pkg-config --variable=prefix gtk-sharp`/lib

EXE_MCS_FLAGS = /r:$(LIBRARY) /r:System.Drawing.dll

LIB_SOURCES = \
	TODOAttribute.cs	\
	AnchorStyles.cs		\
	Application.cs		\
	ContainerControl.cs	\
	Button.cs		\
	ButtonBase.cs		\
	Control.cs		\
	ControlEventArgs.cs	\
	ControlEventHandler.cs	\
	DialogResult.cs		\
	Form.cs 		\
	IButtonControl.cs	\
	IContainerControl.cs	\
	Label.cs		\
	ScrollBars.cs	\
	ScrollBar.cs	\
	RightToLeft.cs	\
	VScrollBar.cs	\
	HScrollBar.cs	\
	ScrollEventHandler.cs \
	ScrollEventArgs.cs \
	ScrollEventType.cs \
	HorizontalAlignment.cs	\
	TextBoxBase.cs	\
	TextBox.cs	\
	CheckBox.cs	\
	RadioButton.cs	\
	GroupBox.cs	\
	ProgressBar.cs	\
	PictureBox.cs	\
	FileDialog.cs	\
	FontDialog.cs	\
	ColorDialog.cs	\
	ComboBox.cs	\
	MessageBox.cs	\
	MessageBoxButtons.cs	\
	MenuItem.cs	\
	MainMenu.cs	\
	Shortcut.cs	\
	PictureBoxSizeMode.cs	\
	ScrollableControl.cs \
	StatusBar.cs \
	StatusBarPanelBorderStyle.cs \
	StatusBarPanel.cs \
	StatusBarDrawItemEventArgs.cs \
	StatusBarPanelClickEventArgs.cs \
	StatusBarPanelStyle.cs \
	StatusBarDrawItemEventHandler.cs \
	StatusBarPanelClickEventHandler.cs \
	StatusBarPanelAutoSize.cs \
	StatusBarPanelCollection.cs \
	DrawItemEventArgs.cs \
	MouseEventArgs.cs \
	DrawItemState.cs \
	MouseButtons.cs \
	DockStyle.cs \
	ListControl.cs \
	ListBox.cs \
	LayoutEventArgs.cs 

all-local: $(LIBRARY) $(EXES)

clean-local:
	rm -f $(LIBRARY) $(EXES)

test-local run-test-local install-local:

DISTFILES = demo.cs Demo2.cs singlewidget.cs $(LIB_SOURCES)

dist-local: dist-default

$(LIBRARY): $(LIB_SOURCES)
	$(CSCOMPILE) $(LIB_MCS_FLAGS) /target:library /out:$@ $^

%.exe: %.cs $(LIBRARY)
	$(CSCOMPILE) $(EXE_MCS_FLAGS) /target:exe /out:$@ $<

