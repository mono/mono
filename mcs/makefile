DIRS=jay nant nunit mcs class tools

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
	(cd class; make test)

clean:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done
