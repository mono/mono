thisdir = class/System.Windows.Forms
include ../../build/rules.make

LIBRARY = System.Windows.Forms.dll
LIB_MCS_FLAGS = /unsafe \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Drawing.dll /r:Accessibility.dll \
	/nowarn:649 /nowarn:169 /nowarn:67 /nowarn:108 /nowarn:114

EXTRA_DISTFILES = \
	README


include ../../build/library.make
