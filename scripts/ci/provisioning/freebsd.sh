#!/bin/sh -e

mkdir -p /mnt/jenkins/pbuilder /mnt/scratch /mnt/jenkins/buildplace
chown builder /mnt/jenkins /mnt/jenkins/pbuilder /mnt/scratch

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

## Validate that pkg is working.
if ! pkg info > /dev/null; then
  ## We can't possibly continue.
  echo "[FATAL] pkgng is non-functional; build impossible."
  exit 1
fi

## These packages are MUST have.
if ! pkg install -y bash git gmake autoconf automake cmake libtool python27 python36 ca_root_nss; then
  ## Kill the attempt quickly if we definitely cannot build.
  echo "[FATAL] Error installing critical packages. Aborting because building is impossible."
  exit 1
fi

## These packages are NICE to have, so be more graceful.
for pn in gettext-runtime gettext-tools cairo libdrm mesa-dri mesa-libs openjdk8 libgdiplus unixODBC sqlite3 xorgproto pango libinotify; do
  pkg install -y $pn || true
done

# for compatibility with the mono build scripts, ideally shouldn't be necessary
ln -s /usr/local/bin/bash /bin/bash
## Do not remove, instead rename; otherwise it's impossible to support ports infrastructure testing
mv /usr/bin/make /usr/bin/bsdmake && ln -s /usr/local/bin/gmake /usr/bin/make

## XXX: System.Security-xunit failures are addressed here
if [ ! -d /usr/share/.mono ]; then
  mkdir /usr/share/.mono
  chown builder /usr/share/.mono
else
 chown builder /usr/share/.mono
fi
