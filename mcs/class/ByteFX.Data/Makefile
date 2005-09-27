thisdir = class/ByteFX.Data
SUBDIRS = 
include ../../build/rules.make

OTHER_RES = \
	mysqlclient/Designers/command.bmp \
	mysqlclient/Designers/connection.bmp \
	mysqlclient/Designers/dataadapter.bmp

LIBRARY = ByteFX.Data.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Data.dll /r:ICSharpCode.SharpZipLib.dll \
	/r:System.Design.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll \
	/r:Mono.Posix.dll \
	$(OTHER_RES:%=/res:%)

NO_TEST = yes
EXTRA_DISTFILES = ChangeLog.txt lgpl.txt readme.txt $(OTHER_RES)

include ../../build/library.make
