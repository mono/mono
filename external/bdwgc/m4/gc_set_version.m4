#
# THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
# OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
#
# Permission is hereby granted to use or copy this program
# for any purpose,  provided the above notices are retained on all copies.
# Permission to modify the code and to distribute modified code is granted,
# provided the above notices are retained, and a notice that the code was
# modified is included with the above copyright notice.

# GC_SET_VERSION
# sets and AC_DEFINEs GC_VERSION_MAJOR, GC_VERSION_MINOR and GC_VERSION_MICRO
# based on the contents of PACKAGE_VERSION; PACKAGE_VERSION must conform to
# [0-9]+[.][0-9]+[.][0-9]+
#
AC_DEFUN([GC_SET_VERSION], [
  AC_MSG_CHECKING(GC version numbers)
  GC_VERSION_MAJOR=`echo $PACKAGE_VERSION | sed 's/^\([[0-9]][[0-9]]*\)[[.]].*$/\1/g'`
  GC_VERSION_MINOR=`echo $PACKAGE_VERSION | sed 's/^[[^.]]*[[.]]\([[0-9]][[0-9]]*\).*$/\1/g'`
  GC_VERSION_MICRO=`echo $PACKAGE_VERSION | sed 's/^[[^.]]*[[.]][[^.]]*[[.]]\([[0-9]][[0-9]]*\)$/\1/g'`

  if test :$GC_VERSION_MAJOR: = :: \
       -o :$GC_VERSION_MINOR: = :: \
       -o :$GC_VERSION_MICRO: = :: ;
  then
    AC_MSG_RESULT(invalid)
    AC_MSG_ERROR([nonconforming PACKAGE_VERSION='$PACKAGE_VERSION'])
  fi

  AC_DEFINE_UNQUOTED([GC_VERSION_MAJOR], $GC_VERSION_MAJOR,
                     [The major version number of this GC release.])
  AC_DEFINE_UNQUOTED([GC_VERSION_MINOR], $GC_VERSION_MINOR,
                     [The minor version number of this GC release.])
  AC_DEFINE_UNQUOTED([GC_VERSION_MICRO], $GC_VERSION_MICRO,
                     [The micro version number of this GC release.])
  AC_MSG_RESULT(major=$GC_VERSION_MAJOR minor=$GC_VERSION_MINOR \
                micro=$GC_VERSION_MICRO)
])

sinclude(libtool.m4)
