#!/bin/sh -e

pkg install -y bash openjdk8 git gmake autoconf automake libtool cmake gettext libgdiplus unixODBC sqlite
mkdir -p /mnt/jenkins/pbuilder /mnt/scratch /mnt/jenkins/buildplace
chown builder /mnt/jenkins /mnt/jenkins/pbuilder /mnt/scratch

# for compatibility with the mono build scripts, ideally shouldn't be necessary
ln -s /usr/local/bin/bash /bin/bash
rm -f /bin/make && ln -s /usr/local/bin/gmake /bin/make
