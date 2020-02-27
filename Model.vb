Public Class Model
    Public Name As String
    Public Type As Triangle_Type
    Public Vertices_Local As List(Of VEC3) 'used to store model coordinates
    Public Vertices_World As List(Of VEC3) 'used to store world coordinates
    Public Vertices_Camera As List(Of VEC3) 'camera coordinates
    Public Vertices_Screen As List(Of VEC3) 'screen space coordinates
    Public Vertex_Normals As List(Of VEC3)
    Public Model_Texture_Coordinates As List(Of PointF)
    Public Model_Triangles As List(Of Triangle)
    Public Model_Colours As List(Of Color) 'colours are stored here  to start with so that self-contained triangles can be constructed later
    Public Texture_Coordinates As List(Of PointF)
    Public Triangles As List(Of Triangle)
    Public Colours As List(Of Color)
    Public texture As Texture
    Public radius As Double

    Public Sub New(ByVal _Name As String,
                   ByVal _Type As Triangle_Type,
                   ByVal _Vertices As List(Of VEC3),
                   ByVal _Triangles As List(Of Triangle),
                   ByVal _Colours As List(Of Color),
                   ByVal _Texture As Texture,
                   ByVal _Texture_Coordinates As List(Of PointF))
        Name = _Name
        Vertices_Local = _Vertices
        Vertices_World = New List(Of VEC3)
        Vertices_Screen = New List(Of VEC3)
        Vertices_Camera = New List(Of VEC3)
        Vertex_Normals = New List(Of VEC3)
        Model_Triangles = _Triangles
        Type = _Type
        If Type = Triangle_Type.Coloured Then
            Model_Colours = _Colours
        ElseIf Type = Triangle_Type.Textured Then
            Model_Texture_Coordinates = _Texture_Coordinates
            texture = _Texture
        End If

        radius = Compute_Radius()
    End Sub

    Sub Prepare_Model()
        Triangles = New List(Of Triangle)
        For Each T As Triangle In Model_Triangles
            Triangles.Add(New Triangle(T.V1, T.V2, T.V3, T.Type))
        Next
        If Type = Triangle_Type.Textured Then
            Texture_Coordinates = New List(Of PointF)
            For Each T As PointF In Model_Texture_Coordinates
                Texture_Coordinates.Add(New PointF(T.X, T.Y))
            Next
        ElseIf Type = Triangle_Type.Coloured Then
            Colours = New List(Of Color)
            For Each C As Color In Model_Colours
                Colours.Add(Color.FromArgb(C.R, C.G, C.B))
            Next
        End If
    End Sub

    ''' <summary>
    ''' Returns the bounding sphere radius.
    ''' </summary>
    Function Compute_Radius() As Double
        Dim max_radius As Double = 0
        Dim temp_radius As Double
        For i As Integer = 1 To Vertices_Local.Count - 1
            temp_radius = Vertices_Local(i).LengthSquared
            'calculates the distance (squared) of the vertex from the origin.
            If temp_radius > max_radius Then max_radius = temp_radius
        Next
        Return Math.Sqrt(max_radius)
    End Function

    ''' <summary>
    ''' Returns the model contained in the file specified.
    ''' </summary>
    Shared Function FromFile(ByVal PathName As String, ByRef List_Textures As List(Of Texture)) As Model
        Dim SR As New IO.StreamReader(PathName)
        'opens up a stream that can be used to read in lines of the text file.
        Dim Name As String = SR.ReadLine
        Dim Type As Triangle_Type = CType([Enum].Parse(GetType(Triangle_Type), SR.ReadLine), Triangle_Type)
        '0 = coloured, 1 = textured

        Dim List_Vertices As New List(Of VEC3)
        Dim List_Triangles As New List(Of Triangle)
        Dim List_Colours As List(Of Color)
        Dim List_Texture_Coords As List(Of PointF)
        Dim _Texture As Texture

        If Type = Triangle_Type.Coloured Then
            'If this is a coloured triangle, initialise the list of colours
            List_Colours = New List(Of Color)
        ElseIf Type = Triangle_Type.Textured Then
            'If this is a textured triangle, initialise the list of textures and texture coordinates
            _Texture = Find_Texture(SR.ReadLine, List_Textures)
            List_Texture_Coords = New List(Of PointF)
        End If

        Dim number As Integer = CInt(SR.ReadLine)

        If Type = Triangle_Type.Coloured Then
            For i As Integer = 0 To number - 1
                'Enter all vertices along with their colours
                Dim V() As String = SR.ReadLine.Split(" ")
                List_Vertices.Add(New VEC3(V(0), V(1), V(2)))
            Next
        ElseIf Type = Triangle_Type.Textured Then
            For i As Integer = 0 To number - 1
                'Enter all vertices along with their colours
                Dim V() As String = SR.ReadLine.Split(" ")
                List_Vertices.Add(New VEC3(V(0), V(1), V(2)))
            Next
        End If

        number = CInt(SR.ReadLine)

        If Type = Triangle_Type.Coloured Then
            For i As Integer = 0 To number - 1
                'Enter all vertex indeces
                Dim V() As String = SR.ReadLine.Split(" ")

                List_Triangles.Add(New Triangle(V(0), V(1), V(2), Triangle_Type.Coloured))
                List_Colours.Add(Color.FromArgb(V(3), V(4), V(5)))
                List_Colours.Add(Color.FromArgb(V(6), V(7), V(8)))
                List_Colours.Add(Color.FromArgb(V(9), V(10), V(11)))
            Next
        ElseIf Type = Triangle_Type.Textured Then
            For i As Integer = 0 To number - 1
                'Enter all vertex indeces along
                Dim V() As String = SR.ReadLine.Split(" ")

                List_Triangles.Add(New Triangle(V(0), V(1), V(2), Triangle_Type.Textured))
                List_Texture_Coords.Add(New PointF(V(3), V(4)))
                List_Texture_Coords.Add(New PointF(V(5), V(6)))
                List_Texture_Coords.Add(New PointF(V(7), V(8)))
            Next
        End If

        SR.Close()
        Return New Model(Name,
                         Type,
                         List_Vertices,
                         List_Triangles,
                         List_Colours,
                         _Texture,
                         List_Texture_Coords)
    End Function

    ''' <summary>
    ''' The first member of the list of textures will always be a magenta 1x1 texture.
    ''' It is returned if the named texture is not found (placeholder texture).
    ''' </summary>
    Shared Function Find_Texture(ByVal Texture_Name As String,
                                 ByRef _Textures As List(Of Texture)) As Texture
        Dim index As Integer = 0
        For i As Integer = 0 To _Textures.Count - 1
            If _Textures(i).Name = Texture_Name Then
                index = i
            End If
        Next
        Return _Textures(index)
    End Function

    Enum Triangle_Type
        Coloured = 0
        Textured = 1
    End Enum

End Class