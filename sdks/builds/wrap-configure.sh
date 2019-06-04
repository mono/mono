#!/bin/bash -e

START=$(date -u +"%s")

# First argument is the directory where we'll execute configure
# We assume the directory name is descriptive and use it in messages
# and temporary file names.
DIR="$1"
D=$(basename "$DIR")
shift

# Check if a cache is being used
if test -f "$D.config.cache"; then
	HAS_CACHE=1
fi

mkdir -p "$D"
echo "Configuring $D"
cd "$DIR"

# The rest of the arguments is the command to execute.
#
# We capture the output to a log (to make the output more quiet), and only
# print it if something goes wrong (in which case we print config.log as well,
# which can be quite useful when looking at configure problems on bots where
# there's much information in that file).
#
# If a cache was used and configure failed, we remove the cache and then try
# again.
#
if ! "$@" > ".stamp-configure-$D.log" 2>&1; then
	FAILED=1
	if [[ x"$HAS_CACHE" == "x1" ]]; then
		echo "Configuring $D failed, but a cache was used. Will try to configure without the cache."
		rm "../$D.config.cache"
		if ! "$@" > ".stamp-configure-$D.log" 2>&1; then
			echo "Configuring $D failed without cache as well."
		else
			FAILED=
		fi
	fi

	if [[ x"$FAILED" == "x1" ]]; then
		echo "Configuring $D failed:"
		sed "s/^/    /" < ".stamp-configure-$D.log"

		# Only show config.log if building on CI (jenkins/wrench)
		SHOW_CONFIG_LOG=0
		if test -n "$JENKINS_HOME"; then
			SHOW_CONFIG_LOG=1
		elif test -n "$BUILD_REVISION"; then
			SHOW_CONFIG_LOG=1
		fi
		if [[ x$SHOW_CONFIG_LOG == x1 ]]; then
			echo
			echo "    *** config.log *** "
			echo
			sed "s/^/    /" < config.log
		fi
		exit 1
	fi
fi

END=$(date -u +"%s")
DIFF=$((END-START))

echo "Configured $D in $((DIFF/60))m $((DIFF%60))s"
