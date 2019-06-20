#! /bin/bash

# Git 2.22.0 in homebrew seems to have a bug:
#  git submodule foreach --recursive git reset --hard HEAD
# errors out with:
#
# error: unknown option `hard'
# usage: git submodule--helper foreach [--quiet] [--recursive] [--] <command>

# which is nonsense because `git foreach -h` says:
# usage:
#    ...
#    or: git submodule [--quiet] foreach [--recursive] <command>

exec git reset --hard "$@"
