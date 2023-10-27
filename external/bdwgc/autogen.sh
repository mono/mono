#!/bin/sh
set -e

# This script creates (or regenerates) configure (as well as aclocal.m4,
# config.h.in, Makefile.in, etc.) missing in the source repository.
#
# If you compile from a distribution tarball, you can skip this.  Otherwise,
# make sure that you have Autoconf, Automake, Libtool, and pkg-config
# installed on your system, and that the corresponding *.m4 files are visible
# to the aclocal.  The latter can be achieved by using packages shipped by
# your OS, or by installing custom versions of all four packages to the same
# prefix.  Otherwise, you may need to invoke autoreconf with the appropriate
# -I options to locate the required *.m4 files.

autoreconf -i

echo
echo "Ready to run './configure'."
