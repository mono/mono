'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()	
	Dim i as integer 
        i = 20-10.5
	System.Console.WriteLine(20-10.5)
	System.Console.WriteLine("i is {0}",i)
'	if i<>15 then
'		Throw New System.Exception("Select not working properly. Expected 15 but got "&i)
'	End if
    End Sub
End Module

