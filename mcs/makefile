VERSION=0.13.99

DIRS=jay nant mcs class nunit tools

all: linux

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make linux)		\
	done

linux:
	for i in $(DIRS); do 			\
		(cd $$i; make linux) || exit 1;	\
	done

test:
	(cd nunit; make)
	(cd class; make test)

clean:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done


dist: 
	(c=`pwd`; d=`basename $$c`; cd ..; cp -a $$d mcs-$(VERSION); cd mcs-$(VERSION); make clean; cd ..; \
	tar czvf mcs-$(VERSION).tar.gz --exclude=CVS --exclude='.#*' --exclude=core --exclude='*~' --exclude='*.exe' mcs-$(VERSION))
