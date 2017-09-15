Public Class FrmLayer

    Private Enum DrawTools
        DrawSMD = 1
        DrawThrougHole = 2
        DrawVia = 3
        DrawRoute = 4
        DrawJunction = 5
        DrawSplit = 6
    End Enum

    Public Enum DrawStates
        None = 0
        DrawObject
        DrawDevicePins
        Connect
        SelectObjects
        MovePCBImage
        StartMoveObjects
        MoveObjects
        AutoRouter
        ResizeObjectSelect
        ResizeObjectSelected
        ResizeObjectResize
        DrawRouteBegin 'first point of a route
        DrawRouteEnd 'next point/end of a route
        SplitRoute 'split route between 2 route points
    End Enum

    Private Structure ClickPoint
        Dim Location As PointF
    End Structure

    Private Enum ResizePointType
        ResizeNone
        ResizeNW
        ResizeN
        ResizeNE
        ResizeE
        ResizeSE
        ResizeS
        ResizeSW
        ResizeW
    End Enum

    Private Structure ResizeInfoStruct
        Public ResizePoints() As ResizePoint
        Public ResizeObject As LayerObject
        Public WindowType As PCB.WindowTypes 'the window type the selected object is located on
        Public ResizingRefPoint As PointF 'reference where resizing began
        Public IsResizing As Boolean 'is now resizing
        Public ResizingType As ResizePointType 'if resizing, this is the direction
        Public ResizeStartLocation As RectangleF  'start location / size before resizing began
    End Structure

    Private Structure ResizePoint
        Public Location As PointF
        Public ResizeType As ResizePointType
        Public CursorType As ResizePointType

        Public Sub New(ByVal Location As PointF, ByVal ResizeType As ResizePointType, ByVal CursorType As ResizePointType)
            Me.Location = Location
            Me.ResizeType = ResizeType
            Me.CursorType = CursorType
        End Sub
        Public Function GetCursor() As Cursor
            Select Case CursorType
                Case ResizePointType.ResizeE
                    Return Cursors.SizeWE
                Case ResizePointType.ResizeN
                    Return Cursors.SizeNS
                Case ResizePointType.ResizeNE
                    Return Cursors.SizeNESW
                Case ResizePointType.ResizeNW
                    Return Cursors.SizeNWSE
                Case ResizePointType.ResizeS
                    Return Cursors.SizeNS
                Case ResizePointType.ResizeSE
                    Return Cursors.SizeNWSE
                Case ResizePointType.ResizeSW
                    Return Cursors.SizeNESW
                Case ResizePointType.ResizeW
                    Return Cursors.SizeWE
            End Select
            Return Nothing
        End Function
    End Structure

    Private Shared sPCB As PCB 'shared PCB object for shared subs
    Private Shared DrawObject As LayerObject 'temp object used to show what you are currently drawing
    Private Shared DrawRouteWindow As PCB.WindowTypes 'if we are drawing a route, this is the window we started drawing the route, clicking the other window has no effect
    Private Shared DrawState As DrawStates
    Private Shared ConnectDevice As Device
    Private Shared ConnectDevicePin As UnconnectedDevicePin
    Private Shared LayerWindows As New List(Of FrmLayer)
    Private Shared WithEvents AutoRouteWindow As FrmManualRoute
    Private Shared WithEvents PropertiesWindow As FrmProperties
    Private Shared ResizeInfo As ResizeInfoStruct 'holds information about the selected object that can be resized now
    Private Shared InfoText As String

    Dim MovePoint As ClickPoint 'last mouse move position
    Dim LeftClickPoint As ClickPoint 'the last point left clicked = mouse down event
    Dim RightClickPoint As ClickPoint 'the last point right clicked
    Dim MiddleClickPoint As ClickPoint 'the last point middle clicked 
    Dim TranslateStartX As Single, TranslateStartY As Single 'used for moving the pcb on the screen with middle mouse button

    Dim m_StartLocation As PointF
    Dim m_Scale As Double
    Dim m_Width As Single
    Dim m_Height As Single
    Dim m_TranslateX As Single
    Dim m_TranslateY As Single

    Dim m_LastRoutePointLocation As PointF  'last rightclicked routepoint location is saved here, in case of a double click we must move back to that location because the last point was removed by right click
    Private Shared m_SplitRouteLines(0 To 1) As Line 'if we split a route, draw lines of new location

    Public WithEvents PCB As PCB 'the pcb
    Public WindowType As PCB.WindowTypes

    ''' <summary>
    ''' Creates a new layer window, the window type and the PCB project are required
    ''' </summary>
    ''' <param name="WindowType"></param>
    ''' <param name="PCB"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal WindowType As PCB.WindowTypes, ByVal PCB As PCB)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Me.PCB = PCB
        sPCB = PCB
        m_Scale = 1
        m_Height = PCB.Height
        m_Width = PCB.Width
        Me.WindowType = WindowType
        LayerWindows.Add(Me)
        UpdateWindowTitle()
    End Sub

    Private Sub UpdateWindowTitle()
        Select Case WindowType
            Case unPCB.PCB.WindowTypes.WindowTypeBottom
                Me.Text = "unPCB [" & PCB.Name & "] - Bottom layer " & IIf(PCB.IsChanged, "*", "")
            Case unPCB.PCB.WindowTypes.WindowTypeTop
                Me.Text = "unPCB [" & PCB.Name & "] - Top layer " & IIf(PCB.IsChanged, "*", "")
        End Select
    End Sub

    Private Sub FrmLayer_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        LayerWindows.Remove(Me)
    End Sub

    Private Sub FrmLayer_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ToolStripButtonUndo.Enabled = PCB.UndoStack.Count > 0
        ToolStripButtonRedo.Enabled = PCB.RedoStack.Count > 0
        FlipHorizontallyToolStripMenuItem.Checked = PCB.HorizontalMirror(WindowType)
        FlipVerticallyToolStripMenuItem.Checked = PCB.VerticalMirror(WindowType)
        RefreshLayerMenuItems()
    End Sub

    ''' <summary>
    ''' Mouse wheel event, forward to the double buffer
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub FrmLayer_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        DoubleBuffer_MouseWheel(sender, e)
    End Sub

    ''' <summary>
    ''' Paint of the form, update the graphics
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub FrmLayer_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        UpdateGraphics()
    End Sub

    Private Sub DoubleBuffer_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles DoubleBuffer.KeyDown
        If e.KeyCode = Keys.Escape Then
            ResetDrawState()
            e.Handled = True
        End If
    End Sub

    Private Sub DoubleBuffer_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseDoubleClick
        Select Case DrawState
            Case DrawStates.DrawRouteBegin
                PCB.UnHighlightAllObjects(True)
                Dim ObjTypes As New List(Of Type)
                Dim Route As Route = CType(DrawObject, Route)
                ObjTypes.Add(GetType(SMDPad))
                ObjTypes.Add(GetType(ThroughHolePad))
                ObjTypes.Add(GetType(RouteJunction))
                ObjTypes.Add(GetType(Via))
                Dim Closest As LayerObject = PCB.GetClosestObject(WindowType, ConvertLocation(e.Location), ObjTypes, 20 / m_Scale, DrawObject)
                If Closest Is Nothing Then
                    Dim Pad As Pad = Nothing
                    If e.Button = Windows.Forms.MouseButtons.Left Then
                        Pad = New RouteJunction()
                    ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
                        Pad = New Via()
                    End If
                    If Pad IsNot Nothing Then
                        Pad.Center = ConvertLocation(e.Location)
                        PCB.PlaceObject(Pad, WindowType)
                        Route.StartPad = Pad
                        DrawRouteWindow = WindowType
                        If DrawRouteWindow = PCB.WindowTypes.WindowTypeTop Then 'add to drawlayer
                            PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Add(DrawObject)
                        End If
                        If DrawRouteWindow = PCB.WindowTypes.WindowTypeBottom Then
                            PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Add(DrawObject)
                        End If
                        Route.AddRoutePoint(ConvertLocation(e.Location))
                    End If
                End If
            Case DrawStates.DrawRouteEnd
                If DrawRouteWindow = WindowType Then
                    PCB.UnHighlightAllObjects(True)
                    Dim Route As Route = CType(DrawObject, Route)
                    Dim ObjTypes As New List(Of Type)
                    ObjTypes.Add(GetType(SMDPad))
                    ObjTypes.Add(GetType(ThroughHolePad))
                    ObjTypes.Add(GetType(RouteJunction))
                    ObjTypes.Add(GetType(Via))
                    Dim Closest As LayerObject = PCB.GetClosestObject(WindowType, ConvertLocation(e.Location), ObjTypes, 20 / m_Scale, DrawObject)
                    If Closest Is Nothing Then
                        Dim Pad As Pad = Nothing
                        If e.Button = Windows.Forms.MouseButtons.Left Then
                            Pad = New RouteJunction()
                        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
                            If Route.RoutePoints.Count > 0 Then
                                Route.LastRoutePoint.Location = m_LastRoutePointLocation
                            End If
                            Pad = New Via()
                        End If
                        If Pad IsNot Nothing Then
                            Pad.Center = ConvertLocation(e.Location)
                            PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(Route)
                            PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(Route)
                            PCB.PlaceObject(Pad, WindowType)
                            If Route.RoutePoints.Count > 0 AndAlso e.Button = Windows.Forms.MouseButtons.Left Then
                                Route.RemoveRoutePoint(Route.LastRoutePoint)
                            End If
                            Route.EndPad = Pad
                        End If
                    End If
                End If
        End Select
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoubleBuffer_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseDown
        Select Case e.Button
            Case MouseButtons.Left
                LeftClickPoint.Location = e.Location
            Case MouseButtons.Middle
                MiddleClickPoint.Location = e.Location
                TranslateStartX = TranslateX
                TranslateStartY = TranslateY
            Case MouseButtons.Right
                RightClickPoint.Location = e.Location
        End Select
        Select Case DrawState
            Case DrawStates.DrawObject, DrawStates.DrawDevicePins
                If e.Button = MouseButtons.Left Then 'set the center
                    DrawObject.Center = ConvertLocation(e.Location) ' New Point(Window.ConvertX(e.X), Window.ConvertY(e.Y))
                End If
            Case DrawStates.SelectObjects
            Case DrawStates.MovePCBImage
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    m_StartLocation = DrawObject.Location
                End If

            Case DrawStates.MoveObjects

            Case DrawStates.ResizeObjectSelect

            Case DrawStates.ResizeObjectSelected 'now we start resizing, if we have a resize type set
                If ResizeInfo.ResizingType <> ResizePointType.ResizeNone Then
                    ResizeInfo.IsResizing = True
                    ResizeInfo.ResizingRefPoint = ConvertLocation(e.Location)
                    DrawState = DrawStates.ResizeObjectResize
                End If
            Case DrawStates.ResizeObjectResize

        End Select
        UpdateGraphics()
    End Sub

    ''' <summary>
    ''' forward to pcb
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoubleBuffer_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseMove
        If e.Button = MouseButtons.Middle Then
            TranslateX = TranslateStartX + ConvertX(e.X) - ConvertX(MiddleClickPoint.Location.X)
            TranslateY = TranslateStartY + ConvertY(e.Y) - ConvertY(MiddleClickPoint.Location.Y)
        End If
        MovePoint.Location = e.Location
        Select Case DrawState
            Case DrawStates.DrawObject, DrawStates.DrawDevicePins
                If e.Button = MouseButtons.Left Then
                    If Distance(LeftClickPoint.Location, e.Location) > 5 Then DrawObject.AutoScale(Me, ConvertLocation(LeftClickPoint.Location), ConvertLocation(e.Location))
                Else
                    DrawObject.Center = ConvertLocation(e.Location)
                End If

                PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                If WindowType = PCB.WindowTypes.WindowTypeTop Then
                    PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Add(DrawObject)
                End If
                If WindowType = PCB.WindowTypes.WindowTypeBottom Then
                    PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Add(DrawObject)
                End If
                If DrawState = DrawStates.DrawDevicePins Then
                    InfoText = ConnectDevicePin.DevicePin.GetUniquePadName(ConnectDevicePin.EagleDevicePadName) 'display info about the pin
                End If
            Case DrawStates.MovePCBImage
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    DrawObject.Location = New Point(m_StartLocation.X + ConvertX(LeftClickPoint.Location.X) - ConvertX(e.X), m_StartLocation.Y + ConvertY(LeftClickPoint.Location.Y) - ConvertY(e.Y))
                End If
            Case DrawStates.MoveObjects
                Dim SelectedObjects As ReadOnlyDictionary(Of Integer, SelectableLayerObject) = PCB.GetSelectedLayerObjects()
                Dim Move As PointF = New PointF(ConvertX(e.X) - m_StartLocation.X, ConvertY(e.Y) - m_StartLocation.Y)
                For Each SelectedObject As KeyValuePair(Of Integer, SelectableLayerObject) In SelectedObjects
                    SelectedObject.Value.Center = New PointF(SelectedObject.Value.StartCenter.X + Move.X, SelectedObject.Value.StartCenter.Y + Move.Y)
                Next
            Case DrawStates.ResizeObjectSelect 'wait for resize object to be selected, do nothing here

            Case DrawStates.ResizeObjectSelected 'the resize object is selected
                Dim SmallestDistance As Single = Single.MaxValue
                Dim SmallestDistancePoint As ResizePoint
                For Each Point As ResizePoint In ResizeInfo.ResizePoints
                    Dim Dist As Single = Distance(Point.Location, ConvertLocation(e.Location))
                    If Dist < SmallestDistance Then
                        SmallestDistance = Dist
                        SmallestDistancePoint = Point
                    End If
                Next
                If SmallestDistance <= 2 Then
                    Cursor = SmallestDistancePoint.GetCursor()
                    ResizeInfo.ResizingType = SmallestDistancePoint.ResizeType
                Else
                    ResizeInfo.ResizingType = ResizePointType.ResizeNone
                    Cursor = Cursors.Default
                End If
            Case DrawStates.ResizeObjectResize 'actual resizing
                ResizeObject(ResizeInfo.ResizeObject, ResizeInfo.ResizeStartLocation, ResizeInfo.ResizingRefPoint, ConvertLocation(e.Location), ResizeInfo.ResizingType)
            Case DrawStates.DrawRouteBegin
                PCB.UnHighlightAllObjects(True)
                Dim ObjTypes As New List(Of Type)
                ObjTypes.Add(GetType(SMDPad))
                ObjTypes.Add(GetType(ThroughHolePad))
                ObjTypes.Add(GetType(RouteJunction))
                ObjTypes.Add(GetType(Via))
                Dim Closest As LayerObject = PCB.GetClosestObject(WindowType, ConvertLocation(e.Location), ObjTypes, 20 / m_Scale, DrawObject)
                If Closest IsNot Nothing Then PCB.HighlightObject(Closest)
            Case DrawStates.DrawRouteEnd
                If DrawRouteWindow = WindowType Then
                    PCB.UnHighlightAllObjects(True)
                    Dim ObjTypes As New List(Of Type)
                    Dim Route As Route = CType(DrawObject, Route)
                    ObjTypes.Add(GetType(SMDPad))
                    ObjTypes.Add(GetType(ThroughHolePad))
                    ObjTypes.Add(GetType(RouteJunction))
                    ObjTypes.Add(GetType(Via))
                    Dim Closest As LayerObject = PCB.GetClosestObject(WindowType, ConvertLocation(e.Location), ObjTypes, 20 / m_Scale, DrawObject)
                    If Closest IsNot Nothing Then PCB.HighlightObject(Closest)
                    If Route.RoutePoints.Count > 0 Then Route.LastRoutePoint.Location = ConvertLocation(e.Location)
                End If
            Case DrawStates.SplitRoute
                For i As Integer = 0 To m_SplitRouteLines.Length - 1
                    PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(m_SplitRouteLines(i))
                    PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(m_SplitRouteLines(i))
                Next

                m_SplitRouteLines(0).Highlighted = True
                m_SplitRouteLines(1).Highlighted = True

                Dim DrawLayer As PCB.LayerTypes = unPCB.PCB.LayerTypes.LayerTypeBottomDrawing
                Dim RouteLayer As PCB.LayerTypes = unPCB.PCB.LayerTypes.LayerTypeBottomRoute

                If WindowType = PCB.WindowTypes.WindowTypeTop Then
                    DrawLayer = unPCB.PCB.LayerTypes.LayerTypeTopDrawing
                    RouteLayer = unPCB.PCB.LayerTypes.LayerTypeTopRoute
                End If

                Dim MaxDistance As Single = 50 / m_Scale
                Dim ClosestRoute As Route = Nothing
                Dim ClosestRoutePointIndex As Integer = -1
                For Each LayerObject As LayerObject In PCB.GetLayer(RouteLayer).LayerObjects
                    If TypeOf LayerObject Is Route Then
                        Dim TmpIndex As Integer = CType(LayerObject, Route).GetClosestRoutePointIndex(ConvertLocation(e.Location), MaxDistance, MaxDistance)
                        If TmpIndex >= 0 Then
                            ClosestRoute = CType(LayerObject, Route)
                            ClosestRoutePointIndex = TmpIndex
                        End If
                    End If
                Next

                If ClosestRoute IsNot Nothing Then
                    m_SplitRouteLines(0).Points(0) = ClosestRoute.GetFirstRouteSegmentLocation(ClosestRoutePointIndex)
                    m_SplitRouteLines(0).Points(1) = ConvertLocation(e.Location)
                    m_SplitRouteLines(1).Points(0) = ConvertLocation(e.Location)
                    m_SplitRouteLines(1).Points(1) = ClosestRoute.GetSecondRouteSegementLocation(ClosestRoutePointIndex)
                    PCB.GetLayer(DrawLayer).LayerObjects.Add(m_SplitRouteLines(0))
                    PCB.GetLayer(DrawLayer).LayerObjects.Add(m_SplitRouteLines(1))
                End If

        End Select
        UpdateGraphics()
    End Sub

    ''' <summary>
    ''' forwardto pcb
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoubleBuffer_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseUp
        If e.Button = MouseButtons.Middle Then
            TranslateX = TranslateStartX + ConvertX(e.X) - ConvertX(MiddleClickPoint.Location.X)
            TranslateY = TranslateStartY + ConvertY(e.Y) - ConvertY(MiddleClickPoint.Location.Y)
        End If
        Select Case DrawState
            Case DrawStates.DrawObject
                If e.Button = MouseButtons.Left Then
                    If DrawObject.Width > 0.001 AndAlso DrawObject.Height > 0.001 Then
                        PCB.PlaceObject(DrawObject, WindowType)
                        ResetDrawState()
                    End If
                End If
            Case DrawStates.DrawDevicePins
                Select Case e.Button
                    Case Windows.Forms.MouseButtons.Left 'left button to place pad
                        If DrawObject.Width > 0.001 AndAlso DrawObject.Height > 0.001 Then
                            PCB.PlaceObject(DrawObject, WindowType)
                            sPCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                            sPCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                            ConnectDevicePin.DevicePin.AddPad(DrawObject, ConnectDevicePin.EagleDevicePadName) 'add pad to connectdevicepin
                            CType(DrawObject, Pad).Name = ConnectDevicePin.DevicePin.GetUniquePadName(ConnectDevicePin.EagleDevicePadName)
                            InfoText = ""
                            ConnectDevicePin = ConnectDevice.GetUnconnectedPin() 'get next unconnected or not fully connected device pin
                            If ConnectDevicePin.DevicePin IsNot Nothing Then 'we are at the end
                                If ConnectDevicePin.DevicePin.DefaultPadType(ConnectDevicePin.EagleDevicePadName).Name = DrawObject.GetType.Name Then 'same pin type, clone object
                                    DrawObject = CType(DrawObject.Clone(), LayerObject)
                                Else
                                    DrawObject = Activator.CreateInstance(System.Type.GetType("unPCB." & ConnectDevicePin.DevicePin.DefaultPadType(ConnectDevicePin.EagleDevicePadName).Name, True, True))
                                End If
                                sPCB.Cursor = Cursors.Cross
                            Else
                                ResetDrawState()
                            End If
                        End If
                    Case Windows.Forms.MouseButtons.Right 'with right mouse we can toggle between SMD and throughole pads
                        PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                        PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                        If DrawObject.GetType().Name = GetType(SMDPad).Name Then
                            DrawObject = New ThroughHolePad()
                        Else
                            DrawObject = New SMDPad()
                        End If
                End Select
            Case DrawStates.SelectObjects
                PCB.UnHighlightAllObjects()
                If e.Button = MouseButtons.Left Then
                    PCB.SelectObjects(WindowType, ConvertLocation(e.Location), My.Computer.Keyboard.CtrlKeyDown)
                ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
                    Menu_SelectConnectedPads.Enabled = PCB.GetSelectedLayerObjects(GetType(Pad)).Count > 0
                    HighlightConnectedPadsToolStripMenuItem.Enabled = Menu_SelectConnectedPads.Enabled
                    MenuRightClickObject.Show(DoubleBuffer, e.Location)
                End If
            Case DrawStates.Connect

            Case DrawStates.MovePCBImage
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    PCB.AddUndoItem(New UndoRedoMovePCBImage(PCB, DrawObject, m_StartLocation))
                End If
            Case DrawStates.StartMoveObjects
                If e.Button = Windows.Forms.MouseButtons.Left Then 'select objects
                    If PCB.GetSelectedLayerObjects().Count = 0 Or My.Computer.Keyboard.CtrlKeyDown Then
                        PCB.SelectObjects(WindowType, ConvertLocation(e.Location), My.Computer.Keyboard.CtrlKeyDown)
                    End If
                    If Not My.Computer.Keyboard.CtrlKeyDown Then
                        Dim SelectedObjects As ReadOnlyDictionary(Of Integer, SelectableLayerObject) = PCB.GetSelectedLayerObjects()
                        For Each SelectedObject As KeyValuePair(Of Integer, SelectableLayerObject) In SelectedObjects
                            SelectedObject.Value.StartCenter = SelectedObject.Value.Center
                        Next
                        m_StartLocation = ConvertLocation(e.Location)
                        DrawState = DrawStates.MoveObjects 'no ctrl key, go to move!
                        PCB.Cursor = Cursors.Cross
                    End If
                End If
            Case DrawStates.MoveObjects
                If e.Button = Windows.Forms.MouseButtons.Left Then 'end moving objects
                    PCB.AddUndoItem(New UndoRedoMoveLayerObject(PCB, PCB.GetSelectedLayerObjects))
                    PCB.DeselectAllObjects()
                    DrawState = DrawStates.StartMoveObjects 'go back
                    PCB.Cursor = Cursors.Default
                Else
                    ResetDrawState()
                End If
            Case DrawStates.ResizeObjectSelect
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    Dim ResizeObject As LayerObject = PCB.SelectObjects(WindowType, ConvertLocation(e.Location), False)
                    SelectResizeObject(ResizeObject)
                End If
            Case DrawStates.ResizeObjectSelected

            Case DrawStates.ResizeObjectResize
                ' add undo item, end resizeing
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    PCB.AddUndoItem(New UndoRedoResizeLayerObject(PCB, ResizeInfo.ResizeObject, ResizeInfo.ResizeStartLocation))
                    DrawState = DrawStates.ResizeObjectSelect
                    SelectResizeObject(ResizeInfo.ResizeObject)
                End If
            Case DrawStates.DrawRouteBegin
                If e.Button = Windows.Forms.MouseButtons.Left Then 'start first point of a route
                    'find the closest object and connect to that
                    Dim Route As Route = CType(DrawObject, Route)
                    If Route.StartPad Is Nothing Then
                        Dim ObjTypes As New List(Of Type)
                        ObjTypes.Add(GetType(SMDPad))
                        ObjTypes.Add(GetType(ThroughHolePad))
                        ObjTypes.Add(GetType(RouteJunction))
                        ObjTypes.Add(GetType(Via))
                        Dim Closest As LayerObject = PCB.GetClosestObject(WindowType, ConvertLocation(e.Location), ObjTypes, 20 / m_Scale, Route)
                        If Closest IsNot Nothing Then 'check if we are near a pad
                            DrawRouteWindow = WindowType
                            If DrawRouteWindow = PCB.WindowTypes.WindowTypeTop Then 'add to drawlayer
                                PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Add(DrawObject)
                            End If
                            If DrawRouteWindow = PCB.WindowTypes.WindowTypeBottom Then
                                PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Add(DrawObject)
                            End If
                            Route.StartPad = Closest 'start
                            Route.AddRoutePoint(ConvertLocation(e.Location)) 'add the first route point
                            DrawState = DrawStates.DrawRouteEnd
                        End If
                    Else
                        DrawState = DrawStates.DrawRouteEnd
                    End If
                End If
            Case DrawStates.DrawRouteEnd
                If DrawRouteWindow = WindowType Then
                    Dim Route As Route = CType(DrawObject, Route)
                    If Route.EndPad Is Nothing Then
                        If e.Button = Windows.Forms.MouseButtons.Left Then 'finish the second point of a route, then start a new one unless we connect to a pad
                            'find the closest object and connect to that
                            Dim ObjTypes As New List(Of Type)
                            ObjTypes.Add(GetType(SMDPad))
                            ObjTypes.Add(GetType(ThroughHolePad))
                            ObjTypes.Add(GetType(RouteJunction))
                            ObjTypes.Add(GetType(Via))
                            Dim Closest As LayerObject = PCB.GetClosestObject(WindowType, ConvertLocation(e.Location), ObjTypes, 20 / m_Scale, Route)
                            If Closest IsNot Nothing Then
                                Route.RemoveRoutePoint(Route.LastRoutePoint)
                                PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                                PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                                Route.EndPad = Closest
                                PCB.PlaceObject(DrawObject, WindowType) 'add the next point
                                PCB.ConnectionMatrix.ConnectPads(Route.StartPad, Route.EndPad)
                                DrawObject = New Route()
                                DrawState = DrawStates.DrawRouteBegin
                            Else
                                Route.AddRoutePoint(ConvertLocation(e.Location))
                            End If
                        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
                            If Route.RoutePoints.Count > 0 Then
                                Route.RemoveRoutePoint(Route.LastRoutePoint)
                                If Route.RoutePoints.Count > 0 Then
                                    m_LastRoutePointLocation = Route.LastRoutePoint.Location
                                Else
                                    ResetDrawState(False)
                                End If
                            End If
                        End If
                    Else
                        If e.Button = Windows.Forms.MouseButtons.Left OrElse e.Button = Windows.Forms.MouseButtons.Right Then
                            PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                            PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                            If e.Button = Windows.Forms.MouseButtons.Left Then Route.RemoveRoutePoint(Route.LastRoutePoint) 'double left click wil first insert a routepoint which must be deleted first
                            PCB.PlaceObject(DrawObject, WindowType) 'add the next point
                            PCB.ConnectionMatrix.ConnectPads(Route.StartPad, Route.EndPad)
                            DrawObject = New Route()
                            DrawState = DrawStates.DrawRouteBegin
                        End If
                    End If
                Else
                    Beep()
                End If
            Case DrawStates.SplitRoute
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    For i As Integer = 0 To m_SplitRouteLines.Length - 1
                        PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(m_SplitRouteLines(i))
                        PCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(m_SplitRouteLines(i))
                    Next

                    Dim RouteLayer As PCB.LayerTypes = unPCB.PCB.LayerTypes.LayerTypeBottomRoute

                    If WindowType = PCB.WindowTypes.WindowTypeTop Then
                        RouteLayer = unPCB.PCB.LayerTypes.LayerTypeTopRoute
                    End If

                    Dim MaxDistance As Single = 50 / m_Scale
                    Dim ClosestRoute As Route = Nothing
                    Dim ClosestRoutePointIndex As Integer = -1
                    For Each LayerObject As LayerObject In PCB.GetLayer(RouteLayer).LayerObjects 'find the route to add the new route point to
                        If TypeOf LayerObject Is Route Then
                            Dim TmpIndex As Integer = CType(LayerObject, Route).GetClosestRoutePointIndex(ConvertLocation(e.Location), MaxDistance, MaxDistance)
                            If TmpIndex >= 0 Then
                                ClosestRoute = CType(LayerObject, Route)
                                ClosestRoutePointIndex = TmpIndex
                            End If
                        End If
                    Next

                    If ClosestRoute IsNot Nothing Then 'add new route point
                        Dim Point As RoutePoint = New RoutePoint(ConvertLocation(e.Location), ClosestRoute, ClosestRoutePointIndex)
                        ClosestRoute.AddRoutePoint(ClosestRoutePointIndex, Point)
                        Point.PlaceObject(PCB, WindowType)
                        PCB.AddObject(Point)
                    End If
                End If
        End Select
        UpdateGraphics()
    End Sub

    ''' <summary>
    ''' Selects an object for resizing, also adjust drawstate if not correct
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Private Sub SelectResizeObject(ByVal LayerObject As SelectableLayerObject)
        If Not DrawState = DrawStates.ResizeObjectSelect Then
            ResetDrawState(False)
            SetDrawState(DrawStates.ResizeObjectSelect)
            PCB.SelectObject(LayerObject)
        End If
        ResizeInfo.ResizeObject = LayerObject
        ReDim ResizeInfo.ResizePoints(0 To 7)
        CalcResizePoints(LayerObject)

        ResizeInfo.ResizeStartLocation = ResizeInfo.ResizeObject.Rect
        ResizeInfo.IsResizing = False
        ResizeInfo.ResizingType = ResizePointType.ResizeNone
        ResizeInfo.WindowType = WindowType
        SetDrawState(DrawStates.ResizeObjectSelected) 'the object is now selected, we can start resizing it
    End Sub

    Private Sub CalcResizePoints(ByVal LayerObject As LayerObject)

        ResizeInfo.ResizePoints(0) = New ResizePoint(New PointF(ResizeInfo.ResizeObject.Rect.Left, ResizeInfo.ResizeObject.Rect.Top), ResizePointType.ResizeNW, IIf(PCB.HorizontalMirror(WindowType) Xor PCB.VerticalMirror(WindowType), ResizePointType.ResizeSW, ResizePointType.ResizeNW))
        ResizeInfo.ResizePoints(1) = New ResizePoint(New PointF(ResizeInfo.ResizeObject.Rect.Right, ResizeInfo.ResizeObject.Rect.Top), ResizePointType.ResizeNE, IIf(PCB.HorizontalMirror(WindowType) Xor PCB.VerticalMirror(WindowType), ResizePointType.ResizeSE, ResizePointType.ResizeNE))
        ResizeInfo.ResizePoints(2) = New ResizePoint(New PointF(ResizeInfo.ResizeObject.Rect.Right, ResizeInfo.ResizeObject.Rect.Bottom), ResizePointType.ResizeSE, IIf(PCB.HorizontalMirror(WindowType) Xor PCB.VerticalMirror(WindowType), ResizePointType.ResizeNE, ResizePointType.ResizeSE))
        ResizeInfo.ResizePoints(3) = New ResizePoint(New PointF(ResizeInfo.ResizeObject.Rect.Left, ResizeInfo.ResizeObject.Rect.Bottom), ResizePointType.ResizeSW, IIf(PCB.HorizontalMirror(WindowType) Xor PCB.VerticalMirror(WindowType), ResizePointType.ResizeNW, ResizePointType.ResizeSW))

        ResizeInfo.ResizePoints(4) = New ResizePoint(MidPoint(ResizeInfo.ResizePoints(0).Location, ResizeInfo.ResizePoints(1).Location), ResizePointType.ResizeN, ResizePointType.ResizeN)
        ResizeInfo.ResizePoints(5) = New ResizePoint(MidPoint(ResizeInfo.ResizePoints(1).Location, ResizeInfo.ResizePoints(2).Location), ResizePointType.ResizeE, ResizePointType.ResizeE)
        ResizeInfo.ResizePoints(6) = New ResizePoint(MidPoint(ResizeInfo.ResizePoints(2).Location, ResizeInfo.ResizePoints(3).Location), ResizePointType.ResizeS, ResizePointType.ResizeS)
        ResizeInfo.ResizePoints(7) = New ResizePoint(MidPoint(ResizeInfo.ResizePoints(3).Location, ResizeInfo.ResizePoints(0).Location), ResizePointType.ResizeW, ResizePointType.ResizeW)
    End Sub

    Private Sub ResizeObject(ByVal LayerObject As LayerObject, ByVal StartSize As RectangleF, ByVal RefPoint As PointF, ByVal ClickPoint As PointF, ByVal Direction As ResizePointType)
        Select Case Direction
            Case ResizePointType.ResizeE
                LayerObject.Width = StartSize.Width + ClickPoint.X - RefPoint.X
            Case ResizePointType.ResizeN
                LayerObject.Location = New PointF(LayerObject.Location.X, StartSize.Location.Y + ClickPoint.Y - RefPoint.Y)
                LayerObject.Height = StartSize.Height + RefPoint.Y - ClickPoint.Y
            Case ResizePointType.ResizeNE
                LayerObject.Location = New PointF(LayerObject.Location.X, StartSize.Location.Y + ClickPoint.Y - RefPoint.Y)
                LayerObject.Width = StartSize.Width + ClickPoint.X - RefPoint.X
                LayerObject.Height = StartSize.Height + RefPoint.Y - ClickPoint.Y
            Case ResizePointType.ResizeNW
                LayerObject.Location = New PointF(StartSize.X + ClickPoint.X - RefPoint.X, StartSize.Location.Y + ClickPoint.Y - RefPoint.Y)
                LayerObject.Width = StartSize.Width + RefPoint.X - ClickPoint.X
                LayerObject.Height = StartSize.Height + RefPoint.Y - ClickPoint.Y
            Case ResizePointType.ResizeS
                LayerObject.Height = StartSize.Height + ClickPoint.Y - RefPoint.Y
            Case ResizePointType.ResizeSE
                LayerObject.Height = StartSize.Height + ClickPoint.Y - RefPoint.Y
                LayerObject.Width = StartSize.Width + ClickPoint.X - RefPoint.X
            Case ResizePointType.ResizeSW
                LayerObject.Location = New PointF(StartSize.X + ClickPoint.X - RefPoint.X, StartSize.Location.Y)
                LayerObject.Height = StartSize.Height + ClickPoint.Y - RefPoint.Y
                LayerObject.Width = StartSize.Width + RefPoint.X - ClickPoint.X
            Case ResizePointType.ResizeW
                LayerObject.Location = New PointF(StartSize.X + ClickPoint.X - RefPoint.X, StartSize.Location.Y)
                LayerObject.Width = StartSize.Width + RefPoint.X - ClickPoint.X
        End Select
        CalcResizePoints(LayerObject)
    End Sub

    ''' <summary>
    ''' Update graphics
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoubleBuffer_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles DoubleBuffer.MouseEnter
        UpdateGraphics()
    End Sub

    ''' <summary>
    ''' adjust window scale
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoubleBuffer_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseWheel
        Dim xBefore As Integer = ConvertX(e.X)
        Dim yBefore As Integer = ConvertY(e.Y)
        Dim xAfter As Integer
        Dim yAfter As Integer

        If e.Delta > 0 Then
            m_Scale = m_Scale / 0.9
        ElseIf e.Delta < 0 Then
            m_Scale = m_Scale * 0.9
        End If

        xAfter = ConvertX(e.X)
        yAfter = ConvertY(e.Y)

        m_TranslateX += -(xBefore - xAfter)
        m_TranslateY += -(yBefore - yAfter)

        UpdateGraphics()
    End Sub

    ''' <summary>
    ''' tells the doublebuffered image box to update the graphics (fire paint event)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub UpdateGraphics()
        DoubleBuffer.Refresh()
    End Sub

    ''' <summary>
    ''' Paints everything
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoubleBuffer_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles DoubleBuffer.Paint
        Dim HorizontalFlip As Double = 1
        Dim VerticalFlip As Double = 1
        Dim Graphics As System.Drawing.Graphics
        Dim TmpTranslateX As Single, TmpTranslateY As Single
        Graphics = e.Graphics

        'Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low ' // or NearestNeighbour
        'Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None
        'Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None
        'Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed
        'Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel

        If PCB.HorizontalMirror(WindowType) Then HorizontalFlip = -1.0
        If PCB.VerticalMirror(WindowType) Then VerticalFlip = -1.0

        Graphics.Clear(Color.Transparent)
        Graphics.ResetTransform()
        'http://www.vb-helper.com/howto_net_draw_flipped_text.html

        Graphics.ScaleTransform(m_Scale * HorizontalFlip, m_Scale * VerticalFlip) 'flip horizontally

        TmpTranslateX = m_TranslateX
        If PCB.VerticalMirror(WindowType) Then
            TmpTranslateY -= m_Height
        End If

        TmpTranslateY = m_TranslateY
        If PCB.HorizontalMirror(WindowType) Then
            TmpTranslateX -= m_Width
        End If

        Graphics.TranslateTransform(TmpTranslateX, TmpTranslateY)

        For Each Layer As KeyValuePair(Of PCB.LayerTypes, Layer) In PCB.Layers
            If PCB.GetWindowTypeOfLayerType(Layer.Key) = WindowType Then
                Layer.Value.Draw(Graphics)
            End If
        Next

        'If DrawState = DrawStates.ResizeObjectSelected Or DrawState = DrawStates.ResizeObjectResize Then
        If ResizeInfo.WindowType = WindowType Then
            If ResizeInfo.ResizeObject IsNot Nothing Then
                Dim Pen As New Pen(Color.FromArgb(225, 255, 128, 0), 2)
                Dim Brush As New SolidBrush(Color.FromArgb(225, 255, 128, 0))
                Pen.DashStyle = Drawing2D.DashStyle.Dash
                Pen.EndCap = Drawing2D.LineCap.SquareAnchor
                Pen.StartCap = Drawing2D.LineCap.SquareAnchor
                Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(0).Location.X, ResizeInfo.ResizePoints(0).Location.Y, ResizeInfo.ResizePoints(1).Location.X, ResizeInfo.ResizePoints(1).Location.Y)
                Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(1).Location.X, ResizeInfo.ResizePoints(1).Location.Y, ResizeInfo.ResizePoints(2).Location.X, ResizeInfo.ResizePoints(2).Location.Y)
                Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(2).Location.X, ResizeInfo.ResizePoints(2).Location.Y, ResizeInfo.ResizePoints(3).Location.X, ResizeInfo.ResizePoints(3).Location.Y)
                Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(3).Location.X, ResizeInfo.ResizePoints(3).Location.Y, ResizeInfo.ResizePoints(0).Location.X, ResizeInfo.ResizePoints(0).Location.Y)

                Graphics.FillRectangle(Brush, New RectangleF(ResizeInfo.ResizePoints(4).Location.X - 2, ResizeInfo.ResizePoints(4).Location.Y - 2, 2, 4))
                Graphics.FillRectangle(Brush, New RectangleF(ResizeInfo.ResizePoints(5).Location.X - 2, ResizeInfo.ResizePoints(5).Location.Y - 2, 4, 2))
                Graphics.FillRectangle(Brush, New RectangleF(ResizeInfo.ResizePoints(6).Location.X - 2, ResizeInfo.ResizePoints(6).Location.Y - 2, 2, 4))
                Graphics.FillRectangle(Brush, New RectangleF(ResizeInfo.ResizePoints(7).Location.X - 2, ResizeInfo.ResizePoints(7).Location.Y - 2, 4, 2))

                'Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(4).Location.X, ResizeInfo.ResizePoints(4).Location.Y, ResizeInfo.ResizePoints(4).Location.X, ResizeInfo.ResizePoints(4).Location.Y)
                'Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(5).Location.X, ResizeInfo.ResizePoints(5).Location.Y, ResizeInfo.ResizePoints(5).Location.X, ResizeInfo.ResizePoints(5).Location.Y)
                'Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(6).Location.X, ResizeInfo.ResizePoints(6).Location.Y, ResizeInfo.ResizePoints(6).Location.X, ResizeInfo.ResizePoints(6).Location.Y)
                'Graphics.DrawLine(Pen, ResizeInfo.ResizePoints(7).Location.X, ResizeInfo.ResizePoints(7).Location.Y, ResizeInfo.ResizePoints(7).Location.X, ResizeInfo.ResizePoints(7).Location.Y)

                'Graphics.DrawRectangle(Pen, DrawObject.Rect.X, DrawObject.Rect.Y, DrawObject.Rect.Width, DrawObject.Rect.Height)
            End If
        End If

        If InfoText <> "" Then
            Dim Cont1 As Drawing2D.GraphicsContainer = Graphics.BeginContainer()
            Graphics.TranslateTransform(ConvertX(MovePoint.Location.X + 5), ConvertY(MovePoint.Location.Y + 5))
            Dim Cont2 As Drawing2D.GraphicsContainer = Graphics.BeginContainer()
            If PCB.HorizontalMirror(WindowType) Then
                Graphics.ScaleTransform(-1, 1)
            End If
            If PCB.VerticalMirror(WindowType) Then
                Graphics.ScaleTransform(1, -1)
            End If
            Graphics.DrawString(InfoText, Me.Font, New SolidBrush(Color.Red), 0, 0)
            Graphics.EndContainer(Cont2)
            Graphics.EndContainer(Cont1)
        End If
        Select Case DrawState
            Case DrawStates.AutoRouter
                StatusBar.Text = "Auto router"
            Case DrawStates.Connect
                StatusBar.Text = "Connect pins"
            Case DrawStates.DrawDevicePins
                StatusBar.Text = "Place pin " & InfoText
            Case DrawStates.DrawObject
                StatusBar.Text = "Draw object"
            Case DrawStates.MoveObjects
                StatusBar.Text = "Move object"
            Case DrawStates.MovePCBImage
                StatusBar.Text = "Move background image"
            Case DrawStates.None
                StatusBar.Text = "Ready"
            Case DrawStates.ResizeObjectResize
                StatusBar.Text = "Resize object"
            Case DrawStates.ResizeObjectSelect
                StatusBar.Text = "Select object to resize"
            Case DrawStates.ResizeObjectSelected

            Case DrawStates.SelectObjects
                StatusBar.Text = "Select objects"
            Case DrawStates.StartMoveObjects
                StatusBar.Text = "Move object"
        End Select
    End Sub

    ''' <summary>
    ''' Start drawing a pad
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub SMDPadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SMDPadToolStripMenuItem.Click
        For Each LayerWindow As FrmLayer In LayerWindows
            LayerWindow.ToolStripSplitButtonDrawPad.Image = SMDPadToolStripMenuItem.Image
            LayerWindow.ToolStripSplitButtonDrawPad.Tag = DrawTools.DrawSMD
        Next
        StartDrawObject(New SMDPad())
    End Sub

    ''' <summary>
    ''' Start drawing throughole pad
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ThroughHolePadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ThroughHolePadToolStripMenuItem.Click
        For Each LayerWindow As FrmLayer In LayerWindows
            LayerWindow.ToolStripSplitButtonDrawPad.Image = ThroughHolePadToolStripMenuItem.Image
            LayerWindow.ToolStripSplitButtonDrawPad.Tag = DrawTools.DrawThrougHole
        Next
        StartDrawObject(New ThroughHolePad())
    End Sub

    ''' <summary>
    ''' gets or sets the scale for the graphics displayed
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DrawScale() As Double
        Get
            Return m_Scale
        End Get
        Set(ByVal value As Double)
            m_Scale = value
        End Set
    End Property

    ''' <summary>
    ''' gets or sets the width of the PCB (also set through constructor from pcb)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PCBWidth() As Single
        Get
            Return m_Width
        End Get
        Set(ByVal value As Single)
            m_Width = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the height of the PCB (also set through constructor from pcb)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PCBHeight() As Single
        Get
            Return m_Height
        End Get
        Set(ByVal value As Single)
            m_Height = value
        End Set
    End Property

    ''' <summary>
    ''' gets or sets the translate X
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TranslateX() As Single
        Get
            Return m_TranslateX
        End Get
        Set(ByVal value As Single)
            m_TranslateX = value
        End Set
    End Property

    ''' <summary>
    ''' gets or sets the translate Y
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TranslateY() As Single
        Get
            Return m_TranslateY
        End Get
        Set(ByVal value As Single)
            m_TranslateY = value
        End Set
    End Property

    ''' <summary>
    ''' Scales width, size pixels on the screen to a width on the pcb
    ''' </summary>
    ''' <param name="Width"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ScaleWidth(ByVal Width As Integer) As Integer
        Return Width / m_Scale
    End Function

    ''' <summary>
    ''' Scales height, size pixels on the screen to a height on the pcb
    ''' </summary>
    ''' <param name="Height"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ScaleHeight(ByVal Height As Integer) As Integer
        Return Height / m_Scale
    End Function

    ''' <summary>
    ''' Converts location X on the screen (window pixels) to X location on the PCB
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertX(ByVal x As Single) As Single
        If PCB.HorizontalMirror(WindowType) Then
            Return x / -m_Scale + m_Width - m_TranslateX
        Else
            Return x / m_Scale - m_TranslateX
        End If
    End Function

    ''' <summary>
    ''' Converts location Y on the screen (window pixels) to Y location on the PCB
    ''' </summary>
    ''' <param name="y"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertY(ByVal y As Single) As Single
        If PCB.VerticalMirror(WindowType) Then

            Return y / -m_Scale - m_TranslateY '+ m_Height
        Else
            Return y / m_Scale - m_TranslateY
        End If
    End Function

    ''' <summary>
    ''' Converts a point on the screen to location on the PCB
    ''' </summary>
    ''' <param name="Location"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertLocation(ByVal Location As Point) As PointF
        Return New PointF(ConvertX(Location.X), ConvertY(Location.Y))
    End Function

    Public Function ConvertLocation(ByVal Location As PointF) As PointF
        Return New PointF(ConvertX(Location.X), ConvertY(Location.Y))
    End Function

    Private Sub ToolStripButtonPointer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonPointer.Click
        StartSelect()
    End Sub

    Private Sub MenuSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuSave.Click
        FrmMain.SaveProject()
    End Sub

    Private Sub MenuOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuOpen.Click
        FrmMain.OpenProject()
    End Sub

    Private Sub ToolStripButtonConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonConnect.Click
        PCB.AddUndoItem(New UndoRedoConnect(PCB))
        PCB.ConnectSelectedPads()
    End Sub

    Private Sub ToolStripButtonUnconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonUnconnect.Click
        If PCB.GetSelectedLayerObjects(GetType(Pad)).Count = 1 Then
            If MsgBox("Only 1 pad selected, do you want this pad to be never connected to all other pads having unknown connection with it at this moment?" & vbCrLf & "You must be sure the pad is not connected with anything else except the pads currently connected to it!", MsgBoxStyle.YesNo Or MsgBoxStyle.Question) = MsgBoxResult.Yes Then
                PCB.AddUndoItem(New UndoRedoConnect(PCB))
                PCB.NOTConnectSelectedPadsToAllPads()
            End If
        Else
            PCB.AddUndoItem(New UndoRedoConnect(PCB))
            PCB.NOTConnectSelectedPads()
        End If
    End Sub

    Private Sub ToolStripDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripDisconnect.Click
        PCB.AddUndoItem(New UndoRedoConnect(PCB))
        PCB.DisconnectSelectedPads()
    End Sub

    Private Sub PCB_BackgroundImageMirrorChanged(ByVal Sender As PCB, ByVal WIndowType As PCB.WindowTypes, ByVal VerticalMirrorChanged As Boolean) Handles PCB.BackgroundImageMirrorChanged
        Me.FlipHorizontallyToolStripMenuItem.Checked = Sender.HorizontalMirror(Me.WindowType)
        Me.FlipVerticallyToolStripMenuItem.Checked = Sender.VerticalMirror(Me.WindowType)
    End Sub

    Private Sub PCB_ChangeCursor(ByVal Sender As PCB, ByVal Cursor As System.Windows.Forms.Cursor) Handles PCB.ChangeCursor
        DoubleBuffer.Cursor = Cursor
    End Sub

    Private Sub PCB_LayerVisibilityChanged(ByVal Sender As PCB, ByVal Layer As Layer, ByVal Visible As Boolean) Handles PCB.LayerVisibilityChanged
        RefreshLayerMenuItems()
        UpdateGraphics()
    End Sub

    Private Sub PCB_NameChanged(ByVal Sender As PCB, ByVal Name As String) Handles PCB.NameChanged
        UpdateWindowTitle()
    End Sub

    Private Sub PCB_ProjectChanged(ByVal Sender As PCB) Handles PCB.ProjectChanged
        UpdateWindowTitle()
    End Sub

    Private Sub PCB_ProjectLoaded(ByVal Sender As PCB, ByVal ZipFile As Ionic.Zip.ZipFile) Handles PCB.ProjectLoaded
        RefreshLayerMenuItems()
    End Sub

    Private Sub PCB_SizeChanged(ByVal Sender As PCB, ByVal Width As Single, ByVal Height As Single) Handles PCB.SizeChanged
        m_Width = Width
        m_Height = Height
    End Sub

    Private Sub PCB_UndoRedoStackUpdate(ByVal Sender As PCB, ByVal UndoStack As System.Collections.Generic.LinkedList(Of UndoRedoItem), ByVal RedoStack As System.Collections.Generic.Stack(Of UndoRedoItem)) Handles PCB.UndoRedoStackUpdate
        ToolStripButtonUndo.Enabled = UndoStack.Count > 0
        ToolStripButtonRedo.Enabled = RedoStack.Count > 0
        If UndoStack.Count > 0 Then ToolStripButtonUndo.ToolTipText = "Undo " & UndoStack.First.Value.Description
        If RedoStack.Count > 0 Then ToolStripButtonRedo.ToolTipText = "Redo " & RedoStack.Peek().Description
    End Sub

    Private Sub PCB_UpdateGraphics(ByVal Sender As PCB, ByVal WindowType As PCB.WindowTypes) Handles PCB.UpdateGraphics
        If WindowType = WindowType Then UpdateGraphics()
    End Sub

    Public Shared ReadOnly Property CurrentDrawState() As DrawStates
        Get
            Return DrawState
        End Get
    End Property

    ''' <summary>
    ''' Resets the draw state after click/mouse up event, resets the finite state machine and prepares for next click
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub ResetDrawState(Optional ByVal KeepSelection As Boolean = False)
        Select Case DrawState
            Case DrawStates.DrawObject
                sPCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                sPCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                DrawObject = CType(DrawObject.Clone(), LayerObject)
                sPCB.Cursor = Cursors.Cross
                SetDrawState(DrawStates.DrawObject)
            Case DrawStates.DrawDevicePins
                sPCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                sPCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                DrawObject = Nothing
                ConnectDevicePin = Nothing
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.None)
                InfoText = ""
            Case DrawStates.MovePCBImage
                DrawObject = Nothing
            Case DrawStates.MoveObjects 'cancels move objects
                Dim SelectedObjects As ReadOnlyDictionary(Of Integer, SelectableLayerObject) = sPCB.GetSelectedLayerObjects()
                For Each SelectedObject As KeyValuePair(Of Integer, SelectableLayerObject) In SelectedObjects
                    SelectedObject.Value.Center = SelectedObject.Value.StartCenter
                Next
                DrawObject = Nothing
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.StartMoveObjects)
            Case DrawStates.AutoRouter
                If AutoRouteWindow IsNot Nothing Then
                    AutoRouteWindow.MustSearchNext()
                End If
            Case DrawStates.ResizeObjectSelect, DrawStates.ResizeObjectSelected
                ResizeInfo = New ResizeInfoStruct()
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.ResizeObjectSelect)
            Case DrawStates.ResizeObjectResize
                If ResizeInfo.ResizeObject IsNot Nothing Then
                    ResizeInfo.ResizeObject.Location = ResizeInfo.ResizeStartLocation.Location
                    ResizeInfo.ResizeObject.Width = ResizeInfo.ResizeStartLocation.Width
                    ResizeInfo.ResizeObject.Height = ResizeInfo.ResizeStartLocation.Height
                End If
                ResizeInfo = New ResizeInfoStruct()
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.ResizeObjectSelect)
            Case DrawStates.SelectObjects
                DrawObject = Nothing
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.SelectObjects)
            Case DrawStates.DrawRouteBegin, DrawStates.DrawRouteEnd
                sPCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(DrawObject)
                sPCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(DrawObject)
                DrawObject = Nothing
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.None)
            Case Else
                DrawObject = Nothing
                sPCB.Cursor = Cursors.Default
                SetDrawState(DrawStates.None)
        End Select

        For i As Integer = 0 To m_SplitRouteLines.Length - 1
            sPCB.GetLayer(PCB.LayerTypes.LayerTypeBottomDrawing).LayerObjects.Remove(m_SplitRouteLines(i))
            sPCB.GetLayer(PCB.LayerTypes.LayerTypeTopDrawing).LayerObjects.Remove(m_SplitRouteLines(i))
        Next

        sPCB.UnHighlightAllObjects()
        If Not KeepSelection Then
            sPCB.DeselectAllObjects()
        End If
        DeselectMenuItems()
    End Sub

    Public Shared Function GetDrawstate() As DrawStates
        Return DrawState
    End Function

    ''' <summary>
    ''' Inits the drawstate to the state value
    ''' checks the correct toolbar menu item
    ''' </summary>
    ''' <param name="State"></param>
    ''' <remarks></remarks>
    Public Shared Sub SetDrawState(ByVal State As DrawStates)
        DrawState = State
        If State = DrawStates.None Then
            DeselectMenuItems()
        Else
            For Each Layerwindow As FrmLayer In LayerWindows
                Select Case DrawState
                    Case DrawStates.Connect
                    Case DrawStates.DrawDevicePins
                    Case DrawStates.DrawObject
                        'Layerwindow.ToolStripSplitButtonDrawPad.checked = True
                    Case DrawStates.MovePCBImage
                        Layerwindow.ToolStripButtonMove.Checked = True
                    Case DrawStates.None

                    Case DrawStates.SelectObjects
                        Layerwindow.ToolStripButtonPointer.Checked = True
                    Case DrawStates.StartMoveObjects
                        Layerwindow.ToolStripButtonMove.Checked = True
                    Case DrawStates.AutoRouter
                        Layerwindow.ToolStripButtonAutoRoute.Checked = True
                    Case DrawStates.ResizeObjectSelect, DrawStates.ResizeObjectSelected, DrawStates.ResizeObjectResize
                        Layerwindow.ToolStripButtonResize.Checked = True
                    Case DrawStates.SplitRoute
                        m_SplitRouteLines(0) = New Line()
                        m_SplitRouteLines(1) = New Line()
                        sPCB.Cursor = Cursors.Cross
                End Select
            Next
        End If

    End Sub

    ''' <summary>
    ''' Deselects all toolbar items
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub DeselectMenuItems()
        For Each LayerWindow As FrmLayer In LayerWindows
            For Each Button As ToolStripItem In LayerWindow.Toolbar.Items
                If TypeOf Button Is ToolStripButton Then
                    CType(Button, ToolStripButton).Checked = False
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Starts drawing pads and automatically connects them to the device pin
    ''' </summary>
    ''' <param name="Device"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DrawDevicePins(ByVal Device As Device) As Boolean
        ResetDrawState()
        ConnectDevice = Device
        ConnectDevicePin = ConnectDevice.GetUnconnectedPin()
        If ConnectDevicePin.DevicePin IsNot Nothing Then
            DrawObject = Activator.CreateInstance(System.Type.GetType("unPCB." & ConnectDevicePin.DevicePin.DefaultPadType(ConnectDevicePin.EagleDevicePadName).Name, True, True))
            SetDrawState(DrawStates.DrawDevicePins)
            sPCB.Cursor = Cursors.Cross
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' starts drawing an object on a layer
    ''' </summary>
    ''' <param name="cObject">The layerobject to draw (pad, text,...)</param>
    ''' <remarks></remarks>
    Public Shared Sub StartDrawObject(ByVal cObject As LayerObject)
        ResetDrawState()
        SetDrawState(DrawStates.DrawObject)
        DrawObject = cObject
        sPCB.Cursor = Cursors.Cross
    End Sub

    ''' <summary>
    ''' Starts selection by clicking with mouse
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub StartSelect()
        ResetDrawState()
        SetDrawState(DrawStates.SelectObjects)
        sPCB.Cursor = Cursors.Default
    End Sub

    ''' <summary>
    ''' Starts moving background image
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub StartMovePCBImage(ByVal PCBImage As PCBImage)
        ResetDrawState()
        SetDrawState(DrawStates.MovePCBImage)
        sPCB.Cursor = Cursors.SizeAll
        DrawObject = PCBImage
    End Sub

    ''' <summary>
    ''' Starts moving the selected objects
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub StartMoveSelectedObjects()
        ResetDrawState(True)
        SetDrawState(DrawStates.StartMoveObjects)
    End Sub

    Public Shared Sub StartResizeObject()
        ResetDrawState()
        SetDrawState(DrawStates.ResizeObjectSelect)
    End Sub

    Private Sub ToolStripButtonAutoRoute_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonAutoRoute.Click
        If DrawState <> DrawStates.AutoRouter Then
            ResetDrawState()
            SetDrawState(DrawStates.AutoRouter)
        End If
        If AutoRouteWindow Is Nothing Then
            AutoRouteWindow = New FrmManualRoute(PCB)
        End If
        AutoRouteWindow.NextCheck()
        AutoRouteWindow.Show()
    End Sub

    Private Shared Sub AutoRouteWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles AutoRouteWindow.Disposed
        AutoRouteWindow = Nothing
    End Sub

    Private Sub Menu_SelectConnectedPads_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_SelectConnectedPads.Click
        Dim SelectedPads As List(Of SelectableLayerObject) = PCB.GetSelectedLayerObjects(GetType(Pad))
        PCB.DeselectAllObjects()
        For Each SelectedPad As SelectableLayerObject In SelectedPads
            'PCB.HighlightObject(SelectedPad)
            PCB.SelectObject(SelectedPad)
            Dim Net As Net = PCB.ConnectionMatrix.GetNet(SelectedPad, ConnectionMatrix.ConnectionTypes.ConnectionTypeConnected)
            For Each Pad As Pad In Net.Pads
                'PCB.HighlightObject(Pad)
                PCB.SelectObject(Pad)
            Next
        Next
        UpdateGraphics()
    End Sub

    Private Sub Menu_Properties_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Menu_Properties.Click
        Dim SelectedObjects As ReadOnlyDictionary(Of Integer, SelectableLayerObject) = PCB.GetSelectedLayerObjects()
        If SelectedObjects.Count = 1 Then
            For Each selectedobject As KeyValuePair(Of Integer, SelectableLayerObject) In SelectedObjects
                ShowProperties(selectedobject.Value.GetProperties())
                Exit Sub
            Next
        Else
            Dim SelectedObjectPropertiesArray As New List(Of LayerObjectProperties)
            For Each selectedobject As KeyValuePair(Of Integer, SelectableLayerObject) In SelectedObjects
                SelectedObjectPropertiesArray.Add(selectedobject.Value.GetProperties())
            Next
            ShowProperties(SelectedObjectPropertiesArray.ToArray())
        End If
    End Sub

    Private Sub ShowProperties(ByVal Obj)
        If PropertiesWindow Is Nothing Then
            PropertiesWindow = New FrmProperties(PCB, Me)
        End If
        If IsArray(Obj) Then
            PropertiesWindow.PropertyGrid.SelectedObjects = Obj
        Else
            PropertiesWindow.PropertyGrid.SelectedObject = Obj
        End If
        PropertiesWindow.Visible = False
        PropertiesWindow.ShowDialog(Me)
    End Sub

    Private Sub Menu_deleteObject_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_deleteObject.Click
        Dim SelectedObjects As ReadOnlyDictionary(Of Integer, SelectableLayerObject)
        SelectedObjects = PCB.GetSelectedLayerObjects()
        If MsgBox("Are you sure you want to delete the selected objects?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question) = MsgBoxResult.Yes Then
            For Each SelectedObject As KeyValuePair(Of Integer, SelectableLayerObject) In SelectedObjects
                PCB.DeleteObject(SelectedObject.Value)
            Next
        End If
    End Sub

    Private Shared Sub PropertiesWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles PropertiesWindow.Disposed
        PropertiesWindow = Nothing
    End Sub

    Private Sub ToolStripButtonUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonUndo.Click
        ResetDrawState()
        PCB.Undo()
    End Sub

    Private Sub ToolStripButtonRedo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ToolStripButtonRedo.Click
        ResetDrawState()
        PCB.Redo()
    End Sub

    Private Sub MoveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MoveToolStripMenuItem.Click
        On Error Resume Next
        Dim PCBImage As PCBImage
        If WindowType = unPCB.PCB.WindowTypes.WindowTypeBottom Then
            PCBImage = PCB.Layers(unPCB.PCB.LayerTypes.LayerTypeBottomImage).LayerObjects(0)
        Else
            PCBImage = PCB.Layers(unPCB.PCB.LayerTypes.LayerTypeTopImage).LayerObjects(0)
        End If
        If PCBImage IsNot Nothing Then
            StartMovePCBImage(PCBImage)
        Else
            MsgBox("There is no PCB background loaded.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub LoadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoadToolStripMenuItem.Click
        Dim ImageLayer As Layer
        Dim LayerImage As PCBImage
        Dim ImageFile As String

        OpenFileDialog.Filter = "Image Files|*.png;*.bmp;*.jpg|Alle files|*"
        If OpenFileDialog.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then

            ImageFile = OpenFileDialog.FileName

            If WindowType = unPCB.PCB.WindowTypes.WindowTypeBottom Then
                ImageLayer = PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeBottomImage)
            Else
                ImageLayer = PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeTopImage)
            End If

            If ImageLayer.LayerObjects.Count > 0 Then
                LayerImage = ImageLayer.LayerObjects(0)
                PCB.DeleteObject(LayerImage)
            End If

            LayerImage = New PCBImage(New Point(0, 0), ImageFile)

            PCB.AddObject(LayerImage)
            ImageLayer.AddObject(LayerImage)

            If LayerImage.Width <> PCB.Width Then
                PCB.Width = Math.Max(PCB.Width, LayerImage.Width)
            End If

            If LayerImage.Height <> PCB.Height Then
                PCB.Height = Math.Max(PCB.Height, LayerImage.Height)
            End If
        End If
    End Sub

    Private Sub FlipHorizontallyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FlipHorizontallyToolStripMenuItem.Click
        ResetDrawState()
        m_TranslateX = 0
        m_TranslateY = 0
        FlipHorizontallyToolStripMenuItem.Checked = Not FlipHorizontallyToolStripMenuItem.Checked
        PCB.AddUndoItem(New UndoRedoMirrorBackgroundImage(PCB, WindowType))
        PCB.HorizontalMirror(WindowType) = FlipHorizontallyToolStripMenuItem.Checked
        UpdateGraphics()
    End Sub

    Private Sub FlipVerticallyToolStripMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles FlipVerticallyToolStripMenuItem.Click
        ResetDrawState()
        m_TranslateX = 0
        m_TranslateY = 0
        FlipVerticallyToolStripMenuItem.Checked = Not FlipVerticallyToolStripMenuItem.Checked
        PCB.AddUndoItem(New UndoRedoMirrorBackgroundImage(PCB, WindowType))
        PCB.VerticalMirror(WindowType) = FlipVerticallyToolStripMenuItem.Checked
        UpdateGraphics()
    End Sub

    Private Sub MoveObjectToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MoveObjectToolStripMenuItem.Click
        StartMoveSelectedObjects()
    End Sub

    Private Sub MenuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuExit.Click
        FrmMain.ExitToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub SaveAsMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAsMenuItem.Click
        FrmMain.SaveProjectAs()
    End Sub

    Private Sub ToolStripButtonConnectionFinished_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonConnectionFinished.Click
        PCB.AddUndoItem(New UndoRedoConnect(PCB))
        PCB.NOTConnectSelectedPadsToAllPads()
    End Sub

    Private Sub ToolStripButtonMove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonMove.Click
        StartMoveSelectedObjects()
    End Sub

    Private Sub ToolStripButtonResize_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonResize.Click
        StartResizeObject()
    End Sub

    Private Sub ToolStripButtonText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonText.Click
        Dim sText As String = InputBox("Enter text", "Enter Text")
        If Text <> "" Then
            DrawObject = New TextBox(sText)
            SetDrawState(DrawStates.DrawObject)
        End If
    End Sub

    Private Sub HighlightConnectedPadsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HighlightConnectedPadsToolStripMenuItem.Click
        Dim SelectedPads As List(Of SelectableLayerObject) = PCB.GetSelectedLayerObjects(GetType(Pad))
        PCB.DeselectAllObjects()
        For Each SelectedPad As SelectableLayerObject In SelectedPads
            PCB.HighlightObject(SelectedPad)
            Dim Net As Net = PCB.ConnectionMatrix.GetNet(SelectedPad, ConnectionMatrix.ConnectionTypes.ConnectionTypeConnected)
            For Each Pad As Pad In Net.Pads
                PCB.HighlightObject(Pad)
            Next
        Next
        UpdateGraphics()
    End Sub

    Public Sub RefreshLayerMenuItems()
        For Each LayerMenuItem As ToolStripMenuItem In LayersToolStripMenuItem.DropDownItems()
            RemoveHandler LayerMenuItem.Click, AddressOf LayerToolstripMenuItem_Click
        Next
        LayersToolStripMenuItem.DropDownItems.Clear()
        For Each Layer As KeyValuePair(Of PCB.LayerTypes, Layer) In PCB.Layers
            If Not Layer.Value.Hidden Then
                With CType(LayersToolStripMenuItem.DropDownItems.Add(Layer.Value.Name), ToolStripMenuItem)
                    .Checked = Layer.Value.Visible
                    .Tag = Layer.Value
                    AddHandler .Click, AddressOf LayerToolstripMenuItem_Click
                End With
            End If
        Next
    End Sub

    Private Sub LayerToolstripMenuItem_Click(ByVal Sender As Object, ByVal e As System.EventArgs)
        Sender.checked = Not Sender.checked
        CType(Sender.tag, Layer).Visible = Sender.checked
    End Sub

    Private Sub DoubleBuffer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DoubleBuffer.Load

    End Sub

    Private Sub ViaToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ViaToolStripMenuItem.Click
        For Each LayerWindow As FrmLayer In LayerWindows
            LayerWindow.ToolStripSplitButtonDrawPad.Image = ThroughHolePadToolStripMenuItem.Image
            LayerWindow.ToolStripSplitButtonDrawPad.Tag = DrawTools.DrawVia
        Next

        StartDrawObject(New Via())
    End Sub

    Private Sub ToolStripSplitButtonDrawPad_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripSplitButtonDrawPad.ButtonClick
        Select Case CType(ToolStripSplitButtonDrawPad.Tag, DrawTools)
            Case DrawTools.DrawSMD
                SMDPadToolStripMenuItem_Click(sender, e)
            Case DrawTools.DrawThrougHole
                ThroughHolePadToolStripMenuItem_Click(sender, e)
            Case DrawTools.DrawVia
                ViaToolStripMenuItem_Click(sender, e)
            Case DrawTools.DrawRoute
                RouteToolStripMenuItem_Click(sender, e)
            Case DrawTools.DrawJunction
                RouteJunctionToolStripMenuItem_Click(sender, e)
            Case DrawTools.DrawSplit

        End Select
    End Sub

    Private Sub RouteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RouteToolStripMenuItem.Click
        For Each LayerWindow As FrmLayer In LayerWindows
            LayerWindow.ToolStripSplitButtonDrawPad.Image = RouteToolStripMenuItem.Image
            LayerWindow.ToolStripSplitButtonDrawPad.Tag = DrawTools.DrawRoute
        Next
        ResetDrawState()
        SetDrawState(DrawStates.DrawRouteBegin)
        DrawObject = New Route()
        sPCB.Cursor = Cursors.Cross
    End Sub

    Private Sub RouteJunctionToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RouteJunctionToolStripMenuItem.Click
        For Each LayerWindow As FrmLayer In LayerWindows
            LayerWindow.ToolStripSplitButtonDrawPad.Image = RouteJunctionToolStripMenuItem.Image
            LayerWindow.ToolStripSplitButtonDrawPad.Tag = DrawTools.DrawJunction
        Next
        StartDrawObject(New RouteJunction())
    End Sub

    Private Sub SplitRouteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SplitRouteToolStripMenuItem.Click
        For Each LayerWindow As FrmLayer In LayerWindows
            LayerWindow.ToolStripSplitButtonDrawPad.Image = SplitRouteToolStripMenuItem.Image
            LayerWindow.ToolStripSplitButtonDrawPad.Tag = DrawTools.DrawSplit
        Next
        ResetDrawState()
        SetDrawState(DrawStates.SplitRoute)

    End Sub
End Class