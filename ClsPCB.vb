Imports System.Collections.ObjectModel
Imports System.IO

Public Class PCB

    Private Class EagleLibraryComparer
        Implements IComparer(Of Eagle.Project)

        Public Function Compare(ByVal x As Eagle.Project, ByVal y As Eagle.Project) As Integer Implements System.Collections.Generic.IComparer(Of Eagle.Project).Compare
            Return StrComp(x.ShortFileName, y.ShortFileName, CompareMethod.Text)
        End Function
    End Class

    Public Structure WindowSetting
        Dim HorizontalMirror As Boolean
        Dim VerticalMirror As Boolean
    End Structure

    Public Enum WindowTypes As Integer
        WindowTypeTop = 0
        WindowTypeBottom = 1
    End Enum

    Public Enum LayerTypes As Integer
        LayerTypeTopImage = 0
        LayerTypeBottomImage = 1
        LayerTypeTopPads = 2
        LayerTypeBottomPads = 3
        LayerTypeTopInfo = 4
        LayerTypeBottomInfo = 5
        LayerTypeTopDevice = 6
        LayerTypeBottomDevice = 7
        LayerTypeTopDrawing = 8 'these m_layers are used for temporary drawing some objects for animation of the cursor
        LayerTypeBottomDrawing = 9
        LayerTypeTopVias = 10
        LayerTypeBottomVias = 11
        LayerTypeTopRoute = 12
        LayerTypeBottomRoute = 13
    End Enum

    Public Const MAX_UNDOREDO_SIZE As Integer = 30

    Protected m_Layers As New ExtentedDictionary(Of LayerTypes, Layer) 'holds the layers
    Protected m_LayerObjects As New ExtentedDictionary(Of Integer, LayerObject) 'all objects on the m_layers
    Protected m_Devices As New ExtentedDictionary(Of String, Device) 'all Devices (ICs, resistors)
    Protected m_Libraries As New List(Of Eagle.Project) 'loaded eagle m_Libraries
    Protected m_ConnectionMatrix As New ConnectionMatrix() 'connections between pads
    Protected m_UndoStack As New LinkedList(Of UndoRedoItem) 'all things that can be undone
    Protected m_RedoStack As New Stack(Of UndoRedoItem)(MAX_UNDOREDO_SIZE) 'all things that have been undone and can now be redone
    Protected m_Schematic As PCBSchematic

    Protected m_SelectedObjects As New ExtentedDictionary(Of Integer, SelectableLayerObject) 'all objects that are selected at this moment
    Protected m_HighLightedObjects As New ExtentedDictionary(Of Integer, SelectableLayerObject) 'all objects that are highlighted at this moment
    Protected m_Width As Single 'width of the PCB in pixels
    Protected m_Height As Single 'height of the PCB in pixels, the images must have both the same size
    Protected m_Name As String 'name of the project
    Protected m_Cursor As System.Windows.Forms.Cursor
    Protected m_WindowSettings() As WindowSetting
    Protected m_isChanged As Boolean
    Protected m_FileName As String

    Public Event SizeChanged(ByVal Sender As PCB, ByVal Width As Single, ByVal Height As Single) 'fired when size of pcb has changed
    Public Event UpdateGraphics(ByVal Sender As PCB, ByVal WindowType As WindowTypes) 'fired when the graphics must be updated on a window
    Public Event ChangeCursor(ByVal Sender As PCB, ByVal Cursor As System.Windows.Forms.Cursor)
    Public Event DeviceAdded(ByVal Sender As PCB, ByVal Device As Device)
    Public Event DeviceRemoved(ByVal Sender As PCB, ByVal Device As Device)
    Public Event NameChanged(ByVal Sender As PCB, ByVal Name As String)
    Public Event UndoRedoAction(ByVal Sender As PCB, ByVal UndoRedoItem As UndoRedoItem, ByVal Undo As Boolean) 'raised when something is undone / redone, if undone the undo is set to true
    Public Event UndoRedoStackUpdate(ByVal Sender As PCB, ByVal UndoStack As LinkedList(Of UndoRedoItem), ByVal RedoStack As Stack(Of UndoRedoItem)) 'if the undo/redo stack was updated
    Public Event ProjectChanged(ByVal Sender As PCB) 'fired if any changed made
    Public Event ProjectLoaded(ByVal Sender As PCB, ByVal ZipFile As Ionic.Zip.ZipFile)
    Public Event ProjectSaved(ByVal Sender As PCB, ByVal ZipFile As Ionic.Zip.ZipFile)
    Public Event ObjectsSelected(ByVal Sender As PCB, ByVal Objects As List(Of SelectableLayerObject))
    Public Event ObjectsDeselected(ByVal Sender As PCB, ByVal Objects As List(Of SelectableLayerObject))
    Public Event ObjectsHighlighted(ByVal Sender As PCB, ByVal Objects As List(Of SelectableLayerObject))
    Public Event ObjectsDeHighlighted(ByVal Sender As PCB, ByVal Objects As List(Of SelectableLayerObject))
    Public Event ObjectsLoaded(ByVal Sender As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile) 'fired after loading of document completed and all objects have been assigned
    Public Event BackgroundImageMirrorChanged(ByVal Sender As PCB, ByVal WIndowType As WindowTypes, ByVal VerticalMirrorChanged As Boolean)
    Public Event LayerVisibilityChanged(ByVal Sender As PCB, ByVal Layer As Layer, ByVal Visible As Boolean)

    'sets up a new unpcb project, creates top and bottom window and adds some default m_layers to them
    Public Sub New()
        Dim Layer_Types As Array = System.Enum.GetValues(GetType(LayerTypes))
        Dim Layer_Type As LayerTypes

        m_Schematic = New PCBSchematic(Me)
        For Each Layer_Type In Layer_Types
            Dim Layer As Layer = New Layer(Layer_Type)
            m_Layers.Add(Layer_Type, Layer)
            AddHandler Layer.LayerVisibilityChanged, AddressOf LayerVisibilityHasChanged
        Next

        ReDim m_WindowSettings(0 To 1)
        IsChanged = False
    End Sub

    Protected Sub LayerVisibilityHasChanged(ByVal Layer As Layer, ByVal Visible As Boolean)
        RaiseEvent LayerVisibilityChanged(Me, Layer, Visible)
    End Sub

    ''' <summary>
    ''' Returns the last saved file name, empty if not yet saved
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FileName() As String
        Get
            Return m_FileName
        End Get
    End Property

    ''' <summary>
    ''' Returns if this PCB project was changed
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsChanged() As Boolean
        Get
            Return m_isChanged
        End Get
        Set(ByVal value As Boolean)
            m_isChanged = value
            RaiseEvent ProjectChanged(Me)
        End Set
    End Property

    ''' <summary>
    ''' Returns all the layers as a dictionary
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Layers() As ReadOnlyDictionary(Of LayerTypes, Layer)
        Get
            Return m_Layers.GetReadonlyDictionary()
        End Get
    End Property

    ''' <summary>
    ''' Returns all the objects which can be located at one or more layers
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property LayerObjects() As ReadOnlyDictionary(Of Integer, LayerObject)
        Get
            Return m_LayerObjects.GetReadonlyDictionary()
        End Get
    End Property

    ''' <summary>
    ''' Returns all the devices that were soldered on the PCB
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Devices() As ReadOnlyDictionary(Of String, Device)
        Get
            Return m_Devices.GetReadonlyDictionary()
        End Get
    End Property

    ''' <summary>
    ''' Returns all eagle libraries
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Libraries() As ReadOnlyCollection(Of Eagle.Project)
        Get
            Return m_Libraries.AsReadOnly()
        End Get
    End Property

    ''' <summary>
    ''' Returns the connections between pads and vias
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ConnectionMatrix() As ConnectionMatrix
        Get
            Return m_ConnectionMatrix
        End Get
    End Property

    ''' <summary>
    ''' Returns all undo actions
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UndoStack() As LinkedList(Of UndoRedoItem)
        Get
            Return m_UndoStack
        End Get
    End Property

    ''' <summary>
    ''' Returns all redo actions
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property RedoStack() As Stack(Of UndoRedoItem)
        Get
            Return m_RedoStack
        End Get
    End Property

    ''' <summary>
    ''' Returns the schematic
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Schematic() As PCBSchematic
        Get
            Return m_Schematic
        End Get
    End Property


    ''' <summary>
    ''' Gets / sets the project name
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            m_Name = value
            RaiseEvent NameChanged(Me, value)
        End Set
    End Property

    ''' <summary>
    ''' Returns the width of the processed PCB image
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Width() As Long
        Get
            Return m_Width
        End Get
        Set(ByVal value As Long)
            m_Width = value
            RaiseEvent SizeChanged(Me, m_Width, m_Height)
        End Set
    End Property

    ''' <summary>
    ''' Returns the height in pixels of the processed PCB image
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Height() As Long
        Get
            Return m_Height
        End Get
        Set(ByVal value As Long)
            m_Height = value
            RaiseEvent SizeChanged(Me, m_Width, m_Height)
        End Set
    End Property

    ''' <summary>
    ''' Returns the window type for a given layertype (says if the layer is on bottom or top layer window)
    ''' </summary>
    ''' <param name="LayerType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetWindowTypeOfLayerType(ByVal LayerType As LayerTypes) As WindowTypes
        If (LayerType Mod 2) <> 0 Then
            Return WindowTypes.WindowTypeBottom
        Else
            Return WindowTypes.WindowTypeTop
        End If
    End Function

    ''' <summary>
    ''' Returns a layer based on it's layer type
    ''' </summary>
    ''' <param name="LayerType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetLayer(ByVal LayerType As LayerTypes) As Layer
        Return m_Layers(LayerType)
    End Function

    ''' <summary>
    ''' Returns a layerobject based on it's ID
    ''' </summary>
    ''' <param name="Id"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetLayerObject(ByVal Id As Integer) As LayerObject
        If m_LayerObjects.ContainsKey(Id) Then
            Return m_LayerObjects(Id)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Returns the layer object by it's name
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns>LayerObject</returns>
    ''' <remarks>Returns nothing if name doesn't exist</remarks>
    Public Function GetLayerObject(ByVal Name As String) As LayerObject
        For Each LayerObject As KeyValuePair(Of Integer, LayerObject) In m_LayerObjects
            If StrComp(LayerObject.Value.Name, Name, CompareMethod.Text) = 0 Then
                Return LayerObject.Value
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Returns if an object with Name already exists in the collection
    ''' </summary>
    ''' <param name="Name">The name to check</param>
    ''' <param name="ExcludeId">Exclude this object id from the search</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LayerObjectNameExists(ByVal Name As String, Optional ByVal ExcludeId As Integer = -1) As Boolean
        Dim Obj As LayerObject = GetLayerObject(Name)
        If Obj IsNot Nothing Then
            Return Obj.id <> ExcludeId
        End If
        Return False
    End Function

    ''' <summary>
    ''' Returns all the currently selected layer objects
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetSelectedLayerObjects() As ReadOnlyDictionary(Of Integer, SelectableLayerObject)
        Return m_SelectedObjects.GetReadonlyDictionary()
    End Function

    ''' <summary>
    ''' Returns all the currently selected layer objects of a certain object type
    ''' </summary>
    ''' <param name="ObjectType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetSelectedLayerObjects(ByVal ObjectType As Type) As List(Of SelectableLayerObject)
        Dim SelectedObjects As New List(Of SelectableLayerObject)

        For Each LayerObject As KeyValuePair(Of Integer, LayerObject) In m_LayerObjects
            If LayerObject.Value.GetType().IsSubclassOf(ObjectType) Then
                If CType(LayerObject.Value, SelectableLayerObject).Selected Then
                    SelectedObjects.Add(LayerObject.Value)
                End If
            End If
        Next
        Return SelectedObjects
    End Function

    ''' <summary>
    ''' Connects all selected pads
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ConnectSelectedPads()
        Dim Selected As List(Of SelectableLayerObject) = GetSelectedLayerObjects(GetType(Pad))
        m_ConnectionMatrix.ConnectPads(Array.ConvertAll(Selected.ToArray(), New Converter(Of SelectableLayerObject, Pad)(AddressOf CastPad)))
        DeselectAllObjects()
    End Sub

    ''' <summary>
    ''' Disconnects all selected pads
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DisconnectSelectedPads()
        Dim Selected As List(Of SelectableLayerObject) = GetSelectedLayerObjects(GetType(Pad))
        m_ConnectionMatrix.DisconnectPads(Array.ConvertAll(Selected.ToArray(), New Converter(Of SelectableLayerObject, Pad)(AddressOf CastPad)))
        DeselectAllObjects()
    End Sub

    ''' <summary>
    ''' Selected pads will never be connected
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub NOTConnectSelectedPads()
        Dim Selected As List(Of SelectableLayerObject) = GetSelectedLayerObjects(GetType(Pad))
        m_ConnectionMatrix.NotConnectPads(Array.ConvertAll(Selected.ToArray(), New Converter(Of SelectableLayerObject, Pad)(AddressOf CastPad)))
        DeselectAllObjects()
    End Sub

    ''' <summary>
    ''' The selected pad(s) will never be connected to unselected pads which have unknown connection state
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub NOTConnectSelectedPadsToAllPads()
        Dim Selected As List(Of SelectableLayerObject) = GetSelectedLayerObjects(GetType(Pad))
        If Selected.Count > 0 Then
            Dim PPad As Pad = Nothing
            For Each SelectedObject As SelectableLayerObject In Selected
                If PPad IsNot Nothing Then
                    m_ConnectionMatrix.ConnectPads(PPad, CType(SelectedObject, Pad))
                End If
                PPad = CType(SelectedObject, Pad)
            Next
            m_ConnectionMatrix.NotConnectToAllPads(CType(Selected(0), Pad))
        End If
        DeselectAllObjects()
    End Sub

    ''' <summary>
    ''' Creates undo information and removes object from the project
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Public Sub DeleteObject(ByVal LayerObject As LayerObject)
        AddUndoItem(LayerObject.GetUndoDeleteItem(Me))
        RemoveObject(LayerObject)
    End Sub

    ''' <summary>
    ''' Removes object from the project without saving undo information
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Public Sub RemoveObject(ByVal LayerObject As LayerObject)
        LayerObject.Remove()
        m_LayerObjects.Remove(LayerObject.id)
    End Sub

    ''' <summary>
    ''' Returns eagle library project by it's name
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetEagleLibraryByName(ByVal Name As String) As Eagle.Project
        For Each Library As Eagle.Project In m_Libraries
            If Library.Drawing.Library.Name = Name Then
                Return Library
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Returns an eagle project with ShortFileName
    ''' </summary>
    ''' <param name="ShortFileName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetEagleLibrary(ByVal ShortFileName As String) As Eagle.Project
        For Each Library As Eagle.Project In m_Libraries
            If Library.ShortFileName = ShortFileName Then
                Return Library
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Adds an eagle library to the collection
    ''' </summary>
    ''' <param name="Library"></param>
    ''' <returns>True if success</returns>
    ''' <remarks></remarks>
    Public Function AddEagleLibrary(ByVal Library As Eagle.Project)
        If GetEagleLibrary(Library.ShortFileName) Is Nothing Then
            m_Libraries.Add(Library)
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Sorts the Eagle libraries alphabetically
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SortEagleLibraries()
        m_Libraries.Sort(New EagleLibraryComparer)
    End Sub

    ''' <summary>
    ''' Loads a new Eagle library
    ''' </summary>
    ''' <param name="FileName"></param>
    ''' <remarks></remarks>
    Public Function LoadEagleLibrary(ByVal FileName As String) As Boolean
        Dim Project As New Eagle.Project()
        If Project.Load(FileName) Then
            Return AddEagleLibrary(Project)
        End If
        Return False
    End Function

    ''' <summary>
    ''' Tries to unload an eagle library, returns false if library is in use
    ''' </summary>
    ''' <param name="ShortFilename"></param>
    ''' <returns></returns>
    ''' <remarks>must save undo item when removing, because the library may still be used by removed devices in the undo/redo list</remarks>
    Public Function RemoveEagleLibrary(ByVal ShortFilename As String) As Boolean
        Dim Library As Eagle.Project = GetEagleLibrary(ShortFilename)
        If Not IsEagleLibraryInUse(Library) Then
            m_Libraries.Remove(Library)
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Returns true if a library is still in use in this project
    ''' </summary>
    ''' <param name="Library"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsEagleLibraryInUse(ByVal Library As Eagle.Project) As Boolean
        If Library IsNot Nothing Then
            For Each Device As KeyValuePair(Of String, Device) In m_Devices
                If Device.Value.EagleDevice.DeviceSet.Library.Drawing.Project.Equals(Library) Then
                    Return True
                End If
            Next
        End If
        Return False
    End Function

    ''' <summary>
    ''' Tries to update a used eagle library
    ''' </summary>
    ''' <param name="FileName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function UpdateEagleLibrary(ByVal FileName As String) As Boolean
        MsgBox("To be implemented")
    End Function

    'Called when an objects name was changed
    Private Sub ObjectNameChanged(ByVal Sender As LayerObject, ByVal OldName As String, ByRef NewName As String)
        If OldName <> NewName Then
            If LayerObjectNameExists(NewName) Then
                Throw New NameExistsException("An object with this name already exists!")
            End If
        End If
    End Sub

    ''' <summary>
    ''' Adds a layer object to the object collection without placing it on one of the m_layers
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Public Function AddObject(ByVal LayerObject As LayerObject) As LayerObject
        If LayerObjectNameExists(LayerObject.Name) Then
            Throw New NameExistsException("An object with this name already exists!")
        End If
        m_LayerObjects.Add(LayerObject.id, LayerObject)
        LayerObject.AddToPCB(Me)
        AddHandler LayerObject.NameChanged, AddressOf ObjectNameChanged
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        Return LayerObject
    End Function

    ''' <summary>
    ''' Places / adds a layerobject on a window at it's current location (depending on the window type it will be placed on its top or bottom layer)
    ''' </summary>
    ''' <param name="Obj"></param>
    ''' <param name="WindowType"></param>
    ''' <remarks></remarks>
    Public Sub PlaceObject(ByVal Obj As LayerObject, ByVal WindowType As PCB.WindowTypes)
        AddUndoItem(Obj.GetUndoAddItem(Me))
        Obj.PlaceObject(Me, WindowType)
        AddObject(Obj)
    End Sub

    ''' <summary>
    ''' Places / adds a layerobject on a window at Location (depending on the window type it will be placed on its top or bottom layer) fires updategraphics event
    ''' </summary>
    ''' <param name="Obj"></param>
    ''' <param name="WindowType"></param>
    ''' <param name="Location"></param>
    ''' <remarks>Also saves undo item</remarks>
    Public Sub PlaceObject(ByVal Obj As LayerObject, ByVal WindowType As PCB.WindowTypes, ByVal Location As Point)
        AddUndoItem(Obj.GetUndoAddItem(Me))
        Obj.PlaceObject(Me, WindowType, Location)
        AddObject(Obj)
    End Sub

    ''' <summary>
    ''' Checks is a device with Name already exists in the collection
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DeviceNameExists(ByVal Name As String) As Boolean
        Return m_Devices.ContainsKey(Name)
    End Function

    ''' <summary>
    ''' Adds a device to the m_Devices collection and fires an event that the list has changed
    ''' If the device has any pins / pads they will also be added to the project if not yet added
    ''' </summary>
    ''' <param name="Device"></param>
    ''' <remarks></remarks>
    Public Function AddDevice(ByVal Device As Device) As Device
        If DeviceNameExists(Device.Name) Then
            Throw New NameExistsException("A device with this name already exists.")
        End If
        m_Devices.Add(Device.Name, Device)
        For Each DevicePin As DevicePin In Device.Pins
            For Each Pad As Pad In DevicePin.Pads
                If GetLayerObject(Pad.id) Is Nothing Then
                    PlaceObject(Pad, Pad.WindowType) 'adds the pad and places it back on the correct layer
                End If
                For Each Route As Route In Pad.Routes
                    If GetLayerObject(Route.id) Is Nothing Then
                        PlaceObject(Route, Route.WindowType) 'add all routes connect to the pads to the correct layer
                    End If
                    'Route.StartPad.Routes.Add(Route)
                    'Route.EndPad.Routes.Add(Route)
                    For Each RoutePoint As RoutePoint In Route.RoutePoints
                        If GetLayerObject(RoutePoint.id) Is Nothing Then
                            PlaceObject(RoutePoint, RoutePoint.WindowType)
                        End If
                    Next
                Next
            Next
        Next
        AddHandler Device.NameChanged, AddressOf DeviceNameChanged
        RaiseEvent DeviceAdded(Me, Device)
        Return Device
    End Function

    'called when the device name is changed
    Private Sub DeviceNameChanged(ByVal Sender As Device, ByVal OldName As String, ByRef NewName As String)
        If OldName <> NewName Then
            If m_Devices.ContainsKey(NewName) Then
                Throw New NameExistsException("The new device name already exists")
            Else
                m_Devices.Remove(OldName)
                m_Devices.Add(NewName, Sender)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Returns a new unique device name for a certain prefix
    ''' </summary>
    ''' <param name="Prefix"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetUniqueDeviceName(ByVal Prefix As String)
        Dim Cnt As Integer = 1
        If Prefix = "" Then Prefix = "U"
        For Each DeviceName As String In m_Devices.Keys
            If Mid(DeviceName, 1, Prefix.Length) = Prefix Then
                Cnt += 1
            End If
        Next
        Return Prefix & Cnt
    End Function

    ''' <summary>
    ''' Removes a device object from the project and it's connected pads wihtout saving undo information
    ''' </summary>
    ''' <param name="Device"></param>
    ''' <remarks></remarks>
    Public Sub RemoveDevice(ByVal Device As Device)
        If m_Devices.ContainsKey(Device.Name) Then
            Device.Remove()
        End If
        m_Devices.Remove(Device.Name)
        RemoveHandler Device.NameChanged, AddressOf DeviceNameChanged
        RaiseEvent DeviceRemoved(Me, Device)
    End Sub

    ''' <summary>
    ''' Deselects all objects
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DeselectAllObjects()
        Dim SelectableLayerObject As SelectableLayerObject
        Dim SelectedObjects As New List(Of SelectableLayerObject)
        For Each LayerObject As KeyValuePair(Of Integer, LayerObject) In m_LayerObjects
            If TypeOf LayerObject.Value Is SelectableLayerObject Then
                SelectableLayerObject = CType(LayerObject.Value, SelectableLayerObject)
                SelectableLayerObject.Selected = False
                'SelectedObjects.Add(SelectableLayerObject)
            End If
        Next
        m_SelectedObjects.Clear()
        RaiseEvent ObjectsDeselected(Me, SelectedObjects)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
    End Sub

    ''' <summary>
    ''' Returns all objects that are highlighted
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHighlightedObjects() As ReadOnlyDictionary(Of Integer, SelectableLayerObject)
        Return m_HighLightedObjects.GetReadonlyDictionary()
    End Function

    ''' <summary>
    ''' Removes the highlight from all objects, fires updategraphics event
    ''' </summary>
    ''' <param name="NoGraphicsUpdate">If set to true the UpdateGraphics event is not fired</param>
    ''' <remarks></remarks>
    Public Sub UnHighlightAllObjects(Optional ByVal NoGraphicsUpdate As Boolean = False)
        Dim HighlightedObjects As New List(Of SelectableLayerObject)
        For Each HighlightedObject As KeyValuePair(Of Integer, SelectableLayerObject) In m_HighLightedObjects
            HighlightedObject.Value.Highlighted = False
            HighlightedObjects.Add(HighlightedObject.Value)
        Next
        m_HighLightedObjects.Clear()
        RaiseEvent ObjectsDeHighlighted(Me, HighlightedObjects)
        If Not NoGraphicsUpdate Then
            RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
            RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
        End If
    End Sub

    ''' <summary>
    ''' Removes the highlight from the layerobject, fires update graphics event
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Public Sub UnHighlightObject(ByVal LayerObject As SelectableLayerObject)
        Dim LayerObjects As New List(Of SelectableLayerObject)
        LayerObjects.Add(LayerObject)
        LayerObject.Highlighted = False
        If m_HighLightedObjects.ContainsKey(LayerObject.id) Then m_HighLightedObjects.Remove(LayerObject.id)
        RaiseEvent ObjectsDeHighlighted(Me, LayerObjects)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
    End Sub

    ''' <summary>
    ''' Highlights an object and fires the updategraphics event
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Public Sub HighlightObject(ByVal LayerObject As SelectableLayerObject)
        Dim LayerObjects As New List(Of SelectableLayerObject)
        If Not LayerObject.Highlighted Then LayerObjects.Add(LayerObject)
        LayerObject.Highlighted = True
        If Not m_HighLightedObjects.ContainsKey(LayerObject.id) Then m_HighLightedObjects.Add(LayerObject.id, LayerObject)
        RaiseEvent ObjectsHighlighted(Me, LayerObjects)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
    End Sub

    ''' <summary>
    ''' Selects an object and fires updategraphics event
    ''' </summary>
    ''' <param name="LayerObject"></param>
    ''' <remarks></remarks>
    Public Sub SelectObject(ByVal LayerObject As SelectableLayerObject)
        Dim LayerObjects As New List(Of SelectableLayerObject)
        If Not LayerObject.Selected Then LayerObjects.Add(LayerObject)
        LayerObject.Selected = True
        If Not m_SelectedObjects.ContainsKey(LayerObject.id) Then m_SelectedObjects.Add(LayerObject.id, LayerObject)
        RaiseEvent ObjectsSelected(Me, LayerObjects)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
    End Sub

    ''' <summary>
    ''' Selects all objects from the given array and fires events to update graphics
    ''' </summary>
    ''' <param name="LayerObjects"></param>
    ''' <remarks></remarks>
    Public Sub SelectObject(ByVal LayerObjects() As SelectableLayerObject)
        Dim Objects As New List(Of SelectableLayerObject)
        For Each obj As SelectableLayerObject In LayerObjects
            If Not obj.Selected Then Objects.Add(obj)
            obj.Selected = True
            If Not m_SelectedObjects.ContainsKey(obj.id) Then m_SelectedObjects.Add(obj.id, obj)
        Next
        RaiseEvent ObjectsSelected(Me, Objects)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
    End Sub

    ''' <summary>
    ''' Returns the closest object on Windowtype in objecttypes located in a max distance
    ''' </summary>
    ''' <param name="WindowType"></param>
    ''' <param name="Point"></param>
    ''' <param name="ObjectTypes"></param>
    ''' <param name="maxDistance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetClosestObject(ByVal WindowType As WindowTypes, ByVal Point As PointF, ByVal ObjectTypes As List(Of Type), Optional ByVal maxDistance As Single = Single.MaxValue, Optional ByVal ExcludeObject As LayerObject = Nothing) As LayerObject
        Dim TmpDistance As Single
        Dim Distance As Single
        Dim ObjectFound As LayerObject = Nothing
        Distance = Single.MaxValue
        For Each Layer As KeyValuePair(Of LayerTypes, Layer) In m_Layers
            If GetWindowTypeOfLayerType(Layer.Key) = WindowType Then
                For Each LayerObject As LayerObject In Layer.Value.LayerObjects
                    If ObjectTypes.Contains(LayerObject.GetType) Then
                        If ExcludeObject Is Nothing OrElse Not LayerObject.Equals(ExcludeObject) Then
                            TmpDistance = LayerObject.GetDistance(Point)
                            If TmpDistance < Distance Then
                                Distance = TmpDistance
                                ObjectFound = LayerObject
                            End If
                        End If
                    End If
                Next
            End If
        Next
        If Distance <= maxDistance Then
            Return ObjectFound
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Selects the object closest to the point and returns this object or nothing if there are no objects there
    ''' </summary>
    ''' <param name="WindowType"></param>
    ''' <param name="Point"></param>
    ''' <param name="MultiSelect"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SelectObjects(ByVal WindowType As WindowTypes, ByVal Point As PointF, Optional ByVal MultiSelect As Boolean = False) As LayerObject
        Dim LayerObject As LayerObject, SelectableLayerObject As SelectableLayerObject
        Dim ClosestObject As SelectableLayerObject = Nothing, Distance As Single
        Dim TmpDistance As Single

        If Not MultiSelect Then
            DeselectAllObjects()
        End If

        Distance = Single.MaxValue
        For Each Layer As KeyValuePair(Of LayerTypes, Layer) In m_Layers
            If Layer.Value.Visible AndAlso GetWindowTypeOfLayerType(Layer.Key) = WindowType Then
                For Each LayerObject In Layer.Value.LayerObjects
                    If TypeOf LayerObject Is SelectableLayerObject Then
                        SelectableLayerObject = CType(LayerObject, SelectableLayerObject)

                        If SelectableLayerObject.Visible Then
                            TmpDistance = SelectableLayerObject.GetDistance(Point) ' Functions.Distance(Point, SelectableLayerObject.Center)
                            If TmpDistance < Distance Then
                                ClosestObject = SelectableLayerObject
                                Distance = TmpDistance
                            End If
                        End If
                    End If
                Next
            End If
        Next
        If Not ClosestObject Is Nothing Then
            SelectObject(ClosestObject)
        End If
        Return ClosestObject
    End Function

    ''' <summary>
    ''' Returns the cursor that should be used on the windows and raises the changecursur event on case of an update
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Cursor()
        Get
            Return m_Cursor
        End Get
        Set(ByVal value)
            m_Cursor = value
            RaiseEvent ChangeCursor(Me, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets if the PCB image should be mirrored horizontally for the specified window
    ''' </summary>
    ''' <param name="WindowType"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HorizontalMirror(ByVal WindowType As WindowTypes) As Boolean
        Get
            Return m_WindowSettings(WindowType).HorizontalMirror
        End Get
        Set(ByVal value As Boolean)
            m_WindowSettings(WindowType).HorizontalMirror = value
            RaiseEvent BackgroundImageMirrorChanged(Me, WindowType, False)
            RaiseEvent UpdateGraphics(Me, WindowType)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets if the PCB image should be mirrored vertically for the specified window
    ''' </summary>
    ''' <param name="WindowType"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VerticalMirror(ByVal WindowType As WindowTypes) As Boolean
        Get
            Return m_WindowSettings(WindowType).VerticalMirror
        End Get
        Set(ByVal value As Boolean)
            m_WindowSettings(WindowType).VerticalMirror = value
            RaiseEvent BackgroundImageMirrorChanged(Me, WindowType, True)
            RaiseEvent UpdateGraphics(Me, WindowType)
        End Set
    End Property

    ''' <summary>
    ''' Add new undo action to the undo stack
    ''' </summary>
    ''' <param name="UndoItem"></param>
    ''' <remarks></remarks>
    Public Function AddUndoItem(ByVal UndoItem As UndoRedoItem) As UndoRedoItem
        m_UndoStack.AddFirst(UndoItem)
        m_RedoStack.Clear()
        If m_UndoStack.Count() > MAX_UNDOREDO_SIZE Then
            m_UndoStack.RemoveLast()
        End If
        RaiseEvent UndoRedoStackUpdate(Me, m_UndoStack, m_RedoStack)
        Return UndoItem
    End Function

    ''' <summary>
    ''' Undo the last command
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Undo()
        Dim UndoItem As UndoRedoItem = m_UndoStack.First().Value
        m_UndoStack.RemoveFirst()
        UndoItem.Undo()
        m_RedoStack.Push(UndoItem)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UndoRedoAction(Me, UndoItem, True)
        RaiseEvent UndoRedoStackUpdate(Me, m_UndoStack, m_RedoStack)
    End Sub

    ''' <summary>
    ''' Redo the last command
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Redo()
        Dim RedoItem As UndoRedoItem = m_RedoStack.Pop()
        RedoItem.Redo()
        m_UndoStack.AddFirst(RedoItem)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UndoRedoAction(Me, RedoItem, False)
        RaiseEvent UndoRedoStackUpdate(Me, m_UndoStack, m_RedoStack)
    End Sub

    ''' <summary>
    ''' Saves the PCB project to an XMLDoc and zip file (for binary data part)
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim WindowRoot As Xml.XmlElement
        Dim WindowNode As Xml.XmlElement
        Dim LayerRoot As Xml.XmlElement
        Dim LayerObjectRoot As Xml.XmlElement
        Dim ConnectionRoot As Xml.XmlElement
        Dim DevicesRoot As Xml.XmlElement
        Dim SchematicRoot As Xml.XmlElement


        Root.Attributes.Append(XMLDoc.CreateAttribute("name")).Value = m_Name
        Root.Attributes.Append(XMLDoc.CreateAttribute("width")).Value = SingleToString(m_Width)
        Root.Attributes.Append(XMLDoc.CreateAttribute("height")).Value = SingleToString(m_Height)

        WindowRoot = Root.AppendChild(XMLDoc.CreateElement("windows"))

        Dim Window_Types As Array = System.Enum.GetValues(GetType(WindowTypes))
        For Each Window_type As WindowTypes In Window_Types
            WindowNode = WindowRoot.AppendChild(XMLDoc.CreateElement("window"))
            WindowNode.Attributes.Append(XMLDoc.CreateAttribute("type")).Value = Window_type.ToString()
            WindowNode.Attributes.Append(XMLDoc.CreateAttribute("horizontalmirror")).Value = m_WindowSettings(Window_type).HorizontalMirror
            WindowNode.Attributes.Append(XMLDoc.CreateAttribute("verticalmirror")).Value = m_WindowSettings(Window_type).VerticalMirror
        Next

        LayerObjectRoot = Root.AppendChild(XMLDoc.CreateElement("layerobjects"))
        For Each LayerObject As KeyValuePair(Of Integer, LayerObject) In m_LayerObjects
            LayerObject.Value.toXML(XMLDoc, LayerObjectRoot.AppendChild(XMLDoc.CreateElement("layerobject")), BinData)
        Next

        LayerRoot = Root.AppendChild(XMLDoc.CreateElement("layers"))
        For Each Layer As KeyValuePair(Of LayerTypes, Layer) In m_Layers
            If Layer.Value.LayerType <> LayerTypes.LayerTypeTopDrawing AndAlso Layer.Value.LayerType <> LayerTypes.LayerTypeBottomDrawing Then
                Layer.Value.toXML(XMLDoc, LayerRoot.AppendChild(XMLDoc.CreateElement("layer")), BinData)
            End If
        Next

        DevicesRoot = Root.AppendChild(XMLDoc.CreateElement("devices"))
        For Each Device As KeyValuePair(Of String, Device) In m_Devices
            Device.Value.toXML(XMLDoc, DevicesRoot.AppendChild(XMLDoc.CreateElement("device")), BinData)
        Next

        ConnectionRoot = Root.AppendChild(XMLDoc.CreateElement("connections"))
        m_ConnectionMatrix.toXML(XMLDoc, ConnectionRoot, BinData)

        SchematicRoot = Root.AppendChild(XMLDoc.CreateElement("schematic"))
        m_Schematic.toXML(XMLDoc, SchematicRoot, BinData)

    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim WindowNodes As Xml.XmlNodeList = Root.SelectNodes("./windows/window")
        Dim LayerObjectNodes As Xml.XmlNodeList = Root.SelectNodes("./layerobjects/layerobject")
        Dim LayerNodes As Xml.XmlNodeList = Root.SelectNodes("./layers/layer")
        Dim ConnectionNode As Xml.XmlNode = Root.SelectSingleNode("./connections")
        Dim DeviceNodes As Xml.XmlNodeList = Root.SelectNodes("./devices/device")
        Dim WindowType As WindowTypes

        m_Name = Root.Attributes("name").Value
        m_Width = StringToSingle(Root.Attributes("width").Value)
        m_Height = StringToSingle(Root.Attributes("height").Value)

        'load the settings for each window
        For Each WindowNode As Xml.XmlElement In WindowNodes
            WindowType = [Enum].Parse(GetType(PCB.WindowTypes), WindowNode.Attributes("type").Value)
            m_WindowSettings(WindowType).HorizontalMirror = WindowNode.Attributes("horizontalmirror").Value
            m_WindowSettings(WindowType).VerticalMirror = WindowNode.Attributes("verticalmirror").Value
        Next

        For Each LayerObjectNode As Xml.XmlElement In LayerObjectNodes
            If LayerObjectNode.Attributes("type") IsNot Nothing Then
                Dim TypeName As String = LayerObjectNode.Attributes("type").Value
                If TypeName <> "" Then
                    Dim ConstructAttributes() As Object = {PCB, LayerObjectNode, BinData}
                    Dim LayerObject As LayerObject = Activator.CreateInstance(System.Type.GetType("unPCB." & TypeName, True, True), ConstructAttributes) 'this will load the new layer object from its type name
                    m_LayerObjects.Add(LayerObject.id, LayerObject)
                End If
            End If
        Next

        For Each LayerNode As Xml.XmlElement In LayerNodes
            Dim LayerType As LayerTypes = [Enum].Parse(GetType(PCB.LayerTypes), LayerNode.Attributes("type").Value)
            m_Layers(LayerType).fromXML(Me, LayerNode, BinData)
        Next

        m_ConnectionMatrix.fromXML(PCB, ConnectionNode, BinData)

        For Each DeviceNode As Xml.XmlNode In DeviceNodes
            Dim Device As New Device(PCB, DeviceNode, BinData)
            m_Devices.Add(Device.Name, Device)
        Next

        m_Schematic.fromXML(PCB, Root.SelectSingleNode("./schematic"), BinData)

        RaiseEvent ObjectsLoaded(Me, Root, BinData)
        RaiseEvent SizeChanged(Me, m_Width, m_Height)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeTop)
        RaiseEvent UpdateGraphics(Me, WindowTypes.WindowTypeBottom)
    End Sub

    ''' <summary>
    ''' Saves the project to a zip file
    ''' </summary>
    ''' <param name="ToFile"></param>
    ''' <param name="BackupSave">If true, the filename/changed state will not be adjust</param>
    ''' <remarks></remarks>
    Public Sub Save(ByVal ToFile As String, Optional ByVal BackupSave As Boolean = False)
        Dim XMLDoc As New Xml.XmlDocument
        Dim Root As Xml.XmlElement
        Dim BinData As New Ionic.Zip.ZipFile
        Dim XmlStream As New MemoryStream()
        If Not BackupSave Then m_FileName = ToFile

        XMLDoc.AppendChild(XMLDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty))

        Root = XMLDoc.CreateElement("pcb")
        XMLDoc.AppendChild(Root)

        toXML(XMLDoc, Root, BinData)

        'XMLDoc.Save(XMlStream)
        'BinData.AddEntry("project.xml", XMlStream.ToString())

        Dim Writer As New Xml.XmlTextWriter(XmlStream, System.Text.Encoding.UTF8)
        Writer.Formatting = Xml.Formatting.None
        Writer.Indentation = 0
        XMLDoc.Save(Writer)
        XmlStream.Position = 0

        BinData.AddEntry("project.xml", XmlStream)
        SaveLibraries(BinData)

        RaiseEvent ProjectSaved(Me, BinData)
        BinData.Save(ToFile)

        If Not BackupSave Then IsChanged = False
    End Sub

    ''' <summary>
    ''' Saves all the eagle m_Libraries to the zip file
    ''' </summary>
    ''' <param name="ZipFile"></param>
    ''' <remarks></remarks>
    Private Sub SaveLibraries(ByVal ZipFile As Ionic.Zip.ZipFile)
        For Each Library As Eagle.Project In m_Libraries
            ZipFile.AddEntry(Library.ShortFileName, Library.SaveXml())
        Next
    End Sub

    ''' <summary>
    ''' Loads an eagle library from a ZIP entry in the project file
    ''' </summary>
    ''' <param name="ZipEntry"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function LoadLibararyEntry(ByVal ZipEntry As Ionic.Zip.ZipEntry) As Boolean
        Dim Stream As New System.IO.MemoryStream
        ZipEntry.Extract(Stream)
        Stream.Position = 0
        Dim strReader As System.IO.StreamReader = New System.IO.StreamReader(Stream, True)
        Dim Library As New Eagle.Project
        If Library.LoadXml(strReader.ReadToEnd(), ZipEntry.FileName) Then
            m_Libraries.Add(Library)
        End If
    End Function

    ''' <summary>
    ''' loads the project from a zip file
    ''' </summary>
    ''' <param name="File"></param>
    ''' <remarks></remarks>
    Public Sub Open(ByVal File As String)
        Dim XMLDoc As New Xml.XmlDocument
        Dim PCBElements As Xml.XmlNodeList
        Dim BinData As Ionic.Zip.ZipFile
        Dim ZipEntry As Ionic.Zip.ZipEntry
        Dim XMLMemoryStream As New System.IO.MemoryStream
        BinData = Ionic.Zip.ZipFile.Read(File)

        For Each ZipEntry In BinData
            If ZipEntry.FileName.ToLower() = "project.xml" Then
                ZipEntry.Extract(XMLMemoryStream)
            ElseIf ZipEntry.FileName.ToLower().EndsWith(".lbr") Then
                LoadLibararyEntry(ZipEntry) 'load eagle libs
            End If
        Next

        XMLMemoryStream.Position = 0

        Dim strReader As System.IO.StreamReader = New System.IO.StreamReader(XMLMemoryStream, True)

        XMLDoc.Load(strReader)

        PCBElements = XMLDoc.GetElementsByTagName("pcb")
        If PCBElements.Count > 0 Then
            fromXML(Me, PCBElements(0), BinData)
        End If
        m_FileName = File
        SortEagleLibraries()
        RaiseEvent ProjectLoaded(Me, BinData)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        For Each Layer As KeyValuePair(Of LayerTypes, Layer) In m_Layers
            RemoveHandler Layer.Value.LayerVisibilityChanged, AddressOf LayerVisibilityHasChanged
        Next
    End Sub
End Class

