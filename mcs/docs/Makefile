thisdir = docs
SUBDIRS = 
include ../build/rules.make

DISTFILES = \
	clr-abi.txt                 \
	compiler.txt                \
	control-flow-analysis.txt   \
	cs-errors.config            \
	monodoc.xml                 \
	new-anonymous-design.txt    \
	order.txt                   \
	$(DOC_SOURCE_FILES)         \
	$(wildcard ecma334/*.xml)

DOC_SOURCE_FILES = \
	lang-csharp.source          \
	Mono.source                 \
	netdocs.source              \
	Novell.source

ASSEMBLED_DOCS = \
	cs-errors.tree cs-errors.zip      \
	ecma334.tree ecma334.zip          \
	Mono.tree Mono.zip                \
	netdocs.tree netdocs.zip          \
	Novell.tree Novell.zip            

MONODOC_SOURCES_INSTALL_FILES = \
	$(ASSEMBLED_DOCS)             \
	$(DOC_SOURCE_FILES)

CLEAN_FILES += $(ASSEMBLED_DOCS)

ifndef DISABLE_MCS_DOCS
ifneq ($(PROFILE), $(DEFAULT_PROFILE))
DISABLE_MCS_DOCS = yes
endif
endif

csproj-local test-local run-test-local run-test-ondotnet-local doc-update:

clean-local:
	-rm -f $(CLEAN_FILES)

dist-local: dist-default


MONODOC_INSTALL_DIR = $(DESTDIR)$(mono_libdir)/monodoc

ifdef DISABLE_MCS_DOCS
all-local:

install-local:

uninstall-local:

else
all-local: $(ASSEMBLED_DOCS)

install-local:
	$(MKINSTALLDIRS) $(MONODOC_INSTALL_DIR) $(MONODOC_INSTALL_DIR)/sources
	$(INSTALL_DATA) $(MONODOC_SOURCES_INSTALL_FILES) $(MONODOC_INSTALL_DIR)/sources
	$(INSTALL_DATA) monodoc.xml $(MONODOC_INSTALL_DIR)

uninstall-local:
	-rm -f $(MONODOC_INSTALL_DIR)/monodoc.xml
	-rm -f $(addprefix $(MONODOC_INSTALL_DIR)/sources/,$(MONODOC_SOURCES_INSTALL_FILES))
endif



classdir = $(topdir)/class
doc_en   = Documentation/en

NETDOCS_DIRS = \
	$(classdir)/corlib/$(doc_en)                                        \
	$(classdir)/Managed.Windows.Forms/$(doc_en)			    \
	$(classdir)/Microsoft.Build.Engine/$(doc_en)                        \
	$(classdir)/Microsoft.Build.Framework/$(doc_en)                     \
	$(classdir)/Microsoft.Build.Utilities/$(doc_en)                     \
	$(classdir)/System.Configuration.Install/$(doc_en)                  \
	$(classdir)/System.Core/$(doc_en)                                   \
	$(classdir)/System.Data/$(doc_en)                                   \
	$(classdir)/System.Design/$(doc_en)                                 \
	$(classdir)/System.DirectoryServices/$(doc_en)                      \
	$(classdir)/System.Drawing/$(doc_en)                                \
	$(classdir)/System.Runtime.Remoting/$(doc_en)                       \
	$(classdir)/System.Runtime.Serialization.Formatters.Soap/$(doc_en)  \
	$(classdir)/System.Security/$(doc_en)                               \
	$(classdir)/System.Web.Services/$(doc_en)                           \
	$(classdir)/System.Web/$(doc_en)                                    \
	$(classdir)/System.XML/$(doc_en)                                    \
	$(classdir)/System/$(doc_en) 

MONO_DIRS = \
	$(classdir)/Commons.Xml.Relaxng/$(doc_en)          \
	$(classdir)/Mono.Cairo/$(doc_en)                   \
	$(classdir)/Mono.CSharp/$(doc_en)                  \
	$(classdir)/Mono.Data.Sqlite/$(doc_en)		   \
	$(classdir)/Mono.Debugger.Soft/$(doc_en)           \
	$(classdir)/Mono.Options/$(doc_en)                 \
	$(classdir)/Mono.Parallel/$(doc_en)		   \
	$(classdir)/Mono.Posix/$(doc_en)                   \
	$(classdir)/Mono.Security.Win32/$(doc_en)          \
	$(classdir)/Mono.Security/$(doc_en)                \
	$(classdir)/Mono.Simd/$(doc_en)                    \
	$(classdir)/Npgsql/$(doc_en)

NOVELL_DIRS = \
	$(topdir)/class/Novell.Directory.Ldap/Documentation/en

NUNIT_DIRS = \
	$(topdir)/nunit20/core/$(doc_en)       \
	$(topdir)/nunit20/framework/$(doc_en)  \
	$(topdir)/nunit20/mocks/$(doc_en)      \
	$(topdir)/nunit20/util/$(doc_en)

# To support `make -jN`, we can't mention multiple targets in the same rule;
# DO NOT DO THIS:
#
#	  foo.tree foo.zip : Makefile
#     $(MDOC) assemble -o foo $(FOO_DIRS)
#
# If you do, make will try to biuld foo.tree & foo.zip concurrently, which
# breaks things badly... (read: "mdoc: sharing violation on path...foo.zip").
#
# The proper solution is to chain the rule dependencies so that only one file
# is the target of a rule.

netdocs.zip : netdocs.tree
netdocs.tree: Makefile
	$(MDOC) --debug assemble -o netdocs $(NETDOCS_DIRS)

Mono.zip : Mono.tree
Mono.tree: Makefile
	$(MDOC) --debug assemble -o Mono $(MONO_DIRS)

Novell.zip : Novell.tree
Novell.tree: Makefile
	$(MDOC) --debug assemble -o Novell $(NOVELL_DIRS)

cs-errors.zip : cs-errors.tree
cs-errors.tree: cs-errors.config Makefile
	$(MDOC) --debug assemble -o cs-errors -f error $<

ecma334.zip : ecma334.tree
ecma334.tree: Makefile
	$(MDOC) --debug assemble -o ecma334 -f ecmaspec ecma334

