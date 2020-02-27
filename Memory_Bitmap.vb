Public Structure Memory_Bitmap
    Private ReadOnly W, H As Integer
    Private ReadOnly _hdr(), _pxb() As Byte
    Private ReadOnly _rowL, _pixL As Integer
    Private MS As IO.MemoryStream

    Public ReadOnly Property Bitmap() As Bitmap
        Get
            With MS
                .Position = 0L
                .Write(_hdr, 0, 54)
                .Position = 54L
                .Write(_pxb, 0, _pixL)
            End With

            Return New Bitmap(MS)
        End Get
    End Property

    Public ReadOnly Property RowByteSize() As Integer
        Get
            Return _rowL
        End Get
    End Property

    Public ReadOnly Property Width() As Integer
        Get
            Return W
        End Get
    End Property

    Public ReadOnly Property Height() As Integer
        Get
            Return H
        End Get
    End Property

    Sub New(ByVal _w As Integer, ByVal _h As Integer)
        W = _w
        H = _h

        Dim bh As New BitmapHeader24(_w, _h)
        With bh
            _rowL = .RowByteLength
            _pixL = .PixelBufferSize

            _hdr = .GetHeaderBytes()
            ReDim _pxb(_pixL - 1)
        End With

        MS = New IO.MemoryStream(_rowL + _pixL)
    End Sub

    Sub New(ByVal bh As BitmapHeader24)
        With bh
            W = .ImageWidth
            H = .ImageHeight

            _rowL = .RowByteLength
            _pixL = .PixelBufferSize

            _hdr = .GetHeaderBytes()
            ReDim _pxb(_pixL - 1)
        End With

        MS = New IO.MemoryStream(_rowL + _pixL)
    End Sub

    Structure BitmapHeader24

        Friend Const BmpSignature As Short = 19778S

        Public ImageWidth, ImageHeight As Integer

        Public ReadOnly Property RowByteLength() As Integer
            Get
                Return ImageWidth * 3 + ImageWidth Mod 4
            End Get
        End Property

        Public ReadOnly Property PixelBufferSize() As Integer
            Get
                Return ImageHeight * RowByteLength
            End Get
        End Property

        Public ReadOnly Property TotalBitmapSize() As Integer
            Get
                Return 54 + PixelBufferSize
            End Get
        End Property

        Sub New(ByVal w As Integer, ByVal h As Integer)
            ImageWidth = w
            ImageHeight = h
        End Sub

        Function GetHeaderBytes() As Byte()
            Dim hdr(53) As Byte

            Dim b() As Byte = BitConverter.GetBytes(TotalBitmapSize)

            Buffer.BlockCopy(BitConverter.GetBytes(BmpSignature), 0, hdr, 0, 2)
            Buffer.BlockCopy(BitConverter.GetBytes(TotalBitmapSize), 0, hdr, 2, 4)
            Buffer.BlockCopy(BitConverter.GetBytes(54), 0, hdr, 10, 4)
            Buffer.BlockCopy(BitConverter.GetBytes(40), 0, hdr, 14, 4)
            Buffer.BlockCopy(BitConverter.GetBytes(ImageWidth), 0, hdr, 18, 4)
            Buffer.BlockCopy(BitConverter.GetBytes(ImageHeight), 0, hdr, 22, 4)
            Buffer.BlockCopy(BitConverter.GetBytes(1S), 0, hdr, 26, 2)
            Buffer.BlockCopy(BitConverter.GetBytes(24S), 0, hdr, 28, 2)
            Buffer.BlockCopy(BitConverter.GetBytes(3780), 0, hdr, 38, 4)
            Buffer.BlockCopy(BitConverter.GetBytes(3780), 0, hdr, 42, 4)

            Return hdr
        End Function

        Shared Function FromHeaderBytes(ByVal hdr() As Byte) As BitmapHeader24
            If hdr.Length = 54 Then
                Return New BitmapHeader24(BitConverter.ToInt32(New Byte() {hdr(18), hdr(19), hdr(20), hdr(21)}, 0), BitConverter.ToInt32(New Byte() {hdr(22), hdr(23), hdr(24), hdr(25)}, 0))
            End If
        End Function

    End Structure

    Function GPIndex(ByVal x As Integer, ByVal y As Integer) As Integer
        Return _pixL - (y * _rowL) + 3 * (x - 1)
    End Function

    Sub SetPixel(ByVal x As Integer, ByVal y As Integer, ByVal c As Color)
        If (x <= W) And (y <= H) And (x > 0) And (y > 0) Then
            Call WritePixelBytes(GPIndex(x, y), c.R, c.G, c.B)
        End If
    End Sub

    Sub SetPixel(ByVal x As Integer, ByVal y As Integer, ByVal r As Byte, ByVal g As Byte, ByVal b As Byte)
        If (x <= W) And (y <= H) And (x > 0) And (y > 0) Then
            Call WritePixelBytes(GPIndex(x, y), r, g, b)
        End If
    End Sub

    Sub WritePixelBytes(ByVal p As Integer, ByVal r As Byte, ByVal g As Byte, ByVal b As Byte)
        _pxb(p) = b
        _pxb(p + 1) = g
        _pxb(p + 2) = r
    End Sub

    Function GetPixel(ByVal x As Integer, ByVal y As Integer) As Color
        If (x <= W) And (y <= H) And (x > 0) And (y > 0) Then
            Dim p As Integer = GPIndex(x, y)
            Return Color.FromArgb(_pxb(p + 2), _pxb(p + 1), _pxb(p))
        End If
        Return Color.FromArgb(0, 0, 0)
    End Function

    Function GetPixelByteArray(ByVal p As Integer) As Byte()
        Return New Byte() {_pxb(p + 2), _pxb(p + 1), _pxb(p)}
    End Function

    Sub CopyPixelTo(ByVal ps As Integer, ByVal hdl As IntPtr)
        Runtime.InteropServices.Marshal.Copy(_pxb, ps, hdl, 3)
    End Sub

    Sub Clear()
        Array.Clear(_pxb, 0, _pixL)
    End Sub

    Shared Function FromFileOld(ByVal p As String) As Memory_Bitmap
        If IO.File.Exists(p) Then
            Dim b As New Bitmap(p)
            Dim MB As New Memory_Bitmap(b.Width, b.Height)
            Dim ms As New IO.MemoryStream
            b.Save(ms, Imaging.ImageFormat.Bmp)
            ms.Position = 54L

            Dim read As Integer
            Dim buffer(2 ^ 24) As Byte
            Do While ((read = ms.Read(buffer, 0, buffer.Length)) > 0)

                MB.MS.Write(buffer, 0, read)
            Loop
            Return MB
        End If
        Return Nothing
    End Function

    Shared Function FromFile(ByVal p As String) As Memory_Bitmap
        If IO.File.Exists(p) Then
            Dim m As Memory_Bitmap
            Dim b As New Bitmap(p)
            Dim h(53), d() As Byte
            Dim s As New IO.MemoryStream

            b.Save(s, Imaging.ImageFormat.Bmp)

            d = s.ToArray
            Buffer.BlockCopy(d, 0, h, 0, 54)
            m = New Memory_Bitmap(BitmapHeader24.FromHeaderBytes(h))

            For x As Integer = 0 To m.W - 1
                For y As Integer = 0 To m.H - 1
                    Dim c As System.Drawing.Color = b.GetPixel(x, y)
                    m.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B))
                Next y
            Next x
            Return m
        End If
        Return Nothing
    End Function

End Structure