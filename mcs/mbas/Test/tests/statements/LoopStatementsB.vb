'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()
	  Dim i as integer = 0
        Dim x As Integer
        x = 3
        Do
           i = i + 1
	     x = x - 1
	 Loop While x <> 1
	 if i <> 2 then 
		Throw new System.Exception("While not working properly. Expected 2 but got "&i)
	 End if 
    End Sub
End Module
