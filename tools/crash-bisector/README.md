# Crash Bisector

It is often difficult finding a bug in an optimization pass.  The test
case for which the optimization produces incorrect results or crashes
the program might have thousands of methods compiled, and the bug
might only show up in one or a handful of them.  It would be much
easier if you knew which methods specifically trigger the bug.

This tool automates the search for those methods.  Given some
reasonable conditions it will find a (locally) minimal set of methods
for which, if a given optimization is applied to them, a test case
will fail or crash.

You will need a test case for which Mono either crashes with your
optimization, or returns a non-zero exit status.  The bisector will
then run the test case without your optimization, gathering a list of
all the methods that are compiled.  It will then start bisecting this
list, applying the optimization to only one half of the methods,
checking whether the test still fails.

At some point bisecting will either terminate with a single method
that still makes the test fail, or it will come to a point where a set
of methods makes the test fail, but either half of that set will not.
In that case it will start trying to remove smaller subsets of
methods, until at some point no single method can be removed anymore,
i.e., all the methods in the set must be optimized for the test to
fail.

## Usage

You run it like so:

    mono crash-bisector.exe --mono ../mini/mono-sgen --opt free-regions -- generics-sharing.2.exe

Here the optimization is `free-regions` and the test case is
`generics-sharing.2.exe`.

Note that if the optimization you're debugging is turned on by default
you'll have to pass a `-O` option to Mono to turn it off, like so:

    mono crash-bisector.exe --mono ../mini/mono-sgen --opt intrins -- -O=-intrins generics-sharing.2.exe

## Assumptions

The bisector assumes that each run of your test case compiles the same
methods, and that applying your optimization to some of them doesn't
change which methods are compiled.

The test case is assumed to succeed or fail deterministically.

The optimization bug must also be deterministic.
