thisdir = docs
SUBDIRS = 
include ../build/rules.make

DISTFILES = \
	clr-abi.txt                 \
	compiler.txt                \
	control-flow-analysis.txt   \
	cs-errors.config            \
	mono-tools.config           \
	monodoc.xml                 \
	new-anonymous-design.txt    \
	order.txt                   \
	$(DOC_SOURCE_FILES)         \
	$(shell find ecma334 -name \*.xml)

DOC_SOURCE_FILES = \
	cs-errors.source            \
	ecma334.source              \
	mono-tools.source           \
	Mono.source                 \
	netdocs.source              \
	Novell.source               \
	nunit-docs.source

ASSEMBLED_DOCS = \
	cs-errors.tree cs-errors.zip      \
	ecma334.tree ecma334.zip          \
	mono-tools.tree mono-tools.zip    \
	Mono.tree Mono.zip                \
	netdocs.tree netdocs.zip          \
	Novell.tree Novell.zip            \
	nunit-docs.tree nunit-docs.zip

MONODOC_SOURCES_INSTALL_FILES = \
	$(ASSEMBLED_DOCS)             \
	$(DOC_SOURCE_FILES)

CLEAN_FILES += $(ASSEMBLED_DOCS)

ifeq (net_2_0, $(PROFILE))
all-local: build-documentation
endif

all-local test-local run-test-local run-test-ondotnet-local doc-update:

clean-local:
	-rm $(CLEAN_FILES)

dist-local: dist-default

MONODOC_INSTALL_DIR = $(mono_libdir)/monodoc

install-local:
	$(MKINSTALLDIRS) $(MONODOC_INSTALL_DIR) $(MONODOC_INSTALL_DIR)/sources
	$(INSTALL_DATA) $(MONODOC_SOURCES_INSTALL_FILES) $(MONODOC_INSTALL_DIR)/sources
	$(INSTALL_DATA) monodoc.xml $(MONODOC_INSTALL_DIR)

uninstall-local:
	-rm -f $(MONODOC_INSTALL_DIR)/monodoc.xml
	-rm -f $(addprefix $(MONODOC_INSTALL_DIR)/sources/,$(MONODOC_SOURCES_INSTALL_FILES))

NETDOCS_DIRS = \
	$(topdir)/class/corlib/Documentation/en                       \
	$(topdir)/class/System/Documentation/en                       \
	$(topdir)/class/System.Core/Documentation/en                  \
	$(topdir)/class/System.XML/Documentation/en                   \
	$(topdir)/class/System.Web/Documentation/en                   \
	$(topdir)/class/System.Web.Services/Documentation/en          \
	$(topdir)/class/System.Data/Documentation/en                  \
	$(topdir)/class/System.DirectoryServices/Documentation/en     \
	$(topdir)/class/System.Drawing/Documentation/en               \
	$(topdir)/class/System.Security/Documentation/en              \
	$(topdir)/class/Microsoft.Build.Framework/Documentation/en    \
	$(topdir)/class/Microsoft.Build.Engine/Documentation/en       \
	$(topdir)/class/Microsoft.Build.Utilities/Documentation/en

MONO_DIRS = \
	$(topdir)/class/Commons.Xml.Relaxng/Documentation/en          \
	$(topdir)/class/Mono.Cairo/Documentation/en                   \
	$(topdir)/class/Mono.GetOptions/Documentation/en              \
	$(topdir)/class/Mono.Options/Documentation/en                 \
	$(topdir)/class/Mono.Posix/Documentation/en                   \
	$(topdir)/class/Mono.Security.Win32/Documentation/en          \
	$(topdir)/class/Mono.Security/Documentation/en

NOVELL_DIRS = \
	$(topdir)/class/Novell.Directory.Ldap/Documentation/en

NUNIT_DIRS = \
	$(topdir)/nunit20/core/Documentation/en       \
	$(topdir)/nunit20/framework/Documentation/en  \
	$(topdir)/nunit20/mocks/Documentation/en      \
	$(topdir)/nunit20/util/Documentation/en

build-documentation: $(ASSEMBLED_DOCS)

netdocs.tree netdocs.zip:
	$(MDOC) assemble -o netdocs $(NETDOCS_DIRS)

Mono.tree Mono.zip:
	$(MDOC) assemble -o Mono $(MONO_DIRS)

Novell.tree Novell.zip:
	$(MDOC) assemble -o Novell $(NOVELL_DIRS)

nunit-docs.tree nunit-docs.zip:
	$(MDOC) assemble -o nunit-docs $(NUNIT_DIRS)

cs-errors.tree cs-errors.zip:
	$(MDOC) assemble -o cs-errors -f error cs-errors.config

ecma334.tree ecma334.zip:
	$(MDOC) assemble -o ecma334 -f ecmaspec ecma334

mono-tools.tree mono-tools.zip: mono-tools.config
	$(MDOC) assemble -o mono-tools -f man mono-tools.config

