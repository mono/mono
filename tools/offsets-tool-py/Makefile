ACTIVATE=offtool/bin/activate

setup: $(ACTIVATE)
	. $(ACTIVATE) && pip3 install clang

$(ACTIVATE):
	pip3 install virtualenv
	virtualenv offtool

