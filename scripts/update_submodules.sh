#!/bin/sh
SUBMODULE_ERROR='Could not recursively update all git submodules. You may experience compilation problems if some submodules are out of date'
SUBMODULE_OK='Git submodules updated successfully'
if test -e .git; then \
	   (git submodule update --init --recursive && echo $SUBMODULE_OK) \
	|| (git submodule init && git submodule update --recursive && echo $SUBMODULE_OK) \
	|| (git submodule init && git submodule update && echo $SUBMODULE_ERROR) \
	|| (echo 'Git submodules could not be updated. Compilation will fail') \
fi

if ! test -e external/corefx/README.md; then
	echo "Error: Couldn't find the required submodules. This usually happens when using an archive from GitHub instead of https://download.mono-project.com/sources/mono/, or something went wrong while updating submodules."
	exit 1
fi
