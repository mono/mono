'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module TypeValRefString
	Sub Main()
		Dim str1, str2 As String
		str1 = "String 1"
		str2 = str1
		if String.Compare(str2,"String 1")<>0
			Throw New Exception ("str2 Should be String1")
		End if
		str2 = "String 2"
		str1 = Nothing
		if (String.Compare(str1,Nothing) <> 0) or (String.Compare(str1,Nothing) <> 0) 
				Throw New Exception ("Str1 should be nothing and Str2 should be String 2 but got Str1 = " & Str1 &" Str2 = " & str2 )
		End if
	End Sub
End Module