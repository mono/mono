'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module EnumToInt

Enum Days
    Sunday
    Monday
    Tuesday
    Wednesday
    Thursday
    Friday
    Saturday
End Enum

Sub Main()
	Dim i As Integer = 4
	if Days.Thursday <> i Then
		Throw New System.Exception ("#Can not Convert from Enum to Integer")
	End if
End Sub
End Module
