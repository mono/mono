thisdir = nunit20/nunit-console
SUBDIRS = 
include ../../build/rules.make

PROGRAM = $(topdir)/class/lib/$(PROFILE)/nunit-console.exe
LOCAL_MCS_FLAGS = /r:nunit.framework.dll /r:nunit.util.dll /r:nunit.core.dll
DISTFILES = app.config App.ico nunit-console.csproj

NO_INSTALL = yes

include ../../build/executable.make
