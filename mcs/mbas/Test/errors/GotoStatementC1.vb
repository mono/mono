'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30132
REM ErrorMessage: Label 'a' is not defined.

Module gotostmt
	Sub Main()
		Dim i as integer
		goto a:
			i = i + 1
		a1:
			i = i + 1
		if i <> 1 then
			Throw new System.Exception("Goto statement not working properly ")
		End If
 	End Sub
End Module
