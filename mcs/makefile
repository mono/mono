DIRS=jay mcs class tools

all: linux

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make linux)		\
	done

linux:
	for i in $(DIRS); do 			\
		(cd $$i; make linux)		\
	done


clean:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done
