DIRS=jay mcs class tools

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make windows)		\
	done

unix:
	echo "'make unix' is broken for now."
