/*
 * Copyright (C) 1998 Francois Gouget
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#ifndef __WINE_TCHAR_H
#define __WINE_TCHAR_H

#ifdef __WINESRC__
#error Wine should not include tchar.h internally
#endif

#include "windef.h"

#ifdef __cplusplus
extern "C" {
#endif

/*****************************************************************************
 * tchar routines
 */
#define _strdec(start,current)  ((start)<(current) ? ((char*)(current))-1 : NULL)
#define _strinc(current)        (((char*)(current))+1)
#define _strncnt(str,max)       (strlen(str)>(max) ? (max) : strlen(str))
#define _strnextc(str)          ((unsigned int)*(str))
#define _strninc(str,n)         (((char*)(str))+(n))
#define _strspnp(s1,s2)         (*((s1)+=strspn((s1),(s2))) ? (s1) : NULL)


/*****************************************************************************
 * tchar mappings
 */
#ifndef _UNICODE
#include <string.h>
#ifndef _MBCS
#define WINE_tchar_routine(std,mbcs,unicode) std
#else /* _MBCS defined */
#define WINE_tchar_routine(std,mbcs,unicode) mbcs
#endif
#else /* _UNICODE defined */
#define WINE_tchar_routine(std,mbcs,unicode) unicode
#endif

#define WINE_tchar_true(a) (1)
#define WINE_tchar_false(a) (0)
#define WINE_tchar_tclen(a) (1)
#define WINE_tchar_tccpy(a,b) do { *(a)=*(b); } while (0)

#define __targv       WINE_tchar_routine(__argv,          __argv,      __wargv)
#define _fgettc       WINE_tchar_routine(fgetc,           fgetc,       fgetwc)
#define _fgettchar    WINE_tchar_routine(fgetchar,        fgetchar,    _fgetwchar)
#define _fgetts       WINE_tchar_routine(fgets,           fgets,       fgetws)
#define _fputtc       WINE_tchar_routine(fputc,           fputc,       fputwc)
#define _fputtchar    WINE_tchar_routine(fputchar,        fputchar,    _fputwchar)
#define _fputts       WINE_tchar_routine(fputs,           fputs,       fputws)
#define _ftprintf     WINE_tchar_routine(fprintf,         fprintf,     fwprintf)
#define _ftscanf      WINE_tchar_routine(fscanf,          fscanf,      fwscanf)
#define _gettc        WINE_tchar_routine(getc,            getc,        getwc)
#define _gettchar     WINE_tchar_routine(getchar,         getchar,     getwchar)
#define _getts        WINE_tchar_routine(gets,            gets,        getws)
#define _isalnum      WINE_tchar_routine(isalnum,         _ismbcalnum, iswalnum)
#define _istalpha     WINE_tchar_routine(isalpha,         _ismbcalpha, iswalpha)
#define _istascii     WINE_tchar_routine(isascii,         __isascii,   iswascii)
#define _istcntrl     WINE_tchar_routine(iscntrl,         iscntrl,     iswcntrl)
#define _istdigit     WINE_tchar_routine(isdigit,         _ismbcdigit, iswdigit)
#define _istgraph     WINE_tchar_routine(isgraph,         _ismbcgraph, iswgraph)
#define _istlead      WINE_tchar_routine(WINE_tchar_false,_ismbblead,  WINE_tchar_false)
#define _istleadbyte  WINE_tchar_routine(WINE_tchar_false,isleadbyte,  WINE_tchar_false)
#define _istlegal     WINE_tchar_routine(WINE_tchar_true, _ismbclegal, WINE_tchar_true)
#define _istlower     WINE_tchar_routine(islower,         _ismbcslower,iswlower)
#define _istprint     WINE_tchar_routine(isprint,         _ismbcprint, iswprint)
#define _istpunct     WINE_tchar_routine(ispunct,         _ismbcpunct, iswpunct)
#define _istspace     WINE_tchar_routine(isspace,         _ismbcspace, iswspace)
#define _istupper     WINE_tchar_routine(isupper,         _ismbcupper, iswupper)
#define _istxdigit    WINE_tchar_routine(isxdigit,        isxdigit,    iswxdigit)
#define _itot         WINE_tchar_routine(_itoa,           _itoa,       _itow)
#define _ltot         WINE_tchar_routine(_ltoa,           _ltoa,       _ltow)
#define _puttc        WINE_tchar_routine(putc,            putc,        putwc)
#define _puttchar     WINE_tchar_routine(putchar,         putchar,     putwchar)
#define _putts        WINE_tchar_routine(puts,            puts,        putws)
#define _sntprintf    WINE_tchar_routine(sprintf,         sprintf,     swprintf)
#define _stprintf     WINE_tchar_routine(sprintf,         sprintf,     swprintf)
#define _stscanf      WINE_tchar_routine(sscanf,          sscanf,      swscanf)
#define _taccess      WINE_tchar_routine(access,          _access,     _waccess)
#define _tasctime     WINE_tchar_routine(asctime,         asctime,     _wasctime)
#define _tccpy        WINE_tchar_routine(WINE_tchar_tccpy,_mbccpy,     WINE_tchar_tccpy)
#define _tchdir       WINE_tchar_routine(chdir,           _chdir,      _wchdir)
#define _tclen        WINE_tchar_routine(WINE_tchar_tclen,_mbclen,     WINE_tchar_tclen)
#define _tchmod       WINE_tchar_routine(chmod,           _chmod,      _wchmod)
#define _tcreat       WINE_tchar_routine(creat,           _creat,      _wcreat)
#define _tcscat       WINE_tchar_routine(strcat,          _mbscat,     wcscat)
#define _tcschr       WINE_tchar_routine(strchr,          _mbschr,     wcschr)
#define _tcsclen      WINE_tchar_routine(strlen,          _mbslen,     wcslen)
#define _tcscmp       WINE_tchar_routine(strcmp,          _mbscmp,     wcscmp)
#define _tcscoll      WINE_tchar_routine(strcoll,         _mbscoll,    wcscoll)
#define _tcscpy       WINE_tchar_routine(strcpy,          _mbscpy,     wcscpy)
#define _tcscspn      WINE_tchar_routine(strcspn,         _mbscspn,    wcscspn)
#define _tcsdec       WINE_tchar_routine(_strdec,         _mbsdec,     _wcsdec)
#define _tcsdup       WINE_tchar_routine(strdup,          _mbsdup,     _wcsdup)
#define _tcsftime     WINE_tchar_routine(strftime,        strftime,    wcsftime)
#define _tcsicmp      WINE_tchar_routine(strcasecmp,      _mbsicmp,    _wcsicmp)
#define _tcsicoll     WINE_tchar_routine(_stricoll,       _stricoll,   _wcsicoll)
#define _tcsinc       WINE_tchar_routine(_strinc,         _mbsinc,     _wcsinc)
#define _tcslen       WINE_tchar_routine(strlen,          strlen,      wcslen)
#define _tcslwr       WINE_tchar_routine(_strlwr,         _mbslwr,     _wcslwr)
#define _tcsnbcnt     WINE_tchar_routine(_strncnt,        _mbsnbcnt,   _wcnscnt)
#define _tcsncat      WINE_tchar_routine(strncat,         _mbsnbcat,   wcsncat)
#define _tcsnccat     WINE_tchar_routine(strncat,         _mbsncat,    wcsncat)
#define _tcsncmp      WINE_tchar_routine(strncmp,         _mbsnbcmp,   wcsncmp)
#define _tcsnccmp     WINE_tchar_routine(strncmp,         _mbsncmp,    wcsncmp)
#define _tcsnccnt     WINE_tchar_routine(_strncnt,        _mbsnccnt,   _wcsncnt)
#define _tcsnccpy     WINE_tchar_routine(strncpy,         _mbsncpy,    wcsncpy)
#define _tcsncicmp    WINE_tchar_routine(_strnicmp,       _mbsnicmp,   _wcsnicmp)
#define _tcsncpy      WINE_tchar_routine(strncpy,         _mbsnbcpy,   wcsncpy)
#define _tcsncset     WINE_tchar_routine(_strnset,        _mbsnset,    _wcsnset)
#define _tcsnextc     WINE_tchar_routine(_strnextc,       _mbsnextc,   _wcsnextc)
#define _tcsnicmp     WINE_tchar_routine(_strnicmp,       _mbsnicmp,   _wcsnicmp)
#define _tcsnicoll    WINE_tchar_routine(_strnicoll,      _strnicoll   _wcsnicoll)
#define _tcsninc      WINE_tchar_routine(_strninc,        _mbsninc,    _wcsninc)
#define _tcsnccnt     WINE_tchar_routine(_strncnt,        _mbsnccnt,   _wcsncnt)
#define _tcsnset      WINE_tchar_routine(_strnset,        _mbsnbset,   _wcsnset)
#define _tcspbrk      WINE_tchar_routine(strpbrk,         _mbspbrk,    wcspbrk)
#define _tcsspnp      WINE_tchar_routine(_strspnp,        _mbsspnp,    _wcsspnp)
#define _tcsrchr      WINE_tchar_routine(strrchr,         _mbsrchr,    wcsrchr)
#define _tcsrev       WINE_tchar_routine(_strrev,         _mbsrev,     _wcsrev)
#define _tcsset       WINE_tchar_routine(_strset,         _mbsset,     _wcsset)
#define _tcsspn       WINE_tchar_routine(strspn,          _mbsspn,     wcsspn)
#define _tcsstr       WINE_tchar_routine(strstr,          _mbsstr,     wcsstr)
#define _tcstod       WINE_tchar_routine(strtod,          strtod,      wcstod)
#define _tcstok       WINE_tchar_routine(strtok,          _mbstok,     wcstok)
#define _tcstol       WINE_tchar_routine(strtol,          strtol,      wcstol)
#define _tcstoul      WINE_tchar_routine(strtoul,         strtoul,     wcstoul)
#define _tcsupr       WINE_tchar_routine(_strupr,         _mbsupr,     _wcsupr)
#define _tcsxfrm      WINE_tchar_routine(strxfrm,         strxfrm,     wcsxfrm)
#define _tctime       WINE_tchar_routine(ctime,           ctime,       _wctime)
#define _tenviron     WINE_tchar_routine(_environ,        _environ,    _wenviron)
#define _texecl       WINE_tchar_routine(execl,           _execl,      _wexecl)
#define _texecle      WINE_tchar_routine(execle,          _execle,     _wexecle)
#define _texeclp      WINE_tchar_routine(execlp,          _execlp,     _wexeclp)
#define _texeclpe     WINE_tchar_routine(execlpe,         _execlpe,    _wexeclpe)
#define _texecv       WINE_tchar_routine(execv,           _execv,      _wexecv)
#define _texecve      WINE_tchar_routine(execve,          _execve,     _wexecve)
#define _texecvp      WINE_tchar_routine(execvp,          _execvp,     _wexecvp)
#define _texecvpe     WINE_tchar_routine(execvpe,         _execvpe,    _wexecvpe)
#define _tfdopen      WINE_tchar_routine(fdopen,          _fdopen,     _wfdopen)
#define _tfinddata_t  WINE_tchar_routine(_finddata_t,     _finddata_t, _wfinddata_t)
#define _tfinddatai64_t WINE_tchar_routine(_finddatai64_t,_finddatai64_t,_wfinddatai64_t)
#define _tfindfirst   WINE_tchar_routine(_findfirst,      _findfirst,  _wfindfirst)
#define _tfindnext    WINE_tchar_routine(_findnext,       _findnext,   _wfindnext)
#define _tfopen       WINE_tchar_routine(fopen,           fopen,       _wfopen)
#define _tfreopen     WINE_tchar_routine(freopen,         freopen,     _wfreopen)
#define _tfsopen      WINE_tchar_routine(_fsopen,         _fsopen,     _wfsopen)
#define _tfullpath    WINE_tchar_routine(_fullpath,       _fullpath,   _wfullpath)
#define _tgetcwd      WINE_tchar_routine(getcwd,          _getcwd,     _wgetcwd)
#define _tgetenv      WINE_tchar_routine(getenv,          getenv,      _wgetenv)
#define _tmain        WINE_tchar_routine(main,            main,        wmain)
#define _tmakepath    WINE_tchar_routine(_makepath,       _makepath,   _wmakepath)
#define _tmkdir       WINE_tchar_routine(mkdir,           _mkdir,      _wmkdir)
#define _tmktemp      WINE_tchar_routine(mktemp,          _mktemp,     _wmktemp)
#define _tperror      WINE_tchar_routine(perror,          perror,      _wperror)
#define _topen        WINE_tchar_routine(open,            _open,       _wopen)
#define _totlower     WINE_tchar_routine(tolower,         _mbctolower, towlower)
#define _totupper     WINE_tchar_routine(toupper,         _mbctoupper, towupper)
#define _tpopen       WINE_tchar_routine(popen,           _popen,      _wpopen)
#define _tprintf      WINE_tchar_routine(printf,          printf,      wprintf)
#define _tremove      WINE_tchar_routine(remove,          remove,      _wremove)
#define _trename      WINE_tchar_routine(rename,          rename,      _wrename)
#define _trmdir       WINE_tchar_routine(rmdir,           _rmdir,      _wrmdir)
#define _tsearchenv   WINE_tchar_routine(_searchenv,      _searchenv,  _wsearchenv)
#define _tscanf       WINE_tchar_routine(scanf,           scanf,       wscanf)
#define _tsetlocale   WINE_tchar_routine(setlocale,       setlocale,   _wsetlocale)
#define _tsopen       WINE_tchar_routine(_sopen,          _sopen,      _wsopen)
#define _tspawnl      WINE_tchar_routine(_spawnl,         _spawnl,     _wspawnl)
#define _tspawnle     WINE_tchar_routine(_spawnle,        _spawnle,    _wspawnle)
#define _tspawnlp     WINE_tchar_routine(_spawnlp,        _spawnlp,    _wspawnlp)
#define _tspawnlpe    WINE_tchar_routine(_spawnlpe,       _spawnlpe,   _wspawnlpe)
#define _tspawnv      WINE_tchar_routine(_spawnv,         _spawnv,     _wspawnv)
#define _tspawnve     WINE_tchar_routine(_spawnve,        _spawnve,    _wspawnve)
#define _tspawnvp     WINE_tchar_routine(_spawnvp,        _spawnvp,    _tspawnvp)
#define _tspawnvpe    WINE_tchar_routine(_spawnvpe,       _spawnvpe,   _tspawnvpe)
#define _tsplitpath   WINE_tchar_routine(_splitpath,      _splitpath,  _wsplitpath)
#define _tstat        WINE_tchar_routine(_stat,           _stat,       _wstat)
#define _tstrdate     WINE_tchar_routine(_strdate,        _strdate,    _wstrdate)
#define _tstrtime     WINE_tchar_routine(_strtime,        _strtime,    _wstrtime)
#define _tsystem      WINE_tchar_routine(system,          system,      _wsystem)
#define _ttempnam     WINE_tchar_routine(tempnam,         _tempnam,    _wtempnam)
#define _ttmpnam      WINE_tchar_routine(tmpnam,          tmpnam,      _wtmpnam)
#define _ttoi         WINE_tchar_routine(atoi,            atoi,        _wtoi)
#define _ttol         WINE_tchar_routine(atol,            atol,        _wtol)
#define _tutime       WINE_tchar_routine(utime,           _utime,      _wutime)
#define _tWinMain     WINE_tchar_routine(WinMain,         WinMain,     wWinMain)
#define _ultot        WINE_tchar_routine(_ultoa,          _ultoa,      _ultow)
#define _ungettc      WINE_tchar_routine(ungetc,          ungetc,      ungetwc)
#define _vftprintf    WINE_tchar_routine(vfprintf,        vfprintf,    vfwprintf)
#define _vsntprintf   WINE_tchar_routine(vsnprintf,       _vsnprintf,  _vsnwprintf)
#define _vstprintf    WINE_tchar_routine(vsprintf,        vsprintf,    vswprintf)
#define _vtprintf     WINE_tchar_routine(vprintf,         vprintf,     vwprintf)
#define _TEOF         WINE_tchar_routine(EOF,             EOF,         WEOF)

#define __T(x) __TEXT(x)
#define _T(x) __T(x)
#define _TEXT(x) __T(x)

typedef CHAR  _TCHARA;
typedef WCHAR _TCHARW;
DECL_WINELIB_TYPE_AW(_TCHAR)
typedef UCHAR  _TUCHARA;
typedef WCHAR _TUCHARW;
DECL_WINELIB_TYPE_AW(_TUCHAR)

#ifdef __cplusplus
} /* extern "C" */
#endif

#endif /* __WINE_TCHAR_H */
