'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module EnumToInt

Enum LongEnum As Long
    MyEnumLong1 = 125.5678
    MyEnumLong2 = 567.125
End Enum

Sub Main()
	Dim i As Integer
	Dim j As Integer = 125
	i = CInt(LongEnum.MyEnumLong1)
	If LongEnum.MyEnumLong1 < j Then 
		Throw New System.Exception ("#Can not Convert from Enum to Integer")
	End if
End Sub
End Module
