RESX_RESOURCES = \
	EventEntryProperties.resources	\
	EventViewer.resources

ifeq (net_2_0, $(PROFILE))
MCS = gmcs
RESGEN = resgen2
else
MCS = mcs
RESGEN = resgen
endif

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

all: clean eventvwr.exe

eventvwr.exe : $(RESX_RESOURCES) $(IMAGE_RESOURCES)
	$(MCS) -target:winexe -out:eventvwr.exe \
		-r:System.Drawing.dll	\
		-r:System.Windows.Forms.dll	\
		-win32icon:Resources/eventlog.ico	\
		@eventvwr.exe.sources	\
		@eventvwr.exe.resources

clean:
	rm -fR eventvwr.exe
	rm -fR EventViewer.csproj.user
	rm -fR EventViewer.suo
	rm -fR bin
	rm -fR obj
	for p in $(RESX_RESOURCES) ; do \
		rm -fR $$p ; \
	done

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $<
