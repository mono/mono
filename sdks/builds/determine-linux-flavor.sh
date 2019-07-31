#!/bin/bash
if [ -f /etc/os-release ]; then
    . /etc/os-release
    FLAVOR=$NAME
elif [ -n "$(which lsb_release)" ]; then
    FLAVOR=$(lsb_release -si)
elif [ -f /etc/lsb-release ]; then
    . /etc/lsb-release
    FLAVOR=$DISTRIB_ID
elif [ -f /etc/debian_version ]; then
    FLAVOR=Debian
else
    FLAVOR=$(uname -s)
fi

echo $FLAVOR
