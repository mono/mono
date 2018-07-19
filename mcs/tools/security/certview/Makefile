thisdir = tools/security/certview
SUBDIRS = 
include ../../../build/rules.make

LIB_REFS = Mono.Security System.Xml
LOCAL_MCS_FLAGS = /win32icon:mono.ico

all-local: certview.exe

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	$(INSTALL_BIN) certview.exe $(DESTDIR)$(prefix)/bin

clean-local:
	rm -f *.exe *.resources

test-local run-test-local:

DISTFILES = \
	CertificateViewer.cs 		\
	CertificateFormatter.cs 		\
	X509ExtensionsHandler.cs	\
	certview.exe.config		\
	certview.glade 			\
	certview.gladep 		\
	gcertview.cs			\
	gcertview.exe.config		\
	mono.ico				\
	$(resources)

resources = $(wildcard *.bmp)		\
	$(wildcard *.png)

dist-local: dist-default

sources = CertificateViewer.cs CertificateFormatter.cs X509ExtensionsHandler.cs ../AssemblyInfo.cs ../../../build/common/Consts.cs
cv_libs = /r:System.Windows.Forms.dll /r:System.Drawing.dll
gcv_libs = -pkg:gtk-sharp -pkg:glade-sharp

certview.exe: $(sources) Mono.Tools.CertView.CertificateViewer.resources
	$(CSCOMPILE) $(cv_libs) -resource:Mono.Tools.CertView.CertificateViewer.resources $(sources)

# We can't make this by default since it relies on GTK#

ress= $(foreach res,$(resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

gcertview.exe: gcertview.cs CertificateFormatter.cs X509ExtensionsHandler.cs ../AssemblyInfo.cs ../../../build/common/Consts.cs
	$(CSCOMPILE) $(gcv_libs) $^ -resource:certview.glade $(ress)

Mono.Tools.CertView.CertificateViewer.resources: CertificateViewer.resx
	$(RESGEN) $^ $@
