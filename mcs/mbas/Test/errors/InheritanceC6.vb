'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 18
REM ExpectedError: BC30610
REM ErrorMessage: Class 'C2' must either be declared 'MustInherit' or override the following inherited 'MustOverride' member(s): Public MustOverride Sub fun().

REM LineNo: 26
REM ExpectedError: BC30376
REM ErrorMessage: 'New' cannot be used on class 'C1' because it contains a 'MustOverride' member that has not been overridden.
      
MustInherit Class C1
        Public MustOverride Sub fun()
End Class

Class C2
        Inherits C1
        Public Sub G()
        End Sub
End Class

Module InheritanceM
        Sub Main()
                Dim t1 as C1=new C1()
                Dim t as C2=new C2()
        End Sub
End Module
