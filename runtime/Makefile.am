#
# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)

install-data-hook:
	-cp $(srcdir)/*.dll $(assembliesdir)

dist-hook:
	-cp $(srcdir)/*.dll $(distdir)

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .