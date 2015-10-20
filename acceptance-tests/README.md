This directory contains acceptance tests, handled by optional non git submodule based submodules. Run the tests via "make check-full" (there are also targets for individual test suites).

The SUBMODULES.json file stores information about the submodules, and make targets are used to check out submodules, check their versions, and update the submodule information.

By convention, submodules repositories are at the same level as the mono repository.

Make targets available:

make check-<module> - Checks whenever <module> is checked out and matches the version in the info file.
make reset-<module> - Clones <module>if neccesary and checks out the revision specified in the info file.
make bump-<module> - Updates the revision stored in the info file for <module>.
make bump-current-<module> - Updates the revision stored in the info file for <module> to the current revision.
make commit-bump-<module> - Same as make bump-<module>, but commits the change.
make commit-bump-current-<module> - Same as make bump-current-<module>, but commits the change.
