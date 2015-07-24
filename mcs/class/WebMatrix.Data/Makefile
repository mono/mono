thisdir = class/WebMatrix.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = WebMatrix.Data.dll

LIB_REFS = System System.Data System.Core System.Configuration
LIB_MCS_FLAGS = -r:$(corlib)
TEST_MCS_FLAGS = -r:System.dll -r:System.Core.dll -r:System.Data.dll -r:Mono.Data.Sqlite.dll -r:Microsoft.CSharp.dll

include ../../build/library.make
