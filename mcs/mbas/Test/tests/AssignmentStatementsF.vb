'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Private b As Byte = 0
    Private i As Integer = 0
    Sub Main()
        b += 1 
        b += i 
        b += CByte(i)
    End Sub
End Module
