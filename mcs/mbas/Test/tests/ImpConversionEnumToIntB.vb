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
    NotValid = -1	
End Enum

Sub Main()
	Dim i As Integer = - 1
	if Days.NotValid < i Then 
		Throw New System.Exception ("#Can not Convert from Enum to Integer")
	End if
End Sub
End Module
