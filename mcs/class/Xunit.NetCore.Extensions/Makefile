thisdir = class/Xunit.NetCore.Extensions
SUBDIRS = 
include ../../build/rules.make

XUNIT_LIBS := xunit.core xunit.abstractions xunit.execution.desktop xunit.assert

LIBRARY = Xunit.NetCore.Extensions.dll
LIB_REFS = System System.Core System.Runtime.InteropServices.RuntimeInformation Facades/System.Runtime Facades/System.Threading.Tasks
LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/../external/xunit-binaries/%.dll,$(XUNIT_LIBS))

EXTRA_DISTFILES =

NO_TEST = yes
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes

include ../../build/library.make
