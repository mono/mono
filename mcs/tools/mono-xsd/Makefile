thisdir = tools/mono-xsd
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.Xml.dll /r:System.Data.dll
PROGRAM = xsd.exe
PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/1.0

include ../../build/executable.make
