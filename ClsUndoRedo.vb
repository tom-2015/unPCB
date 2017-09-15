Public MustInherit Class UndoRedoItem

    Protected m_PCB As PCB
    Protected m_ChangedPCB As Boolean 'is true if we changed the pcb project (if changed are already made by another action, this is false

    Public MustOverride ReadOnly Property Description() As String

    Public Sub New(ByVal PCB As PCB)
        m_ChangedPCB = Not PCB.IsChanged
        If m_ChangedPCB Then PCB.IsChanged = True
        m_PCB = PCB
    End Sub

    Public Overridable Sub Undo()
        If m_PCB.IsChanged = False Then 'project has been saved and we will change it if we undo
            m_ChangedPCB = True
        End If
        If m_ChangedPCB Then
            m_PCB.IsChanged = Not m_PCB.IsChanged
        End If
    End Sub

    Public Overridable Sub Redo()
        If m_PCB.IsChanged = False Then 'project has been saved and we will change it if we redo
            m_ChangedPCB = True
        End If
        If m_ChangedPCB Then
            m_PCB.IsChanged = Not m_PCB.IsChanged
        End If
    End Sub
End Class

''' <summary>
''' Undo / Redo any connection type of 2 or more pads
''' </summary>
''' <remarks></remarks>
Public Class UndoRedoConnect
    Inherits UndoRedoItem

    Dim m_UndoConnections As ConnectionMatrix
    Dim m_RedoConnections As ConnectionMatrix
    Dim m_ByAutoRouter As Boolean

    Public Sub New(ByVal PCB As PCB)
        MyBase.New(PCB)
        m_UndoConnections = PCB.ConnectionMatrix.BackUp()
    End Sub

    ''' <summary>
    ''' If the connection was made by auto router
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="ByAutoRouter"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal PCB As PCB, ByVal ByAutoRouter As Boolean)
        Me.New(PCB)
        m_ByAutoRouter = ByAutoRouter
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "connect pads."
        End Get
    End Property

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_RedoConnections = m_PCB.ConnectionMatrix.BackUp() 'backup before undo
        m_PCB.ConnectionMatrix.Restore(m_UndoConnections)
        If m_ByAutoRouter Then
            If IsFormOpen(GetType(FrmManualRoute)) Then
                CType(GetForm(GetType(FrmManualRoute)), FrmManualRoute).NextCheck()
            End If
        End If
    End Sub

    Public Overrides Sub Redo()
        MyBase.Undo()
        m_PCB.ConnectionMatrix.Restore(m_RedoConnections)
        If m_ByAutoRouter Then
            If IsFormOpen(GetType(FrmManualRoute)) Then
                CType(GetForm(GetType(FrmManualRoute)), FrmManualRoute).NextCheck()
            End If
        End If
    End Sub

End Class

Public Class UndoRedoMovePCBImage
    Inherits UndoRedoItem

    Dim Loc As PointF
    Dim RedoLoc As PointF
    Dim Image As PCBImage

    Public Sub New(ByVal PCB As PCB, ByVal Image As PCBImage, ByVal StartLocation As PointF)
        MyBase.New(PCB)
        Loc = StartLocation
        Me.Image = Image
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "move background"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        Image.Location = New Point(RedoLoc.X, RedoLoc.Y)
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        RedoLoc = Image.Location
        Image.Location = New Point(Loc.X, Loc.Y)
    End Sub
End Class

Public Class UndoRedoAddDevice
    Inherits UndoRedoItem

    Protected Device As Device

    Public Sub New(ByVal PCB As PCB, ByVal Device As Device)
        MyBase.New(PCB)
        Me.Device = Device
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "add device"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_PCB.AddDevice(Device)
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_PCB.RemoveDevice(Device)
    End Sub
End Class

Public Class UndoRedoDeleteDevice
    Inherits UndoRedoItem

    Dim Connections As ConnectionMatrix
    Dim RedoConnections As ConnectionMatrix
    Dim Device As Device 'the device + device pins  + connected pads remain in the memory by this pointer but are removed from the PCB project
    'Dim m_UndoDeletePads As New Stack(Of UndoRedoDeletePad)
    'Dim m_RedoDeletePads As New Stack(Of UndoRedoDeletePad)

    Public Sub New(ByVal PCB As PCB, ByVal Device As Device)
        MyBase.New(PCB)
        Me.Device = Device
        Connections = PCB.ConnectionMatrix.BackUp 'backup the connections

        'For Each Pin As DevicePin In Device.Pins
        '    For Each Pad As Pad In Pin.Pads
        '        m_UndoDeletePads.Push(Pad.GetUndoDeleteItem(PCB))
        '    Next
        'Next

    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "delete device"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()

        'While m_RedoDeletePads.Count > 0
        '    Dim UndoItem As UndoRedoItem = m_RedoDeletePads.Pop()
        '    UndoItem.Redo()
        '    m_UndoDeletePads.Push(UndoItem)
        'End While

        m_PCB.ConnectionMatrix.Restore(RedoConnections)
        m_PCB.RemoveDevice(Device)
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        RedoConnections = m_PCB.ConnectionMatrix.BackUp

        'While m_UndoDeletePads.Count > 0
        '    Dim RedoItem As UndoRedoItem = m_UndoDeletePads.Pop()
        '    RedoItem.Undo()
        '    m_UndoDeletePads.Push(RedoItem)
        'End While

        m_PCB.AddDevice(Device)
        m_PCB.ConnectionMatrix.Restore(Connections)
    End Sub
End Class

Public Class UndoRedoDeviceValueChange
    Inherits UndoRedoItem

    Dim m_Device As Device
    Dim m_OldValue As String
    Dim m_NewValue As String

    Public Sub New(ByVal PCB As PCB, ByVal Device As Device, ByVal OldValue As String, ByVal NewValue As String)
        MyBase.New(PCB)
        m_Device = Device
        m_OldValue = OldValue
        m_NewValue = NewValue
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "change value."
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Device.Value = m_NewValue
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_Device.Value = m_OldValue
    End Sub

End Class

Public Class UndoRedoDeviceNameChange
    Inherits UndoRedoItem

    Dim m_Device As Device
    Dim m_OldName As String
    Dim m_NewName As String


    Public Sub New(ByVal PCB As PCB, ByVal Device As Device, ByVal OldName As String, ByVal NewName As String)
        MyBase.New(PCB)
        m_Device = Device
        m_OldName = OldName
        m_NewName = NewName
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "rename device."
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Device.Name = m_NewName
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_Device.Name = m_OldName
    End Sub

End Class

Public Class UndoRedoNameChange
    Inherits UndoRedoItem

    Dim m_OldName As String
    Dim m_NewName As String
    Dim m_Object As LayerObject

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject, ByVal OldName As String, ByVal NewName As String)
        MyBase.New(PCB)
        m_Object = Obj
        m_OldName = OldName
        m_NewName = NewName
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "change name " & m_OldName
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Object.Name = m_NewName
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_Object.Name = m_OldName
    End Sub
End Class

Public Class UndoRedoMirrorBackgroundImage
    Inherits UndoRedoItem

    Dim m_HorizontalMirror As Boolean
    Dim m_VerticalMirror As Boolean
    Dim m_RedoHorizontalMirror As Boolean
    Dim m_RedoVerticalMirror As Boolean
    Dim m_WindowType As PCB.WindowTypes

    Public Sub New(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        MyBase.New(PCB)
        m_HorizontalMirror = PCB.HorizontalMirror(WindowType)
        m_VerticalMirror = PCB.VerticalMirror(WindowType)
        m_PCB = PCB
        m_WindowType = WindowType
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "flip background"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_PCB.HorizontalMirror(m_WindowType) = m_RedoHorizontalMirror
        m_PCB.VerticalMirror(m_WindowType) = m_RedoVerticalMirror
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_RedoHorizontalMirror = m_PCB.HorizontalMirror(m_WindowType)
        m_RedoVerticalMirror = m_PCB.VerticalMirror(m_WindowType)
        m_PCB.HorizontalMirror(m_WindowType) = m_HorizontalMirror
        m_PCB.VerticalMirror(m_WindowType) = m_VerticalMirror
    End Sub
End Class

Public Class UndoRedoResizeLayerObject
    Inherits UndoRedoItem

    Dim m_OriginalLocation As RectangleF
    Dim m_NewLocation As RectangleF
    Dim m_LayerObject As SelectableLayerObject

    Public Sub New(ByVal PCB As PCB, ByVal LayerObject As SelectableLayerObject, ByVal OriginalLocation As RectangleF)
        MyBase.New(PCB)
        m_LayerObject = LayerObject
        m_PCB = PCB
        m_OriginalLocation = OriginalLocation
        m_NewLocation = LayerObject.Rect
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "resize"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_LayerObject.Location = m_NewLocation.Location
        m_LayerObject.Width = m_NewLocation.Width
        m_LayerObject.Height = m_NewLocation.Height
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_LayerObject.Location = m_OriginalLocation.Location
        m_LayerObject.Width = m_OriginalLocation.Width
        m_LayerObject.Height = m_OriginalLocation.Height
    End Sub
End Class

Public Class UndoRedoMoveLayerObject
    Inherits UndoRedoItem

    Private Structure UndoMoveObjectItem
        Public LayerObject As SelectableLayerObject
        Public OriginalCenterLocation As PointF
        Public NewCenterLocation As PointF
        Public Sub New(ByVal LayerObject As SelectableLayerObject)
            Me.LayerObject = LayerObject
            Me.OriginalCenterLocation = LayerObject.StartCenter
            Me.NewCenterLocation = LayerObject.Center
        End Sub
    End Structure

    Dim m_MoveItems As New List(Of UndoMoveObjectItem)

    Public Sub New(ByVal PCB As PCB, ByVal LayerObjects As ReadOnlyDictionary(Of Integer, SelectableLayerObject))
        MyBase.New(PCB)
        For Each LayerObject As KeyValuePair(Of Integer, SelectableLayerObject) In LayerObjects
            m_MoveItems.Add(New UndoMoveObjectItem(LayerObject.Value)) 'back up the start position
        Next
        m_PCB = PCB
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Move objects"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        For Each MoveItem As UndoMoveObjectItem In m_MoveItems
            MoveItem.LayerObject.Center = MoveItem.NewCenterLocation
        Next
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        For Each MoveItem As UndoMoveObjectItem In m_MoveItems
            MoveItem.LayerObject.Center = MoveItem.OriginalCenterLocation
        Next
    End Sub
End Class

Public Class UndoRedoDisconnectPin
    Inherits UndoRedoItem

    Protected DevicePin As DevicePin
    Protected Pad As Pad
    Protected EaglePadName As String
    Protected PadName As String
    Protected RedoPadName As String
    Protected RedoDevicePin As DevicePin
    Protected RedoEaglePadName As String

    Public Sub New(ByVal PCB As PCB, ByVal DevicePin As DevicePin, ByVal Pad As Pad)
        MyBase.New(PCB)
        Me.DevicePin = DevicePin
        Me.Pad = Pad
        Me.PadName = Pad.Name
        Me.EaglePadName = Pad.EaglePadName
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Disconnect pin and pad"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        DevicePin.RemovePad(Pad)
        Pad.DevicePin = RedoDevicePin
        If RedoDevicePin IsNot Nothing Then RedoDevicePin.AddPad(Pad, RedoEaglePadName)
        Pad.Name = RedoPadName
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        RedoDevicePin = Pad.DevicePin
        RedoEaglePadName = Pad.EaglePadName
        If RedoDevicePin IsNot Nothing Then RedoDevicePin.RemovePad(Pad)
        Pad.DevicePin = DevicePin
        DevicePin.AddPad(Pad, EaglePadName)
        RedoPadName = Pad.Name
        Pad.Name = PadName
    End Sub
End Class

Public Class UndoRedoConnectPin
    Inherits UndoRedoItem

    Protected DevicePin As DevicePin
    Protected Pad As Pad
    Protected EaglePadName As String
    Protected PadName As String
    Protected RedoPadName As String
    Protected RedoEaglePadName As String

    Public Sub New(ByVal PCB As PCB, ByVal DevicePin As DevicePin, ByVal Pad As Pad)
        MyBase.New(PCB)
        Me.DevicePin = DevicePin
        Me.Pad = Pad
        Me.PadName = Pad.Name
        Me.EaglePadName = Pad.EaglePadName
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "connect pin and pad"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        Pad.Name = RedoPadName
        DevicePin.AddPad(Pad, RedoEaglePadName)
        Pad.DevicePin = DevicePin
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        RedoEaglePadName = Pad.EaglePadName
        DevicePin.RemovePad(Pad)
        Pad.DevicePin = Nothing
        RedoPadName = Pad.Name
        Pad.Name = PadName
        Pad.EaglePadName = EaglePadName
    End Sub
End Class

Public Class UndoRedoAddPad
    Inherits UndoRedoAddObject

    Protected DevicePin As DevicePin
    Protected RedoEaglePadName As String
    Protected EaglePadName As String

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB, Obj)
        EaglePadName = CType(Obj, Pad).EaglePadName
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "add pad"
        End Get
    End Property

    Public Overrides Sub Redo()
        If DevicePin IsNot Nothing Then
            CType(Obj, Pad).DevicePin = DevicePin
            DevicePin.AddPad(Obj, RedoEaglePadName)
        End If
        MyBase.Redo()
    End Sub

    Public Overrides Sub Undo()
        DevicePin = CType(Obj, Pad).DevicePin
        RedoEaglePadName = CType(Obj, Pad).EaglePadName
        CType(Obj, Pad).EaglePadName = EaglePadName
        MyBase.Undo()
    End Sub

End Class

Public Class UndoRedoAddObject
    Inherits UndoRedoItem

    Protected Obj As LayerObject
    Protected OnLayers As New List(Of Layer)

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB)
        Me.Obj = Obj
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "add object"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_PCB.AddObject(Obj)
        For Each Layer As Layer In OnLayers
            Layer.AddObject(Obj)
        Next
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        'remember the layers the object is placed on
        OnLayers.Clear()
        For Each Layer As KeyValuePair(Of PCB.LayerTypes, Layer) In m_PCB.Layers
            If Layer.Value.LayerObjects.Contains(Obj) Then
                OnLayers.Add(Layer.Value)
            End If
        Next
        m_PCB.RemoveObject(Obj)
    End Sub
End Class

Public Class UndoRedoDeletePad
    Inherits UndoRedoDeleteObject

    Dim Connections As ConnectionMatrix
    Dim RedoConnections As ConnectionMatrix
    Dim DevicePin As DevicePin
    Dim m_UndoDeleteRoutes As New Stack(Of UndoRedoDeleteRoute)
    Dim m_RedoDeleteRoutes As New Stack(Of UndoRedoDeleteRoute)

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB, Obj)
        Connections = PCB.ConnectionMatrix.BackUp
        DevicePin = CType(Obj, Pad).DevicePin
        For Each Route As Route In CType(Obj, Pad).Routes
            m_UndoDeleteRoutes.Push(Route.GetUndoDeleteItem(PCB, False))
        Next
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "delete pad"
        End Get
    End Property

    Public Overrides Sub Undo()
        RedoConnections = m_PCB.ConnectionMatrix.BackUp
        MyBase.Undo()
        If DevicePin IsNot Nothing Then
            DevicePin.Pads.Remove(Obj)
        End If
        While m_UndoDeleteRoutes.Count > 0
            Dim RedoItem As UndoRedoItem = m_UndoDeleteRoutes.Pop()
            RedoItem.Undo()
            m_RedoDeleteRoutes.Push(RedoItem)
        End While
        m_PCB.ConnectionMatrix.Restore(Connections) 'restore the connection matrix
    End Sub

    Public Overrides Sub Redo()
        MyBase.Redo()

        m_PCB.RemoveObject(Obj)
        If DevicePin IsNot Nothing Then
            DevicePin.Pads.Add(Obj)
        End If
        While m_RedoDeleteRoutes.Count > 0
            Dim UndoItem As UndoRedoItem = m_RedoDeleteRoutes.Pop()
            UndoItem.Redo()
            m_UndoDeleteRoutes.Push(UndoItem)
        End While
        m_PCB.ConnectionMatrix.Restore(RedoConnections) 'restore the connections from before the undo
    End Sub

End Class

Public Class UndoRedoDeleteObject
    Inherits UndoRedoItem

    Protected Obj As LayerObject
    Protected OnLayers As New List(Of Layer)

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB)
        Me.Obj = Obj
        'save on which layer the object was displayed
        For Each Layer As KeyValuePair(Of PCB.LayerTypes, Layer) In PCB.Layers
            If Layer.Value.LayerObjects.Contains(Obj) Then
                OnLayers.Add(Layer.Value)
            End If
        Next
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "delete object"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_PCB.RemoveObject(Obj)
        For Each layer As Layer In OnLayers
            layer.LayerObjects.Remove(Obj)
        Next
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        m_PCB.AddObject(Obj)
        For Each Layer As Layer In OnLayers
            Layer.AddObject(Obj)
        Next
    End Sub
End Class

Public Class UndoRedoRemoveLibrary
    Inherits UndoRedoItem

    Dim m_Library As Eagle.Project

    Public Sub New(ByVal PCB As PCB, ByVal Library As Eagle.Project)
        MyBase.New(PCB)
        m_Library = Library
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Remove Eagle library"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_PCB.RemoveEagleLibrary(m_Library.Drawing.Library.Name)
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        If Not m_PCB.AddEagleLibrary(m_Library) Then
            MsgBox("Cannot undo remove Eagle Library because another library with the same name is already added!")
        End If
    End Sub
End Class

Public Class UndoRedoTextChange
    Inherits UndoRedoItem

    Dim m_Font As Font
    Dim m_Text As String
    Dim m_Textbox As TextBox
    Dim m_RedoFont As Font
    Dim m_RedoText As String

    Public Sub New(ByVal PCB As PCB, ByVal TextObject As TextBox)
        MyBase.New(PCB)
        m_Textbox = TextObject
        m_Text = TextObject.Text
        m_Font = TextObject.Font.Clone()
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Change text"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Textbox.Font = m_RedoFont
        m_Textbox.Text = m_RedoText
    End Sub

    Public Overrides Sub Undo()
        m_RedoFont = m_Textbox.Font
        m_RedoText = m_Textbox.Text
        MyBase.Undo()
        m_Textbox.Text = m_Text
        m_Textbox.Font = m_Font
    End Sub

End Class

Public Class UndoRedoChangeColor
    Inherits UndoRedoItem

    Dim m_Color As Color
    Dim m_RedoColor As Color
    Dim m_Object As SelectableLayerObject

    Public Sub New(ByVal PCB As PCB, ByVal LayerObject As SelectableLayerObject)
        MyBase.New(PCB)
        m_Color = LayerObject.color
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Change color"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Object.Color = m_RedoColor
    End Sub

    Public Overrides Sub Undo()
        m_RedoColor = m_Object.Color
        MyBase.Undo()
        m_Object.Color = m_Color
    End Sub

End Class

Public Class UndoRedoChangeRotation
    Inherits UndoRedoItem

    Dim m_Rotation As Single
    Dim m_RedoRotation As Single
    Dim m_Object As LayerObject

    Public Sub New(ByVal PCB As PCB, ByVal LayerObject As LayerObject)
        MyBase.New(PCB)
        m_Rotation = LayerObject.Rotation
        m_Object = LayerObject
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Change color"
        End Get
    End Property

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Object.Rotation = m_RedoRotation
    End Sub

    Public Overrides Sub Undo()
        m_RedoRotation = m_Object.Rotation
        MyBase.Undo()
        m_Object.Rotation = m_Rotation
    End Sub

End Class


Public Class UndoRedoMoveSchematicInstance
    Inherits UndoRedoItem

    Dim m_Instance As SchematicInstance
    Dim m_Location As PointF
    Dim m_RedoLocation As PointF
    Dim m_Rotation As Single
    Dim m_RedoRotation As Single

    Public Sub New(ByVal PCB As PCB, ByVal Instance As SchematicInstance)
        MyBase.New(PCB)
        m_Instance = Instance
        m_Location = Instance.StartLocation
        m_Rotation = Instance.StartRotation
    End Sub


    Public Overrides Sub Redo()
        MyBase.Redo()
        m_Instance.Location = m_RedoLocation
    End Sub

    Public Overrides Sub Undo()
        m_RedoLocation = m_Instance.Location
        m_RedoRotation = m_Instance.Rotation
        MyBase.Undo()
        m_Instance.Location = m_Location
        m_Instance.Rotation = m_Rotation
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Move item on schematic."
        End Get
    End Property
End Class

Public Class UndoRedoDeleteRoutePoint
    Inherits UndoRedoDeleteObject

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB, Obj)
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Delete Route Point"
        End Get
    End Property

End Class

Public Class UndoRedoDeleteRoute
    Inherits UndoRedoDeleteObject

    Dim Connections As ConnectionMatrix
    Dim RedoConnections As ConnectionMatrix
    Dim m_BackupConnections As Boolean

    Protected m_UndoRoutePointsStack As New Stack(Of UndoRedoDeleteRoutePoint)
    Protected m_RedoRoutePointsStack As New Stack(Of UndoRedoDeleteRoutePoint)

    Public Sub New(ByVal PCB As PCB, ByVal Obj As Route)
        Me.New(PCB, Obj, True)
    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal Obj As Route, ByVal BackupConnections As Boolean)
        MyBase.New(PCB, Obj)
        If (BackupConnections) Then
            Connections = PCB.ConnectionMatrix.BackUp
        End If
        m_BackupConnections = BackupConnections

        Dim Route As Route = CType(Obj, Route)
        For Each RoutePoint As RoutePoint In Route.RoutePoints
            m_UndoRoutePointsStack.Push(RoutePoint.GetUndoDeleteItem(PCB))
        Next
    End Sub

    Public Overrides Sub Redo()
        MyBase.Redo()
        If m_BackupConnections Then
            m_PCB.ConnectionMatrix.Restore(RedoConnections)
        End If
        While m_RedoRoutePointsStack.Count > 0
            Dim UndoItem As UndoRedoItem = m_RedoRoutePointsStack.Pop()
            UndoItem.Redo()
            m_UndoRoutePointsStack.Push(UndoItem)
        End While
    End Sub

    Public Overrides Sub Undo()
        MyBase.Undo()
        If m_BackupConnections Then RedoConnections = m_PCB.ConnectionMatrix.BackUp()
        While m_UndoRoutePointsStack.Count > 0
            Dim RedoItem As UndoRedoItem = m_UndoRoutePointsStack.Pop()
            RedoItem.Undo()
            m_RedoRoutePointsStack.Push(RedoItem)
        End While
        If m_BackupConnections Then
            m_PCB.ConnectionMatrix.Restore(Connections)
        End If
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Delete Route"
        End Get
    End Property
End Class

Public Class UndoRedoAddRoutePoint
    Inherits UndoRedoAddObject

    Public Sub New(ByVal PCB As PCB, ByVal RoutePoint As RoutePoint)
        MyBase.New(PCB, RoutePoint)
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Add Route point."
        End Get
    End Property

End Class

''' <summary>
''' Undo / Redo drawing route point
''' </summary>
''' <remarks></remarks>
Public Class UndoRedoAddRoute
    Inherits UndoRedoAddObject

    Dim m_UndoConnections As ConnectionMatrix
    Dim m_RedoConnections As ConnectionMatrix
    Protected m_UndoRoutePointsStack As New Stack(Of UndoRedoAddRoutePoint)
    Protected m_RedoRoutePointsStack As New Stack(Of UndoRedoAddRoutePoint)

    Public Sub New(ByVal PCB As PCB, ByVal Route As Route)
        MyBase.New(PCB, Route)
        m_UndoConnections = PCB.ConnectionMatrix.BackUp()
        For i As Integer = Route.RoutePoints.Count - 1 To 0 Step -1
            m_UndoRoutePointsStack.Push(Route.RoutePoints(i).GetUndoAddItem(PCB))
        Next
        'For Each RoutePoint As RoutePoint In Route.RoutePoints

        'Next
    End Sub

    Public Overrides Sub Undo()
        m_RedoConnections = m_PCB.ConnectionMatrix.BackUp()
        While m_UndoRoutePointsStack.Count > 0
            Dim RedoItem As UndoRedoItem = m_UndoRoutePointsStack.Pop()
            RedoItem.Undo()
            m_RedoRoutePointsStack.Push(RedoItem)
        End While
        MyBase.Undo()
        m_PCB.ConnectionMatrix.Restore(m_UndoConnections)
    End Sub

    Public Overrides Sub Redo()
        MyBase.Redo()
        m_PCB.ConnectionMatrix.Restore(m_RedoConnections)
        While m_RedoRoutePointsStack.Count > 0
            Dim UndoItem As UndoRedoItem = m_RedoRoutePointsStack.Pop()
            UndoItem.Redo()
            m_UndoRoutePointsStack.Push(UndoItem)
        End While
    End Sub

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "Draw Route."
        End Get
    End Property

End Class