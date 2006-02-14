#!/bin/sh 
################## System.Drawing: run-samples.sh #######################
#                                                                       #
# This script compiles and runs samples from each directory in          #
# System.Drawing.Samples directory. Compiled exes and output            #
# images, if any, are saved to respective directories.                  #
# Compile time logs are saved to compile-log.txt and runtime logs are   #
# saved to runtime-log.txt. Both log files are saved at the same        #
# location where this script is run.                                    #
#                                                                       #
# Following are the two ways to run this script,                        #
#        $ run-samples.sh                                               #
#        OR                                                             #
#        $ run-samples.sh [option]                                      #
#                                                                       #
# NOTE: Possible options are (m)ake, (c)lean, (r)un, (a)ll              #
#        --run is default option, when no option is specified.          #
#          Only one option can be specified at a time.                  #
#        -m, --make - compiles all the samples                          #
#        -c, --clean - deletes all the exes generated                   #
#        -r, --run - compiles and runs all the samples. [Default]       #
#        -a, --all - runs all the samples and also cleans               #
#                                                                       #
# **** This script would hang, if any sample hangs!!!                   #
#                                                                       #
#        Authors:                                                       #
#                Sachin <skumar1@novell.com>                            #
#                Ravindra <rkumar@novell.com>                           #
#                                                                       #
#        Copyright (C) 2004, Novell, Inc. http://www.novell.com         #
#                                                                       #
#########################################################################

# Prints the script usage
print_usage ()
{
    echo "Usage: run-samples [option]"
    echo "Only one option is processed at a time."
    echo "Possible options are: (m)ake, (c)lean, (r)un, (a)ll"
    echo "        -m, --make: Just compiles all the samples."
    echo "        -c, --clean: Just removes all the exes."
    echo "        -r, --run: makes and runs all the samples. [Default]"
    echo "        -a, --all: same as run and clean combined."
    echo "        --run option is assumed, if no option is specified."
    exit 1
}

# Compiles all the samples
compile ()
{
    echo === Compiling samples in $dir ===

    for src in *.cs
      do
      echo " $src"
      echo -n " $src:: " >> $CLOG
      $MCS $COMPILE_OPS $src >> $CLOG 2>&1
    done
}

# Deletes all the exes
clean ()
{
    echo === Cleaning $dir ===
    rm *.exe
}

# Compiles and runs all the samples
run ()
{
    compile
    echo === Running samples in $dir ===
    for exe in *.exe
      do
      echo " $exe"
      echo >> $RLOG
      echo "$dir: $exe :: " >> $RLOG
      echo >> $RLOG
      $MONO $RUN_OPS $exe >> $RLOG 2>&1
    done
}

# Compliles, runs and deletes all the exes
all ()
{
    run
    clean
}

# Environment setup

ROOT=$PWD
CLOG=$ROOT/compile-log.txt
RLOG=$ROOT/runtime-log.txt
MCS=mcs
MONO=mono
LIB=System.Drawing
COMPILE_OPS="-g -r:$LIB"
RUN_OPS=--debug

# Uncomment the following line, if you are running this script on MS
#MSNet=yes

# We don't process more than one command line arguments
if [ $# -gt 1 ]; then
    print_usage
fi

# Default option is run, if no command line argument is present
if [ -z $1 ]; then
    arg=--run
else
    arg=$1
fi

# Creates the log files
echo '*** LOG FILE for compile-time messages for System.Drawing Samples ***' > $CLOG
echo '*** LOG FILE for run-time output messages for System.Drawing Samples ***' > $RLOG

# All directories are processed under Samples.
for dir in `ls -d System.Drawing*`
  do
  echo >> $CLOG
  echo ===== $dir ===== >> $CLOG

  echo >> $RLOG
  echo ===== $dir ===== >> $RLOG

  # Change dir if it exists
  if [ -d $ROOT/$dir ]; then
      cd $ROOT/$dir
      case $arg in
	  "-m") compile ;;
	  "--make") compile ;;
	  "-r") run ;;
	  "--run") run ;;
	  "-a") all ;;
	  "--all") all ;;
	  "-c") clean ;;
	  "--clean") clean ;;
	  *) print_usage ;;
      esac
      cd ..
  else
      echo "$dir not found." >> $CLOG
      echo "$dir not found." >> $RLOG
  fi
done
