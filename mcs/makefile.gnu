DIRS=jay mcs class nunit monoresgen ilasm
DIST=monocharge-`date -u +%Y%m%d`

#nant doesn't work yet

default: all

all:
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

install:
	if test x$$prefix = x; then \
		echo Usage is: make -f makefile.gnu install prefix=YOURPREFIX; \
		exit 1; \
	fi;
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

test: all
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

clean:
	-rm -f monocharge-*.tar.gz
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

# Please do only use `binary-snapshot', the `dist' target will disappear really soon !
binary-snapshot: dist

dist: all
	mkdir $(DIST)
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu install prefix=$(PWD)/$(DIST) || exit 1; \
	done
	tar -c $(DIST) | gzip > $(DIST).tar.gz
	rm -rf $(DIST)

