@echo off
set X=cscript //nologo xslt.wsf /inFile:data\md-schema.xml

: dir /b /on *.cs > common.src
:cscript //nologo xslt.wsf /inFile:bitconv-types.xml /styleFile:bitconverter.xsl


%X% /styleFile:table-id.xsl > code\TableId.cs
%X% /styleFile:coded-id.xsl > code\CodedTokenId.cs
%X% /styleFile:elem-type.xsl > code\ElementType.cs
%X% /styleFile:tabs-decoder.xsl > code\TabsDecoder.cs
%X% /styleFile:tabs-base.xsl > code\TablesHeapBase.cs
%X% /styleFile:rows.xsl > code\Rows.cs
%X% /styleFile:tabs.xsl > code\Tables.cs
