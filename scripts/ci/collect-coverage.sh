#!/bin/bash -x

# This script is meant to be executed on all "slave" machines that run coverage collection.

COV_DIR=coverage
COV_NAME="$(echo $JOB_NAME | sed 's#/#-#g').info"
COV_INFO="$COV_DIR/$COV_NAME"

# Build Mono and collect coverage on the test suite.
CI_TAGS="collect-coverage,monolite,$CI_TAGS" scripts/ci/run-jenkins.sh

# Place the coverage info file into the coverage directory.
# Multiple such files can be assembled to create a unified coverage report that spans multiple architectures and operating systems.
mkdir "$COV_DIR"
scripts/ci/run-step.sh --label=coverage-lcov --timeout=20m lcov --no-external -c -d mono -d support -d tools -o "$COV_INFO"

# Generate HTML coverage report in the lcov directory at the root of the project.
scripts/ci/run-step.sh --label=coverage-genhtml --timeout=20m genhtml -s "$COV_INFO" -o lcov

# Make the paths relative so that they could be assembled from different Jenkins workspaces.
sed -Eie "s#^SF:$WORKSPACE/?#SF:#" "$COV_INFO"
