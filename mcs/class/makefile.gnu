DIRS =	corlib				\
	System				\
	System.XML			\
	System.Drawing			\
	System.Data			\
	System.EnterpriseServices	\
	System.Web			\
	System.Web.Services		\
	System.Configuration.Install

#	Microsoft.VisualBasic		\

default: all

all clean:
	@for i in $(DIRS) ; do \
		(cd $$i && $(MAKE) -f makefile.gnu $@) || exit 1; \
	done

