'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 32
REM ExpectedError: BC30685
REM ErrorMessage: 'fun' is ambiguous across the inherited interfaces 'AB' and 'AC'.

'This program is used to check the functioning of Shadows

Interface A
        Sub fun(ByVal i As Integer)
End Interface

Interface AB
        Inherits A
        Shadows Sub fun(ByVal i As Integer)
End Interface

Interface AC
        Inherits A
        Shadows Sub fun(ByVal i As Integer)
End Interface

Interface ABS
        Inherits AB, AC
End Interface

Class Test
        Sub D(ByVal d As ABS)
        d.fun(2)
        CType(d, A).fun(2)
        CType(d, AB).fun(2)
        CType(d, AC).fun(2)       
	 End Sub
End Class

Module InheritanceO
        Sub Main()
                Dim a as Test=new Test()
        End Sub
End Module
