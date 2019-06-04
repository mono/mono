thisdir = class/System.Runtime.CompilerServices.Unsafe
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.CompilerServices.Unsafe.dll

XTEST_LIB_FLAGS = -unsafe

#NO_BUILD = yes
NO_INSTALL = yes

EXTRA_DISTFILES = AssemblyInfo.il

include ../../build/library.make

$(build_lib): AssemblyInfo.il ../corlib/System.Runtime.CompilerServices/Unsafe.il
	$(ILASM) AssemblyInfo.il ../corlib/System.Runtime.CompilerServices/Unsafe.il /dll /out:$(build_lib)

