DIRS=jay nant mcs class nunit tools doctools

all: linux

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make linux)		\
	done

linux:
	for i in $(DIRS); do 			\
		(cd $$i; make linux)		\
	done

test:
	(cd nunit; make)
	(cd class; make test)

clean:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done

