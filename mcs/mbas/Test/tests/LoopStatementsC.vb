'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()
        Dim x As Integer
        Dim i As Integer=0
        x = 3
        Do While x <> 1
            i = i + 1
		x = x - 1
        Loop
	  if i <> 2 then 
		Throw new System.Exception("While not working properly. Expected 2 but got "&i)
	  End if 
    End Sub
End Module
