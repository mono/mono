'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()
	  Dim a as Integer = 10
	  Dim s as String 
	  s = a.toString()
	  if s <> "10" then
		Throw new System.Exception("Assignment not working. Expected 10 but got " &s)
	  End if	
    End Sub
End Module
