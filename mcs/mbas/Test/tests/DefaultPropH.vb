'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Class Class2
   ' Define a local variable to store the property value.
   Private PropertyValues(10) As Integer 
   ' Define the default property.
   Default Public Property Prop1(ByVal Index As Integer) As integer
      Get
         Return PropertyValues(Index)
      End Get
      Set(ByVal Value As integer)
         PropertyValues(Index) = Value
      End Set
   End Property
End Class


module M
    sub main
    Dim C As New Class2()
' The first two lines of code access a property the standard way.
    C.Prop1(0) = 10 ' Property assignment.
    System.Console.WriteLine(C.Prop1(0)) ' Property retrieval.

    ' The following two lines of code use default property syntax.
    C(1) = 20 ' Property assignment.
    System.Console.WriteLine(C(1))  ' Property retrieval.
    end sub
end module
