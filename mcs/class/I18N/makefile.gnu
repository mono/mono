INSTALL = /usr/bin/install
prefix = /usr

DIRS =	Common West MidEast Other Rare

default: all

all clean test:
	@for i in $(DIRS) ; do \
		if [ -d "$$i" ] && [ -f "$$i/makefile.gnu" ] ; then	\
			(cd $$i && $(MAKE) -f makefile.gnu $@) || exit 1; \
		fi	\
	done

