'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 29
REM ExpectedError: BC30311
REM ErrorMessage:  Value of type 'M.SD' cannot be converted to 'M.SD1'.

Imports System

Module M
        Delegate Function SD(i as Integer)as Integer
        Delegate Function SD1(i as Long) 

        Function f(i as long)
		return 10
        End Function 
        Function f(i as integer)as Integer
		return 10
        End Function 

        Sub Main()
                dim d1 as SD
		    dim d2 as SD1
                d1= new SD(AddressOf f)
                d1.Invoke(10)
		    d2= new SD1(AddressOf f)
		    d2 = d1
        End Sub
End Module
