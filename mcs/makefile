DIRS=jay mcs class tools

all:
	@echo "You must use 'make windows' or 'make linux'."

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make windows)		\
	done

linux:
	for i in $(DIRS); do 			\
		(cd $$i; make linux)		\
	done

	
clean:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done
