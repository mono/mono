  '
  ' Copyright (c) 2002-2003 Mainsoft Corporation.
  '
  ' Permission is hereby granted, free of charge, to any person obtaining a
  ' copy of this software and associated documentation files (the "Software"),
  ' to deal in the Software without restriction, including without limitation
  ' the rights to use, copy, modify, merge, publish, distribute, sublicense,
  ' and/or sell copies of the Software, and to permit persons to whom the
  ' Software is furnished to do so, subject to the following conditions:
  ' 
  ' The above copyright notice and this permission notice shall be included in
  ' all copies or substantial portions of the Software.
  ' 
  ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  ' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  ' DEALINGS IN THE SOFTWARE.
  '
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        'BeginCode
        Dim dates() As Date = {"12/30/1215", _
                                "9/11/2038", _
                                "10/9/1001", _
                                "9/24/1918", _
                                "2/11/1946", _
                                "5/1/1980", _
                                "2/28/2001", _
                                "3/3/2003", _
                                "9/10/1972", _
                                "1/12/1487", _
                                "7/7/100", _
                                "2/1/22", _
                                "6/6/666", _
                                "1/1/2000", _
                                "12/31/2000", _
                                "5/5/1000", _
                                "1/1/1970"}
        Dim s As String
        Dim d As Date
        For Each d In dates
            s &= FormatDateTime(d, DateFormat.LongDate)
        Next
        Return s
        'EndCode
    End Function
End Class
