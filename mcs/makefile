VERSION=0.13.99

DIRS=jay nant mcs class nunit tools monoresgen

all: 
	if test x$(OS) = xWindows_NT; then make linux; else make -f makefile.gnu; fi

install:
	if test x$(OS) = xWindows_NT; then echo Can not install on Windows ; else make -f makefile.gnu install; fi

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make linux) || exit 1;	\
	done

linux:
	for i in $(DIRS); do 			\
		(cd $$i; make linux) || exit 1;	\
	done

test:
	(cd nunit; make)
	(cd class; make test)

clean:
	if test x$(OS) = xWindows_NT; then make cleanwindows; else make cleanlinux; fi

cleanwindows:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done

cleanlinux:
	for i in $(DIRS); do 			\
		(cd $$i; make -f makefile.gnu clean)		\
	done

dist: 
	(c=`pwd`; d=`basename $$c`; cd ..; cp -a $$d mcs-$(VERSION); cd mcs-$(VERSION); make clean; cd ..; \
	tar czvf $$d/mcs-$(VERSION).tar.gz --exclude=CVS --exclude='.#*' --exclude=core --exclude='*~' --exclude='*.exe' mcs-$(VERSION); \
	rm -rf mcs-$(VERSION))
