DIRS=framework util nunit-console

default: all

all install clean:
	@for i in $(DIRS) ; do \
		if [ -d "$$i" ] && [ -f "$$i/makefile.gnu" ] ; then	\
			$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
		fi	\
	done

test:

