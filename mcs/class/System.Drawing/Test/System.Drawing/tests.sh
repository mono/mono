#!/bin/sh

if [ ! -d "mono" ]; then
	mkdir mono
fi

mcs TestPoint.cs /r:NUnit.Framework.dll /r:System.Drawing.dll /target:library 
nunit-console TestPoint.dll

mcs TestBitmap.cs /r:NUnit.Framework.dll /r:System.Drawing.dll  /target:library 
mono --debug nunit-console.exe TestBitmap.dll

mcs TestSize.cs /r:NUnit.Framework.dll /r:System.Drawing.dll  /target:library 
mono --debug nunit-console.exe TestSize.dll

mcs TestSizeF.cs /r:NUnit.Framework.dll /r:System.Drawing.dll  /target:library 
mono --debug nunit-console.exe TestSizeF.dll

mcs TestStringFormat.cs /r:NUnit.Framework.dll /r:System.Drawing.dll  /target:library 
mono --debug nunit-console.exe TestStringFormat.dll

