'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Option Compare text
Imports System

Module LikeOperator2
	 Public Sub Main()
		dim a as boolean
		a = "o" Like "[A-Z]"
		If a <> False Then
		    Throw new System.Exception("#A1-LikeOperator:Failed")
		End If
	end sub
End Module
