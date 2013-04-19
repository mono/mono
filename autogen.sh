#!/usr/bin/env bash
# Run this to generate all the initial makefiles, etc.
# Ripped off from GNOME macros version

DIE=0

srcdir=`dirname $0`
test -z "$srcdir" && srcdir=.

if [ -n "$MONO_PATH" ]; then
	# from -> /mono/lib:/another/mono/lib
	# to -> /mono /another/mono
	for i in `echo ${MONO_PATH} | tr ":" " "`; do
		i=`dirname ${i}`
		if [ -n "{i}" -a -d "${i}/share/aclocal" ]; then
			ACLOCAL_FLAGS="-I ${i}/share/aclocal $ACLOCAL_FLAGS"
		fi
		if [ -n "{i}" -a -d "${i}/bin" ]; then
			PATH="${i}/bin:$PATH"
		fi
	done
	export PATH
fi

(autoconf --version) < /dev/null > /dev/null 2>&1 || {
  echo
  echo "**Error**: You must have \`autoconf' installed to compile Mono."
  echo "Download the appropriate package for your distribution,"
  echo "or get the source tarball at ftp://ftp.gnu.org/pub/gnu/"
  DIE=1
}

if [ -z "$LIBTOOL" ]; then
  LIBTOOL=`which glibtool 2>/dev/null` 
  if [ ! -x "$LIBTOOL" ]; then
    LIBTOOL=`which libtool`
  fi
fi

(grep "^AM_PROG_LIBTOOL" $srcdir/configure.in >/dev/null) && {
  ($LIBTOOL --version) < /dev/null > /dev/null 2>&1 || {
    echo
    echo "**Error**: You must have \`libtool' installed to compile Mono."
    echo "Get ftp://ftp.gnu.org/pub/gnu/libtool-1.2d.tar.gz"
    echo "(or a newer version if it is available)"
    DIE=1
  }
}

grep "^AM_GNU_GETTEXT" $srcdir/configure.in >/dev/null && {
  grep "sed.*POTFILES" $srcdir/configure.in >/dev/null || \
  (gettext --version) < /dev/null > /dev/null 2>&1 || {
    echo
    echo "**Error**: You must have \`gettext' installed to compile Mono."
    echo "Get ftp://alpha.gnu.org/gnu/gettext-0.10.35.tar.gz"
    echo "(or a newer version if it is available)"
    DIE=1
  }
}

(automake --version) < /dev/null > /dev/null 2>&1 || {
  echo
  echo "**Error**: You must have \`automake' installed to compile Mono."
  echo "Get ftp://ftp.gnu.org/pub/gnu/automake-1.3.tar.gz"
  echo "(or a newer version if it is available)"
  DIE=1
  NO_AUTOMAKE=yes
}


# if no automake, don't bother testing for aclocal
test -n "$NO_AUTOMAKE" || (aclocal --version) < /dev/null > /dev/null 2>&1 || {
  echo
  echo "**Error**: Missing \`aclocal'.  The version of \`automake'"
  echo "installed doesn't appear recent enough."
  echo "Get ftp://ftp.gnu.org/pub/gnu/automake-1.3.tar.gz"
  echo "(or a newer version if it is available)"
  DIE=1
}

if test "$DIE" -eq 1; then
  exit 1
fi

if test x$NOCONFIGURE = x && test -z "$*"; then
  echo "**Warning**: I am going to run \`configure' with no arguments."
  echo "If you wish to pass any to it, please specify them on the"
  echo \`$0\'" command line."
  echo
fi

case $CC in
xlc )
  am_opt=--include-deps;;
esac


if grep "^AM_PROG_LIBTOOL" configure.in >/dev/null; then
  if test -z "$NO_LIBTOOLIZE" ; then 
    echo "Running libtoolize..."
    ${LIBTOOL}ize --force --copy
  fi
fi


#
# Plug in the extension module
#
has_ext_mod=false
for PARAM; do
    if test "$PARAM" = "--enable-extension-module" ; then
		has_ext_mod=true
	fi
done

if test x$has_ext_mod = xtrue; then
	pushd ../mono-extensions/scripts
	sh ./prepare-repo.sh || exit 1
	popd
else
	cat mono/mini/Makefile.am.in > mono/mini/Makefile.am
	cat mono/metadata/Makefile.am.in > mono/metadata/Makefile.am
fi


echo "Running aclocal -I m4 -I . $ACLOCAL_FLAGS ..."
aclocal -Wnone -I m4 -I . $ACLOCAL_FLAGS || {
  echo
  echo "**Error**: aclocal failed. This may mean that you have not"
  echo "installed all of the packages you need, or you may need to"
  echo "set ACLOCAL_FLAGS to include \"-I \$prefix/share/aclocal\""
  echo "for the prefix where you installed the packages whose"
  echo "macros were not found"
  exit 1
}

if grep "^AC_CONFIG_HEADERS" configure.in >/dev/null; then
  echo "Running autoheader..."
  autoheader || { echo "**Error**: autoheader failed."; exit 1; }
fi

echo "Running automake --gnu $am_opt ..."
automake --add-missing --gnu -Wno-portability -Wno-obsolete $am_opt ||
  { echo "**Error**: automake failed."; exit 1; }
echo "Running autoconf ..."
autoconf || { echo "**Error**: autoconf failed."; exit 1; }

if test -d $srcdir/libgc; then
  echo Running libgc/autogen.sh ...
  (cd $srcdir/libgc ; NOCONFIGURE=1 ./autogen.sh "$@")
  echo Done running libgc/autogen.sh ...
fi

if test -d $srcdir/eglib; then
  echo Running eglib/autogen.sh ...
  (cd $srcdir/eglib ; NOCONFIGURE=1 ./autogen.sh "$@")
  echo Done running eglib/autogen.sh ...
fi


conf_flags="--enable-maintainer-mode --enable-compile-warnings" #--enable-iso-c

if test x$NOCONFIGURE = x; then
  echo Running $srcdir/configure $conf_flags "$@" ...
  $srcdir/configure $conf_flags "$@" \
  && echo Now type \`make\' to compile $PKG_NAME || exit 1
else
  echo Skipping configure process.
fi
