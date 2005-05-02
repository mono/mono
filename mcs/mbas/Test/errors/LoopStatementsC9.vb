'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 18
REM ExpectedError: BC30238
REM ErrorMessage: 'Loop' cannot have a condition if matching 'Do' has one.

Module Test
    Sub Main()
        Dim x As Integer
        Dim i As Integer=0
        x = 3
        Do While x <> 1
            i = i + 1
		x = x - 1
        Loop While x <> 1
	  if i <> 4 then 
		Throw new System.Exception("While not working properly. Expected 4 but got "&i)
	  End if 
    End Sub
End Module
