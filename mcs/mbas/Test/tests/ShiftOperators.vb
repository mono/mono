Imports System

Module M
    Sub main()
        Console.WriteLine(f())
    End Sub

    Function f()
        Dim arr(15) As Boolean

        ' Left shift operator tests 

        Dim a1 As Byte = 5

        a1 = a1 << 1001
        If a1 = 10 Then arr(0) = True

        a1 = Byte.MaxValue
        a1 = a1 << 2001
        If a1 = 254 Then arr(1) = True

        a1 = Byte.MinValue
        a1 = a1 << 2002
        If a1 = 0 Then arr(2) = True

        Dim b1 As Integer = 5
        Dim b2 As Integer = -5

        b1 = b1 << 1001
        If b1 = 2560 Then arr(3) = True

        b1 = -5
        b1 = b1 << 1001
        If b1 = -2560 Then arr(4) = True

        b1 = Integer.MaxValue
        b1 = b1 << 1001
        If b1 = -512 Then arr(5) = True

        b1 = Integer.MinValue
        b1 = b1 << 1001
        If b1 = 0 Then arr(6) = True

        b1 = 0
        b1 = b1 << 1001
        If b1 = 0 Then arr(7) = True

        ' Right shift operator tests

        Dim c1 As Byte = 5

        c1 = c1 >> 1001
        If c1 = 2 Then arr(8) = True

        c1 = Byte.MaxValue
        c1 = c1 >> 2001
        If c1 = 127 Then arr(9) = True

        c1 = Byte.MinValue
        c1 = c1 >> 2002
        If c1 = 0 Then arr(10) = True

        Dim d1 As Integer = 5

        d1 = d1 >> 1001
        If d1 = 0 Then arr(11) = True

        d1 = -5
        d1 = d1 >> 1001
        If d1 = -1 Then arr(12) = True

        d1 = Integer.MaxValue
        d1 = d1 >> 1001
        If d1 = 4194303 Then arr(13) = True

        d1 = Integer.MinValue
        d1 = d1 >> 1001
        If d1 = -4194304 Then arr(14) = True

        d1 = 0
        d1 = d1 >> 1001
        If d1 = 0 Then arr(15) = True

        'For i As Integer = 0 To arr.GetUpperBound(0) 
        '   Console.WriteLine("{0}: {1}", i, arr(i))
        'Next

        For Each bval As Boolean In arr
            If Not bval Then
                Return 1
            End If
        Next

        Return 0

    End Function
End Module