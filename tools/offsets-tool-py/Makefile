ACTIVATE=offtool/bin/activate

setup: $(ACTIVATE)

$(ACTIVATE):
	python3 -m pip install --user virtualenv
	# `virtualenv` might not end up in $PATH
	python3 -m virtualenv offtool

