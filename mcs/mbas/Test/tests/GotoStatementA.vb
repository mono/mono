'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module gotostmt
	Sub Main()
		Dim i as integer
		goto a:
			i = i + 1
		a:
			i = i + 1
		if i <> 1 then
			Throw new System.Exception("Goto statement not working properly ")
		End If
 	End Sub
End Module
