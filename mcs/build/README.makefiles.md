# The MCS makefiles

Author: Peter Williams <peter@newton.cx>

The new makefiles try to abstract building on Windows and Linux. They
try to provide a consistent set of ways to express the things that our
build system needs to let us do, specifically:

* Build recursively
* Build libraries and executables easily
* Let developers use different runtimes and class libaries
* Make distributions easily
* Provide a framework for testing
* Build platform-independently whenever possible
* Generate, update, and build monodoc documentation.

Makefile structure
==================

A general makefile looks like this:

```
thisdir = class/Mono.My.Library
SUBDIRS =
include ../../build/rules.make

all-local:
	do some stuff

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/share/
	$(INSTALL_DATA) myfile.txt $(DESTDIR)$(prefix)/share/myfiledir

clean-local:
	rm -f my-generated-file

test-local: my_test_program.exe

run-test-local:
	$(RUNTIME) my_test_program.exe

run-test-ondotnet-local:
	$(RUNTIME) my_test_program.exe

DISTFILES = myfile.txt my_test_source.cs

dist-local: dist-default

doc-update-local:

my_test_program.exe: my_test_source.cs
	$(CSCOMPILE) /target:exe /out:$@ $<
```

Each makefile follows the same pattern: it does some setup, includes
the standard make rules, and provides rules for eight standard targets:
`all`, `install`, `test`, `run-test`, `clean`, `dist`, and `doc-update`.

"Some setup" is defining two variables: `$(thisdir)` and
`$(SUBDIRS)`. `$(thisdir)` is the directory that the makefile lives in,
relative to the top directory (ie, `class/corlib`) and `$(SUBDIRS)`
defines the subdirectories that should be built in.

The eight targets do the following:

* `all-local` builds whatever someone would expect to be built
  when they just type `make'` Most likely `Foo.dll` or `Foo.exe`

* `install-local` installs whatever got built by `all-local`.

* `test-local` _builds_ the test programs or libraries but does
  _not_ run them.

* `run-test-local` actually runs the tests. It shouldn't
  necessarily exit in an error if the test fails, but should make that
  situation obvious. It should only run tests that take care of
  themselves automatically; interactive tests should have an individual
  target. The idea is that `make run-test` from the toplevel should be
  able to proceed unsupervised and test everything that can be tested in
  such a manner.

* `run-xunit-test-local` this is a variation of the above, but it runs the
  tests that were built with Xunit instead of NUnit.

* `run-test-ondotnet-local` is a variant of `run-test-local`. It is used only to validate if our tests themselves works fine under Microsoft runtime (on Windows). Basically, in this target, we should not use $(TEST_RUNTIME) to test our libraries.

* `clean-local` removes built files; `make clean` should leave
  only files that go into a distribution tarball. (But it is not
  necessarily true that all files that go into a tarball need to be left
  after a make clean.)

* `dist-local` copies files into the distribution tree, which is
  given by the variable `$(distdir)`. `dist-local` always depends on the
  target `dist-default`.  See ** `make dist` below.

* `doc-update-local` should generate or update monodoc documentation,
  if appropriate.  This is usually only appropriate for libraries.  It's
  defined as a standard target so that it can easily be run recursively
  across all libraries within the module.

Build configuration
===================

In general, MCS needs to be able to build relying only on the
existence of a runtime and core libraries (`mscorlib`, `System`,
`System.Xml`).  So there shouldn't be any checking for libraries or
whatnot; MCS should be able to build out of the box. We try to keep
platform detection and feature testing (ie, for HP/UX echo) inside
the makefiles; right now, there's no configuration script, and it'd
be nice to keep it that way. (I am told that some people build on
both Windows and Linux in the same tree, which would be impossible to
do if we cached platform-related configury values.)

That being said, it's very convenient for developers to be able to
customize their builds to suit their needs. To allow this, the
Makefile rules are set up to allow people to override pretty much any
important variable.

Configuration variables are given defaults in `config-default.make`;
`rules.make` optionally includes `$(topdir)/build/config.make`, so you
can customize your build without CVS trying to commit your modified
`config-default.make` all the time.  Platform-specific variables are
defined in `$(topdir)/build/platforms/$(BUILD_PLATFORM).make`, where
`$(BUILD_PLATFORM)` is detected in `config-default.make`. (Currently, the only
choices are `linux.make` and `win32.make`)

The best way to learn what the configuration variables are is to read
`config.make` and `platform.make`. There aren't too many and hopefully
they should be self-explanatory; see the numerous examples below for
more information if you're confused.

Recommendations for platform specifics
--------------------------------------

If you find yourself needing a platform-specific customization, try
and express it in terms of a feature, rather than a platform test. In
other words, this is good:

```
run-test-local: my-test.exe
ifdef PLATFORM_NEEDS_CRAZY_CRAP
	crazy-crap
endif
	$(RUNTIME) my-test.exe
```

and this is bad:

```
run-test-local: my-test.exe
ifdef WINDOWS
	crazy-crap
else
ifdef AMIGA
	crazy-crap
endif
endif
	$(RUNTIME) my-test.exe
```

The latter accumulates and gets unpleasant and it sucks. Granted,
right now we only have two platforms, so it's not a big deal, but it's
good form to get used to and practice. Anyway, take a look at how we
do the various corlib building hacks for examples of how we've done
platform-specificity. It certainly isn't pretty, but at least it's a
little structured.

Saving effort
=============

The point of the build system is to abstract things and take
care of all the easy stuff. So if you find yourself writing a
Makefile, know that there's probably already infrastructure to do what
you want. Here are all the common cases I can think of.

Compiling C# code? use:
-----------------------

```
my-program.exe: my-source.cs
	 $(CSCOMPILE) /target:exe /out:$@ $^
```

or

```
my-lib.dll: my-source.cs
	 $(CSCOMPILE) /target:library /out:$@ $^
```

Note the `$@` and `$^` variables. The former means "the name of the
file that I am trying to make" and the latter means "all the
dependencies of the file I am trying to make." USE THESE VARIABLES
AGGRESSIVELY. Say that you add a new source to your program:

```
my-program.exe: my-source.cs my-new-source.cs
	 $(CSCOMPILE) /target:exe /out:$@ $^
```

Because of the $^ variable, you don't need to remember to add another
file to the command line. Similarly, if you rename your program, you
won't need to remember to change the rule:

```
MonoVaporizer.exe: my-source.cs my-new-source.cs
	 $(CSCOMPILE) /target:exe /out:$@ $^
```

will still work. Another useful variable is $<, which means "the first
dependency of whatever I'm building." If you order your dependencies
carefully it can be extremely useful.

Just building an executable? 
----------------------------

Then use:

```
PROGRAM = myprogram.exe
LOCAL_MCS_FLAGS = /r:System.Xml.dll

include ../build/executable.make
```

executable.make builds a program in the current directory. Its name is
held in `$(PROGRAM)`, and its sources are listed in the file
`$(PROGRAM).sources`. It might seem to make more sense to just list the
program's sources in the Makefile, but when we build on Windows we
need to change slashes around, which is much easier to do if the
sources are listed in a file. The variable `$(LOCAL_MCS_FLAGS)` changes
the flags given to the compiler; it is included in `$(CSCOMPILE)` so you
don't need to worry about it.

`executable.make` does a lot for you: it builds the program in 'make
all-local', installs the program in `$(prefix)/bin`, distributes the
sources, and defines empty test targets. Now, if your program has a
test, set the variable `HAS_TEST`:

```
PROGRAM = myprogram.exe
LOCAL_MCS_FLAGS = /r:System.Xml.dll
HAS_TEST = yes
include ../build/executable.make

test-local: mytester.exe

run-test-local: mytester.exe
	$(RUNTIME) $<

mytester.exe: mytester.cs
	$(CSCOMPILE) /target:exe /out:$@ mytester.cs
```

If your program has NUnit tests, set the variable `HAS_NUNIT_TEST`:

```
PROGRAM = myprogram.exe
LOCAL_MCS_FLAGS = /r:System.Xml.dll
HAS_NUNIT_TEST = yes
include ../build/executable.make
```

`HAS_NUNIT_TEST` tests follow `library.make` NUnit test conventions: 
the files should be in a subdirectory called `Test/`, and if
your program is called `myprogram.exe`, they should be listed in
`myprogram_test.dll.sources`. The names in that files should *not* have
the `Test/` prefix. `make test'`will build `myprogram_test_$(PROFILE).dll` 
in the current directory, automatically supplying the flags to 
reference the original program and `NUnit.Framework.dll`.

If your program has 'built sources', that is, source files generated
from other files (say, generated by jay), define a variable called
`BUILT_SOURCES` and do *not* list the sources in `$(PROGRAM).sources`:

```
PROGRAM = myprogram.exe
LOCAL_MCS_FLAGS = /r:System.Xml.dll
BUILT_SOURCES = parser.cs
CLEAN_FILES = y.output

include ../build/executable.make

parser.cs: parser.jay
	$(topdir)/jay/jay $< > $@
```

`executable.make` will automatically delete the `$(BUILT_SOURCES)` files
on `make clean`. Since this situation is a common occurrence and jay
happens to leave behind y.output files, you can also define a variable
called `$(CLEAN_FILES)` that lists extra files to be deleted when
`make clean` is called. (That's in addition to your executable and the built sources).

Buildling a library? Use
------------------------

```
LIBRARY = Mono.MyLib.dll
LIB_MCS_FLAGS = /unsafe
TEST_MCS_FLAGS = /r:System.Xml.dll

include ../../build/library.make
```

Where you library is called `$(LIBRARY)`; it will be put into
`$(topdir)/class/lib`. `LIB_MCS_FLAGS` is the set of MCS flags to use when
compiling the library; in addition, a global set of flags called
`$(LIBRARY_FLAGS)` is added (that variable is defined in
`config-defaults.make`), as well as the usual `$(LOCAL_MCS_FLAGS)`.

As in `executable.make`, the sources for your library are listed in
`$(LIBRARY).sources`. Note: these source lists should have Unix forward
slashes and Unix newlines (\n, not \r\n.) If you get an error about
"touch: need a filename", that means your .sources file doesn't end in
a newline. It should.

Now `library.make` also assumes that your library has an NUnit2 test
harness. The files should be in a subdirectory called `Test/`, and if
your library is called `Mono.Foo.dll`, they should be listed in
`Mono.Foo_test.dll.sources`. The names in that files should *not* have
the `Test/` prefix. `make test` will build `Mono.Foo_test.dll` in the
current directory, automatically supplying the flags to reference the
original library and `NUnit.Framework.dll`.

If you don't have a test, just do this:

```
LIBRARY = Mono.MyLib.dll
LIB_MCS_FLAGS = /unsafe
NO_TEST = yes

include ../../build/library.make
```

and feel ashamed. Every good library has a test suite!

Extra flags needed to compile the test library should be listed in
$(TEST_MCS_FLAGS); often you will have a line like this:

```
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
```

Again, `library.make` does a lot for you: it builds the dll, it
generates makefile fragments to track the dependencies, it installs
the library, it builds the test dll on `make test`, it runs
`$(TEST_HARNESS)` on it on `make run-test`, it removes the appropriate
files on 'make clean', and it distributes all the source files on
`make dist`. (`TEST_HARNESS` defaults to be `nunit-console.exe` but it may
be overridden to, say, `nunit-gtk`). If you have extra files to
distribute when using either `library.make` or `executable.make`, use the
variable `$(EXTRA_DISTFILES)`:

```
EXTRA_DISTFILES = \
	Test/testcase1.in		\
	Test/testcase1.out		\
	README
```

Again, `library.make` and `executable.make` do the right things so that we
can build on Windows, doing some trickery to invert slashes and
overcome command-line length limitations. Use them unless you have a
really good reason not to. If you're building a bunch of small
executables, check out `tools/Makefile` or `tools/security/Makefile`; if
all the files are in the current directory, changing slashes isn't a
big deal, and command-line lengths won't be a problem, so
executable.make isn't necessary (and indeed it won't work, since it
can only build one .exe in a directory).

If you're building a library, `library.make` is highly recommended; the
only DLL that doesn't use it is corlib, because building corlib is a
fair bit more complicated than it should be. Oh well.

`library.make` also automatically supports generating and updating 
monodoc documentation.  Documentation is stored within the 
Documentation directory (a sibling to the Test directory), and is 
generated/updated whenever the doc-update target is executed.  
Assembling of the documentation so that the monodoc browser can
display the documentation is handled separately within the mcs/docs
all-local target; see mcs/docs/Makefile for details.

Running a C# program? Use $(RUNTIME)
------------------------------------

```
run-test-local: myprog.exe
	$(RUNTIME) myprog.exe
```

`$(RUNTIME)` might be empty (if you're on windows), so don't expect to
be able to give it any arguments. If you're on a platform which has an
interpreter or jitter, $(RUNTIME_FLAGS) is included in $(RUNTIME), so
set that variable.

$(TEST_RUNTIME) is the runtime to use when running tests. Right now it's
just "mono --debug".



Calling the compiler directly? Use $(MCS).
------------------------------------------

Really, you should use $(CSCOMPILE) whenever possible, but $(MCS) is
out there. $(BOOTSTRAP_MCS) is the C# compiler that we use to build
mcs.exe; on Linux, we then use mcs.exe to build everything else, but
on Windows, we use csc.exe to build everything. Only use
$(BOOTSTRAP_MCS) if you know what you're doing.

Compiling C code? Use $(CCOMPILE)
---------------------------------

To give it flags, set $(LOCAL_CFLAGS). As with compiling C#, the
variable $(CFLAGS) will automatically be included on the command line.


Compiling resources with resgen
-------------------------------

If you have a resource that should be compiled with resgen and
included in your assembly, you can use the RESOURCES_DEFS variable.
This variable can contain lists of pairs that are separated by comma
to represent the resource ID as embedded in the assembly followed by
the file name, like this:

RESOURCE_DEFS = Messages,TextResources.resx Errors,ErrorList.txt


Documentation-related needs? Use $(MDOC)
----------------------------------------

$(MDOC) is a front-end to the monodoc documentation system, supporting
documentation generation, updating, importing from Microsoft XML
Documentation and ECMA documentation formats, assembling documentation
for use within the monodoc documentation browser, and exporting
documentation to various other output formats such as static HTML.

It is currently only used for library.make's doc-update-local target
and for assembling documentation within $topdir/docs.

Installing files? Use $(MKINSTALLDIRS), $(INSTALL_DATA) or $(INSTALL_BIN), $(prefix), and $(DESTDIR).
-----------------------------------------------------------------------------------------------------

Every time a file is installed the commands should look like this:

```
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/my/dir
	$(INSTALL_DATA) myfile $(DESTDIR)$(prefix)/my/dir
```

This way the directory is created recursively if needed (admittedly, we could
probably rely on mkdir -p), the file is given the correct permissions,
the user can override $(MKINSTALLDIRS) and $(INSTALL) if they need to,
and we can support $(DESTDIR) installs. We use $(DESTDIR) to make
monocharge tarballs, and it's useful otherwise, so try and use it
consistently.

'make dist'? Use $(DISTFILES)
-----------------------------

The 'dist-default' target will copy the files listed in $(DISTFILES)
into the distribution directory, as well as Makefile and ChangeLog if
they exist. This is almost always all that you need, so ideally your
make dist support should only be:

```
DISTFILES = README Test/thoughts.txt

dist-local: dist-default
```

DISTFILES will cope correctly with files in subdirectories, by the
way. Note that if you put a nonexistant file or a directory in
DISTFILES it will *not* complain; it will just ignore it.

If you want to test your 'make dist' code, you can try

```
$ cd class/Mono.MyClass
$ make dist-local distdir=TEST
```

And your files should be copied into TEST/ in the current directory.
There is a toplevel 'make distcheck' target, which will build a dist
tarball, try to build it, install files to a temporary prefix, make
clean it, make a distribution, and compare the files left over to the
files originally in the tarball: they should be the same. But this
takes about 15 minutes to run on my 1.1 Ghz computer, so it's not for
the faint of heart.

Lots of files? Use $(wildcard *.foo)
------------------------------------

When specifying the sources to a library or executable, wildcards are
not encouraged; in fact they're not allowed if you use library.make or
executable.make. But there are times when they're useful, eg:

```
DISTFILES = $(wildcard Test/*.in) $(wildcard Test/*.out)
```

Just so you know that `make` has this feature.

Referencing files in other directories? Use $(topdir).
------------------------------------------------------

$(topdir) is the path to the top directory from the current build
directory. Basically it's a sequence of ../.. computed from the value
that you give $(thisdir) at the top of your Makefile. Try to reference
things from $(topdir), so your code can be moved or cut-and-pasted
around with a minimum of fuss.

Conditional building? Use ifdef/ifndef/endif
--------------------------------------------

Now in general we want to avoid conditional building, but sometimes
something doesn't work on Linux or already exists on Windows or
whatnot. (See below on recommended form for how to build
platform-specifically.) GNU Make supports the following construction:

```
BUILD_EXPERIMENTAL = yes

ifdef BUILD_EXPERIMENTAL
experimental_stuff = my-experiment.exe
else
experimental_stuff = 
endif

all-local: my-sane.exe $(experimental_stuff)
```

'ifdef' means 'if the variable is set to nonempty', so you could have 

```
BUILD_EXPERIMENTAL = colorless green ideas sleep furiously
```

and Make would be happy. I hope that the meaning of 'ifndef' should be
obvious. If you want to only sometimes build a target, the above
construction is the recommended way to go about it; it's nice to have
the rules exist in a Makefile even if they aren't invoked.

If you want to see why conditionals aren't nice, take a look at
library.make or class/corlib/Makefile.


'Private' directories that shouldn't be built by default? Use DIST_ONLY_SUBDIRS
--------------------------------------------------------------------------------

Several of the MCS class libraries have demo or experimental
implementations that depend on things not included with MCS (say,
Gtk#). We don't want to build them by default, because the user might
not have those dependencies installed, but it's nice to have a
Makefile for them to be built nicely.

First of all, there's nothing stopping you from writing a Makefile for
such a directory; just don't put it in the SUBDIRS line of its parent
directory. That way, you can do all the normal build things like 'make
all' or 'make clean' in that directory, but people trying to bootstrap
their system won't run into problems.

At the same time you probably want to include this directory in the
distribution so that people can use your demo or experimental code if
they know what they're doing. Hence the variable
$(DIST_ONLY_SUBDIRS). As you might guess, it's like the SUBDIRS
variable: it lists subdirectories that a regular shouldn't recurse
into, but should have their 'make dist' rules invoked. 

Say you've written Mono.MyFancyLib.dll and you have
a demo app using Gtk# called MyFancyDemo. The Makefile rules might
look like this:

class/Mono.MyFancyLib/Makefile
```
thisdir = class/Mono.MyFancyLib
SUBDIRS =
DIST_ONLY_SUBDIRS = MyFancyDemo
include ../../build/rules.make

LIBRARY = Mono.MyFancyLib.dll
LIB_MCS_FLAGS = /r:System.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
```

class/Mono.MyFancyLib/MyFancyDemo/Makefile
```
thisdir = class/Mono.MyFancyLib/MyFancyDemo
SUBDIRS =
include ../../../build/rules.make

PROGRAM = FancyDemo.exe
LOCAL_MCS_FLAGS = /r:gtk-sharp.dll

include ../../../build/executable.make
```




Special recursion needs?
------------------------

By default, rules.make defines the all, install, clean, etc. targets
to look something like this:

```
   all: all-recursive
	$(MAKE) all-local
```

Sometimes that doesn't cut it; say for example you want to check for
something before doing a lengthy recursive build (see
$(topdir)/Makefile) or you have a something like this

```
	class/MyLibrary:
		Build MyLibrary.dll
	class/MyLibrary/Test:
		Build TestMyLibrary.exe
```

`make clean test` will fail here, because the build will happen in
the Test subdirectory first, so there will be no `MyLibrary.dll` to link
against. (Unless you write a nasty evil relative path rule which is
strongly discouraged.)

Anyway, to solve this problem you can do

```
thisdir = class/MyLibrary
SUBDIRS = Test
include ../../build/rules.make

# Normally, make runs 'all-recursive' first, and then 'all-local'
# With this, we ensure that 'all-local' is executed first.
all-recursive: all-local

test-recursive: test-local
...
```


A few implementation details
----------------------------

The way rules.make does its recursion is very standard; it maps
`{all,install,clean,dist,test}` to `$@-recursive`, which executes that rule
in each directory in `$(SUBDIRS)`, and then calls `$@-local` bin the current
directory. So something that gets built in a subdirectory cannot rely on
something that gets built in its parent directory. If this is a problem,
see the previous section.  Note that the recursive rule for 'dist' is
different; it makes dist-recursive in subdirectories, so you at least
have to define that rule.

Note that even a directory that doesn't, for example, have any tests
must still define test-local; otherwise 'make test' run from the
toplevel directory will break.

Flags for Tools
---------------

We want to make it so that the user can specify certain flags to
always be given to a tool, so there's a general way of implementing
FLAGS variables:

* `$(foo_FLAGS)` remains unset or defaulted to something
  sensible; the user can provide overrides this way.

* `$(LOCAL_foo_FLAGS)` is set in a specific Makefile to
  provide necessary values.

* `$(PLATFORM_foo_FLAGS)` is set in the platform configuration
  to provide platform-specific values.

* `$(PROFILE_foo_FLAGS)` is set in the profile configuration
  to provide profile-specific values.

* `$(USE_foo_FLAGS)` is defined to be the combination of all of
  the above, and it's what is actually passed to $(foo).

`$(MCS_FLAGS)` and `$(CFLAGS)` follow this model. If you end up finding
that another tool is used commonly (hm, jay...), please follow this form.

Portability tips
----------------

Always use the icky Windows /argument way of passing parameters to the C#
compiler so that csc can be used.

Always use `/r:foo.dll`, not `/r:foo`. Windows requires the former.

If you're writing shell script code as part of a make rule, remember
that Windows has command-line length limits. So something like

```
mytool $(all_the_sources_to_corlib)
```

Is probably going to cause problems. As I understand it, 

```
for f in  $(all_the_sources_to_corlib) ; do ...
```

is ok, since the shell itself doesn't have those limitations. Other
than that, you should still try to write fairly portable shell
script. Linux and Cygwin both use the GNU utilities, but there's at
least one hardy soul trying to build Mono on HP/UX, and no doubt there
will be ports to more Unices as time goes on.






Misc
----

We still don't use `/d:NET_1_1`; it causes some build problems right
now.

There's a hack in class/System.Data/Makefile to work around a very
strange crash in the runtime with some custom attribute stuff. It'd be
nice to fix it.

Also, there's a /lib:$(prefix)/lib in the System.dll Makefile, which
is for some reason necessary if System.Xml.dll hasn't been built yet.
(Well, it's necessary because of the /r:System.Xml.dll, but that
should be in the search path, it seems.)

A lot of the weird targets in the old makefiles have been dropped; I
have a feeling that a lot of them are archaic and not needed anymore.

I'd really like to write a build tool in C#. It would be nice to have
something really extensible and well-designed and clean. NAnt is,
IMHO, an apalling abomination and a tragically bad attempt at solving
the software building problem. Just so you know.

(On the other hand, NUnit is really neat.)

Peter
