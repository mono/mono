DIRS=jay mcs class mbas nunit nunit20 monoresgen ilasm tools
DIST=monocharge-`date -u +%Y%m%d`
MCS = mcs
INSTALL=/usr/bin/install
DOCFILES= README.building

#nant doesn't work yet

default: all

all:
	if ! which $(MCS); then \
		echo You must have a C\# compiler installed to continue.; \
		echo This is typically provided by \'mono\'.; \
		echo Read INSTALL.txt for details.; \
		exit 1; \
	fi;
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
	mkdir -p $(prefix)/share/doc/mono
	$(INSTALL) -m 644 $(DOCFILES) $(prefix)/share/doc/mono


test: all
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

testcorlib:
	$(MAKE) -C class/corlib/Test -f makefile.gnu test

clean:
	-rm -f monocharge-*.tar.gz
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

corlib:
	$(MAKE) -C class/corlib -f makefile.gnu

# Please do only use `binary-snapshot', the `dist' target will disappear really soon !
binary-snapshot: dist

dist: all
	mkdir $(DIST)
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu install prefix=$(PWD)/$(DIST) || exit 1; \
	done
	tar -c $(DIST) | gzip > $(DIST).tar.gz
	rm -rf $(DIST)

