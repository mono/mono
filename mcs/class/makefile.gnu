INSTALL = /usr/bin/install
prefix = /usr

DIRS =	corlib				\
	I18N				\
	System				\
	System.XML			\
	System.Drawing			\
	System.EnterpriseServices	\
	Mono.Data.Tds			\
	System.Security			\
	System.Data			\
	Mono.GetOptions			\
	System.Management		\
	System.Web			\
	System.Web.Services		\
	System.Runtime.Remoting		\
	System.Runtime.Serialization.Formatters.Soap \
	System.Configuration.Install 	\
	Mono.CSharp.Debugger		\
	Mono.Data.DB2Client		\
	Mono.Data.MySql			\
	Mono.Data.PostgreSqlClient	\
	Mono.Data.SqliteClient		\
	Mono.Data.SybaseClient		\
	Mono.Data.TdsClient		\
	System.Data.OracleClient	\
	Mono.PEToolkit			\
	Mono.Posix			\
	Accessibility			\
	Microsoft.VisualBasic		\
	Microsoft.VisualC		\
	Cscompmgd			\
	System.Windows.Forms		\
	PEAPI				\

default: all

all clean:
	@for i in $(DIRS) ; do \
		if [ -d "$$i" ] && [ -f "$$i/makefile.gnu" ] ; then	\
			(cd $$i && $(MAKE) -f makefile.gnu $@) || exit 1; \
		fi	\
	done

install: all
	mkdir -p $(prefix)/lib/
	$(INSTALL) -m 644 lib/*.dll $(prefix)/lib/

test: all
	@for i in $(DIRS) ; do \
		if [ -d "$$i" ] && [ -f "$$i/makefile.gnu" ] ; then	\
			(cd $$i && $(MAKE) -f makefile.gnu $@) || exit 1; \
		fi	\
	done

