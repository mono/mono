thisdir = tools/wsdl
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.Xml.dll /r:System.Web.Services.dll

ifeq (net_2_0, $(PROFILE))
PROGRAM = wsdl2.exe
PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/2.0
else
PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/1.0
PROGRAM = wsdl.exe
endif

include ../../build/executable.make
