'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Module M
        Delegate Function SD(i as Integer)as Integer
        Function f(i as integer)as Integer
		return 10
        End Function 
        Function f(i as Single)as Integer
		return 12
        End Function

        Sub Main()
		dim b as boolean=false
		dim i as Integer
                dim d1 as SD
                d1= new SD(AddressOf f)
                i = d1.Invoke(10)
		    if (i<>10)
			Throw New System.Exception("Delegate not working Properly")
		    End If 
        End Sub
End Module
