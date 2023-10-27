# Microsoft Developer Studio Project File - Name="gc" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Dynamic-Link Library" 0x0102

CFG=gc - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE
!MESSAGE NMAKE /f "gc.mak".
!MESSAGE
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE
!MESSAGE NMAKE /f "gc.mak" CFG="gc - Win32 Debug"
!MESSAGE
!MESSAGE Possible choices for configuration are:
!MESSAGE
!MESSAGE "gc - Win32 Release" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "gc - Win32 Debug" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "gc - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "..\..\..\bin"
# PROP Intermediate_Dir "..\..\..\obj\Release\gc"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MT /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "GC_EXPORTS" /YX /FD /c
# ADD CPP /nologo /MD /W3 /GX /Zi /O2 /I "..\..\include" /FI"stdafx.h" /D "NDEBUG" /D "_WINDOWS" /D "_USRDLL" /D "GC_DLL" /D "WIN32" /D "_MBCS" /D "GC_THREADS" /Yu"stdafx.h" /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /i "..\..\include" /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /dll /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib /nologo /base:"0x37C30000" /subsystem:console /dll /debug /machine:I386 /out:"..\..\..\bin/gc60.dll" /implib:"..\..\..\lib/gc.lib" /opt:ref /release
# SUBTRACT LINK32 /pdb:none
# Begin Special Build Tool
OutDir=.\..\..\..\bin
SOURCE="$(InputPath)"
PostBuild_Cmds=del $(OutDir)\..\lib\gc.exp
# End Special Build Tool

!ELSEIF  "$(CFG)" == "gc - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "..\..\..\bin"
# PROP Intermediate_Dir "..\..\..\obj\Debug\gc"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "GC_EXPORTS" /YX /FD /GZ /c
# ADD CPP /nologo /MDd /W3 /GX /Zi /Od /I "..\..\include" /FI"stdafx.h" /D "_DEBUG" /D "_WINDOWS" /D "_USRDLL" /D "GC_DLL" /D "WIN32" /D "_MBCS" /D "GC_THREADS" /Yu"stdafx.h" /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /i "..\..\include" /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo /o"..\..\..\bin/gcd.bsc"
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /dll /debug /machine:I386 /pdbtype:sept
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib /nologo /base:"0x37C30000" /subsystem:console /dll /incremental:no /debug /machine:I386 /out:"..\..\..\bin/gc60d.dll" /implib:"..\..\..\lib/gcd.lib" /pdbtype:sept
# SUBTRACT LINK32 /pdb:none
# Begin Special Build Tool
OutDir=.\..\..\..\bin
SOURCE="$(InputPath)"
PostBuild_Cmds=del $(OutDir)\..\lib\gcd.exp
# End Special Build Tool

!ENDIF

# Begin Target

# Name "gc - Win32 Release"
# Name "gc - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "c;cpp;cc;cxx;tcc;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=..\..\allchblk.c
# End Source File
# Begin Source File

SOURCE=..\..\alloc.c
# End Source File
# Begin Source File

SOURCE=..\..\backgraph.c
# End Source File
# Begin Source File

SOURCE=..\..\blacklst.c
# End Source File
# Begin Source File

SOURCE=..\..\dbg_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\gcj_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\fnlz_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\dyn_load.c
# End Source File
# Begin Source File

SOURCE=..\..\finalize.c
# End Source File
# Begin Source File

SOURCE=..\..\headers.c
# End Source File
# Begin Source File

SOURCE=..\..\mach_dep.c
# End Source File
# Begin Source File

SOURCE=..\..\malloc.c
# End Source File
# Begin Source File

SOURCE=..\..\mallocx.c
# End Source File
# Begin Source File

SOURCE=..\..\mark.c
# End Source File
# Begin Source File

SOURCE=..\..\mark_rts.c
# End Source File
# Begin Source File

SOURCE=..\..\misc.c
# End Source File
# Begin Source File

SOURCE=..\..\extra\msvc_dbg.c
# End Source File
# Begin Source File

SOURCE=..\..\new_hblk.c
# End Source File
# Begin Source File

SOURCE=..\..\obj_map.c
# End Source File
# Begin Source File

SOURCE=..\..\os_dep.c
# End Source File
# Begin Source File

SOURCE=..\..\ptr_chck.c
# End Source File
# Begin Source File

SOURCE=..\..\reclaim.c
# End Source File
# Begin Source File

SOURCE=..\stdafx.c
# ADD CPP /Yc"stdafx.h"
# End Source File
# Begin Source File

SOURCE=..\..\typd_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\win32_threads.c
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hh;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=..\..\include\gc.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_allocator.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_backptr.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_config_macros.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_cpp.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_gcj.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\gc_hdrs.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_inl.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_inline.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\gc_locks.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_mark.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_disclaim.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\gc_pmark.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\gc_priv.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_pthread_redirects.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_typed.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\gcconfig.h
# End Source File
# Begin Source File

SOURCE=..\..\include\javaxfc.h
# End Source File
# Begin Source File

SOURCE=..\..\include\leak_detector.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\msvc_dbg.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_alloc_ptrs.h
# End Source File
# Begin Source File

SOURCE=..\..\include\new_gc_alloc.h
# End Source File
# Begin Source File

SOURCE=..\stdafx.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_version.h
# End Source File
# Begin Source File

SOURCE=..\..\include\weakpointer.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# Begin Source File

SOURCE=..\gc.def
# End Source File
# Begin Source File

SOURCE=..\gc.rc
# End Source File
# Begin Source File

SOURCE=..\gc.ver
# End Source File
# End Group
# End Target
# End Project
