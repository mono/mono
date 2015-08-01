# localecharset.m4 serial 1
AC_DEFUN([AM_LOCALCHARSET],
[
  AC_REQUIRE([AM_ICONV_LINK])
  AC_CHECK_HEADERS_ONCE([localcharset.h])dnl 
  if test $ac_cv_header_localcharset_h = yes; then
	AC_LIB_HAVE_LINKFLAGS([charset],[],[#include <localcharset.h>],[locale_charset()],[not found])
	AS_VAR_PUSHDEF([ac_Search], [ac_cv_search_localcharset])dnl    
    AC_CACHE_CHECK([for library containing locale_charset], [ac_Search],
	  [ac_func_search_save_LIBS=$LIBS
	  AC_LANG_CONFTEST([AC_LANG_PROGRAM([#include <localcharset.h>],[locale_charset()])])
	  ac_res="none required"
	  AC_LINK_IFELSE([],[AS_VAR_SET([ac_Search], [$ac_res])],[
	    ac_res="$LTLIBICONV"
		LIBS="$LIBICONV $ac_func_search_save_LIBS"
	    AC_LINK_IFELSE([],[AS_VAR_SET([ac_Search], [$ac_res])],[
		  ac_res="$LTLIBCHARSET"
		  LIBS="$LIBCHARSET $ac_func_search_save_LIBS"
		  AC_LINK_IFELSE([],[AS_VAR_SET([ac_Search], [$ac_res])])
		])
	  ])
      AS_VAR_SET_IF([ac_Search], ,[AS_VAR_SET([ac_Search],[no])])
	  rm conftest.$ac_ext
	  LIBS=$ac_func_search_save_LIBS])
    AS_VAR_COPY([ac_res],[ac_Search])
    AS_IF([test "$ac_res" != no],
		[test "$ac_res" = "none required" || LTLIBCHARSET="$ac_res"])
    AS_VAR_POPDEF([ac_Search])dnl
  fi
  AC_SUBST([HAVE_LOCALCHARSET_H])
])
