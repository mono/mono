#!/bin/sh

if [ ! -d "MSNet" ]; then
	mkdir MSNet
fi

export MSNet=Yes

mcs /target:library  TestPoint.cs /r:NUnit.Framework.dll /r:System.Drawing.dll 
nunit-console TestBitmap.dll

mcs /target:library  TestBitmap.cs /r:NUnit.Framework.dll /r:System.Drawing.dll 
nunit-console TestBitmap.dll

mcs /target:library  TestSizeF.cs /r:NUnit.Framework.dll /r:System.Drawing.dll 
nunit-console TestSizeF.dll

mcs /target:library  TestSize.cs /r:NUnit.Framework.dll /r:System.Drawing.dll 
nunit-console TestSize.dll