CSC=mcs
SWFF=-r gdk-sharp -r gtk-sharp -r glib-sharp -r System.Drawing -r gnome-sharp 

SOURCES = \
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
	ScrollableControl.cs

all: demo.exe Demo2.exe

demo.exe: demo.cs System.Windows.Forms.dll
	$(CSC) demo.cs -r System.Windows.Forms.dll -r System.Drawing 

Demo2.exe: Demo2.cs System.Windows.Forms.dll
	$(CSC) Demo2.cs -r System.Windows.Forms.dll -r System.Drawing 

System.Windows.Forms.dll: $(SOURCES)
	$(CSC) $(SWFF) --target library -o System.Windows.Forms.dll $(SOURCES)
