DIRS =	corlib				\
	System				\
	System.XML			\
	System.Drawing			\
	System.Data			\
	System.EnterpriseServices

#	System.Web			\
#	Microsoft.VisualBasic		\
#	System.Configuration.Install

default: all

all clean:
	@for i in $(DIRS) ; do \
		(cd $$i && $(MAKE) -f makefile.gnu $@) || exit 1; \
	done

