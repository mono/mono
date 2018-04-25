Acceptance tests
================

This directory contains acceptance tests, which are third party test suites and frameworks that are used to validate Mono against a wider range of test cases that go beyond the Mono unit tests that run as part of CI.

In order to make checking out those test suites optional we don't use traditional git submodules, but instead clone them on demand when needed. The custom submodule repositories are checked out into the acceptance-tests/external/ directory.

## Usage

Running all test suites is possible via "make check-full". There are also targets for running individual test suites, see below.

Some of the test suites require an installed Mono (i.e. they don't work with the in-tree build), those will ask you to pass in the PREFIX variable pointing to the installation directory when invoking make. Note that this directory needs to be writable as we overwrite some files there as part of testing.

## Individual test suites and targets

* `make check-ms-test-suite` - Runs tests that were shared with Xamarin, those are not available publically and will be skipped when the repository is not accessible.
* `make check-roslyn` - Runs the Roslyn test suite.
* `make check-coreclr` - Runs the CoreCLR test suite.
  * `make coreclr-runtest-coremanglib` - Runs only the CoreMangLib portion of the CoreCLR tests, those tests mostly target the BCL behavior.
  * `make coreclr-runtest-basic` - Runs only the CoreCLR tests that target runtime behavior and stability.
  * `make coreclr-compile-tests` - Convenience target that precompiles all the test cases in parallel.
  * `make coreclr-gcstress` - Runs the CoreCLR GC stress tests.

## Updating submodules

The SUBMODULES.json file stores information about the submodules, and make targets are used to check out submodules, check their versions, and update the submodule information:

* `make validate-<module>` - Checks whenever `<module>` is checked out and matches the version in the info file.
* `make reset-<module>` - Clones `<module>` if neccesary and checks out the revision specified in the info file.
* `make bump-<module>` - Updates the revision stored in the info file for `<module>`.
* `make bump-current-<module>` - Updates the revision stored in the info file for `<module>` to the current revision.
* `make commit-bump-<module>` - Same as `make bump-<module>`, but commits the change.
* `make commit-bump-current-<module>` - Same as `make bump-current-<module>`, but commits the change.

Example: when making a change in the CoreCLR submodule (like disabling something on Mono), you'd commit the change there and then run `make bump-current-coreclr` or `make commit-bump-current-coreclr` to update the version in SUBMODULES.json.
