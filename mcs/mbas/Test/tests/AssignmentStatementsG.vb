'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()
        Dim s1 As String = "abcdefg"
        Dim s2 As String = "1234567"

        Mid$(s1, 3, 3) = s2
	  If s1<>"ab123fg" then
		Throw new System.Exception("Mid Assingnment is not working. Excpected ab123fg but got "&s1)        
	 End if
    End Sub
End Module
