CSC=csc 
SWFF=/r:gtk-sharp.dll /r:glib-sharp.dll /noconfig /r:System.dll /r:System.Drawing.dll /nowarn:169

SOURCES = \
	Application.cs		\
	ContainerControl.cs	\
	Control.cs		\
	Form.cs 		\
	ScrollableControl.cs

all: demo.exe

demo.exe: demo.cs System.Windows.Forms.dll
	$(CSC) demo.cs /r:System.Windows.Forms.dll

System.Windows.Forms.dll: $(SOURCES)
	$(CSC) $(SWFF) /target:library /out:System.Windows.Forms.dll $(SOURCES)
