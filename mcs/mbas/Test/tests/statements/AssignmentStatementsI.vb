'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Expected  System.ArgumentException: Argument 'Start' is not a valid value

Module Test
    Sub Main()
        Dim s1 As String = "abcdefg"
        Dim s2 As String = "1234567"
	  Dim i as integer = 0
	try
        Mid$(s1,8, 3) = s2
	  Catch e as System.Exception 
		i = 1		 
	end try
	if i<>1
		Throw new System.Exception("Mid Assignment statement is not working properly") 
	End if
    End Sub
End Module

