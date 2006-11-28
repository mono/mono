#!/bin/bash

for file in $*; do PEVerify /NOLOGO $file ; done
