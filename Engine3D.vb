Option Strict On
Option Explicit On

Public Class Engine3D

#Region "Essential Declarations"

    Dim Zbuffer As Double()
    Dim MB As Memory_Bitmap
    Dim scwidth, scheight As Integer
    Dim ASP As Double
    Dim Fon As Double
    Dim CotFov2 As Double
    Dim invAR As Double
    Dim FoV As Double = 0.5
    Dim S1 As New Stopwatch

#End Region

#Region "Debug Information"

    Dim Faces_Culled As Integer
    Dim Objects_Culled As Integer
    Dim Frametime As Integer

#End Region

#Region "Settings"

    Dim near As Double = 1.0F
    Dim far As Double = 100.0F
    Dim Drawing_Mode As Fill_Mode = Fill_Mode.Gouraud
    Dim Face_Culling As Face_Cull_Mode = Face_Cull_Mode.Back
    Dim Object_Culling As Boolean = True
    Dim Fill_Type As Fill_Mode

#End Region

    Public Sub New(ByVal W As Integer,
                   ByVal H As Integer)
        MB = New Memory_Bitmap(W, H)

        near = 1
        far = 10000

        FoV = 0.5
        Fon = 1 / Math.Tan(FoV * 0.5)

        Resize_Display(W, H)
    End Sub

    Public Function Draw(ByVal world As World) As Integer

        S1.Reset()
        S1.Start()
        Dim VBuffer_Coloured As New List(Of Vertex_Coloured)
        Dim VBuffer_Textured As New List(Of Vertex_Textured)
        Dim LTx As New List(Of Texture)
        'Dim LTriangle_Coloured As New List(Of Triangle_Coloured)

        MB.Clear()
        Clear_Zbuffer()

        Dim Render_List As New List(Of Game_Object)
        'This is the list of all objects in the world that we have to render

        Prepare_Render_List(Render_List, world)
        'Fill the render list with the objects in the world

        Transform_Objects_Local_To_World(Render_List)
        'Moves all the models coordinates into the world

        Cull_Objects_Camera(Render_List, world.Camera)
        '

        Calc_Surface_Normals(Render_List)
        '

        If Face_Culling = Face_Cull_Mode.Back Then
            Remove_Back_Faces(Render_List, world.Camera.Camera_Position)
        ElseIf Face_Culling = Face_Cull_Mode.Front Then
            Remove_Front_Faces(Render_List, world.Camera.Camera_Position)
        End If

        world.Camera.Prepare_Matrix()
        'Set up the camera matrix

        Transform_Objects_World_To_Camera(Render_List, world.Camera)
        'Moves all the objects into camera space

        Transform_Objects_Camera_To_Screen(Render_List,
                                           VBuffer_Coloured,
                                           VBuffer_Textured,
                                           LTx,
                                           world.Camera.Camera_Position)

        Draw_Triangles_Coloured(VBuffer_Coloured)
        Draw_Triangles_Textured(VBuffer_Textured, LTx)
        S1.Stop()
        Frametime = CInt(S1.ElapsedMilliseconds)
        Return 0
    End Function

    Private Sub Draw_Triangles_Textured(ByRef VBTx As List(Of Vertex_Textured),
                                ByRef LTx As List(Of Texture))
        Dim i As Integer = VBTx.Count - 1
        Dim n_Tx As Integer = CInt((VBTx.Count - 2) / 3)
        Dim VT1, VT2, VT3, VT4, VTtemp As Vertex_Textured

        While i > 0
            VT1 = VBTx(i)
            VT2 = VBTx(i - 1)
            VT3 = VBTx(i - 2)

            If VT1.Y > VT2.Y Then Swap(VT1, VT2, VTtemp)
            If VT1.Y > VT3.Y Then Swap(VT1, VT3, VTtemp)
            If VT2.Y > VT3.Y Then Swap(VT2, VT3, VTtemp)

            If CInt(VT2.Y) = CInt(VT3.Y) Then

                'we have a flat bottom triangle
                If VT2.X > VT3.X Then Swap(VT2, VT3, VTtemp)
                Text_Bottom_Triangle(VT1, VT2, VT3, LTx(n_Tx))

            ElseIf CInt(VT1.Y) = CInt(VT2.Y) Then

                'we have a flat top triangle
                If VT1.X > VT2.X Then Swap(VT1, VT2, VTtemp)
                Text_Top_Triangle(VT1, VT2, VT3, LTx(n_Tx))
            Else

                VT4 = Vertex_Textured.LERP(VT1, VT3, (VT2.Y - VT1.Y) / (VT3.Y - VT1.Y))
                If VT2.X > VT3.X Then Swap(VT2, VT4, VTtemp)
                Text_Bottom_Triangle(VT1, VT2, VT4, LTx(n_Tx))

                If VT1.X > VT2.X Then Swap(VT4, VT2, VTtemp)
                Text_Top_Triangle(VT4, VT2, VT3, LTx(n_Tx))

            End If

            n_Tx -= 1
            i -= 3
        End While

    End Sub

    Private Sub Draw_Triangles_Coloured(ByRef VBCr As List(Of Vertex_Coloured))
        Dim i As Integer = VBCr.Count - 1
        Dim VC1, VC2, VC3, VC4, VCtemp As Vertex_Coloured

        While i > 0
            VC1 = VBCr(i)
            VC2 = VBCr(i - 1)
            VC3 = VBCr(i - 2)
            If Fill_Type = Fill_Mode.Wireframe Then
                Draw_Line(VC1, VC2)
                Draw_Line(VC2, VC3)
                Draw_Line(VC3, VC1)
            Else
                If VC1.Y > VC2.Y Then Swap(VC1, VC2, VCtemp)
                If VC1.Y > VC3.Y Then Swap(VC1, VC3, VCtemp)
                If VC2.Y > VC3.Y Then Swap(VC2, VC3, VCtemp)

                If CInt(VC2.Y) = CInt(VC3.Y) Then

                    'we have a flat bottom triangle
                    If VC2.X > VC3.X Then Swap(VC2, VC3, VCtemp)
                    Select Case Fill_Type
                        Case Fill_Mode.Solidfill : Fill_Bottom_Triangle(VC1.Position, VC2.Position, VC3.Position, VC1.Colour)
                        Case Fill_Mode.Flatshaded : Fill_Bottom_Triangle(VC1.Position, VC2.Position, VC3.Position, VC1.Colour)
                        Case Fill_Mode.Gouraud : Fill_Bottom_Triangle(VC1, VC2, VC3)
                    End Select

                ElseIf CInt(VC1.Y) = CInt(VC2.Y) Then

                    'we have a flat top triangle
                    If VC1.X > VC2.X Then Swap(VC1, VC2, VCtemp)
                    Select Case Fill_Type
                        Case Fill_Mode.Solidfill : Fill_Top_Triangle(VC1.Position, VC2.Position, VC3.Position, VC1.Colour)
                        Case Fill_Mode.Flatshaded : Fill_Top_Triangle(VC1.Position, VC2.Position, VC3.Position, VC1.Colour)
                        Case Fill_Mode.Gouraud : Fill_Top_Triangle(VC1, VC2, VC3)
                    End Select
                Else

                    VC4 = Vertex_Coloured.LERP(VC1, VC3, (VC2.Y - VC1.Y) / (VC3.Y - VC1.Y))

                    If VC2.X > VC3.X Then Swap(VC2, VC4, VCtemp)
                    Select Case Fill_Type
                        Case Fill_Mode.Solidfill : Fill_Bottom_Triangle(VC1.Position, VC2.Position, VC4.Position, VC1.Colour)
                        Case Fill_Mode.Flatshaded : Fill_Bottom_Triangle(VC1.Position, VC2.Position, VC4.Position, VC1.Colour)
                        Case Fill_Mode.Gouraud : Fill_Bottom_Triangle(VC1, VC2, VC4)
                    End Select

                    If VC1.X > VC2.X Then Swap(VC4, VC2, VCtemp)
                    Select Case Fill_Type
                        Case Fill_Mode.Solidfill : Fill_Top_Triangle(VC4.Position, VC2.Position, VC3.Position, VC3.Colour)
                        Case Fill_Mode.Flatshaded : Fill_Top_Triangle(VC4.Position, VC2.Position, VC3.Position, VC3.Colour)
                        Case Fill_Mode.Gouraud : Fill_Top_Triangle(VC4, VC2, VC3)
                    End Select

                End If
            End If
            i -= 3
        End While

    End Sub

    Private Sub Prepare_Render_List(ByRef Render_List As List(Of Game_Object),
                            ByRef world As World)
        For Each obj As Game_Object In world.Objects
            Render_List.Add(obj)
        Next
    End Sub

    Private Sub Transform_Objects_Local_To_World(ByRef Render_List As List(Of Game_Object))
        Dim Mtranslate As MAT44
        For Each obj As Game_Object In Render_List
            obj.Object_Model.Prepare_Model()
            obj.Object_Model.Vertices_World.Clear()
            Mtranslate = New MAT44(1, 0, 0, obj.Position.X,
                                   0, 1, 0, obj.Position.Y,
                                   0, 0, 1, obj.Position.Z,
                                   0, 0, 0, 1)
            obj.matrix = Mtranslate * (MAT44.RotationMatrixYAxis(obj.Rotation.Y, New VEC3) * (MAT44.RotationMatrixXAxis(obj.Rotation.X, New VEC3)))

            For i As Integer = 0 To obj.Object_Model.Vertices_Local.Count - 1
                obj.Object_Model.Vertices_World.Add(obj.matrix.MPoint(obj.Object_Model.Vertices_Local(i)))
                'We multiply each local vertex in the object by the object matrix, to setup the object in the world
            Next
        Next
    End Sub

    Private Sub Transform_Objects_World_To_Camera(ByRef Render_List As List(Of Game_Object),
                                          ByRef Camera As Game_Camera)
        For Each obj As Game_Object In Render_List
            obj.Object_Model.Vertices_Camera.Clear()
            For i As Integer = 0 To obj.Object_Model.Vertices_World.Count - 1
                obj.Object_Model.Vertices_Camera.Add(Camera.Matrix.MPoint(obj.Object_Model.Vertices_World(i)))
            Next
        Next
    End Sub

    Private Sub Transform_Objects_Camera_To_Screen(ByRef Render_List As List(Of Game_Object),
                                           ByRef VB_Cr As List(Of Vertex_Coloured),
                                           ByRef VB_Tx As List(Of Vertex_Textured),
                                           ByRef LT_Tx As List(Of Texture),
                                           ByRef Light_Pos As VEC3)
        Dim m As Model
        Dim VC1, VC2, VC3, VCt1, VCt2 As Vertex_Coloured
        Dim VT1, VT2, VT3, VTt1, Vtt2 As Vertex_Textured
        Dim zlerp1, zlerp2 As Double
        Dim T As Triangle
        For Each loop_object As Game_Object In Render_List
            m = loop_object.Object_Model
            m.Vertices_Screen.Clear()

            For i As Integer = 0 To m.Triangles.Count - 1
                T = m.Triangles(i)
                If T.Type = Model.Triangle_Type.Coloured Then
                    VC1 = New Vertex_Coloured(m.Vertices_Camera(T.V1), m.Colours((i * 3)), m.Vertex_Normals(i))
                    VC2 = New Vertex_Coloured(m.Vertices_Camera(T.V2), m.Colours((i * 3) + 1), m.Vertex_Normals(i))
                    VC3 = New Vertex_Coloured(m.Vertices_Camera(T.V3), m.Colours((i * 3) + 2), m.Vertex_Normals(i))

                    If Not (Fill_Type = Fill_Mode.Solidfill Or Fill_Type = Fill_Mode.Wireframe) Then
                        VC1 = Apply_Lighting(VC1, Light_Pos)
                        VC2 = Apply_Lighting(VC2, Light_Pos)
                        VC3 = Apply_Lighting(VC3, Light_Pos)
                    End If

                    'sort the vertices so that VC1 has the smallest Z
                    If VC1.Position.Z > VC2.Position.Z Then
                        Swap(VC1, VC2, VCt1)
                    End If
                    If VC2.Position.Z > VC3.Position.Z Then
                        Swap(VC2, VC3, VCt1)
                    End If
                    If VC1.Position.Z > VC2.Position.Z Then
                        Swap(VC1, VC2, VCt1)
                    End If

                    If VC3.Position.Z < near Then
                        'all the points are behind the near clipping plane
                    ElseIf VC2.Position.Z < near Then

                        zlerp1 = (VC3.Z - near) / (VC3.Z - VC1.Z)
                        zlerp2 = (VC3.Z - near) / (VC3.Z - VC2.Z)
                        VC1 = Vertex_Coloured.LERP(VC3, VC1, zlerp1)
                        VC2 = Vertex_Coloured.LERP(VC3, VC2, zlerp2)

                        VB_Cr.Add(VC1)
                        VB_Cr.Add(VC2)
                        VB_Cr.Add(VC3)

                    ElseIf VC1.Position.Z < near Then

                        zlerp1 = (VC3.Z - near) / (VC3.Z - VC1.Z)
                        zlerp2 = (VC2.Z - near) / (VC2.Z - VC1.Z)
                        VCt1 = Vertex_Coloured.LERP(VC3, VC1, zlerp1)
                        VCt2 = Vertex_Coloured.LERP(VC2, VC1, zlerp2)

                        VB_Cr.Add(VC3)
                        VB_Cr.Add(VC2)
                        VB_Cr.Add(VCt1)

                        VB_Cr.Add(VC2)
                        VB_Cr.Add(VCt2)
                        VB_Cr.Add(VCt1)
                    Else

                        VB_Cr.Add(VC1)
                        VB_Cr.Add(VC2)
                        VB_Cr.Add(VC3)

                    End If

                ElseIf T.Type = Model.Triangle_Type.Textured Then

                    VT1 = New Vertex_Textured(m.Vertices_Camera(T.V1), m.Texture_Coordinates((i * 3)), m.Vertex_Normals(i))
                    VT2 = New Vertex_Textured(m.Vertices_Camera(T.V2), m.Texture_Coordinates((i * 3) + 1), m.Vertex_Normals(i))
                    VT3 = New Vertex_Textured(m.Vertices_Camera(T.V3), m.Texture_Coordinates((i * 3) + 2), m.Vertex_Normals(i))

                    VT1 = Apply_Lighting(VT1, Light_Pos)
                    VT2 = Apply_Lighting(VT2, Light_Pos)
                    VT3 = Apply_Lighting(VT3, Light_Pos)

                    'If VT1.Position.Z < 1 Then Continue For
                    'If VT2.Position.Z < 1 Then Continue For
                    'If VT3.Position.Z < 1 Then Continue For

                    'sort the vertices so that VC1 has the smallest Z
                    If VT1.Position.Z > VT2.Position.Z Then
                        Swap(VT1, VT2, VTt1)
                    End If
                    If VT2.Position.Z > VT3.Position.Z Then
                        Swap(VT2, VT3, VTt1)
                    End If
                    If VT1.Position.Z > VT2.Position.Z Then
                        Swap(VT1, VT2, VTt1)
                    End If

                    If VT3.Position.Z < near Then
                        'all the points are behind the near clipping plane
                    ElseIf VT2.Position.Z < near Then

                        zlerp1 = (VT3.Z - near) / (VT3.Z - VT1.Z)
                        zlerp2 = (VT3.Z - near) / (VT3.Z - VT2.Z)
                        VT1 = Vertex_Textured.LERP(VT3, VT1, zlerp1)
                        VT2 = Vertex_Textured.LERP(VT3, VT2, zlerp2)

                        VB_Tx.Add(VT1)
                        VB_Tx.Add(VT2)
                        VB_Tx.Add(VT3)
                        LT_Tx.Add(m.texture)

                    ElseIf VT1.Position.Z < near Then

                        zlerp1 = (VT3.Z - near) / (VT3.Z - VT1.Z)
                        zlerp2 = (VT2.Z - near) / (VT2.Z - VT1.Z)
                        VTt1 = Vertex_Textured.LERP(VT3, VT1, zlerp1)
                        Vtt2 = Vertex_Textured.LERP(VT2, VT1, zlerp2)

                        VB_Tx.Add(VT3)
                        VB_Tx.Add(VT2)
                        VB_Tx.Add(VTt1)
                        LT_Tx.Add(m.texture)

                        VB_Tx.Add(VT2)
                        VB_Tx.Add(Vtt2)
                        VB_Tx.Add(VTt1)
                        LT_Tx.Add(m.texture)
                    Else

                        VB_Tx.Add(VT1)
                        VB_Tx.Add(VT2)
                        VB_Tx.Add(VT3)
                        LT_Tx.Add(m.texture)

                    End If

                End If
            Next
        Next

        For i As Integer = 0 To VB_Cr.Count - 1
            VB_Cr(i) = Project_Point_Perspective(VB_Cr(i))
        Next
        For i As Integer = 0 To VB_Tx.Count - 1
            VB_Tx(i) = Project_Point_Perspective(VB_Tx(i))
        Next
    End Sub

    Private Function Project_Point_Perspective(ByVal P As VEC3) As VEC3
        Dim sx, sy As Double
        Dim iz As Double = 1 / P.Z

        sx = (scwidth * 0.5) * (1 + Fon * (iz * P.X))
        sy = (scheight * 0.5) * (1 - ASP * Fon * (iz * P.Y))

        'Dim d As Double = 0.5 * scwidth * Fon
        'sx = d * P.X * iz
        'sy = d * P.Y * iz * ASP

        Return New VEC3(sx, sy, P.Z)
    End Function

    Private Function Project_Point_Perspective(ByVal P As Vertex_Coloured) As Vertex_Coloured
        Dim sx, sy As Double
        Dim iz As Double = 1 / P.Z

        sx = (scwidth * 0.5) * (1 + Fon * (iz * P.X))
        sy = (scheight * 0.5) * (1 - ASP * Fon * (iz * P.Y))

        Return New Vertex_Coloured(New VEC3(sx, sy, P.Z), P.Colour, P.Normal)
    End Function

    Private Function Project_Point_Perspective(ByVal P As Vertex_Textured) As Vertex_Textured
        Dim sx, sy As Double
        Dim iz As Double = 1 / P.Z

        sx = (scwidth * 0.5) * (1 + Fon * (iz * P.X))
        sy = (scheight * 0.5) * (1 - ASP * Fon * (iz * P.Y))

        Return New Vertex_Textured(New VEC3(sx, sy, P.Z), P.TextureC, P.Normal, P.Light_Level)
    End Function

    Private Function Apply_Lighting(ByVal VT As Vertex_Textured,
                            ByRef CPos As VEC3) As Vertex_Textured 'and list of lights
        Dim dot As Double
        Dim lookat As VEC3
        lookat = VEC3.UnitVector(VT.Position - CPos)
        dot = VEC3.DotProduct(-1 * lookat, VT.Normal)
        dot = Math.Abs(dot)
        Return New Vertex_Textured(VT.Position, VT.TextureC, VT.Normal, dot)
    End Function

    Private Function Apply_Lighting(ByRef VC As Vertex_Coloured,
                            ByRef CPos As VEC3) As Vertex_Coloured 'and list of lights
        Dim dot As Double
        Dim lookat As VEC3
        lookat = VEC3.UnitVector(VC.Position - CPos)
        dot = VEC3.DotProduct(lookat, VC.Normal)
        dot = Math.Abs(dot)
        Return New Vertex_Coloured(VC.Position, Color.FromArgb(CInt(VC.R * dot), CInt(VC.G * dot), CInt(VC.B * dot)), VC.Normal)
    End Function

    Private Sub Calc_Surface_Normals(ByVal Render_List As List(Of Game_Object))
        Dim V1 As VEC3
        Dim V2 As VEC3
        For Each temp_object As Game_Object In Render_List
            With temp_object.Object_Model
                .Vertex_Normals = New List(Of VEC3)
                For Each T As Triangle In temp_object.Object_Model.Triangles
                    V1 = (.Vertices_World(T.V3) - .Vertices_World(T.V2))
                    V2 = (.Vertices_World(T.V2) - .Vertices_World(T.V1))
                    .Vertex_Normals.Add(VEC3.UnitVector(VEC3.CrossProduct(V2, V1)))
                Next
            End With
        Next
    End Sub

    Private Sub Remove_Back_Faces(ByRef Render_List As List(Of Game_Object),
                                  ByRef Camera_Pos As VEC3)
        Dim Lookat, Tri_Position As VEC3
        Dim m As Model
        Dim r, i, t As Integer
        For Each o As Game_Object In Render_List
            m = o.Object_Model
            t = m.Triangles.Count
            While i < t
                Tri_Position = (m.Vertices_World(m.Triangles(i - r).V1) +
                                m.Vertices_World(m.Triangles(i - r).V2) +
                                m.Vertices_World(m.Triangles(i - r).V3)) / 3
                Lookat = VEC3.UnitVector(Tri_Position - Camera_Pos)
                If VEC3.DotProduct(Lookat, m.Vertex_Normals(i - r)) >= 0 Then
                    If m.Triangles(i - r).Type = Model.Triangle_Type.Coloured Then
                        m.Colours.RemoveAt((i - r) * 3 + 2)
                        m.Colours.RemoveAt((i - r) * 3 + 1)
                        m.Colours.RemoveAt((i - r) * 3)
                    Else
                        m.Texture_Coordinates.RemoveAt((i - r) * 3 + 2)
                        m.Texture_Coordinates.RemoveAt((i - r) * 3 + 1)
                        m.Texture_Coordinates.RemoveAt((i - r) * 3)
                    End If
                    m.Vertex_Normals.RemoveAt(i - r)
                    m.Triangles.RemoveAt(i - r)
                    r += 1
                End If
                i += 1
            End While
        Next
        Faces_Culled = r
    End Sub

    Private Sub Remove_Front_Faces(ByRef Render_List As List(Of Game_Object),
                                   ByRef Camera_Pos As VEC3)
        Dim Lookat As VEC3
        Dim Tri_Position As VEC3
        Dim m As Model
        Dim r As Integer
        Dim i As Integer
        For Each o As Game_Object In Render_List
            m = o.Object_Model
            While i < m.Triangles.Count
                Tri_Position = (m.Vertices_World(m.Triangles(i - r).V1) +
                                m.Vertices_World(m.Triangles(i - r).V2) +
                                m.Vertices_World(m.Triangles(i - r).V3)) / 3
                Lookat = VEC3.UnitVector(Tri_Position - Camera_Pos)
                If VEC3.DotProduct(Lookat, m.Vertex_Normals(i - r)) >= 0 Then
                    If m.Triangles(i).Type = Model.Triangle_Type.Coloured Then
                        m.Colours.RemoveAt((i - r) * 3 + 2)
                        m.Colours.RemoveAt((i - r) * 3 + 1)
                        m.Colours.RemoveAt((i - r) * 3)
                    Else
                        m.Texture_Coordinates.RemoveAt((i - r) * 3 + 2)
                        m.Texture_Coordinates.RemoveAt((i - r) * 3 + 1)
                        m.Texture_Coordinates.RemoveAt((i - r) * 3)
                    End If
                    m.Vertex_Normals.RemoveAt(i - r)
                    m.Triangles.RemoveAt(i - r)
                    r += 1
                End If
                i += 1
            End While
        Next
        Faces_Culled = r
    End Sub

    Private Sub Cull_Objects_Camera(ByRef Source_Objects As List(Of Game_Object),
                            ByRef camera As Game_Camera)
        Dim result_list As New List(Of Game_Object)
        For Each temp_object As Game_Object In Source_Objects
            Dim r As Double = temp_object.Object_Model.radius
            Dim VP As VEC3 = camera.Matrix.MPoint(temp_object.Position)
            If VP.Z + r < near Then
                Continue For
            End If
            If VP.Z - r > far Then
                Continue For
            End If
            If VP.X + r < -VP.Z Then
                Continue For
            End If
            If VP.X - r > VP.Z Then
                Continue For
            End If
            If VP.Y + r < -VP.Z Then
                Continue For
            End If
            If VP.Y - r > VP.Z Then
                Continue For
            End If
            result_list.Add(temp_object)
        Next
        Objects_Culled = (Source_Objects.Count - result_list.Count)
        Source_Objects = result_list
    End Sub

    Private Sub Swap(ByRef V1 As Vertex_Textured,
             ByRef V2 As Vertex_Textured,
             ByVal temp As Vertex_Textured)
        temp = V1
        V1 = V2
        V2 = temp
    End Sub

    Private Sub Swap(ByRef V1 As Vertex_Coloured,
             ByRef V2 As Vertex_Coloured,
             ByVal temp As Vertex_Coloured)
        temp = V1
        V1 = V2
        V2 = temp
    End Sub

    Private Sub Clear_Zbuffer()
        For y As Integer = 0 To scheight - 1
            For x As Integer = 0 To scwidth - 1
                Zbuffer(x + (scwidth * y)) = 0
            Next
        Next
    End Sub

#Region "Wireframe"

    Private Sub Draw_Line(ByVal v1 As Vertex_Coloured,
                          ByVal v2 As Vertex_Coloured)
        Dim t As Double
        Dim x1 As Double = v1.X
        Dim x2 As Double = v2.X
        Dim y1 As Double = v1.Y
        Dim y2 As Double = v2.Y

        If ((y2 - y1) / (x2 - x1)) ^ 2 > 1 Then
            If y1 > y2 Then
                t = x1
                x1 = x2
                x2 = t
                t = y1
                y1 = y2
                y2 = t
            End If
            Dim xd As Double = x2 - x1
            Dim yd As Double = y2 - y1
            Dim grad As Double
            Dim Y As Double = y1
            If yd = 0 Then
                grad = 10000
            Else
                grad = xd / yd
            End If
            While Y <= y2
                MB.SetPixel(CInt(x1 + grad * (Y - y1)), CInt(Y), v1.Colour)
                Y += 1
            End While
        Else
            If x1 > x2 Then
                t = x1
                x1 = x2
                x2 = t
                t = y1
                y1 = y2
                y2 = t
            End If
            Dim xd As Double = x2 - x1
            Dim yd As Double = y2 - y1
            Dim grad As Double
            Dim X As Double = x1
            If xd = 0 Then
                grad = 10000
            Else
                grad = yd / xd
            End If
            While X <= x2
                MB.SetPixel(CInt(X), CInt(y1 + grad * (X - x1)), v1.Colour)
                X += 1
            End While
        End If
    End Sub

#End Region

#Region "Solid/Flat Zbuffer"

    Private Sub Fill_Bottom_Triangle(ByVal v1 As VEC3,
                                     ByVal v2 As VEC3,
                                     ByVal v3 As VEC3,
                                     ByVal col As Color)

        Dim y1, y2 As Integer
        y1 = CInt(v1.Y)
        y2 = CInt(v2.Y)

        Dim inv As Double = 1.0 / (y2 - y1)

        Dim f_x1 As Double = v1.X
        Dim f_x2 As Double = v1.X
        Dim i_x1, i_x2, t As Integer

        Dim z1 As Double = v1.Z
        Dim z2 As Double = v1.Z

        Dim z, d, zslope As Double

        Dim slopex1 As Double = CInt(v2.X - v1.X) * inv
        Dim slopex2 As Double = CInt(v3.X - v1.X) * inv

        Dim slopez1 As Double = (v2.Z - v1.Z) * inv
        Dim slopez2 As Double = (v3.Z - v1.Z) * inv

        For scanlineY As Integer = y1 To y2
            If (scanlineY < scheight AndAlso scanlineY >= 0) Then
                i_x1 = CInt(f_x1)
                i_x2 = CInt(f_x2)

                If (i_x2 < i_x1) Then
                    t = i_x2
                    i_x2 = i_x1
                    i_x1 = t
                End If

                If (i_x2 > i_x1) Then
                    zslope = (z2 - z1) / (i_x2 - i_x1)
                Else
                    zslope = 0.0
                End If
                z = z1

                For x As Integer = i_x1 To i_x2
                    If x < scwidth AndAlso x >= 0 Then
                        d = Zbuffer(x + (scwidth * scanlineY))
                        If (d = 0.0) Xor (d > z) Then
                            MB.SetPixel(x, scanlineY, col)
                            Zbuffer(x + (scwidth * scanlineY)) = z
                        End If
                    End If
                    z += zslope
                Next
            End If

            z1 += slopez1
            z2 += slopez2
            f_x1 += slopex1
            f_x2 += slopex2
        Next
    End Sub

    Private Sub Fill_Top_Triangle(ByVal v1 As VEC3,
                                 ByVal v2 As VEC3,
                                 ByVal v3 As VEC3,
                                 ByVal col As Color)
        Dim y1, y2 As Integer
        y1 = CInt(v1.Y)
        y2 = CInt(v3.Y)

        Dim inv As Double = 1 / (y2 - y1)
        Dim f_x1 As Double = v3.X
        Dim f_x2 As Double = v3.X
        Dim i_x1, i_x2, t As Integer

        Dim z1 As Double = v3.Z
        Dim z2 As Double = v3.Z

        Dim z, d, zslope As Double

        Dim slopex1 As Double = CInt(v3.X - v1.X) * inv
        Dim slopex2 As Double = CInt(v3.X - v2.X) * inv
        Dim slopez1 As Double = (v3.Z - v1.Z) * inv
        Dim slopez2 As Double = (v3.Z - v2.Z) * inv

        For scanlineY As Integer = y2 To y1 Step -1
            If (scanlineY < scheight AndAlso scanlineY >= 0) Then
                i_x1 = CInt(f_x1)
                i_x2 = CInt(f_x2)

                If (i_x2 < i_x1) Then
                    t = i_x2
                    i_x2 = i_x1
                    i_x1 = t
                End If

                If (i_x2 > i_x1) Then
                    zslope = (z2 - z1) / (i_x2 - i_x1)
                Else
                    zslope = 0.0
                End If
                z = z1

                For x As Integer = i_x1 To i_x2
                    If x < scwidth AndAlso x >= 0 Then
                        d = Zbuffer(x + (scwidth * scanlineY))
                        If (d = 0.0) Xor (d > z) Then
                            MB.SetPixel(x, scanlineY, col)
                            Zbuffer(x + (scwidth * scanlineY)) = z
                        End If
                    End If
                    z += zslope
                Next
            End If
            z1 -= slopez1
            z2 -= slopez2
            f_x1 -= slopex1
            f_x2 -= slopex2
        Next
    End Sub

#End Region

#Region "Gouraud"

    Private Sub Fill_Bottom_Triangle(ByVal v1 As Vertex_Coloured,
                                     ByVal v2 As Vertex_Coloured,
                                     ByVal v3 As Vertex_Coloured)

        Dim y1, y2 As Integer
        y1 = CInt(v1.Y)
        y2 = CInt(v2.Y)

        Dim inv As Double = 1.0 / (y2 - y1)

        Dim f_x1 As Double = v1.X
        Dim f_x2 As Double = v1.X
        Dim i_x1, i_x2, t As Integer
        Dim i_r1, i_r2 As Integer
        Dim i_g1, i_g2 As Integer
        Dim i_b1, i_b2 As Integer
        Dim zi1, zi2 As Double

        Dim z1 As Double = v1.Z
        Dim z2 As Double = v1.Z
        Dim r1 As Double = v1.R
        Dim r2 As Double = v1.R
        Dim g1 As Double = v1.G
        Dim g2 As Double = v1.G
        Dim b1 As Double = v1.B
        Dim b2 As Double = v1.B

        Dim z, r, g, b, d, i_xd As Double
        Dim zslope, rslope, gslope, bslope As Double

        Dim slopex1 As Double = CInt(v2.X - v1.X) * inv
        Dim slopex2 As Double = CInt(v3.X - v1.X) * inv
        Dim slopez1 As Double = (v2.Z - v1.Z) * inv
        Dim slopez2 As Double = (v3.Z - v1.Z) * inv
        Dim sloper1 As Double = (v2.R - v1.R) * inv
        Dim sloper2 As Double = (v3.R - v1.R) * inv
        Dim slopeg1 As Double = (v2.G - v1.G) * inv
        Dim slopeg2 As Double = (v3.G - v1.G) * inv
        Dim slopeb1 As Double = (v2.B - v1.B) * inv
        Dim slopeb2 As Double = (v3.B - v1.B) * inv

        For scanlineY As Integer = y1 To y2
            If (scanlineY < scheight AndAlso scanlineY >= 0) Then
                i_x1 = CInt(f_x1)
                i_x2 = CInt(f_x2)
                i_r1 = CInt(r1)
                i_r2 = CInt(r2)
                i_g1 = CInt(g1)
                i_g2 = CInt(g2)
                i_b1 = CInt(b1)
                i_b2 = CInt(b2)
                zi1 = z1
                zi2 = z2

                If (i_x2 < i_x1) Then
                    t = i_x2
                    i_x2 = i_x1
                    i_x1 = t

                    t = i_r1
                    i_r1 = i_r2
                    i_r2 = t

                    t = i_g1
                    i_g1 = i_g2
                    i_g2 = t

                    t = i_b1
                    i_b1 = i_b2
                    i_b2 = t

                    d = zi1
                    zi1 = zi2
                    zi2 = d
                End If

                If (i_x2 > i_x1) Then
                    i_xd = 1 / (i_x2 - i_x1)
                    zslope = (zi2 - zi1) * i_xd
                    rslope = (i_r2 - i_r1) * i_xd
                    gslope = (i_g2 - i_g1) * i_xd
                    bslope = (i_b2 - i_b1) * i_xd
                End If

                z = zi1
                r = i_r1
                g = i_g1
                b = i_b1

                For x As Integer = i_x1 To i_x2
                    If x < scwidth AndAlso x >= 0 Then
                        d = Zbuffer(x + (scwidth * scanlineY))
                        If (d = 0.0) Xor (d > z) Then
                            MB.SetPixel(x, scanlineY, Color.FromArgb(CInt(r), CInt(g), CInt(b)))
                            Zbuffer(x + (scwidth * scanlineY)) = z
                        End If
                    End If
                    z += zslope
                    r += rslope
                    g += gslope
                    b += bslope
                Next
            End If

            z1 += slopez1
            z2 += slopez2
            r1 += sloper1
            r2 += sloper2
            g1 += slopeg1
            g2 += slopeg2
            b1 += slopeb1
            b2 += slopeb2

            f_x1 += slopex1
            f_x2 += slopex2
        Next
    End Sub

    Private Sub Fill_Top_Triangle(ByVal v1 As Vertex_Coloured,
                                  ByVal v2 As Vertex_Coloured,
                                  ByVal v3 As Vertex_Coloured)
        Dim y1, y2 As Integer
        y1 = CInt(v1.Y)
        y2 = CInt(v3.Y)

        Dim inv As Double = 1 / (y2 - y1)
        Dim f_x1 As Double = v3.X
        Dim f_x2 As Double = v3.X
        Dim i_x1, i_x2, t As Integer
        Dim ri1, ri2 As Double
        Dim gi1, gi2 As Double
        Dim bi1, bi2 As Double
        Dim zi1, zi2 As Double

        Dim z1 As Double = v3.Z
        Dim z2 As Double = v3.Z
        Dim r1 As Double = v3.R
        Dim r2 As Double = v3.R
        Dim g1 As Double = v3.G
        Dim g2 As Double = v3.G
        Dim b1 As Double = v3.B
        Dim b2 As Double = v3.B

        Dim z, d, zslope, rslope, gslope, bslope As Double
        Dim r, g, b, i_xd As Double

        Dim slopex1 As Double = CInt(v3.X - v1.X) * inv
        Dim slopex2 As Double = CInt(v3.X - v2.X) * inv
        Dim slopez1 As Double = (v3.Z - v1.Z) * inv
        Dim slopez2 As Double = (v3.Z - v2.Z) * inv
        Dim sloper1 As Double = (v3.R - v1.R) * inv
        Dim sloper2 As Double = (v3.R - v2.R) * inv
        Dim slopeg1 As Double = (v3.G - v1.G) * inv
        Dim slopeg2 As Double = (v3.G - v2.G) * inv
        Dim slopeb1 As Double = (v3.B - v1.B) * inv
        Dim slopeb2 As Double = (v3.B - v2.B) * inv

        For scanlineY As Integer = y2 To y1 Step -1
            If (scanlineY < scheight AndAlso scanlineY >= 0) Then
                i_x1 = CInt(f_x1)
                i_x2 = CInt(f_x2)
                ri1 = CInt(r1)
                ri2 = CInt(r2)
                gi1 = CInt(g1)
                gi2 = CInt(g2)
                bi1 = CInt(b1)
                bi2 = CInt(b2)
                zi1 = z1
                zi2 = z2

                If (i_x2 < i_x1) Then
                    t = i_x2
                    i_x2 = i_x1
                    i_x1 = t

                    d = ri1
                    ri1 = ri2
                    ri2 = d

                    d = gi1
                    gi1 = gi2
                    gi2 = d

                    d = bi1
                    bi1 = bi2
                    bi2 = d

                    d = zi1
                    zi1 = zi2
                    zi2 = d
                End If

                If (i_x2 > i_x1) Then
                    i_xd = 1 / (i_x2 - i_x1)
                    zslope = (zi2 - zi1) * i_xd
                    rslope = (ri2 - ri1) * i_xd
                    gslope = (gi2 - gi1) * i_xd
                    bslope = (bi2 - bi1) * i_xd
                End If

                z = zi1
                r = ri1
                g = gi1
                b = bi1

                For x As Integer = i_x1 To i_x2
                    If x < scwidth AndAlso x >= 0 Then
                        d = Zbuffer(x + (scwidth * scanlineY))
                        If (d = 0.0) Xor (d > z) Then
                            MB.SetPixel(x, scanlineY, Color.FromArgb(CInt(r), CInt(g), CInt(b)))
                            Zbuffer(x + (scwidth * scanlineY)) = z
                        End If
                    End If
                    z += zslope
                    r += rslope
                    g += gslope
                    b += bslope
                Next
            End If
            z1 -= slopez1
            z2 -= slopez2
            r1 -= sloper1
            r2 -= sloper2
            g1 -= slopeg1
            g2 -= slopeg2
            b1 -= slopeb1
            b2 -= slopeb2
            f_x1 -= slopex1
            f_x2 -= slopex2
        Next
    End Sub

#End Region

#Region "Texture Mapping"

    Private Sub Text_Bottom_Triangle(ByVal p1 As Vertex_Textured,
                                     ByVal p2 As Vertex_Textured,
                                     ByVal p3 As Vertex_Textured,
                                     ByVal Tx As Texture)

        Dim y1, y2 As Integer
        y1 = CInt(p1.Y)
        y2 = CInt(p2.Y)

        Dim inv As Double = 1.0 / (y2 - y1)
        Dim avg_light As Double = (p1.Light_Level + p2.Light_Level + p3.Light_Level) / 3
        Dim f_x1 As Double = p1.X
        Dim f_x2 As Double = p1.X
        Dim i_x1, i_x2, t As Integer
        Dim zi1, zi2 As Double
        Dim ui1, ui2 As Single
        Dim vi1, vi2 As Single

        Dim s As Single

        Dim z1 As Double = p1.Z
        Dim z2 As Double = p1.Z
        Dim u1 As Single = p1.U
        Dim u2 As Single = p1.U
        Dim v1 As Single = p1.V
        Dim v2 As Single = p1.V

        Dim z, d, zslope As Double
        Dim u, v As Single
        Dim uslope, vslope As Single
        Dim invxr As Single

        Dim slopex1 As Double = CInt(p2.X - p1.X) * inv
        Dim slopex2 As Double = CInt(p3.X - p1.X) * inv
        Dim slopez1 As Double = (p2.Z - p1.Z) * inv
        Dim slopez2 As Double = (p3.Z - p1.Z) * inv
        Dim slopeu1 As Single = (p2.U - p1.U) * CSng(inv)
        Dim slopeu2 As Single = (p3.U - p1.U) * CSng(inv)
        Dim slopev1 As Single = (p2.V - p1.V) * CSng(inv)
        Dim slopev2 As Single = (p3.V - p1.V) * CSng(inv)

        For scanlineY As Integer = y1 To y2
            If (scanlineY < scheight AndAlso scanlineY >= 0) Then
                i_x1 = CInt(f_x1)
                i_x2 = CInt(f_x2)
                zi1 = z1
                zi2 = z2
                ui1 = u1
                ui2 = u2
                vi1 = v1
                vi2 = v2

                If (i_x2 < i_x1) Then
                    t = i_x2
                    i_x2 = i_x1
                    i_x1 = t

                    d = zi1
                    zi1 = zi2
                    zi2 = d

                    s = ui1
                    ui1 = ui2
                    ui2 = s

                    s = vi1
                    vi1 = vi2
                    vi2 = s
                End If

                invxr = 1.0F / (i_x2 - i_x1)
                If (i_x2 > i_x1) Then
                    zslope = (zi2 - zi1) * invxr
                    uslope = (ui2 - ui1) * invxr
                    vslope = (vi2 - vi1) * invxr
                End If

                z = zi1
                u = ui1
                v = vi1

                For x As Integer = i_x1 To i_x2
                    If x < scwidth AndAlso x >= 0 Then
                        d = Zbuffer(x + (scwidth * scanlineY))
                        If (d = 0.0) Xor (d > z) Then
                            Dim C As Color = Tx.P(u, v)
                            MB.SetPixel(x, scanlineY, Color.FromArgb(CInt(avg_light * C.R), CInt(avg_light * C.G), CInt(avg_light * C.B)))
                            Zbuffer(x + (scwidth * scanlineY)) = z
                        End If
                    End If
                    z += zslope
                    u += uslope
                    v += vslope
                    'EPQ.PB.Refresh()
                Next
            End If
            u1 += slopeu1
            u2 += slopeu2
            v1 += slopev1
            v2 += slopev2
            z1 += slopez1
            z2 += slopez2
            f_x1 += slopex1
            f_x2 += slopex2
        Next
    End Sub

    Private Sub Text_Top_Triangle(ByVal p1 As Vertex_Textured,
                                  ByVal p2 As Vertex_Textured,
                                  ByVal p3 As Vertex_Textured,
                                  ByVal Tx As Texture)
        Dim y1, y2 As Integer
        y1 = CInt(p1.Y)
        y2 = CInt(p3.Y)

        Dim inv As Double = 1 / (y2 - y1)
        Dim avg_light As Double = (p1.Light_Level + p2.Light_Level + p3.Light_Level) / 3
        Dim f_x1 As Double = p3.X
        Dim f_x2 As Double = p3.X
        Dim i_x1, i_x2, t As Integer
        Dim zi1, zi2 As Double
        Dim ui1, ui2 As Single
        Dim vi1, vi2 As Single

        Dim z1 As Double = p3.Z
        Dim z2 As Double = p3.Z
        Dim u1 As Single = p3.U
        Dim u2 As Single = p3.U
        Dim v1 As Single = p3.V
        Dim v2 As Single = p3.V

        Dim z, d, zslope As Double
        Dim u, v As Single
        Dim uslope, vslope As Single
        Dim invxr, s As Single

        Dim slopex1 As Double = CInt(p3.X - p1.X) * inv
        Dim slopex2 As Double = CInt(p3.X - p2.X) * inv
        Dim slopez1 As Double = (p3.Z - p1.Z) * inv
        Dim slopez2 As Double = (p3.Z - p2.Z) * inv
        Dim slopeu1 As Single = (p3.U - p1.U) * CSng(inv)
        Dim slopeu2 As Single = (p3.U - p2.U) * CSng(inv)
        Dim slopev1 As Single = (p3.V - p1.V) * CSng(inv)
        Dim slopev2 As Single = (p3.V - p2.V) * CSng(inv)

        For scanlineY As Integer = y2 To y1 Step -1
            If (scanlineY < scheight AndAlso scanlineY >= 0) Then
                i_x1 = CInt(f_x1)
                i_x2 = CInt(f_x2)
                zi1 = z1
                zi2 = z2
                ui1 = u1
                ui2 = u2
                vi1 = v1
                vi2 = v2

                If (i_x2 < i_x1) Then
                    t = i_x2
                    i_x2 = i_x1
                    i_x1 = t

                    d = zi1
                    zi1 = zi2
                    zi2 = d

                    s = ui1
                    ui1 = ui2
                    ui2 = s

                    s = vi1
                    vi1 = vi2
                    vi2 = s
                End If

                invxr = 1.0F / (i_x2 - i_x1)
                If (i_x2 > i_x1) Then
                    zslope = (zi2 - zi1) * invxr
                    uslope = (ui2 - ui1) * invxr
                    vslope = (vi2 - vi1) * invxr
                End If
                z = zi1
                u = ui1
                v = vi1
                For x As Integer = i_x1 To i_x2
                    If x < scwidth AndAlso x >= 0 Then
                        d = Zbuffer(x + (scwidth * scanlineY))
                        If (d = 0.0) Xor (d > z) Then
                            Dim C As Color = Tx.P(u, v)
                            MB.SetPixel(x, scanlineY, Color.FromArgb(CInt(avg_light * C.R),
                                                                     CInt(avg_light * C.G),
                                                                     CInt(avg_light * C.B)))
                            Zbuffer(x + (scwidth * scanlineY)) = z
                        End If
                    End If
                    z += zslope
                    u += uslope
                    v += vslope
                Next
            End If
            z1 -= slopez1
            z2 -= slopez2
            u1 -= slopeu1
            u2 -= slopeu2
            v1 -= slopev1
            v2 -= slopev2
            f_x1 -= slopex1
            f_x2 -= slopex2
        Next
    End Sub

#End Region

    Public ReadOnly Property Debug_Info() As List(Of String)
        Get
            Dim Result As New List(Of String)
            Result.Add("Triangles Culled: " & Faces_Culled)
            Result.Add("Objects Culled: " & Objects_Culled)
            Result.Add("Face Culling: " & Face_Culling.ToString)
            Result.Add("Object Culling: " & Object_Culling)
            Result.Add("Fill Mode: " & Fill_Type.ToString)
            Result.Add("Frametime: " & Frametime)
            Return Result
        End Get
    End Property

    Public Sub Resize_Display(ByVal x As Integer, ByVal y As Integer)
        ReDim Zbuffer(x * y - 1)
        scwidth = x
        scheight = y
        ASP = scwidth / scheight
        MB = New Memory_Bitmap(x, y)
    End Sub

    Public Sub Set_Far_Distance(ByVal distance As Double)
        If distance > near Then far = distance
    End Sub

    Public Sub Set_Near_Distance(ByVal distance As Double)
        If distance > 0 Then near = distance
    End Sub

    Public Sub Set_Face_Culling(ByVal value As Face_Cull_Mode)
        Face_Culling = value
    End Sub

    Public Sub Set_Object_Culling(ByVal value As Boolean)

    End Sub

    Public Sub Set_Fill_Mode(ByVal value As Fill_Mode)
        Fill_Type = value
    End Sub

    Public ReadOnly Property Front_Buffer() As Memory_Bitmap
        Get
            Return MB
        End Get
    End Property

    Enum Fill_Mode
        Invisible
        Wireframe
        Solidfill
        Flatshaded
        Gouraud
    End Enum

    Enum Face_Cull_Mode
        None
        Back
        Front
    End Enum

End Class