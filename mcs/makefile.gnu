DIRS=class jay mcs nunit nunit/src/NUnitConsole

#nant doesn't work yet

default: all

all clean:
	for i in $(DIRS) ; do \
		(cd $$i; $(MAKE) -f makefile.gnu $@) || exit 1; \
	done
