#!/bin/bash

TOPDIR=~/mono
INSTALL=$TOPDIR/install
BACKUP=$TOPDIR/install.bak
SENDMAIL=$TOPDIR/mono/mcs/tools/tinderbox/smtp

LOGBASE=$TOPDIR/.build.log
LOG=$LOGBASE.txt
LOGPREV=$LOGBASE.prev.txt
LOGFATAL=$LOGBASE.fatal.txt
LOGDATE=$TOPDIR/.build.date
LOGLOG=$TOPDIR/.build.log.log.txt
export LOGDATE

EMAIL_FATAL="piersh@friskit.com"
EMAIL_MESSAGE="piersh@friskit.com"
#EMAIL_MESSAGE="mono-hackers-list@ximian.com"
EMAIL_FROM="piersh@friskit.com"
EMAIL_HOST="zeus.sfhq.friskit.com"
EMAIL_CC="piersh@friskit.com"

DELAY_SUCCESS=5m			# wait after a successful build
DELAY_CHECK_BROKEN=5m		# wait while verifying the build is broken
DELAY_STILL_BROKEN=3m		# wait while waiting for fix
DELAY_BROKEN=5m				# wait after notification sent

FILTER_LOG="sed -e 's/^in <0x[0-9a-z]*>//' -e 's/(process:[0-9]*)://' -e 's/^\[[0-9][0-9]*:[0-9][0-9]*\] - .*//' -e 's/^[0-9][0-9]* - Member cache//' -e 's/^[0-9][0-9]* - Misc counter//'"

function fatal ()
{
	$SENDMAIL -h $EMAIL_HOST -f $EMAIL_FROM -t $EMAIL_FATAL -c $EMAIL_CC -a $LOGFATAL -s "[MONOBUILD] FATAL ERROR"
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
	cvs -z3 update -APd mcs mono 2>&1 | tee -a $LOGFATAL
	[ $PIPESTATUS == "0" ] || fatal

	# clean mcs
	cd $TOPDIR/mcs
	make -f makefile.gnu clean 2>&1 | tee -a $LOGFATAL
	[ $PIPESTATUS == "0" ] || fatal

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

	cd $TOPDIR/mono

	# configure mono build
	./autogen.sh --prefix=/home/server/mono/install 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# clean mono
	make clean 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# build mono
	make 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

	# clean old DLLs from runtime
	cd $TOPDIR/mono/runtime
	rm -f *.dll

	# install everything else
	cd $TOPDIR/mono
	make install 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1


	# copy new MCS and CORLIB to build tools
	cp -f $TOPDIR/mcs/class/lib/corlib.dll $INSTALL/lib/
	cp -f $TOPDIR/mcs/mcs/mcs.exe $INSTALL/bin/

	# make runtime libraries/tools
	cd $TOPDIR/mcs
	make -f makefile.gnu 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1


	# retrieve runtime libraries
	cd $TOPDIR/mono/runtime
	make 2>&1 | tee -a $LOG
	[ $PIPESTATUS == "0" ] || return 1

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
	touch $LOG
	touch $LOGPREV
	echo Comparing build logs
	eval $FILTER_LOG $LOG > $LOG.tmp
	eval $FILTER_LOG $LOGPREV > $LOGPREV.tmp
	diff -b --brief $LOG.tmp $LOGPREV.tmp
}

function build_fixed ()
{
	echo "Build fixed:       `date`" > .build.msg
	echo "Previous breakage: `cat .build.date.last_fail`" >> .build.msg
	echo "Previous build:    `cat .build.date.last_success`" >> .build.msg
	echo >> .build.msg

	cat .build.msg | $SENDMAIL -h $EMAIL_HOST -f $EMAIL_FROM -t $EMAIL_MESSAGE -c $EMAIL_CC -s "[MONOBUILD] fixed"
}

function build_failed ()
{
	echo "Build broken:   `date`" > .build.msg
	echo "Broken since:   `cat .build.date.last_fail`" >> .build.msg
	echo "Previous build: `cat .build.date.last_success`" >> .build.msg
	echo >> .build.msg

	cat .build.msg | $SENDMAIL -h $EMAIL_HOST -f $EMAIL_FROM -t $EMAIL_MESSAGE -c $EMAIL_CC -a $LOG -s "[MONOBUILD] broken"
}

#rm -f $LOG $LOGPREV

while [ 1 ] ; do
	
	if build_mono ; then

		cd $TOPDIR

		echo "|||||||||||||||||||||||||"
		echo "|||| BUILD SUCCEEDED ||||"
		echo "|||||||||||||||||||||||||"
		echo SUCCESS: `date` >> $LOGLOG
		date > $LOGDATE.last_success
		sleep $DELAY_SUCCESS

	else

		cd $TOPDIR
		rm -f $LOGPREV

		until compare_logs ; do

			date > $LOGDATE.last_fail
			echo logs differ

			sleep $DELAY_CHECK_BROKEN

			if build_mono ; then

				cd $TOPDIR
				build_fixed
				echo "|||||||||||||||||||||"
				echo "|||| BUILD FIXED ||||"
				echo "|||||||||||||||||||||"
				echo FIXED: `date` >> $LOGLOG
				date > $LOGDATE.last_success
				sleep $DELAY_SUCCESS
				break 2
			fi

			cd $TOPDIR
			echo CHECK: `date` >> $LOGLOG

		done

		build_failed
		echo "||||||||||||||||||||||"
		echo "|||| BUILD BROKEN ||||"
		echo "||||||||||||||||||||||"
		echo BROKEN: `date` >> $LOGLOG
		date > $LOGDATE.last_fail
		sleep $DELAY_BROKEN

		until build_mono ; do

			cd $TOPDIR
			echo "||||||||||||||||||||||||||||"
			echo "|||| BUILD STILL BROKEN ||||"
			echo "||||||||||||||||||||||||||||"
			echo STILL BROKEN: `date` >> $LOGLOG
			date > $LOGDATE.last_fail
			sleep $DELAY_STILL_BROKEN

		done

		cd $TOPDIR
		build_fixed
		echo "|||||||||||||||||||||"
		echo "|||| BUILD FIXED ||||"
		echo "|||||||||||||||||||||"
		echo FIXED: `date` >> $LOGLOG
		date > $LOGDATE.last_success
		sleep $DELAY_SUCCESS

	fi

done

