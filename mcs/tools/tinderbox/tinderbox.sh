#!/bin/bash

# TOPDIR: set this to the directory containing
# your 'mono' and 'mcs' directories. Do NOT use '~' here.
TOPDIR=$HOME/mono

# BACKUP: initially this directory should contain a working install of mono.
# this directory should minimally contain the 'bin' and 'lib' dirs.
# after each successful build, the results are placed in here.
BACKUP=$TOPDIR/install.bak

# INSTALL: this is used as the install directory for
# the various stages of the build.
INSTALL=$TOPDIR/install

# SENDMAIL: uncomment this line if you want to send notifications.
# be careful to check the recipients below before running this script!
SENDMAIL=$TOPDIR/mcs/tools/tinderbox/smtp

# EMAIL_*: notification addresses. please change these before running!
EMAIL_FATAL="piersh@friskit.com"
EMAIL_MESSAGE="mono-patches@lists.ximian.com"
#EMAIL_MESSAGE="piersh@friskit.com"
#EMAIL_MESSAGE="mono-hackers-list@ximian.com"
EMAIL_FROM="piersh@friskit.com"
EMAIL_CC="-c mono-hackers-list@lists.ximian.com"
EMAIL_HOST="zeus.sfhq.friskit.com"

LOGBASE=$TOPDIR/.build.log
LOG=$LOGBASE.txt
LOGPREV=$LOGBASE.prev
LOGFATAL=$LOGBASE.fatal
LOGDATE=$TOPDIR/.build.date
LOGLOG=$TOPDIR/.build.log.log
BUILDMSG=$TOPDIR/.build.msg
export LOGDATE

DELAY_SUCCESS=5m			# wait after a successful build
DELAY_CHECK_BROKEN=5m		# wait while verifying the build is broken
DELAY_STILL_BROKEN=3m		# wait while waiting for fix
DELAY_BROKEN=5m				# wait after notification sent

FILTER_LOG="sed -e 's/^in <0x[0-9a-z]*>//' -e 's/:[0-9]*): WARNING \*\*:/): WARNING **/' -e 's/^\[[0-9][0-9]*:[0-9][0-9]*\] - .*//' -e 's/^[0-9][0-9]* - Member cache//' -e 's/^[0-9][0-9]* - Misc counter//' -e 's/: [0-9]* Trace\/breakpoint/ : Trace\/breakpoint/' -e 's/Needed to allocate blacklisted block at 0x[0-9a-f]*//'"

function fatal ()
{
	$SENDMAIL -h $EMAIL_HOST -f $EMAIL_FROM -t $EMAIL_FATAL -a $LOGFATAL -s "[MONOBUILD] FATAL ERROR (`uname -s -m`)"
	echo FATAL: `date` >> $LOGLOG
	echo FATAL ERROR
	exit 1
}

function build_mono ()
{
	# try to build

	echo building...

	cd $TOPDIR

	rm -f $LOGFATAL
	touch $LOGFATAL

	[ -f $LOG ] && mv $LOG $LOGPREV
	touch $LOG

	# restore tools from backup
	rm -fr $INSTALL
	cp -a $BACKUP $INSTALL

	# update from CVS
	cvs -z3 update -APd mcs mono 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# clean mcs
	cd $TOPDIR/mcs
	make -f makefile.gnu clean 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# build JAY
	cd $TOPDIR/mcs/jay
	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# build MCS compiler
	cd $TOPDIR/mcs/mcs
	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# build CORLIB with old tools
	cd $TOPDIR/mcs/class/corlib
	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# copy new MCS and CORLIB to build tools
	cp -f $TOPDIR/mcs/class/lib/corlib.dll $INSTALL/lib/
	cp -f $TOPDIR/mcs/mcs/mcs.exe $INSTALL/bin/



	cd $TOPDIR/mono

	# configure mono build
	rm -f config.cache
	./autogen.sh --prefix=$INSTALL 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# clean mono
	make clean 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# build mono
	make 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# install runtime
	cd $TOPDIR/mono/mono
	make install 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1



	# clean/make runtime libraries/tools
	cd $TOPDIR/mcs
	make -f makefile.gnu clean 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1



	# retrieve runtime libraries
	cd $TOPDIR/mono/runtime
	rm -f *.dll *.exe

	# install everything
	cd $TOPDIR/mono
	make install 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1


	# test mcs self-build
	cd $TOPDIR/mcs/mcs
	make -f makefile.gnu clean 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1
	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1


	# test corlib self-build
	cd $TOPDIR/mcs/class/corlib
	make -f makefile.gnu clean 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1
	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1


	# it worked, backup build tools
	rm -fr $BACKUP
	cp -a $INSTALL $BACKUP

	return 0
}

function compare_logs ()
{
	touch $LOG $LOGPREV
	echo Comparing build logs
	eval $FILTER_LOG $LOG > $LOG.tmp
	eval $FILTER_LOG $LOGPREV > $LOGPREV.tmp
	diff -b --brief $LOG.tmp $LOGPREV.tmp
}

function build_fixed ()
{
	echo "Build fixed:       `date`" > $BUILDMSG
	echo "Previous breakage: `cat .build.date.last_fail`" >> $BUILDMSG
	echo "Previous build:    `cat .build.date.last_success`" >> $BUILDMSG
	echo >> $BUILDMSG

	$SENDMAIL -h $EMAIL_HOST -f $EMAIL_FROM -t $EMAIL_MESSAGE $EMAIL_CC -s "[MONOBUILD] fixed (`uname -s -m`)" -m $BUILDMSG
	rm -f $BUILDMSG
}

function build_failed ()
{
	echo "Build broken:   `date`" > $BUILDMSG
	echo "Broken since:   `cat .build.date.last_fail`" >> $BUILDMSG
	echo "Previous build: `cat .build.date.last_success`" >> $BUILDMSG
	echo >> $BUILDMSG

	sed -e 's/$//' < $LOG > errors.txt
	tail -25 errors.txt >> $BUILDMSG
	rm -f errors.txt.gz
	gzip errors.txt

	$SENDMAIL -h $EMAIL_HOST -f $EMAIL_FROM -t $EMAIL_MESSAGE $EMAIL_CC -a errors.txt.gz -s "[MONOBUILD] broken (`uname -s -m`)" -m $BUILDMSG
	rm -f $BUILDMSG errors.txt.gz
}

function stabilize ()
{
	cd $TOPDIR
	while ! compare_logs ; do

		date > $LOGDATE.last_fail
		echo "|||||||||||||||||||||||||"
		echo "|||||| LOGS DIFFER ||||||"
		echo "|||||||||||||||||||||||||"
		echo CHECK: `date` >> $LOGLOG

		echo sleeping for $DELAY_CHECK_BROKEN
		sleep $DELAY_CHECK_BROKEN

		if build_mono ; then
			return 0
		fi

		cd $TOPDIR

	done
	return 1
}

[ -f $LOGPREV ] && mv $LOGPREV $LOG

while [ 1 ] ; do

	cd $TOPDIR
	
	if build_mono ; then

		cd $TOPDIR

		echo "|||||||||||||||||||||||||"
		echo "|||| BUILD SUCCEEDED ||||"
		echo "|||||||||||||||||||||||||"
		echo SUCCESS: `date` >> $LOGLOG
		date > $LOGDATE.last_success
		echo sleeping for $DELAY_SUCCESS
		sleep $DELAY_SUCCESS

	else

		if ! stabilize ; then

			build_failed

			echo "||||||||||||||||||||||"
			echo "|||| BUILD BROKEN ||||"
			echo "||||||||||||||||||||||"
			echo BROKEN: `date` >> $LOGLOG
			date > $LOGDATE.last_fail
			echo sleeping for $DELAY_BROKEN
			sleep $DELAY_BROKEN

			until build_mono ; do

				cd $TOPDIR
				echo "||||||||||||||||||||||||||||"
				echo "|||| BUILD STILL BROKEN ||||"
				echo "||||||||||||||||||||||||||||"
				echo STILL BROKEN: `date` >> $LOGLOG
				date > $LOGDATE.last_fail
				echo sleeping for $DELAY_STILL_BROKEN
				sleep $DELAY_STILL_BROKEN

			done

			cd $TOPDIR
			build_fixed
			echo "|||||||||||||||||||||"
			echo "|||| BUILD FIXED ||||"
			echo "|||||||||||||||||||||"
			echo FIXED: `date` >> $LOGLOG
			date > $LOGDATE.last_success
			echo sleeping for $DELAY_SUCCESS
			sleep $DELAY_SUCCESS

		fi

	fi

done

