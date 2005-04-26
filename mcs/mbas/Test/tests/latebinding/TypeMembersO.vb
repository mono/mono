'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class base
        Public Default Property Item(ByVal i as Integer)As Integer
                Get
                        Return i
                End Get
                Set          
                End set
        End Property
End Class

Module DefaultA
        Sub Main()
                Dim a as Object=new base()
                Dim i as Integer
                i=a(10)
                if i<>10 Then
                        Throw New Exception("Binding Not Working")
                End If
        End Sub
End Module
