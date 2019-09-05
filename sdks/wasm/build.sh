#!/usr/bin/env bash

# Stop script if unbound variable found (use ${var:-} if intentional)
set -u

# Stop script if command returns non-zero exit code.
# Prevents hidden errors caused by missing error code propagation.
set -e

usage()
{
  echo "Common settings:"
  echo "  --configuration <value>    Build configuration: 'Debug' or 'Release' (short: -c)"
  echo "  --help                     Print help and exit (short: -h)"
  echo ""

  echo "Actions:"
  echo "  --clean                    Clean up the clean targets"
  echo "  --reconfigure              Force provision and configure"
  echo "  --test                     Run all tests (short: -t)"
  echo "  --skiparchive              Do not archive"
  echo "  --skipbuild                Do not build"
  echo ""

  echo "Command line arguments starting with '/p:' are passed through to MSBuild."
  echo "Arguments can also be passed in with a single hyphen."
}

cleanall=false
configuration='Release'
force_reconfigure=false
test=false
skiparchive=false
skipbuild=false

while [[ $# > 0 ]]; do
  opt="$(echo "${1/#--/-}" | awk '{print tolower($0)}')"
  case "$opt" in
    -help|-h)
      usage
      exit 0
      ;;
    -clean)
      cleanall=true
      ;;
    -configuration|-c)
      configuration=$2
      shift
      ;;
    -reconfigure)
      force_reconfigure=true
      ;;
    -test|-t)
      test=true
      ;;
    -skiparchive)
      skiparchive=true
      ;;
    -skipbuild)
      skipbuild=true
      ;;
    *)
      echo "Invalid argument: $1"
      usage
      exit 1
      ;;
  esac

  shift
done

CPU_COUNT=$(getconf _NPROCESSORS_ONLN || echo 4)

# clean all 
if [ "$cleanall" = "true" ]; then
  make clean
  exit 0;
fi

# provision and configuration
if [[ "$force_reconfigure" == "true" || ! -f .configured ]]; then
  # re-create Make.config
  echo "DISABLE_ANDROID=1" > ../Make.config
  echo "DISABLE_IOS=1" >> ../Make.config
  echo "DISABLE_MAC=1" >> ../Make.config
  echo "DISABLE_DESKTOP=1" >> ../Make.config
  #echo "ENABLE_CXX=1" >> ../Make.config
  if [[ "$configuration" == "Debug" ]]; then
    echo "CONFIGURATION=debug" >> ../Make.config
  fi
  #echo "ENABLE_WASM_THREADS=1" >> ../Make.config

  make -C ../builds provision-wasm
  make -j ${CPU_COUNT} -C ../builds configure-wasm NINJA=
  touch .configured
fi

# build
if [ "$skipbuild" = "false" ]; then
  make -j ${CPU_COUNT} -C ../builds build-wasm NINJA=
fi

# archive
if [ "$skiparchive" = "false" ]; then
  make -j ${CPU_COUNT} -C ../builds archive-wasm NINJA=
fi

# run all tests
if [ "$test" = "true" ]; then
  export aot_test_suites="System.Core"
  export mixed_test_suites="System.Core"
  export xunit_test_suites="System.Core corlib"

  make -j ${CPU_COUNT} build
  make run-ch-mini
  make run-v8-mini
  make run-sm-mini
  make run-jsc-mini
  make run-all-corlib
  #The following tests are not passing yet, so enabling them would make us perma-red
  #make run-all-System
  make run-all-System.Core
  for suite in ${xunit_test_suites}; do make run-${suite}-xunit; done
  # disable for now until https://github.com/mono/mono/pull/13622 goes in
  #make test-debugger
  make run-browser-tests
  #make run-browser-threads-tests
  make run-v8-corlib
  make -j ${CPU_COUNT} run-aot-mini
  make -j ${CPU_COUNT} build-aot-all
  for suite in ${aot_test_suites}; do make run-aot-${suite}; done
  for suite in ${mixed_test_suites}; do make run-aot-mixed-${suite}; done
  #make check-aot
  make package
fi

exit 0