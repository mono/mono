thisdir = tools/eventvwr
SUBDIRS = 
include ../../build/rules.make

RESX_RESOURCES = \
	EventEntryProperties.resources	\
	EventViewer.resources

IMAGES_RESOURCES = \
	Resources/error.ico		\
	Resources/info.ico		\
	Resources/warning.ico		\
	Resources/eventlog.ico		\
	Resources/computer.ico		\
	Resources/successaudit.ico	\
	Resources/failureaudit.ico	\
	Resources/properties.png	\
	Resources/refresh.png

EXTRA_DISTFILES = \
	eventvwr.exe.resources	\
	$(RESX_RESOURCES:.resources=.resx)	\
	$(IMAGES_RESOURCES)

LOCAL_MCS_FLAGS = -target:winexe -r:System.Drawing.dll	\
	-r:System.Windows.Forms.dll	\
	-win32icon:Resources/eventlog.ico	\
	@eventvwr.exe.resources

PROGRAM = $(topdir)/class/lib/$(PROFILE)/eventvwr.exe

CLEAN_FILES = $(RESX_RESOURCES)

include ../../build/executable.make

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

$(PROGRAM) : $(RESX_RESOURCES) $(IMAGES_RESOURCES)
