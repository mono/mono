ACTIVATE=offtool/bin/activate

setup: $(ACTIVATE)
	source $(ACTIVATE) && pip3 install clang

$(ACTIVATE):
	pip3 install virtualenv
	virtualenv offtool

