thisdir = class/ByteFX.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = ByteFX.Data.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Data.dll /r:ICSharpCode.SharpZipLib.dll \
	/r:System.Design.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll \
	/res:mysqlclient/Designers/command.bmp \
	/res:mysqlclient/Designers/connection.bmp \
	/res:mysqlclient/Designers/dataadapter.bmp
NO_TEST = yes
EXTRA_DISTFILES = ChangeLog.txt lgpl.txt readme.txt

include ../../build/library.make
