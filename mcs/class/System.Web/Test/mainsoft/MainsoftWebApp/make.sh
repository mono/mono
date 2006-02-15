#!/bin/bash

mcs -out:./bin/MainsoftWebApp.dll -target:library -recurse:./*.cs -r:System.Web.dll -r:System.Data.dll -r:System.Drawing.dll
