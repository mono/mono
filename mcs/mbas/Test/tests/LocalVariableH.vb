'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module F
	Dim i as Integer
	Sub Main()	
		if I<>0 then
			Throw new System.Exception("Local Variables not working properly. Expected 0 but got"&i)
		End if
	End Sub
End Module
