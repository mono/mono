#!/bin/sh
SUBMODULE_ERROR='Could not recursively update all git submodules. You may experience compilation problems if some submodules are out of date'
SUBMODULE_OK='Git submodules updated successfully'
if test -d .git; then \
	   (git submodule update --init --recursive && echo $SUBMODULE_OK) \
	|| (git submodule init && git submodule update --recursive && echo $SUBMODULE_OK) \
	|| (git submodule init && git submodule update && echo $SUBMODULE_ERROR) \
	|| (echo 'Git submodules could not be updated. Compilation will fail') \
fi

