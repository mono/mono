CSC=mcs
X11R6_INCLUDE=/usr/X11R6/include
WINE_INCLUDE=/usr/include/wine
WINE_LIB=/usr/lib/wine
GLIB20_INCLUDE=`pkg-config --cflags glib-2.0` `pkg-config --cflags gmodule-2.0`
GLIB20_LIB=`pkg-config --libs glib-2.0` `pkg-config --libs gmodule-2.0`
# GLIB20_LIB_INCLUDE=/usr/lib/glib-2.0/include
LIBMONO=/mono/lib/libmono.a
SOURCES = \
	win32Enums.cs\
	win32functions.cs\
	win32Handles.cs\
	win32Structs.cs\
	win32RegistryUtil.cs\
	win32Interfaces.cs\
	Font.cs \
	MenuItem.cs \
	DrawItemEventArgs.cs \
	DrawItemEventHandler.cs \
	../System.Windows.Forms/BorderStyle.cs \
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
	../System.Windows.Forms/MessageBoxButtons.cs \
	../System.Windows.Forms/MessageBoxIcon.cs \
	../System.Windows.Forms/MessageBoxDefaultButton.cs \
	../System.Windows.Forms/MessageBoxOptions.cs \
	../System.Windows.Forms/FlatStyle.cs \
	../System.Windows.Forms/ImageListStreamer.cs \
	../System.Windows.Forms/ImageList.cs \
	../System.Windows.Forms/ColorDepth.cs \
	IContainerControl.cs \
	Control.cs \
	ScrollableControl.cs \
	ContainerControl.cs \
	Label.cs \
	Form.cs \
	Application.cs \
	NativeWindow.cs \
	ButtonBase.cs \
	Button.cs \
	MessageBox.cs

all:	monostub.exe.so System.Windows.Forms.dll NativeWindowTest.exe \
	FormTest.exe Test.exe

##########################################################################
# build the mono stub application
monostub.exe.so: monostub.o monostub.exe.spec.o monostub.exe.dbg.o
	gcc -shared -Wl,-Bsymbolic -D_REENTRANT -DWINELIB -o monostub.exe.so \
	monostub.exe.spec.o \
	monostub.o monostub.exe.dbg.o -I/usr/local/include \
	$(GLIB20_INCUDE) -L/usr/lib \
	$(LIBMONO)  -lgc \
	-lwine -lntdll.dll $(GLIB20_LIB) -lm -lpthread 

clean:	
	rm *.o monostub.exe.dbg.c monostub.exe.spec.c monostub.exe.so *.exe *.dll

monostub.o: monostub.c
	gcc -c -I. -I$(WINE_INCLUDE)  $(GLIB20_INCLUDE) -g -O2 -Wall -I$(X11R6_INCLUDE) -D_REENTRANT -DWINELIB -o monostub.o monostub.c

monostub.exe.tmp.o: monostub.o 
	ld -r  monostub.o -o monostub.exe.tmp.o  
	strip --strip-unneeded monostub.exe.tmp.o

monostub.exe.spec.c: monostub.exe.tmp.o
	winebuild -sym monostub.exe.tmp.o -o monostub.exe.spec.c -exe monostub.exe -mgui -L$(WINE_LIB)  -lcomdlg32 -lshell32 -luser32 -lgdi32 -lkernel32

monostub.exe.spec.o: monostub.exe.spec.c
	gcc -c -I. -I. -I$(WINE_INCLUDE) -g -O2 -I$(X11R6_INCLUDE) -D_REENTRANT -DWINELIB -o monostub.exe.spec.o monostub.exe.spec.c

monostub.exe.dbg.c: monostub.exe.spec.o
	winebuild -o monostub.exe.dbg.c -debug -C. monostub.c

monostub.exe.dbg.o: monostub.exe.dbg.c
	gcc -c -I. -I. -I$(WINE_INCLUDE)  -g -O2 -I$(X11R6_INCLUDE) -D_REENTRANT -DWINELIB -o monostub.exe.dbg.o monostub.exe.dbg.c

##########################################################################
# build System.Windows.Forms.dll and test/sample applications

Test.exe: Test.cs
	$(CSC) Test.cs

NativeWindowTest.exe: NativeWindowTest.cs System.Windows.Forms.dll
	$(CSC) -r System.Windows.Forms.dll NativeWindowTest.cs

FormTest.exe: FormTest.cs System.Windows.Forms.dll
	$(CSC) -r System.Windows.Forms.dll FormTest.cs

System.Windows.Forms.dll: $(SOURCES)
	$(CSC) --target library -o System.Windows.Forms.dll $(SOURCES) -r System.Drawing
