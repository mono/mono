thisdir = class/System.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Messaging.dll
LIB_MCS_FLAGS = /resource:System.Messaging/MessageQueue.resx \
	        /r:System.dll \
		/r:System.Configuration.Install.dll \
		/r:System.Drawing.dll \
		/r:System.Windows.Forms.dll

EXTRA_DISTFILES = System.Messaging/MessageQueue.resx

include ../../build/library.make
