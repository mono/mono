'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'Byte'.

Module Test
    Sub Main()
		For i as Byte = 2 to 4 step 300
			For j as integer = 5 to 6
			Next 
		Next 
    End Sub
End Module
