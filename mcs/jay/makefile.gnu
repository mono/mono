INSTALL = /usr/bin/install
prefix = /usr

datafiles = ACKNOWLEDGEMENTS NEW_FEATURES NOTES README README.jay skeleton skeleton.cs

all:
	$(MAKE) -f makefile linux

clean:
	$(MAKE) -f makefile clean

install: all
	mkdir -p $(prefix)/bin
	mkdir -p $(prefix)/share/jay
	mkdir -p $(prefix)/man/man1
	$(INSTALL) -m 755 jay $(prefix)/bin
	for datafile in $(datafiles) ; do \
	   $(INSTALL) -m 644 $$datafile $(prefix)/share/jay ; \
	done
	$(INSTALL) -m 644 jay.1 $(prefix)/man/man1
	
test:

