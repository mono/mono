CSC=mcs
X11R6_INCLUDE=/usr/X11R6/include
WINE_INCLUDE=/usr/local/include/wine
WINE_LIB=/usr/local/lib/wine
GLIB20_INCLUDE=/usr/include/glib-2.0
GLIB20_LIB_INCLUDE=/usr/lib/glib-2.0/include
LIBMONO=/usr/local/lib/libmono.a
SOURCES = \
	Win32.cs \
	Font.cs \
	DrawItemEventArgs.cs \
	../System.Windows.Forms/DrawItemEventHandler.cs \
	../System.Windows.Forms/Message.cs \
	../System.Windows.Forms/CreateParams.cs \
	../System.Windows.Forms/MdiLayout.cs \
	../System.Windows.Forms/InputLanguage.cs \
	../System.Windows.Forms/InputLanguageCollection.cs \
	../System.Windows.Forms/InputLanguageChangedEventArgs.cs \
	../System.Windows.Forms/InputLanguageChangingEventArgs.cs \
	../System.Windows.Forms/PaintEventArgs.cs \
	../System.Windows.Forms/Keys.cs \
	../System.Windows.Forms/BoundsSpecified.cs \
	../System.Windows.Forms/RightToLeft.cs \
	../System.Windows.Forms/Shortcut.cs \
	../System.Windows.Forms/MenuMerge.cs \
	../System.Windows.Forms/MeasureItemEventArgs.cs \
	../System.Windows.Forms/SizeGripStyle.cs \
	../System.Windows.Forms/FormStartPosition.cs \
	../System.Windows.Forms/FormWindowState.cs \
	../System.Windows.Forms/ImeMode.cs \
	../System.Windows.Forms/InputLanguageChangedEventHandler.cs \
	../System.Windows.Forms/MeasureItemEventHandler.cs \
	../System.Windows.Forms/InputLanguageChangingEventHandler.cs \
	../System.Windows.Forms/DrawItemState.cs \
	../System.Windows.Forms/ComVisible.cs \
	../System.Windows.Forms/Menu.cs \
	../System.Windows.Forms/MenuItem.cs \
	../System.Windows.Forms/MainMenu.cs \
	../System.Windows.Forms/ContextMenu.cs \
	../System.Windows.Forms/DialogResult.cs \
	../System.Windows.Forms/IButtonControl.cs \
	../System.Windows.Forms/FormBorderStyle.cs \
	../System.Windows.Forms/AccessibleEvents.cs \
	../System.Windows.Forms/IWin32Window.cs \
	../System.Windows.Forms/DragDropEffects.cs \
	../System.Windows.Forms/ControlStyles.cs \
	../System.Windows.Forms/ControlEventArgs.cs \
	../System.Windows.Forms/DragEventArgs.cs \
	../System.Windows.Forms/GiveFeedbackEventArgs.cs \
	../System.Windows.Forms/HelpEventArgs.cs \
	../System.Windows.Forms/InvalidateEventArgs.cs \
	../System.Windows.Forms/KeyEventArgs.cs \
	../System.Windows.Forms/KeyPressEventArgs.cs \
	../System.Windows.Forms/MouseEventArgs.cs \
	../System.Windows.Forms/LayoutEventArgs.cs \
	../System.Windows.Forms/QueryContinueDragEventArgs.cs \
	../System.Windows.Forms/HorizontalAlignment.cs \
	../System.Windows.Forms/LeftRightAlignment.cs \
	../System.Windows.Forms/AccessibleRole.cs \
	../System.Windows.Forms/AnchorStyles.cs \
	../System.Windows.Forms/Cursor.cs \
	../System.Windows.Forms/BaseCollection.cs \
	../System.Windows.Forms/DockStyle.cs \
	../System.Windows.Forms/ControlEventHandler.cs \
	../System.Windows.Forms/MouseButtons.cs \
	../System.Windows.Forms/UICues.cs \
	../System.Windows.Forms/UICuesEventArgs.cs \
	../System.Windows.Forms/UICuesEventHandler.cs \
	../System.Windows.Forms/DragEventHandler.cs \
	../System.Windows.Forms/HelpEventHandler.cs \
	../System.Windows.Forms/GiveFeedbackEventHandler.cs \
	../System.Windows.Forms/InvalidateEventHandler.cs \
	../System.Windows.Forms/KeyEventHandler.cs \
	../System.Windows.Forms/KeyPressEventHandler.cs \
	../System.Windows.Forms/LayoutEventHandler.cs \
	../System.Windows.Forms/MouseEventHandler.cs \
	../System.Windows.Forms/PaintEventHandler.cs \
	../System.Windows.Forms/QueryAccessibilityHelpEventHandler.cs \
	../System.Windows.Forms/QueryContinueDragEventHandler.cs \
	../System.Windows.Forms/IDataObject.cs \
	../System.Windows.Forms/DragAction.cs \
	../System.Windows.Forms/QueryAccessibilityHelpEventArgs.cs \
	../System.Windows.Forms/IMessageFilter.cs \
	../System.Windows.Forms/ApplicationContext.cs \
	IContainerControl.cs \
	Control.cs \
	ScrollableControl.cs \
	ContainerControl.cs \
	Form.cs \
	Application.cs \
	NativeWindow.cs

# The Mono 0.13 build used at the time this code was written seems to be 
# missing some object code in the static libs. Due to conflicts between 
# functions in the shared libs of Mono and Wine this stub at this point 
# it seems we will need to statically link the embedded Mono engine.
#
# TODO: is the above statement correct or is the link phase missing an
# object/library or gcc/ld switch?
#
# These are the two object files are missing in the libmono.a so link these 
# in also:
LIBMETADATA=/home/john/mono-src/mono-0.13/mono/metadata/.libs/libmetadata.al
LIBMONORUNTIME=/home/john/mono-src/mono-0.13/mono/metadata/.libs/libmonoruntime.al

all:  monostub.exe.so System.Windows.Forms.dll NativeWindowTest.exe FormTest.exe 
##########################################################################
# build the mono stub application
monostub.exe.so: monostub.o monostart.o monostub.exe.spec.o monostub.exe.dbg.o
	gcc -shared -Wl,-Bsymbolic -o monostub.exe.so monostub.exe.spec.o monostub.o monostub.exe.dbg.o monostart.o -I/usr/local/include -I -I$(GLIB20_INCUDE) -I$(GLIB20_LIB_INCLUDE) -L/usr/lib $(LIBMONO) -lwine -lntdll.dll -lglib-2.0 -lgmodule-2.0 -lm $(LIBMETADATA) $(LIBMONORUNTIME)

clean:	
	rm *.o monostub.exe.dbg.c monostub.exe.spec.c monostub.exe.so *.exe *.dll

monostub.o: monostub.c
	gcc -c -I. -I$(WINE_INCLUDE)  -g -O2 -Wall -I$(X11R6_INCLUDE) -o monostub.o monostub.c

monostart.o: monostart.c
	gcc -c -I. -g -O2 -Wall -I$(X11R6_INCLUDE) -o monostart.o monostart.c -I$(GLIB20_INCLUDE) -I$(GLIB20_LIB_INCLUDE) -I$(WINE_INCLUDE)

monostub.exe.tmp.o: monostub.o monostart.o
	ld -r  monostub.o -o monostub.exe.tmp.o  
	strip --strip-unneeded monostub.exe.tmp.o

monostub.exe.spec.c: monostub.exe.tmp.o
	winebuild -sym monostub.exe.tmp.o -o monostub.exe.spec.c -exe monostub.exe -mgui -L$(WINE_LIB)  -lcomdlg32 -lshell32 -luser32 -lgdi32 -lkernel32

monostub.exe.spec.o: monostub.exe.spec.c
	gcc -c -I. -I. -I$(WINE_INCLUDE) -g -O2 -I$(X11R6_INCLUDE) -o monostub.exe.spec.o monostub.exe.spec.c

monostub.exe.dbg.c: monostub.exe.spec.o
	winebuild -o monostub.exe.dbg.c -debug -C. monostub.c

monostub.exe.dbg.o: monostub.exe.dbg.c
	gcc -c -I. -I. -I$(WINE_INCLUDE)  -g -O2 -I$(X11R6_INCLUDE) -o monostub.exe.dbg.o monostub.exe.dbg.c

##########################################################################
# build System.Windows.Forms.dll and test/sample applications

NativeWindowTest.exe: NativeWindowTest.cs System.Windows.Forms.dll
	$(CSC) --unsafe -r System.Windows.Forms.dll NativeWindowTest.cs

FormTest.exe: FormTest.cs System.Windows.Forms.dll
	$(CSC) --unsafe -r System.Windows.Forms.dll FormTest.cs

System.Windows.Forms.dll: $(SOURCES)
	$(CSC) --unsafe --target library -o System.Windows.Forms.dll $(SOURCES) -r System.Drawing
