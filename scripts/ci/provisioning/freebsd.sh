#!/bin/sh -ex

######################################################################
# Build Script Prerequisites
######################################################################
mkdir -p /mnt/jenkins/pbuilder /mnt/scratch /mnt/jenkins/buildplace
chown builder /mnt/jenkins /mnt/jenkins/pbuilder /mnt/scratch

## Prevent false System.Security-xunit failures 
if [ ! -d /usr/share/.mono ]; then
  mkdir /usr/share/.mono
  chown builder /usr/share/.mono
else
 chown builder /usr/share/.mono
fi

######################################################################
# System Prerequisites
######################################################################
## Set up our kernel modules
for km in fdescfs linprocfs; do
  kldload $km || true
done
## Set up the appropriate mounts prior to pkg installation. It's easier this way.
if [ ! -e /dev/fd ]; then
  echo "fdesc /dev/fd fdescfs rw 0 0" >> /etc/fstab
  mount /dev/fd
fi
## Linux compatibility shim here; this may be required for some unit tests and applications
if [ ! -d /compat/linux/proc ]; then
  mkdir -p /compat/linux/proc
  echo "linprocfs /compat/linux/proc linprocfs rw 0 0" >> /etc/fstab
  mount /compat/linux/proc
elif [ -e /compat/linux/proc ]; then
  echo "linprocfs /compat/linux/proc linprocfs rw 0 0" >> /etc/fstab
  mount /compat/linux/proc
fi

######################################################################
# Package Installation
######################################################################
## Validate that pkg is working.
if ! pkg info > /dev/null; then
  ## We can't possibly continue.
  echo "[FATAL] pkg is non-functional; build impossible."
  exit 1
fi
## As of 2020Q1, need to update pkg itself due to pkg itself being upgraded.
if [ $(date +%s) -gt 158625000 ]; then
	sed -i '' -E '/.?IGNORE_OSVERSION.*/d' /usr/local/etc/pkg.conf
	echo 'IGNORE_OSVERSION = true;' >> /usr/local/etc/pkg.conf
	/usr/bin/env ASSUME_ALWAYS_YES=yes /usr/sbin/pkg bootstrap -f
	pkg update
	pkg upgrade -y
fi

## These packages are MUST have; use the python3 metaport.
if ! pkg install -y bash git gmake autoconf automake cmake libtool python3 ca_root_nss; then
  ## Kill the attempt quickly if we definitely cannot build.
  echo "[FATAL] Error installing critical packages. Aborting because building is impossible."
  exit 1
fi

## These packages are NICE to have, so be more graceful.
for pn in gettext-runtime gettext-tools cairo libdrm mesa-dri mesa-libs openjdk8 libgdiplus unixODBC sqlite3 xorgproto pango libinotify; do
  pkg install -y $pn || true
done

## Jan 7 2020 - work around image having mangled perl defaults
if [ ! -f /usr/local/bin/perl ]; then
  ## Force reinstall so it places /usr/local/bin/perl binary
  pkg install -f -y perl5
fi

# for compatibility with the mono build scripts, ideally shouldn't be necessary
ln -s /usr/local/bin/bash /bin/bash
# fix for gen-descriptor-tests.py
if ! command -v python3 ; then
  if [ -f /usr/local/bin/python3.7 ]; then
    ln -s /usr/local/bin/python3.7 /usr/local/bin/python3
  elif [ -f /usr/local/bin/python3.6 ]; then
    ln -s /usr/local/bin/python3.6 /usr/local/bin/python3
  else
    echo "[NOTICE] Some tests will fail due to calling python3 explicitly."
  fi
fi
## Do not remove, instead rename; otherwise it's impossible to support ports infrastructure testing
mv /usr/bin/make /usr/bin/bsdmake && ln -s /usr/local/bin/gmake /usr/bin/make

# force internal IP of Jenkins master
echo "10.0.0.4 jenkins.mono-project.com" >> /etc/hosts
