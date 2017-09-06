
Public Enum SchematicItemTypes
    DeviceGate = 1
End Enum

Public MustInherit Class SchematicInstance

    Protected m_Location As PointF
    Protected m_StartLocation As PointF 'start location of moving selected objects
    Protected m_PCB As PCB
    Protected m_Selected As Boolean
    Protected m_Selectable As Boolean 'true if is selectable instance
    Protected m_Rotation As Single
    Protected m_startrotation As Single 'start of rotation value

    Public Sub New(ByVal PCB As PCB)
        m_PCB = PCB
    End Sub

    Public MustOverride ReadOnly Property Type() As SchematicItemTypes
    Public MustOverride Sub Render(ByVal Graphics As Graphics)

    ''' <summary>
    ''' Returns true if this object is selected
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Selected() As Boolean
        Get
            Return m_Selected
        End Get
        Set(ByVal value As Boolean)
            m_Selected = value
        End Set
    End Property

    ''' <summary>
    ''' Returns true if this object can be selected
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Selectable() As Boolean
        Get
            Return m_Selectable
        End Get
        Set(ByVal value As Boolean)
            m_Selectable = value
        End Set
    End Property

    ''' <summary>
    ''' The location of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Location() As PointF
        Get
            Return m_Location
        End Get
        Set(ByVal value As PointF)
            m_Location = value
        End Set
    End Property

    ''' <summary>
    ''' Start location for moving the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property StartLocation() As PointF
        Get
            Return m_StartLocation
        End Get
        Set(ByVal value As PointF)
            m_StartLocation = value
        End Set
    End Property

    Public Property Rotation() As Single
        Get
            Return m_Rotation
        End Get
        Set(ByVal value As Single)
            m_Rotation = value
        End Set
    End Property

    Public Property StartRotation() As Single
        Get
            Return m_startrotation
        End Get
        Set(ByVal value As Single)
            m_startrotation = value
        End Set
    End Property

    ''' <summary>
    ''' Converts to xml
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overridable Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Root.Attributes.Append(XMLDoc.CreateAttribute("x")).Value = SingleToString(m_Location.X)
        Root.Attributes.Append(XMLDoc.CreateAttribute("y")).Value = SingleToString(m_Location.Y)
        Root.Attributes.Append(XMLDoc.CreateAttribute("type")).Value = Type()
        Root.Attributes.Append(XMLDoc.CreateAttribute("rotation")).Value = SingleToString(m_Rotation)
    End Sub

    ''' <summary>
    ''' Converts from xml
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overridable Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        m_Location.X = StringToSingle(Root.Attributes("x").Value)
        m_Location.Y = StringToSingle(Root.Attributes("y").Value)
        m_Rotation = StringToSingle(Root.Attributes("rotation").Value)
    End Sub

End Class

Public Class GateInstance
    Inherits SchematicInstance

    Protected WithEvents m_Device As Device
    Protected m_Gate As Eagle.Gate
    Protected m_Symbol As Eagle.Symbol 'clone of the symbol in the library
    Protected m_DeviceGateCount As Integer
    Protected m_Pins As New Dictionary(Of String, Eagle.Pin)


    Public Sub New(ByVal PCB As PCB, ByVal Device As Device, ByVal Gate As Eagle.Gate)
        MyBase.New(PCB)
        Load(Device, Gate)
        m_Location = Gate.Location
    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.New(PCB)
        fromXML(PCB, Root, BinData)
    End Sub

    Private Sub Load(ByVal Device As Device, ByVal Gate As Eagle.Gate)
        m_Device = Device
        m_Gate = Gate
        m_Symbol = Gate.GetSymbol().Clone()
        m_DeviceGateCount = Device.EagleDevice.DeviceSet.Gates.Count

        For Each Pin As Eagle.Pin In m_Symbol.Pins
            m_Pins.Add(Pin.Name, Pin)
            Pin.PadName = m_Device.EagleDevice.GetPadName(m_Gate.Name, Pin.Name)
        Next

        UpdateDeviceNameValue()

        m_Selectable = True
    End Sub

    Private Sub UpdateDeviceNameValue()
        If m_DeviceGateCount = 1 Then
            m_Symbol.SetParameter(Eagle.ParameterType.PT_Name, m_Device.Name)
        Else
            m_Symbol.SetParameter(Eagle.ParameterType.PT_Name, m_Device.Name & Gate.Name)
        End If

        m_Symbol.SetParameter(Eagle.ParameterType.PT_Value, m_Device.Value)
    End Sub

    ''' <summary>
    ''' Returns the PCB device object attached to this gateinstance
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Device() As Device
        Get
            Return m_Device
        End Get
    End Property

    ''' <summary>
    ''' Returns the Eagle Gate object from the Eagle Library
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Gate() As Eagle.Gate
        Get
            Return m_Gate
        End Get
    End Property

    ''' <summary>
    ''' Returns the type of instance
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property Type() As SchematicItemTypes
        Get
            Return SchematicItemTypes.DeviceGate
        End Get
    End Property

    ''' <summary>
    ''' Renders the gate symbol on graphics
    ''' </summary>
    ''' <param name="Graphics"></param>
    ''' <remarks></remarks>
    Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
        Dim Container As Drawing2D.GraphicsContainer = Graphics.BeginContainer()
        Graphics.RotateTransform(m_Rotation)
        m_Symbol.Render(Graphics)
        Graphics.EndContainer(Container)
    End Sub

    ''' <summary>
    ''' Returns the location of a PinName of this gate instance
    ''' </summary>
    ''' <param name="PinName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetPinLocation(ByVal PinName As String) As PointF
        Dim Location As PointF
        If m_Rotation <> 0 Then
            Location = Eagle.Functions.RotateLocation(m_Pins(PinName).GetWireLocation(), m_Rotation / 180 * Math.PI)
        Else
            Location = m_Pins(PinName).GetWireLocation()
        End If
        Location.X = Location.X + Math.Round(m_Location.X, 5)
        Location.Y = Location.Y + Math.Round(m_Location.Y, 5)
        Return Location
    End Function

    ''' <summary>
    ''' Gets / sets if the instance is currently selected and if we must draw the symbol as selected colored
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property Selected() As Boolean
        Get
            Return MyBase.Selected
        End Get
        Set(ByVal value As Boolean)
            m_Symbol.Selected = value
            MyBase.Selected = value
        End Set
    End Property



    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        Dim DeviceName As String = Root.Attributes("device").Value
        Dim GateName As String = Root.Attributes("gate").Value
        Dim Device As Device = PCB.Devices(DeviceName)
        Dim Gate As Eagle.Gate = Device.EagleDevice.DeviceSet.GetGate(GateName)

        Device.GateInstances.Add(Me)
        Load(Device, Gate)
    End Sub

    Public Overrides Sub toXML(ByVal XMLDoc As System.Xml.XmlDocument, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Root.Attributes.Append(XMLDoc.CreateAttribute("device")).Value = m_Device.Name
        Root.Attributes.Append(XMLDoc.CreateAttribute("gate")).Value = m_Gate.Name

    End Sub

    Private Sub m_Device_NameChanged(ByVal Sender As Device, ByVal OldName As String, ByRef NewName As String) Handles m_Device.NameChanged
        UpdateDeviceNameValue()
    End Sub

    Private Sub m_Device_ValueChanged(ByVal Sender As Device, ByVal OldValue As String, ByRef NewValue As String) Handles m_Device.ValueChanged
        UpdateDeviceNameValue()
    End Sub
End Class


Public Class PCBSchematic

    Protected WithEvents m_PCB As PCB
    Protected m_SchematicInstances As New List(Of SchematicInstance) 'holds everything that is printed on the schematic
    Protected m_SelectedInstances As New List(Of SchematicInstance)
    Protected m_Grid As Eagle.Grid

    Public Sub New(ByVal PCB As PCB)
        m_PCB = PCB
        m_Grid = New Eagle.Grid()
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    ''' <summary>
    ''' Returns the closest located schematic item to the given point of Type within a maximum distance of maxdistance
    ''' </summary>
    ''' <param name="ToPoint"></param>
    ''' <param name="Type"></param>
    ''' <param name="MaxDistance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetClosestItem(ByVal ToPoint As PointF, Optional ByVal Type As SchematicItemTypes = SchematicItemTypes.DeviceGate, Optional ByVal MaxDistance As Single = 25, Optional ByVal OnlySelectableObjects As Boolean = True) As SchematicInstance
        Dim ClosestItem As SchematicInstance = Nothing
        Dim ClosestDistance As Single = Single.MaxValue
        For Each SchematicItem As SchematicInstance In m_SchematicInstances
            If (OnlySelectableObjects AndAlso SchematicItem.Selectable) OrElse OnlySelectableObjects = False Then
                Dim CurrentDistance As Single = Distance(SchematicItem.Location, ToPoint)
                If CurrentDistance < ClosestDistance AndAlso CurrentDistance < MaxDistance Then
                    ClosestDistance = CurrentDistance
                    ClosestItem = SchematicItem
                End If
            End If
        Next
        Return ClosestItem
    End Function

    ''' <summary>
    ''' Deselects all items on the schematic
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub DeselectAllInstances()
        Dim TmpSelectedInstances As New List(Of SchematicInstance)(m_SelectedInstances)
        For Each SelectedInstance As SchematicInstance In TmpSelectedInstances
            DeselectInstance(SelectedInstance)
        Next
    End Sub

    ''' <summary>
    ''' Deselects item on the schematic
    ''' </summary>
    ''' <param name="Instance"></param>
    ''' <remarks></remarks>
    Public Overridable Sub DeselectInstance(ByVal Instance As SchematicInstance)
        m_SelectedInstances.Remove(Instance)
        Instance.Selected = False
    End Sub

    ''' <summary>
    ''' Selects item on the schematic
    ''' </summary>
    ''' <param name="Instance"></param>
    ''' <remarks></remarks>
    Public Overridable Sub SelectInstance(ByVal Instance As SchematicInstance)
        m_SelectedInstances.Add(Instance)
        Instance.Selected = True
    End Sub

    ''' <summary>
    ''' Returns a list of the current selected items
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property SelectedInstances() As List(Of SchematicInstance)
        Get
            Return m_SelectedInstances
        End Get
    End Property

    ''' <summary>
    ''' Returns all items on the schematic
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Instances() As List(Of SchematicInstance)
        Get
            Return m_SchematicInstances
        End Get
    End Property

    ''' <summary>
    ''' Returns the grid the schematic is drawn on
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Grid() As Eagle.Grid
        Get
            Return m_Grid
        End Get
    End Property


    ''' <summary>
    ''' Returns the closest located devicepin near ToPoint
    ''' </summary>
    ''' <param name="Net"></param>
    ''' <param name="ToPoint"></param>
    ''' <param name="ExcludePins"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetClosestDevicePin(ByVal Net As Net, ByVal ToPoint As PointF, ByVal ExcludePins As Dictionary(Of DevicePin, DevicePin)) As DevicePin
        Dim Loc As PointF
        Return GetClosestDevicePin(Net, ToPoint, ExcludePins, Loc)
    End Function

    ''' <summary>
    ''' Returns the closest devicepin located near ToPoint and stores the location in ClosestLocation
    ''' </summary>
    ''' <param name="Net"></param>
    ''' <param name="ToPoint"></param>
    ''' <param name="ExcludePins">Exclude this pin </param>
    ''' <param name="ClosestLocation">Returns the point of the closest found pin</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetClosestDevicePin(ByVal Net As Net, ByVal ToPoint As PointF, ByVal ExcludePins As Dictionary(Of DevicePin, DevicePin), ByRef ClosestLocation As PointF) As DevicePin
        Dim ClosestItem As DevicePin = Nothing
        Dim ClosestDistance As Single = Single.MaxValue
        For Each Pad As Pad In Net.Pads
            If Pad.DevicePin IsNot Nothing AndAlso Not ExcludePins.ContainsKey(Pad.DevicePin) Then
                'Dim Gate As Eagle.Gate = Pad.DevicePin.PinConnection.Device.DeviceSet.GetGate(Pad.DevicePin.PinConnection.GateName)
                Dim Location As PointF = Pad.DevicePin.GateInstance.GetPinLocation(Pad.DevicePin.PinConnection.PinName)
                Dim CurrentDistance As Single = Distance(Location, ToPoint)
                If CurrentDistance < ClosestDistance Then
                    ClosestDistance = CurrentDistance
                    ClosestItem = Pad.DevicePin
                    ClosestLocation = Location
                End If
            End If
        Next
        Return ClosestItem
    End Function

    ''' <summary>
    ''' Exports to .sch file
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExportEagleSchematic(ByVal FileName As String) As Boolean
        Dim Project As New Eagle.Project
        Dim Drawing As Eagle.Drawing = Project.CreateDrawing(m_Grid)
        Dim Schematic As Eagle.Schematic = Drawing.CreateSchematic()
        Dim Sheet As Eagle.Sheet = Schematic.AddSheet()

        'first add all libraries, devices and instances to the scheamtic
        For Each Device As KeyValuePair(Of String, Device) In m_PCB.Devices
            With Device.Value
                If .EagleDevice IsNot Nothing Then
                    Dim LibraryName As String = .EagleDevice.DeviceSet.Library.Name
                    'add library to the schematic
                    If Not Schematic.LibraryNodes.ContainsKey(LibraryName) Then
                        Schematic.AddXMLLibraryNode(.EagleDevice.DeviceSet.Library.Name, .EagleDevice.DeviceSet.Library.Node)
                        Schematic.AddLibrary(.EagleDevice.DeviceSet.Library) 'adds a library object, required if we want to calculate some things like pin locations
                    End If

                    'add a part then create an instance
                    Dim Part As Eagle.Part = Schematic.AddPart(.EagleDevice, .EagleDeviceTechnology, .Name, .Value)

                    'instantiate each gate on a schematic location
                    For Each GateInstance As GateInstance In .GateInstances
                        Dim Instance As Eagle.Instance = Sheet.AddInstance(Part, GateInstance.Gate, GateInstance.Location, GateInstance.Rotation)
                    Next
                End If
            End With
        Next

        'get all nets, connect all device pins,...

        Dim Nets As List(Of Net) = m_PCB.ConnectionMatrix.GetNets()
        Dim i As Integer = 0
        For Each Net As Net In Nets
            If Net.HasMultipleDevicePins Then
                If Net.Name = "" Then
                    Net.Name = "N$" & i
                    i += 1
                End If

                Dim Segment As Eagle.Segment = Sheet.AddNet(Net.Name).AddSegment()
                Dim AlreadyConnectedDevicePins As New Dictionary(Of DevicePin, DevicePin) 'contains devicepins that are already connected on the schematic
                For Each Pad As Pad In Net.Pads

                    If Pad.DevicePin IsNot Nothing Then
                        'If Pad.Name = "TH1.5" Then Stop
                        If Not AlreadyConnectedDevicePins.ContainsKey(Pad.DevicePin) Then 'check if this devicepin is already connected
                            AlreadyConnectedDevicePins.Add(Pad.DevicePin, Pad.DevicePin)
                            Segment.AddPinRef(Pad.DevicePin.Device.Name, Pad.DevicePin.GateName, Pad.DevicePin.PinConnection.PinName)
                            Dim Inst1 As Eagle.Instance = Sheet.GetInstance(Pad.DevicePin.Device.Name, Pad.DevicePin.GateName)
                            Dim Location1 As PointF = Inst1.GetPinLocation(Pad.DevicePin.PinConnection.PinName) 'location of the current to connect pin on the schematic
                            Dim Location2 As PointF
                            Location1 = Drawing.Grid.RoundLocation(Location1)
                            If GetClosestDevicePin(Net, Location1, AlreadyConnectedDevicePins, Location2) IsNot Nothing Then 'now we get the location of the closest pin that are already connected in the net
                                Location2 = Drawing.Grid.RoundLocation(Location2)
                                Segment.AddWire(Location1, Location2, Schematic.DefaultWireWidth, Schematic.DefaultWireLayer) ' Inst2.GetPinLocation(Pad.DevicePin.PinConnection.PinName), Schematic.DefaultWireWidth, Schematic.DefaultWireLayer)
                            End If
                            'AlreadyConnectedDevicePins.Add(Pad.DevicePin, Pad.DevicePin)
                        End If
                    End If
                Next
            End If
        Next

        Project.SaveSchematic(FileName)
    End Function

    ''' <summary>
    ''' Converts to xml
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overridable Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim InstanceNodes As Xml.XmlNode = Root.AppendChild(XMLDoc.CreateElement("instances"))
        For Each Instance As SchematicInstance In m_SchematicInstances
            Dim InstanceNode As Xml.XmlNode = instancenodes.appendChild(XMLDoc.CreateElement("instance"))
            Instance.toXML(XMLDoc, InstanceNode, BinData)
        Next
        m_Grid.WriteXml(XMLDoc, Root.AppendChild(XMLDoc.CreateElement("grid")))
    End Sub

    ''' <summary>
    ''' Converts from xml
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overridable Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        m_PCB = PCB
        Dim InstanceNodes As Xml.XmlNodeList = Root.SelectNodes("./instances/instance")
        For Each InstanceNode As Xml.XmlNode In InstanceNodes
            Dim InstanceType As SchematicItemTypes = InstanceNode.Attributes("type").Value
            Select Case InstanceType
                Case SchematicItemTypes.DeviceGate
                    Dim Instance As New GateInstance(PCB, InstanceNode, BinData)
                    m_SchematicInstances.Add(Instance)
            End Select
        Next
    End Sub

    Private Sub m_PCB_DeviceAdded(ByVal Sender As PCB, ByVal Device As Device) Handles m_PCB.DeviceAdded
        For Each SchematicItem As SchematicInstance In Device.GateInstances 'add all gates to the schematic
            m_SchematicInstances.Add(SchematicItem)
        Next
    End Sub

    Private Sub m_PCB_DeviceRemoved(ByVal Sender As PCB, ByVal Device As Device) Handles m_PCB.DeviceRemoved
        For Each SchematicItem As SchematicInstance In Device.GateInstances 'remove all gates from the schematic
            m_SchematicInstances.Remove(SchematicItem)
            DeselectInstance(SchematicItem)
        Next
    End Sub
End Class
