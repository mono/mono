#!/bin/bash

MCS_ROOT=/monobuild/mcs

mcs -out:./bin/HtmlAgilityPack.dll -target:library -define:MONO HtmlAgilityPack/*.cs 
mcs -out:./bin/MainsoftWebTest.exe -r:./bin/HtmlAgilityPack.dll Harness.cs WebTest.cs TestsCatalog.cs XmlComparer.cs
mcs -out:./bin/MainsoftWebTest.dll -target:library -define:MONO -define:NUNIT-r:$MCS_ROOT/class/lib/default/nunit.framework.dll -r:$MCS_ROOT/class/lib/default/nunit.core.dll -r:./bin/HtmlAgilityPack.dll Harness.cs WebTest.cs TestsCatalog.cs XmlComparer.cs
cp ./almost_config.xml ./bin/ 
cp ./test_catalog.xml ./bin/ 
cp ./App.config ./bin/MainsoftWebTest.exe.config
cp ./App.config ./bin/MainsoftWebTest.dll.config
