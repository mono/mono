Mono is a software platform designed to allow developers to easily create cross platform applications.
It is an open source implementation of Microsoft's .NET Framework based on the ECMA standards for C# and the Common Language Runtime.

1. [Compilation and Installation](#compilation-and-installation)
2. [Using Mono](#using-mono)
3. [Directory Roadmap](#directory-roadmap)
4. [Contributing to Mono](#contributing-to-mono)
5. [Reporting bugs](#reporting-bugs)
6. [Configuration Options](#configuration-options)

**Build Status**

|      debian-amd64       |      debian-i386       |      debian-ppc64el       |      centos-s390x       |       windows-amd64       |
|:-----------------------:|:----------------------:|:-------------------------:|:-----------------------:|:-------------------------:|
| [![debian-amd64][1]][2] | [![debian-i386][3]][4] | [![debian-ppc64el][5]][6] | [![centos-s390x][7]][8] | [![windows-amd64][9]][10] |

[1]: http://jenkins.mono-project.com/job/test-mono-mainline/label=debian-amd64/badge/icon
[2]: http://jenkins.mono-project.com/job/test-mono-mainline/label=debian-amd64/
[3]: http://jenkins.mono-project.com/job/test-mono-mainline/label=debian-i386/badge/icon
[4]: http://jenkins.mono-project.com/job/test-mono-mainline/label=debian-i386/
[5]: http://jenkins.mono-project.com/job/test-mono-mainline-communityarchitectures/label=debian-ppc64el/badge/icon
[6]: http://jenkins.mono-project.com/job/test-mono-mainline-communityarchitectures/label=debian-ppc64el/
[7]: http://jenkins.mono-project.com/job/test-mono-mainline-communityarchitectures/label=centos-s390x/badge/icon
[8]: http://jenkins.mono-project.com/job/test-mono-mainline-communityarchitectures/label=centos-s390x/
[9]: https://ci.appveyor.com/api/projects/status/1e61ebdfpbiei58v/branch/master?svg=true
[10]: https://ci.appveyor.com/project/ajlennon/mono-817/branch/master

Compilation and Installation
============================

Building the Software
---------------------

Please see our guides for building Mono on
[Mac OS X](http://www.mono-project.com/docs/compiling-mono/mac/),
[Linux](http://www.mono-project.com/docs/compiling-mono/linux/) and 
[Windows](http://www.mono-project.com/docs/compiling-mono/windows/).

Note that building from Git assumes that you already have Mono installed,
so please download and [install the latest Mono release](http://www.mono-project.com/download/)
before trying to build from Git. This is required because the Mono build
relies on a working Mono C# compiler to compile itself
(also known as [bootstrapping](http://en.wikipedia.org/wiki/Bootstrapping_(compilers))).

If you don't have a working Mono installation
---------------------------------------------

If you don't have a working Mono installation, you can try a slightly
more risky approach: getting the latest version of the 'monolite' distribution,
which contains just enough to run the 'mcs' compiler. You do this with:

    # Run the following line after ./autogen.sh
    make get-monolite-latest

This will download and place the files appropriately so that you can then
just run:

    make EXTERNAL_MCS=${PWD}/mcs/class/lib/monolite/basic.exe

The build will then use the files downloaded by `make get-monolite-latest`.

Testing and Installation
------------------------

You can run the mono and mcs test suites with the command: `make check`.

Expect to find a few test suite failures. As a sanity check, you
can compare the failures you got with [https://wrench.mono-project.com/Wrench/](https://wrench.mono-project.com/Wrench/)
and [http://jenkins.mono-project.com/](http://jenkins.mono-project.com/).

You can now install mono with: `make install`

You can verify your installation by using the mono-test-install
script, it can diagnose some common problems with Mono's install.
Failure to follow these steps may result in a broken installation. 

Using Mono
==========

Once you have installed the software, you can run a few programs:

* `mono program.exe` runtime engine

* `mcs program.cs` C# compiler 

* `monodis program.exe` CIL Disassembler

See the man pages for mono(1), mcs(1) and monodis(1) for further details.

Directory Roadmap
=================

* `data/` - Configuration files installed as part of the Mono runtime.

* `docs/` - Technical documents about the Mono runtime.

* `external/` - Git submodules for external libraries (Newtonsoft.Json, ikvm, etc).

* `man/` - Manual pages for the various Mono commands and programs.

* `mcs/` - The class libraries, compiler and tools

  * `class/` - The class libraries (like System.*, Microsoft.Build, etc.)

  * `mcs/` - The Mono C# compiler written in C#

  * `tools/` - Tools like gacutil, ikdasm, mdoc, etc.

* `mono/` - The core of the Mono Runtime.

  * `arch/` - Architecture specific portions.

  * `cil/` - Common Intermediate Representation, XML
definition of the CIL bytecodes.

  * `dis/` - CIL executable Disassembler

  * `io-layer/` - The I/O layer and system abstraction for 
emulating the .NET IO model.

  * `metadata/` - The object system and metadata reader.

  * `mini/` - The Just in Time Compiler.

* `runtime/` - A directory that contains the Makefiles that link the
mono/ and mcs/ build systems.

* `samples/` -Some simple sample programs on uses of the Mono
runtime as an embedded library.   

* `scripts/` - Scripts used to invoke Mono and the corresponding program.

* `../olive/` - Incubation code from [Olive](https://github.com/mono/olive).

  * If the directory ../olive is present (as an
independent checkout) from the Mono module, that
directory is automatically configured to share the
same prefix than this module gets.

Contributing to Mono
====================

Before submitting changes to Mono, please review the [contribution guidelines](http://www.mono-project.com/community/contributing/).
Please pay particular attention to the [Important Rules](http://www.mono-project.com/community/contributing/#important-rules) section.

Reporting bugs
==============

To submit bug reports, please use [Xamarin's Bugzilla](https://bugzilla.xamarin.com/)

Please use the search facility to ensure the same bug hasn't already
been submitted and follow our [guidelines](http://www.mono-project.com/community/bugs/make-a-good-bug-report/)
on how to make a good bug report.

Configuration Options
=====================

The following are the configuration options that someone
building Mono might want to use:

* `--with-sgen=yes,no` - Generational GC support: Used to enable or disable the
compilation of a Mono runtime with the SGen garbage collector.

  * On platforms that support it, after building Mono, you will have
both a `mono` binary and a `mono-sgen` binary. `mono` uses Boehm, while
`mono-sgen` uses the Simple Generational GC.

* `--with-gc=[included, boehm,  none]` - Selects the default Boehm garbage
collector engine to use.

  * *included*: (*slighty modified Boehm GC*)
This is the default value for the Boehm GC, and it's 
the most feature complete, it will allow Mono 
to use typed allocations and support the debugger. 

  * *boehm*:
This is used to use a system-install Boehm GC,
it is useful to test new features available in
Boehm GC, but we do not recommend that people
use this, as it disables a few features.

  * *none*:
Disables the inclusion of a garbage collector.

  * This defaults to `included`.

* `--with-tls=__thread,pthread`

  * Controls how Mono should access thread local storage,
pthread forces Mono to use the pthread APIs, while
__thread uses compiler-optimized access to it.

  * Although __thread is faster, it requires support from
the compiler, kernel and libc. Old Linux systems do
not support with __thread.

  * This value is typically pre-configured and there is no
need to set it, unless you are trying to debug a problem.

* `--with-sigaltstack=yes,no`

  * **Experimental**: Use at your own risk, it is known to
cause problems with garbage collection and is hard to
reproduce those bugs.

  * This controls whether Mono will install a special
signal handler to handle stack overflows. If set to
`yes`, it will turn stack overflows into the
StackOverflowException. Otherwise when a stack
overflow happens, your program will receive a
segmentation fault.

  * The configure script will try to detect if your
operating system supports this. Some older Linux
systems do not support this feature, or you might want
to override the auto-detection.

* `--with-static_mono=yes,no`

  * This controls whether `mono` should link against a
static library (libmono.a) or a shared library
(libmono.so). 

  * This defaults to `yes`, and will improve the performance
of the `mono` program. 

  * This only affects the `mono' binary, the shared
library libmono.so will always be produced for
developers that want to embed the runtime in their
application.

* `--with-xen-opt=yes,no` - Optimize code for Xen virtualization.

  * It makes Mono generate code which might be slightly
slower on average systems, but the resulting executable will run
faster under the Xen virtualization system.

  * This defaults to `yes`.

* `--with-large-heap=yes,no` - Enable support for GC heaps larger than 3GB.

  * This defaults to `no`.

* `--enable-small-config=yes,no` - Enable some tweaks to reduce memory usage
and disk footprint at the expense of some capabilities.

  * Typically this means that the number of threads that can be created
is limited (256), that the maximum heap size is also reduced (256 MB)
and other such limitations that still make mono useful, but more suitable
to embedded devices (like mobile phones).

  * This defaults to `no`.

* `--with-ikvm-native=yes,no` - Controls whether the IKVM JNI interface library is
built or not.

  * This is used if you are planning on
using the IKVM Java Virtual machine with Mono.

  * This defaults to `yes`.

* `--with-profile4=yes,no` - Whether you want to build the 4.x profile libraries
and runtime.

  * This defaults to `yes`.

* `--with-libgdiplus=installed,sibling,<path>` - Configure where Mono
searches for libgdiplus when running System.Drawing tests.

  * It defaults to `installed`, which means that the
library is available to Mono through the regular
system setup.

  * `sibling` can be used to specify that a libgdiplus
that resides as a sibling of this directory (mono)
should be used.

 * Or you can specify a path to a libgdiplus.

* `--disable-shared-memory`

  * Use this option to disable the use of shared memory in
Mono (this is equivalent to setting the MONO_DISABLE_SHM
environment variable, although this removes the feature
completely).

  * Disabling the shared memory support will disable certain
features like cross-process named mutexes.

* `--enable-minimal=LIST`

  * Use this feature to specify optional runtime
components that you might not want to include.  This
is only useful for developers embedding Mono that
require a subset of Mono functionality.
  * The list is a comma-separated list of components that
should be removed, these are:

    * `aot`:
Disables support for the Ahead of Time compilation.

    * `attach`:
Support for the Mono.Management assembly and the
VMAttach API (allowing code to be injected into
a target VM)

    * `com`:
Disables COM support.

    * `debug`:
Drop debugging support.

    * `decimal`:
Disables support for System.Decimal.

    * `full_messages`:
By default Mono comes with a full table
of messages for error codes. This feature
turns off uncommon error messages and reduces
the runtime size.

    * `generics`:
Generics support.  Disabling this will not
allow Mono to run any 2.0 libraries or
code that contains generics.

    * `jit`:
Removes the JIT engine from the build, this reduces
the executable size, and requires that all code
executed by the virtual machine be compiled with
Full AOT before execution.

    * `large_code`:
Disables support for large assemblies.

    * `logging`:
Disables support for debug logging.

    * `pinvoke`:
Support for Platform Invocation services,
disabling this will drop support for any
libraries using DllImport.

    * `portability`:
Removes support for MONO_IOMAP, the environment
variables for simplifying porting applications that 
are case-insensitive and that mix the Unix and Windows path separators.

    * `profiler`:
Disables support for the default profiler.

    * `reflection_emit`:
Drop System.Reflection.Emit support

    * `reflection_emit_save`:
Drop support for saving dynamically created
assemblies (AssemblyBuilderAccess.Save) in
System.Reflection.Emit.

    * `shadow_copy`:
Disables support for AppDomain's shadow copies
(you can disable this if you do not plan on 
using appdomains).

    * `simd`:
Disables support for the Mono.SIMD intrinsics
library.

    * `ssa`:
Disables compilation for the SSA optimization
framework, and the various SSA-based optimizations.

* `--enable-llvm`
* `--enable-loadedllvm`

  * This enables the use of LLVM as a code generation engine
for Mono.  The LLVM code generator and optimizer will be 
used instead of Mono's built-in code generator for both
Just in Time and Ahead of Time compilations.

  * See http://www.mono-project.com/docs/advanced/mono-llvm/ for the 
full details and up-to-date information on this feature.

  * You will need to have an LLVM built that Mono can link
against.

  * The `--enable-loadedllvm` variant will make the LLVM backend
into a runtime-loadable module instead of linking it directly
into the main mono binary.

* `--enable-big-arrays` - Enable use of arrays with indexes larger
than Int32.MaxValue.

  * By default Mono has the same limitation as .NET on
Win32 and Win64 and limits array indexes to 32-bit
values (even on 64-bit systems).

  * In certain scenarios where large arrays are required,
you can pass this flag and Mono will be built to
support 64-bit arrays.

  * This is not the default as it breaks the C embedding
ABI that we have exposed through the Mono development
cycle.

* `--enable-parallel-mark`

  * Use this option to enable the garbage collector to use
multiple CPUs to do its work.  This helps performance
on multi-CPU machines as the work is divided across CPUS.

  * This option is not currently the default as we have
not done much testing with Mono.

* `--enable-dtrace`

  * On Solaris and MacOS X builds a version of the Mono
runtime that contains DTrace probes and can
participate in the system profiling using DTrace.

* `--disable-dev-random`

  * Mono uses /dev/random to obtain good random data for
any source that requires random numbers.   If your
system does not support this, you might want to
disable it.

  * There are a number of runtime options to control this
also, see the man page.

* `--enable-nacl`

  * This configures the Mono compiler to generate code
suitable to be used by Google's Native Client:
http://code.google.com/p/nativeclient/

  * Currently this is used with Mono's AOT engine as
Native Client does not support JIT engines yet.
