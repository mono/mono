DIRS=framework util nunit-console

default: all

all:
	for d in $(DIRS) ; do \
		$(MAKE) -C $$d -f makefile.gnu || exit 1; \
	done

clean:
	for d in $(DIRS) ; do \
		$(MAKE) -C $$d -f makefile.gnu clean || exit 1; \
	done

