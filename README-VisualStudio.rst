README.vsnet          Last updated: 2006-02-01


SVN includes a Visual Studio .NET 2005 solution, mono.sln, and some 
projects files to build most of the unmanaged parts in Mono.

The ``mono.sln`` solution file contains the VC projects files for:

- Embedded Samples
  - test-invoke.vcproj
  - test-metadata.vcproj
  - teste.vcproj
- Libraries
  - libgc.vcproj
  - libmono.vcproj
- Tools
  - genmdesc.vcproj
  - monoburg.vcproj
  - monodiet.vcproj
  - monodis.vcproj
  - monograph.vcproj
  - pedump.vcproj
- mono.vcproj
        

Requirements
============

- A working (i.e. where you could succesfully build mono) cygwin 
(http://www.cygwin.com/) setup! This is required to:

    - generate some files (via monoburg and genmdesc);
    - build the class libraries; and
    - test for regressions.

- Visual Studio .NET 2005. Previous Visual Studio versions may work or 
requires, hopefully minimal, changes.
        
- VSDependencies.zip must be decompressed under the ``/mono/`` directory 
(otherwise you will need to edit all the projects files). This file can 
be downloaded from:

        http://www.go-mono.com/archive/VSDependencies.zip


Local Changes
=============

Sadly solution/projects files aren't easy to move from computers to computers
(well unless everyone follow the same naming convention) so you'll likely have
to changes some options in order to compile the solution.

Each executed assembly (i.e. the EXE) must be able to find a working
``mscorlib.dll`` (and all the other required assemblies). This can be done in
different ways. My preference is to use the project "properties pages" in the
"Configuration Properties \Debugging\Environment" options and set ``MONO_PATH``
to the class libraries directory build by cygwin (local) or on Linux (remote).

::
        
        e.g. MONO_PATH=z:\svn\mcs\class\lib\default\
        
allows me to use the class libs build under Linux, while

::        

        MONO_PATH=C:\cygwin\opt\mono\lib\mono\1.0

uses the one built from cygwin (after a make install).

Some useful informations to adapt the solution/project files...

    - My cygwin root dir is: ``c:\cygwin\``
    - My username is: ``poupou``
    - My mono install prefix is: ``/opt/mono``
                

Building
========

Once everything is installed (and edited) you can right-click on the ``mono``
solution (in the Solution Explorer), select *Clean Solution* (for the first
time) then *Build Solution*.


Known Issues
============

#.  Most, BUT NOT ALL, the regressions tests pass under this build. The
    failures seems limited to some mathematical differences and to code
    relying on the stack walking functions. The hacks to replace the GCC
    functions (``__builtin_frame_address`` and ``__builtin_return_address``)
    are incomplete;

#.  The solution doesn't provide complete (i.e. from scratch) build. It
    requires a working cygwin environment to create some files (e.g. via
    genmdesc, monoburg). This isn't so bad as without cygwin you wouldn't
    be able to test Mono properly (see REQUIREMENTS);

#.  Only the Debug target is configured properly (that would be easy to 
    fix, but would require turning off some optimizations like omitting 
    stack frames). Anyway there are other issues [1] to fix before switching
    to Release and IMHO the _biggest_ advantage to VS.NET is it's debugger/
    debugging tools;

#.  The C compiler emits _lots_ of warning during compilation. Some warnings
    have been turned off for some projects (there was so much that it slowed
    down compilation). You can bring them back (or hide more of them) using
    the project "Properties Pages" windows,
    "Configuration Properties\C/C++\Advanced\Disable Specific Warnings";

#.  Visual Studio 2005 should have all the latest header files required, 
    but if not (or if you're using an older version of VS) then install MS 
    Platform SDK (Windows Server 2003 is the latest) to ensure you have the 
    latest Windows header files. You can download it from:
    http://www.microsoft.com/msdownload/platformsdk/sdkupdate/psdk-full.htm

#.  Not everyone has VS.NET so it is possible you may have to add some
    (new) files to the build from time to time. See "more informations" for
    reporting those changes.

#.  Probably a lot more I didn't discover... or has changed since.


More Information
================

Please email mono-devel-list@lists.ximian.com if you have any problems and/or
if there's something wrong/missing in the instructions.

An online version of this file is available at:

http://www.mono-project.com/Compiling_Mono_VSNET
