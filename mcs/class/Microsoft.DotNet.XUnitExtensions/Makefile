thisdir = class/Microsoft.DotNet.XUnitExtensions
SUBDIRS =
include ../../build/rules.make

XUNIT_BINARIES_DIR = $(topdir)/../../external/xunit-binaries

LIBRARY = Microsoft.DotNet.XUnitExtensions.dll
LIB_REFS = $(XUNIT_BINARIES_DIR)/xunit.core \
	$(XUNIT_BINARIES_DIR)/xunit.abstractions \
	$(XUNIT_BINARIES_DIR)/xunit.execution.dotnet \
	mscorlib System System.Core \
	Facades/System.Runtime \
	Facades/System.Reflection \
	Facades/System.Threading \
	Facades/System.Threading.Tasks \
	Facades/netstandard

NO_SIGN_ASSEMBLY = yes

include ../../build/library.make
