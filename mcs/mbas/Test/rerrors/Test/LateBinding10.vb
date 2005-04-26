'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class TestTypeMembers12
        Public Default Property Item(ByVal i as Date)As date
                Get
                        Return i
                End Get
                Set          
                End set
        End Property
End Class


<TestFixture>_
Public Class TypeMembers12
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()	
                Dim a as Object=new TestTypeMembers12()
                Dim i as date
                i=a(10)    		
        End Sub
End Class 


