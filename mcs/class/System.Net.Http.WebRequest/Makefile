thisdir = class/System.Net.Http.WebRequest
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Net.Http.WebRequest.dll

LIB_REFS = System.Net.Http System System.Core
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS =

TEST_MCS_FLAGS = -d:WEBREQUEST_HANDLER
TEST_LIB_REFS = System System.Core System.Net.Http System.Net.Http.WebRequest

ifndef SOCKETSHTTPHANDLER
TEST_MCS_FLAGS += -d:LEGACY_HTTPCLIENT
LIB_MCS_FLAGS += -d:LEGACY_HTTPCLIENT
endif

include ../../build/library.make
