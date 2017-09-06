Imports System.Xml

Public Class RoutePoint
    Inherits SelectableLayerObject

    Protected m_Route As Route
    Protected m_Index As Integer
    Protected m_SelectedPen As Pen
    Protected m_HighlightedPen As Pen

    Public Sub New(ByVal Center As System.Drawing.PointF, ByVal Route As Route, ByVal Index As Integer)
        MyBase.New()
        Init()
        Me.Center = Center
        m_Route = Route
        m_Index = Index
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
        Init()
    End Sub

    Protected Sub Init()
        m_Color = Drawing.Color.Purple
        m_SelectedColor = Drawing.Color.PaleVioletRed
        m_HighlightColor = Drawing.Color.Blue
        m_SelectedPen = New Pen(m_SelectedColor, 1)
        m_HighlightedPen = New Pen(m_HighlightColor, 1)
        m_Rect.Width = 1
        m_Rect.Height = 1
        m_Name = "routepoint_" & m_id
    End Sub

    ''' <summary>
    ''' Returns the route object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Route() As Route
        Get
            Return m_Route
        End Get
        Set(ByVal Value As Route)
            m_Route = Value
        End Set
    End Property

    ''' <summary>
    ''' Returns or sets the routepoint index in the route
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Index() As Integer
        Get
            Return m_Index
        End Get
        Set(ByVal Value As Integer)
            m_Index = Value
        End Set
    End Property

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        Dim Pen As Pen = Route.Pen
        If m_Selected Then
            Pen = m_SelectedPen
        End If
        If m_Highlighted Then
            Pen = m_HighlightedPen
        End If
        Graphics.DrawEllipse(Pen, m_Rect)
    End Sub

    ''' <summary>
    ''' Called by the PCB class when removing the object from the project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub Remove()
        MyBase.Remove()
        m_Route.RemoveRoutePoint(Me)
    End Sub

    Public Overrides Sub AddToPCB(ByVal PCB As PCB)
        MyBase.AddToPCB(PCB)
        If Not m_Route.RoutePoints.Contains(Me) Then
            m_Route.AddRoutePoint(m_Index, Me)
        End If
    End Sub

    ''' <summary>
    ''' Called by PCB class when deleting the object by the user, class has to create undo information and adds to project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function GetUndoDeleteItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoDeleteRoutePoint(PCB, Me)
    End Function

    ''' <summary>
    ''' Called by PCB class before the user places the object on the PCB
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Returns a undo redo stack item to undo adding the object</remarks>
    Public Overrides Function GetUndoAddItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoAddRoutePoint(PCB, Me)
    End Function

    ''' <summary>
    ''' Called when the user repositions the object, returns the undoredo position object item
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetUndoPositionItem(ByVal PCB As PCB) As UndoRedoItem
        Return Nothing
    End Function

    Public Overrides Property Width() As Single
        Get
            Return MyBase.Width
        End Get
        Set(ByVal value As Single)
        End Set
    End Property

    Public Overrides Property Height() As Single
        Get
            Return MyBase.Height
        End Get
        Set(ByVal value As Single)
        End Set
    End Property

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        MyBase.PlaceObject(PCB, WindowType)
    End Sub

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, Location)
        If WindowType = PCB.WindowTypes.WindowTypeBottom Then
            PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomRoute).LayerObjects.Add(Me)
        Else
            PCB.GetLayer(PCB.LayerTypes.LayerTypeTopRoute).LayerObjects.Add(Me)
        End If
    End Sub

    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        m_Index = Root.Attributes("index").Value
    End Sub

    Public Overrides Sub toXML(ByVal XMLDoc As System.Xml.XmlDocument, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Root.Attributes.Append(XMLDoc.CreateAttribute("index")).Value = m_Index
    End Sub
End Class


Public Class Route
    Inherits SelectableLayerObject

    Dim m_Start As Pad 'start point
    Dim m_End As Pad 'end point
    Dim m_RoutePoints As New List(Of RoutePoint)
    Dim m_Path As Drawing2D.GraphicsPath
    Dim m_Pen As Pen
    Dim m_SelectedPen As Pen
    Dim m_HighlightedPen As Pen

    Dim m_StartId As Integer 'only used when loading from xml to store the ID of the pads
    Dim m_EndId As Integer
    Dim m_RoutePointIds As List(Of Integer)

    Public Sub New()
        MyBase.New()
        Init()
    End Sub

    Public Sub New(ByVal location As System.Drawing.PointF)
        MyBase.New(location)
        Init()
        'm_RoutePoints(0).Location = location
        'm_RoutePoints(1).Location = location
    End Sub

    Public Sub New(ByVal location As System.Drawing.PointF, ByVal Width As Single, ByVal Height As Single)
        MyBase.New(location, Width, Height)
        Init()
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Init()
        fromXML(PCB, Root, BinData)
    End Sub

    Protected Sub Init()
        m_Color = Drawing.Color.Purple
        m_SelectedColor = Drawing.Color.PaleVioletRed
        m_HighlightColor = Drawing.Color.Blue
        m_Pen = New System.Drawing.Pen(m_Color, 1)
        m_SelectedPen = New Pen(m_SelectedColor, 1)
        m_HighlightedPen = New Pen(m_HighlightColor, 1)
        m_Name = "route_" & m_id
    End Sub

    ''' <summary>
    ''' Returns one of the 2 route endpoints
    ''' Each endpoint can be connected to another routepoint OR to a layerobject which implements Connectpoint interface (pad/via)
    ''' </summary>
    ''' <param name="Index"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Points(ByVal Index As Integer) As RoutePoint
        Get
            Return m_RoutePoints(Index)
        End Get
    End Property

    ''' <summary>
    ''' Returns distance from a point to the route
    ''' </summary>
    ''' <param name="ToPoint"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetDistance(ByVal ToPoint As System.Drawing.PointF) As Single
        Dim Res As Single = Single.MaxValue
        'Dim ChooseRoutePoint As Boolean
        If m_Start IsNot Nothing Then
            Dim PPoint As PointF = m_Start.Center

            For Each RoutePoint As RoutePoint In m_RoutePoints
                Dim TmpDistance As Single = DistanceToLineSegment(PPoint, RoutePoint.Center, ToPoint)
                If TmpDistance < Res Then Res = TmpDistance
                If Distance(RoutePoint.Center, ToPoint) <= Distance(PPoint, RoutePoint.Center) * 0.15 Then
                    Return Single.MaxValue
                End If
                PPoint = RoutePoint.Center
            Next

            If m_End IsNot Nothing Then
                Dim TmpDistance As Single = DistanceToLineSegment(PPoint, m_End.Center, ToPoint)
                If TmpDistance < Res Then Res = TmpDistance
                If Distance(m_End.Center, ToPoint) <= Distance(PPoint, m_End.Center) * 0.15 Then
                    Return Single.MaxValue
                End If
                Dim SecondPoint As PointF = m_End.Center
                If m_RoutePoints.Count > 0 Then SecondPoint = m_RoutePoints(0).Center
                If Distance(m_Start.Center, ToPoint) <= Distance(m_Start.Center, SecondPoint) * 0.15 Then
                    Return Single.MaxValue
                End If
            End If


        End If
        Return Res
    End Function

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        MyBase.PlaceObject(PCB, WindowType)
    End Sub

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, Location)
        If WindowType = PCB.WindowTypes.WindowTypeBottom Then
            PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomRoute).LayerObjects.Add(Me)
        Else
            PCB.GetLayer(PCB.LayerTypes.LayerTypeTopRoute).LayerObjects.Add(Me)
        End If
        For Each RoutePoint As RoutePoint In m_RoutePoints
            RoutePoint.PlaceObject(PCB, WindowType)
        Next
    End Sub

    Public Overrides Sub AddToPCB(ByVal PCB As PCB)
        MyBase.AddToPCB(PCB)
        For Each RoutePoint As RoutePoint In m_RoutePoints
            PCB.AddObject(RoutePoint)
        Next
        If m_Start IsNot Nothing Then
            If Not m_Start.Routes.Contains(Me) Then m_Start.Routes.Add(Me)
        End If
        If m_End IsNot Nothing Then
            If Not m_End.Routes.Contains(Me) Then m_End.Routes.Add(Me)
        End If
    End Sub

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        Dim Pen As Pen = m_Pen
        If m_Selected Then
            Pen = m_SelectedPen
        End If
        If m_Highlighted Then
            Pen = m_HighlightedPen
        End If
        If m_Start IsNot Nothing Then
            Graphics.DrawPath(Pen, CalculatePath())
        End If
    End Sub

    Public Overridable ReadOnly Property Pen() As Pen
        Get
            If m_Selected Then
                Return m_SelectedPen
            End If
            If m_Highlighted Then
                Return m_HighlightedPen
            End If
            Return m_Pen
        End Get
    End Property

    Protected Overridable Function CalculatePath() As Drawing2D.GraphicsPath
        m_Path = New Drawing2D.GraphicsPath
        If m_Start IsNot Nothing Then
            Dim PPoint As PointF = m_Start.Center
            For Each RoutePoint As RoutePoint In m_RoutePoints
                m_Path.AddLine(PPoint, RoutePoint.Center)
                PPoint = RoutePoint.Center
            Next
            If m_End IsNot Nothing Then
                m_Path.AddLine(PPoint, m_End.Center)
            End If
            m_Rect = m_Path.GetBounds()
            m_Center.X = m_Rect.X + m_Rect.Width / 2
            m_Center.Y = m_Rect.Y + m_Rect.Height / 2
        End If
        Return m_Path
    End Function


    ''' <summary>
    ''' returns the drawing location (X,Y start of the rect)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property Location() As System.Drawing.PointF
        Get
            Return m_Rect.Location
        End Get
        Set(ByVal value As System.Drawing.PointF)
        End Set
    End Property

    ''' <summary>
    ''' Gets / sets the center point of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property Center() As System.Drawing.PointF
        Get
            Return New PointF(m_Rect.X + m_Rect.Width / 2, m_Rect.Y + m_Rect.Height / 2) ' m_Rect.Location, m_RoutePoints(1).Location)
        End Get
        Set(ByVal value As System.Drawing.PointF)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets  the center X location of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property CenterX() As Single
        Get
            Return Center.Y
        End Get
        Set(ByVal value As Single)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the center Y of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property CenterY() As Single
        Get
            Return Center.Y
        End Get
        Set(ByVal value As Single)
            'Center = New PointF(Center.X, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the height of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property Height() As Single
        Get
            Return m_Rect.Height
        End Get
        Set(ByVal value As Single)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the width of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property Width() As Single
        Get
            Return m_Rect.Width
        End Get
        Set(ByVal value As Single)
        End Set
    End Property

    ''' <summary>
    ''' Called by PCB class when deleting the object by the user, class has to create undo information and adds to project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function GetUndoDeleteItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoDeleteRoute(PCB, Me)
    End Function

    ''' <summary>
    ''' Called by PCB class when deleting the object by the user, class has to create undo information and adds to project
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="BackUpConnections">Set to true to make a backup of the connection matrix (true by default)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function GetUndoDeleteItem(ByVal PCB As PCB, ByVal BackUpConnections As Boolean) As UndoRedoItem
        Return New UndoRedoDeleteRoute(PCB, Me, BackUpConnections)
    End Function

    ''' <summary>
    ''' Called by PCB class before the user places the object on the PCB
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Returns a undo redo stack item to undo adding the object</remarks>
    Public Overrides Function GetUndoAddItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoAddRoute(PCB, Me)
    End Function

    ''' <summary>
    ''' Called when the user repositions the object, returns the undoredo position object item
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetUndoPositionItem(ByVal PCB As PCB) As UndoRedoItem
        Return Nothing
    End Function

    ''' <summary>
    ''' Adds a new route point to the route
    ''' </summary>
    ''' <param name="Center">The center of the route point</param>
    ''' <returns>The new route point</returns>
    ''' <remarks></remarks>
    Public Overridable Function AddRoutePoint(ByVal Center As PointF) As RoutePoint
        Dim Point As New RoutePoint(Center, Me, m_RoutePoints.Count)
        m_RoutePoints.Add(Point)
        Return Point
    End Function

    ''' <summary>
    ''' Adds a route point
    ''' </summary>
    ''' <param name="Index"></param>
    ''' <param name="RoutePoint"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function AddRoutePoint(ByVal Index As Integer, ByVal RoutePoint As RoutePoint) As RoutePoint
        m_RoutePoints.Insert(Index, RoutePoint)
        RoutePoint.Route = Me
        RebuildRoutePointIndex()
        Return RoutePoint
    End Function

    ''' <summary>
    ''' Removes route point from the collection
    ''' must still be removed from the projects PCB!
    ''' </summary>
    ''' <param name="Point"></param>
    ''' <remarks></remarks>
    Public Overridable Sub RemoveRoutePoint(ByVal Point As RoutePoint)
        m_RoutePoints.Remove(Point)
        RebuildRoutePointIndex()
    End Sub

    ''' <summary>
    ''' When a routepoint is added at an index or removed, we must change the index of all routepoints
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub RebuildRoutePointIndex()
        Dim i As Integer = 0
        For Each RoutePoint As RoutePoint In m_RoutePoints
            RoutePoint.Index = i
            i = i + 1
        Next
    End Sub

    ''' <summary>
    ''' Returns all route points in this route
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property RoutePoints() As List(Of RoutePoint)
        Get
            Return m_RoutePoints
        End Get
    End Property

    Public ReadOnly Property LastRoutePoint() As RoutePoint
        Get
            Return m_RoutePoints(m_RoutePoints.Count - 1)
        End Get
    End Property

    Public Overridable Property StartPad() As Pad
        Get
            Return m_Start
        End Get
        Set(ByVal value As Pad)
            m_Start = value
        End Set
    End Property

    Public Overridable Property EndPad() As Pad
        Get
            Return m_End
        End Get
        Set(ByVal value As Pad)
            m_End = value
        End Set
    End Property

    Public Overrides Sub Remove()
        MyBase.Remove()
        Dim TmpRoutePoints() As RoutePoint = m_RoutePoints.ToArray()
        For Each RoutePoint As RoutePoint In TmpRoutePoints
            m_PCB.RemoveObject(RoutePoint)
        Next

        m_RoutePoints = New List(Of RoutePoint)
        For Each RoutePoint As RoutePoint In TmpRoutePoints 'to make undo work properly remember the route point
            m_RoutePoints.Add(RoutePoint)
        Next

        If m_Start IsNot Nothing Then
            m_Start.Routes.Remove(Me)
        End If
        If m_End IsNot Nothing Then
            m_End.Routes.Remove(Me)
        End If
        If m_Start IsNot Nothing Then
            If m_Start.Routes.Count = 0 Then
                m_PCB.ConnectionMatrix.DisconnectPad(m_Start)
            End If
        End If
        If m_End IsNot Nothing Then
            If m_End.Routes.Count = 0 Then
                m_PCB.ConnectionMatrix.DisconnectPad(m_End)
            End If
        End If

        If m_Start IsNot Nothing AndAlso m_End IsNot Nothing Then
            If m_Start.Routes.Count > 0 AndAlso m_End.Routes.Count > 0 Then
                Dim Net As Net = m_PCB.ConnectionMatrix.GetNet(m_Start, ConnectionMatrix.ConnectionTypes.ConnectionTypeConnected)
                m_PCB.ConnectionMatrix.DisconnectPads(Net.Pads.ToArray()) 'disconnect all pads,! will also disconnect pads added manually!
                m_PCB.ConnectionMatrix.ConnectPads(m_Start.GetConnectedPadsByRoutes().ToArray()) 'connect all pads connected by routes to start
                m_PCB.ConnectionMatrix.ConnectPads(m_End.GetConnectedPadsByRoutes().ToArray()) 'connect all pads connected by routes to end
            End If
        End If
    End Sub

    Public Function GetFirstRouteSegmentLocation(ByVal ClosestRoutePointIndex As Integer) As PointF
        If ClosestRoutePointIndex = 0 Then
            Return m_Start.Center
        Else
            'If ClosestRoutePointIndex >= m_RoutePoints.Count Then Return m_End.Center
            Return m_RoutePoints(ClosestRoutePointIndex - 1).Center
        End If
    End Function

    Public Function GetSecondRouteSegementLocation(ByVal ClosestRoutePointIndex As Integer) As PointF
        If ClosestRoutePointIndex >= m_RoutePoints.Count Then
            Return m_End.Center
        Else
            Return m_RoutePoints(ClosestRoutePointIndex).Center
        End If
    End Function

    ''' <summary>
    ''' Returns the index of the last route point of the route segment closest to ToPoint
    ''' 0 => start --> point(0), 1 => 2, 2 => end 
    ''' </summary>
    ''' <param name="ToPoint"></param>
    ''' <param name="maxDistance">Max distance to route</param>
    ''' <param name="Distance">Returns the distance measured</param>
    ''' <returns>-1 if no points match</returns>
    ''' <remarks></remarks>
    Public Function GetClosestRoutePointIndex(ByVal ToPoint As PointF, Optional ByVal maxDistance As Single = Single.MaxValue, Optional ByRef Distance As Single = Single.MaxValue) As Integer
        Dim Dst As Single = Single.MaxValue
        'Dim ChooseRoutePoint As Boolean
        If m_Start IsNot Nothing Then
            Dim PPoint As PointF = m_Start.Center
            Dim ClosestPointIndex As Integer

            For Each RoutePoint As RoutePoint In m_RoutePoints
                Dim TmpDistance As Single = DistanceToLineSegment(PPoint, RoutePoint.Center, ToPoint)
                If TmpDistance < Dst Then
                    Dst = TmpDistance
                    ClosestPointIndex = RoutePoint.Index
                End If
                PPoint = RoutePoint.Center
            Next

            If m_End IsNot Nothing Then
                Dim TmpDistance As Single = DistanceToLineSegment(PPoint, m_End.Center, ToPoint)
                If TmpDistance < Dst Then
                    Dst = TmpDistance
                    ClosestPointIndex = m_RoutePoints.Count
                End If
            End If

            If Dst <= maxDistance Then
                Distance = Dst
                Return ClosestPointIndex
            End If

        End If
        Return -1
    End Function

    ''' <summary>
    ''' Returns all pads connected to
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetConnectedPads() As List(Of Pad)
        Dim Lst As New List(Of Pad)
        If m_Start IsNot Nothing Then Lst.Add(m_Start)
        If m_End IsNot Nothing Then Lst.Add(m_End)
        Return Lst
    End Function

    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        Dim RoutePointNodes As Xml.XmlNodeList = Root.SelectNodes("./points/pt") 'get all routepoints
        m_RoutePoints = New List(Of RoutePoint)
        m_RoutePointIds = New List(Of Integer)
        For Each RoutePointNode As XmlNode In RoutePointNodes
            m_RoutePointIds.Add(CInt(RoutePointNode.Attributes("id").Value))
        Next
        m_StartId = Root.SelectSingleNode("start").Attributes("id").Value
        m_EndId = Root.SelectSingleNode("end").Attributes("id").Value
        Init()
        RebuildRoutePointIndex()
    End Sub

    Public Overrides Sub toXML(ByVal XMLDoc As System.Xml.XmlDocument, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        m_StartId = -1
        m_EndId = -1
        If m_Start IsNot Nothing Then m_StartId = m_Start.id
        If m_End IsNot Nothing Then m_EndId = m_End.id
        Root.AppendChild(XMLDoc.CreateElement("start")).Attributes.Append(XMLDoc.CreateAttribute("id")).Value = m_StartId
        Root.AppendChild(XMLDoc.CreateElement("end")).Attributes.Append(XMLDoc.CreateAttribute("id")).Value = m_EndId
        Dim PointsNode As XmlNode = Root.AppendChild(XMLDoc.CreateElement("points"))
        For Each RoutePoint As RoutePoint In m_RoutePoints
            RoutePoint.toXML(XMLDoc, PointsNode.AppendChild(XMLDoc.CreateElement("pt")), BinData)
        Next
    End Sub

    Private Sub m_PCB_ObjectsLoaded(ByVal Sender As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile) Handles m_PCB.ObjectsLoaded
        m_Start = m_PCB.GetLayerObject(m_StartId)
        m_End = m_PCB.GetLayerObject(m_EndId)
        For Each RoutePointId As Integer In m_RoutePointIds
            Dim RoutePoint As RoutePoint = m_PCB.GetLayerObject(RoutePointId)
            AddRoutePoint(RoutePoint.Index, RoutePoint)
        Next
        m_Start.Routes.Add(Me)
        m_End.Routes.Add(Me)
    End Sub

End Class
