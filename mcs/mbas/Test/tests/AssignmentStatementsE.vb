'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Dim a as Integer = 0
    Function GetIndex()
		a = a+1 		
		return 1
    End Function

    Sub Main()	  
        Dim a1(2) As Integer
        a1(GetIndex()) = a1(GetIndex()) + 1  	 	  
	  if a <> 2 then
    		Throw new System.Exception("Assingment not working properly. Expected 2 but got "&a) 
	  End if
	  a = 0
        a1(GetIndex()) += 1  	 	  
	  if a <> 1 then
    		Throw new System.Exception("Compound Assingment not working properly. Expected 1 but got "&a) 
	  End if
    End Sub
End Module
