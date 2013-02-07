Contributing to Mono
####################


Guidelines
==========

When contributing to the Mono project, please follow the `Mono Coding
Guidelines`_.  We have been using a coding style for many years,
please make your patches conform to these guidelines.

.. _`Mono Coding Guidelines`:  http://www.mono-project.com/Coding_Guidelines


Etiquette
=========

In general, we do not accept patches that merely shuffle code around,
split classes in multiple files, reindent the code or are the result
of running a refactoring tool on the source code.  This is done for
three reasons: (a) we have our own coding guidelines; (b) Some modules
are imported from upstream sources and we want to respect their coding
guidelines and (c) it destroys valuable history that is often used to
investigate bugs, regressions and problems.


License
=======

The Mono project uses the MIT X11, GNU LGPL version 2 and the Apache
License 2.0. We also imported some Microsoft code licensed under the
open source Microsoft Public License (MS-PL).

Different parts of Mono use different licenses. The actual details of
which licenses are used for which parts are detailed on the LICENSE
file in this directory.

When contributing code, make sure that your contribution falls under
the appropriate license. For example, contributions to code licensed
under MIT X11 code, should be MIT X11 code.

The Runtime is a special case. The code is dual licensed by Xamarin
under both the GNU LGPL v2 license and is also available under
commercial terms.  For the runtime, you should either sign an
agreement that grants Xamarin the rights to relicense your code under
other licenses other than the LGPL v2 or your contribution must be
made as an MIT X11 license which grants us the same rights, but
involves no paperwork.


Submitting Patches
==================

When Submitting patches to the dual-licensed portions, please specify
on the commit the license that the code is under.
