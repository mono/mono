thisdir = tools/security/certview
SUBDIRS = 
include ../../../build/rules.make

RESGEN = monoresgen
LOCAL_MCS_FLAGS = /lib:$(topdir)/class/lib /r:Mono.Security.dll /r:System.Xml.dll

# SWF isn't quite good enough to build this yet.

all-local: certview.exe

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	$(INSTALL_BIN) certview.exe $(DESTDIR)$(prefix)/bin

clean-local:
	rm -f *.exe *.resources

test-local run-test-local:

DISTFILES = \
	CertificateViewer.cs 		\
	CertificateFormatter.cs 	\
	X509ExtensionsHandler.cs	\
	certview.exe.config		\
	certview.glade 			\
	certview.gladep 		\
	gcertview.cs			\
	gcertview.exe.config		\
	mono.ico			\
	$(wildcard *.bmp)		\
	$(wildcard *.png)

dist-local: dist-default

# FIXME: should add  /win32icon:mono.ico to the flags for certview.exe

sources = CertificateViewer.cs CertificateFormatter.cs X509ExtensionsHandler.cs ../AssemblyInfo.cs
cv_libs = /r:System.Windows.Forms.dll /r:System.Drawing.dll
gcv_libs = /r:gdk-sharp.dll /r:glib-sharp.dll /r:glade-sharp.dll /r:gtk-sharp.dll

certview.exe: $(sources) Mono.Tools.CertView.CertificateViewer.resources
	$(CSCOMPILE) $(cv_libs) /res:Mono.Tools.CertView.CertificateViewer.resources $(sources)

# We can't make this by default since it relies on GTK#

gcertview.exe: gcertview.cs CertificateFormatter.cs X509ExtensionsHandler.cs ../AssemblyInfo.cs
	$(CSCOMPILE) $(gcv_libs) /unsafe $^

Mono.Tools.CertView.CertificateViewer.resources: CertificateViewer.resx
	$(RESGEN) $^ $@
