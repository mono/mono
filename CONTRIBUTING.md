Guidelines
==========

When contributing to the Mono project, please follow the [Mono Coding
Guidelines][1].  We have been using a coding style for many years,
please make your patches conform to these guidelines.

[1]: http://www.mono-project.com/community/contributing/coding-guidelines/

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
License 2.0.  We also imported some Microsoft code licensed under the
open source Microsoft Public License.

Different parts of Mono use different licenses.  The actual details of
which licenses are used for which parts are detailed on the LICENSE
file in this directory.

When contributing code, make sure that your contribution falls under
the appropriate license.  For example, contributions to code licensed
under MIT/X11 code, should be MIT/X11 code.

The runtime (`mono/...`) is a special case.  The code is dual-licensed
by Xamarin under both the GNU LGPL v2 license and is also available
under commercial terms.  For the runtime, you should either sign an
agreement that grants Xamarin the rights to relicense your code under
other licenses other than the LGPL v2 or your contribution must be
made as an MIT/X11 license which grants us the same rights, but
involves no paperwork.  For the latter case, please specify on your
commit(s) that you are licensing the changes under MIT/X11.

For other parts of the project that are dual-licensed, please state
on your commit(s) what license you are contributing the changes under.

Testing
=======

Pull requests go through testing on our [Jenkins server][2]. We will
usually only merge a pull request if it causes no regressions in a
test run there.

When you submit a pull request, one of two things happens:

* If you are a new contributor, Jenkins will ask for permissions (on
  the pull request) to test it. A maintainer will reply to approve
  the test run if they find the patch appropriate. After you have
  submitted a few patches, a maintainer will whitelist you so that
  all of your future pull requests are tested automatically.
* If you are a well-known, whitelisted contributor, Jenkins will go
  ahead and test your pull request as soon as a test machine is
  available.

When your pull request has been built, Jenkins will update the build
status of your pull request. If it succeeded and we like the changes,
a maintainer will likely merge it. Otherwise, you can amend your pull
request to fix build breakage and Jenkins will test it again.

[2]: http://jenkins.mono-project.com/

# Inactivity

Occasionally, a pull request sits for several months without any
response from the author. This isn't necessarily an issue, but we may
sometimes decide to close pull requests that have not seen any
progress for a long time. This is in interest of keeping the pull
request list clean so that other pull requests don't get lost in the
clutter.

If we do close your pull request due to inactivity, you're more than
welcome to submit it anew after you address any comments or issues that
were brought up on the original pull request.
