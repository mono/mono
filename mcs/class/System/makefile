all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /target:library /out:System.dll /nowarn:1595 /recurse:*.cs

unix:
	@echo "'make unix' is broken for now."
