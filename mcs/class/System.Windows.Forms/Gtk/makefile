CSC=mcs
SWFF=-r gdk-sharp -r gtk-sharp -r glib-sharp -r System.Drawing 

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
	HorizontalAlignment.cs	\
	TextBoxBase.cs	\
	TextBox.cs	\
	CheckBox.cs	\
	RadioButton.cs	\
	GroupBox.cs	\
	ProgressBar.cs	\
	PictureBox.cs	\
	FileDialog.cs	\
	ColorDialog.cs	\
	ComboBox.cs	\
	ScrollableControl.cs

all: demo.exe

demo.exe: demo.cs System.Windows.Forms.dll
	$(CSC) demo.cs -r System.Windows.Forms.dll -r System.Drawing 

System.Windows.Forms.dll: $(SOURCES)
	$(CSC) $(SWFF) --target library -o System.Windows.Forms.dll $(SOURCES)
