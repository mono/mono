REM LineNo: 22
REM ExpectedError: BC30039
REM ErrorMessage: Loop control variable cannot be a property or a late-bound indexed array.

Imports System

Module ForEachC2

    Private index As Integer = 0
    Public Property myindex() As Integer
        Get
            Return index
        End Get
        Set(ByVal Value As Integer)
            index = Value
        End Set
    End Property

    Sub main()
        Dim arr() As Integer = {1, 2, 3}

        For Each myindex In arr
            Console.WriteLine(myindex)
        Next

    End Sub

End Module