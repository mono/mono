#!/bin/bash -e

exec external/bockbuild/bb MacSDK --package --arch=darwin-64 "$@"
