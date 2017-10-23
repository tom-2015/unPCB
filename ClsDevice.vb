Imports System.Xml
Imports System.Collections.ObjectModel

Public Structure UnconnectedDevicePin
    Dim DevicePin As DevicePin
    Dim EagleDevicePadName As String
    Public Sub New(ByVal DevicePin As DevicePin, ByVal EagleDevicePadName As String)
        Me.DevicePin = DevicePin
        Me.EagleDevicePadName = EagleDevicePadName
    End Sub
End Structure

Public Class Device

    Private Class PadNameComparer
        Inherits Comparer(Of String)

        Public Overrides Function Compare(ByVal x As String, ByVal y As String) As Integer
            Dim xi As Integer, yi As Integer
            Dim i As Integer = 0
            Dim Ln As Integer = Math.Min(x.Length, y.Length)
            Const ZeroChr As Char = "0"
            Const NineChr As Char = "9"

            'skip all non numeric characters which x and y both start with (case for a non numeric prefix)
            While i < Ln AndAlso (x(i) < ZeroChr OrElse x(i) > NineChr) AndAlso x(i) = y(i)
                i = i + 1
            End While

            If i = Ln Then Return 0 'equal

            If i > 0 Then
                x = x.Substring(i)
                y = y.Substring(i)
            End If

            If Integer.TryParse(x, xi) AndAlso Integer.TryParse(y, yi) Then
                If xi < yi Then Return -1
                If xi > yi Then Return 1
                Return 0
            End If
            Return String.Compare(x, y, True)
        End Function

    End Class

    Dim m_PinCount As Integer
    Dim m_Pins As New List(Of DevicePin) 'the pins / pads in this device
    Dim m_EagleDevice As Eagle.Device 'the eagle device
    Dim m_EagleDeviceTechnology As Eagle.Technology 'in case a different technology used (optional)
    Dim m_Name As String
    Dim m_Value As String
    Dim m_PCB As PCB
    Dim m_GateInstances As New List(Of GateInstance) 'gate items on the schematic, created when constructed, the schematic listens to the add event and will add these items to its collection
    'Dim m_GateLocations As New Dictionary(Of String, PointF) 'origin location of each gate on the schematic window

    Public Event NameChanged(ByVal Sender As Device, ByVal OldName As String, ByRef NewName As String) 'called before the device name was changed, newname can be set a new value in case of a problem
    Public Event ValueChanged(ByVal Sender As Device, ByVal OldValue As String, ByRef NewValue As String)

    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal EagleDevice As Eagle.Device, ByVal Technology As Eagle.Technology, ByVal Name As String, Optional ByVal Value As String = "")
        m_PCB = PCB
        m_EagleDevice = EagleDevice
        m_Name = Name
        m_Value = Value
        m_EagleDeviceTechnology = Technology
        Dim Pads As List(Of Eagle.EaglePad) = EagleDevice.DeviceSet.Library.GetPackage(EagleDevice.PackageName).GetPads()

        For Each Gate As Eagle.Gate In EagleDevice.DeviceSet.Gates
            m_GateInstances.Add(New GateInstance(PCB, Me, Gate))
        Next

        For Each PinConnection As Eagle.PinConnection In EagleDevice.PinConnections
            m_Pins.Add(New DevicePin(PCB, Me, PinConnection))
        Next
    End Sub

    ''' <summary>
    ''' Returns all pins of the device
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Pins() As List(Of DevicePin)
        Get
            Return m_Pins
        End Get
    End Property

    ''' <summary>
    ''' Returns number of unconnected device pins
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UnconnectedPinCount() As Integer
        Get
            Dim Count As Integer = 0
            For Each Pin As DevicePin In m_Pins
                If Not Pin.Connected() Then Count += 1
            Next
            Return Count
        End Get
    End Property

    ''' <summary>
    ''' Returns the name of the device R1, IC1,...
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            Dim OldName As String = m_Name
            m_Name = value
            RaiseEvent NameChanged(Me, OldName, value)
        End Set
    End Property

    ''' <summary>
    ''' Returns the value of the device (for ex. 1K, PIC18F2550)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value() As String
        Get
            Return m_Value
        End Get
        Set(ByVal value As String)
            Dim OldValue As String = m_Value
            m_Value = value
            RaiseEvent ValueChanged(Me, OldValue, value)
        End Set
    End Property


    ''' <summary>
    ''' Returns the first found unconnected device pin and eagle pad name
    ''' </summary>
    ''' <returns>Empty unconnectedevicepin structure if all pins and pads are connected</returns>
    ''' <remarks></remarks>
    Public Function GetUnconnectedPin() As UnconnectedDevicePin
        Dim UnconnectedDevicePins As New Dictionary(Of String, DevicePin)
        Dim UnconnectedPadNames As New List(Of String)
        For Each DevicePin As DevicePin In m_Pins
            Dim Unconnected As List(Of String) = DevicePin.GetUnconnectedPadNames()
            For Each UnconnectedPadName As String In Unconnected
                UnconnectedDevicePins.Add(UnconnectedPadName, DevicePin)
                UnconnectedPadNames.Add(UnconnectedPadName)
            Next
        Next

        If UnconnectedPadNames.Count > 0 Then
            UnconnectedPadNames.Sort(New PadNameComparer)
            Return New UnconnectedDevicePin(UnconnectedDevicePins(UnconnectedPadNames(0)), UnconnectedPadNames(0))
        End If
        Return New UnconnectedDevicePin(Nothing, "")
    End Function


    ''' <summary>
    ''' The device object of the eagle library
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property EagleDevice() As Eagle.Device
        Get
            Return m_EagleDevice
        End Get
    End Property

    ''' <summary>
    ''' Returns the technology of the eagle device
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property EagleDeviceTechnology() As Eagle.Technology
        Get
            Return m_EagleDeviceTechnology
        End Get
    End Property

    ''' <summary>
    ''' returns name
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ToString() As String
        Return m_Name
    End Function

    '''' <summary>
    '''' Gets / sets the location of a gate in the schematic
    '''' </summary>
    '''' <value></value>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Overridable Property GateLocation(ByVal GateName As String) As PointF
    '    Get
    '        Return m_GateLocations(GateName)
    '    End Get
    '    Set(ByVal value As PointF)
    '        If m_GateLocations.ContainsKey(GateName) Then
    '            m_GateLocations(GateName) = value
    '        Else
    '            m_GateLocations.Add(GateName, value)
    '        End If
    '    End Set
    'End Property

    ''' <summary>
    ''' Returns a list of schematicitems which discribe the location of each gate in this device in the schematic window
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property GateInstances() As List(Of GateInstance)
        Get
            Return m_GateInstances
        End Get
    End Property

    ''' <summary>
    ''' Returns the gate instance for gatename (slow function)
    ''' </summary>
    ''' <param name="GateName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetGateInstance(ByVal GateName As String) As GateInstance
        For Each GateInstance As GateInstance In m_GateInstances
            If GateInstance.Gate.Name = GateName Then
                Return GateInstance
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Called when removed from the project
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Remove()
        For Each DevicePin As DevicePin In m_Pins
            DevicePin.Remove()
        Next
    End Sub

    Public Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Root.Attributes.Append(XMLDoc.CreateAttribute("pincount")).Value = m_PinCount
        Root.Attributes.Append(XMLDoc.CreateAttribute("eagle_library")).Value = m_EagleDevice.DeviceSet.Library.Drawing.Project.ShortFileName
        Root.Attributes.Append(XMLDoc.CreateAttribute("eagle_device")).Value = m_EagleDevice.Name
        Root.Attributes.Append(XMLDoc.CreateAttribute("eagle_device_set")).Value = m_EagleDevice.DeviceSet.Name
        If m_EagleDeviceTechnology IsNot Nothing AndAlso m_EagleDeviceTechnology.Name <> "" Then
            Root.Attributes.Append(XMLDoc.CreateAttribute("eagle_device_technology")).Value = m_EagleDeviceTechnology.Name
        End If
        Root.Attributes.Append(XMLDoc.CreateAttribute("name")).Value = m_Name
        Root.Attributes.Append(XMLDoc.CreateAttribute("value")).Value = m_Value

        'Dim GateLocationNodes As Xml.XmlNode = Root.AppendChild(XMLDoc.CreateElement("gate_locations"))
        'For Each GateLocation As KeyValuePair(Of String, PointF) In m_GateLocations
        '    Dim GateLocationNode As Xml.XmlNode = GateLocationNodes.AppendChild(XMLDoc.CreateElement("gate_location"))
        '    GateLocationNode.Attributes.Append(XMLDoc.CreateAttribute("gate")).Value = GateLocation.Key
        '    GateLocationNode.Attributes.Append(XMLDoc.CreateAttribute("x")).Value = SingleToString(GateLocation.Value.X)
        '    GateLocationNode.Attributes.Append(XMLDoc.CreateAttribute("y")).Value = SingleToString(GateLocation.Value.Y)
        'Next

        Dim PinRoot As Xml.XmlNode = Root.AppendChild(XMLDoc.CreateElement("pins"))
        For Each Pin As DevicePin In m_Pins
            Pin.toXML(XMLDoc, PinRoot.AppendChild(XMLDoc.CreateElement("pin")), BinData)
        Next
    End Sub

    Public Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim EagleLibraryName As String, EagleDeviceSetName As String, EagleDeviceName As String, EagleDeviceTechnologyName As String = "", EagleLibrary As Eagle.Project
        m_PCB = PCB
        m_PinCount = Root.Attributes("pincount").Value
        m_Name = Root.Attributes("name").Value
        m_Value = Root.Attributes("value").Value
        'm_Location.X = StringToSingle(Root.Attributes("x").Value)
        'm_Location.Y = StringToSingle(Root.Attributes("y").Value)

        EagleLibraryName = Root.Attributes("eagle_library").Value
        EagleDeviceName = Root.Attributes("eagle_device").Value
        EagleDeviceSetName = Root.Attributes("eagle_device_set").Value
        If Root.Attributes("eagle_device_technology") IsNot Nothing Then
            EagleDeviceTechnologyName = Root.Attributes("eagle_device_technology").Value
        End If

        EagleLibrary = PCB.GetEagleLibrary(EagleLibraryName)
        m_EagleDevice = EagleLibrary.Drawing.Library.GetDevice(EagleDeviceSetName, EagleDeviceName)
        m_EagleDeviceTechnology = m_EagleDevice.GetTechnology(EagleDeviceTechnologyName)

        'Dim GateLocationNodes As Xml.XmlNodeList = Root.SelectNodes("gate_locations/gate_location")
        'For Each GatelocationNode As Xml.XmlNode In GateLocationNodes
        '    Dim GateName As String = GatelocationNode.Attributes("gate").Value
        '    Dim X As Single = StringToSingle(GatelocationNode.Attributes("x").Value)
        '    Dim Y As Single = StringToSingle(GatelocationNode.Attributes("y").Value)
        '    m_GateLocations.Add(GateName, New PointF(X, Y))
        'Next

        Dim PinNodes As Xml.XmlNodeList = Root.SelectNodes("./pins/pin")
        For Each PinNode As Xml.XmlNode In PinNodes
            Dim Pin As New DevicePin(Me, PCB, PinNode, BinData)
            m_Pins.Add(Pin)
        Next
    End Sub
End Class

Public Class DevicePin
    Dim m_PinConnection As Eagle.PinConnection
    Dim m_Pads As New List(Of Pad) 'pads on the PCB, usually this contains only one pad but more may be added even if the eagle device doesn't have more
    Dim WithEvents m_Device As Device
    Dim m_DefaultPadTypes As New Dictionary(Of String, Type) 'default type of pad object (SMD or Throughhole)
    Dim m_PCB As PCB
    Dim m_GateInstance As GateInstance 'reference to the gate instance in the schematic

    Public Event PadAdded(ByVal Sender As DevicePin, ByVal Pad As Pad)
    Public Event PadRemoved(ByVal Sender As DevicePin, ByVal Pad As Pad)


    Public Sub New(ByVal PCB As PCB, ByVal Device As Device, ByVal PinConnection As Eagle.PinConnection)
        m_Device = Device
        m_PCB = PCB
        m_PinConnection = PinConnection
        SetDefaultPadTypes()
    End Sub

    Public Sub New(ByVal Device As Device, ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        m_Device = Device
        fromXML(PCB, Root, BinData)
    End Sub

    ''' <summary>
    ''' Sets the default pad types 
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetDefaultPadTypes()
        For i As Integer = 0 To m_PinConnection.PadNames.Length - 1
            Dim PackageName As String = m_PinConnection.Device.PackageName
            If m_PinConnection.Device.DeviceSet.Library.Packages(PackageName).GetPad(m_PinConnection.PadNames(i)).PadType = Eagle.PadTypes.PadTypeSMD Then
                m_DefaultPadTypes.Add(m_PinConnection.PadNames(i), GetType(SMDPad))
            Else
                m_DefaultPadTypes.Add(m_PinConnection.PadNames(i), GetType(ThroughHolePad))
            End If
        Next
    End Sub

    ''' <summary>
    ''' Returns a list of all connected pads
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Pads() As List(Of Pad)
        Get
            Return m_Pads
        End Get
    End Property

    ''' <summary>
    ''' Returns if this pin is fully connected to the pads as defined in the eagle lib
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Connected() As Boolean
        Get
            Return m_Pads.Count >= m_DefaultPadTypes.Count
        End Get
    End Property

    ''' <summary>
    ''' Returns a description for the pin of this device
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            If m_PinConnection.Device.DeviceSet.Gates.Count > 1 Then
                Return m_PinConnection.GateName & "." & m_PinConnection.PinName
            Else
                Return m_PinConnection.PinName
            End If
        End Get
    End Property

    Public ReadOnly Property PinConnection() As Eagle.PinConnection
        Get
            Return m_PinConnection
        End Get
    End Property

    ''' <summary>
    ''' Returns all the names of the pads connected to this symbol pin
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PadNames() As String()
        Get
            Return m_PinConnection.PadNames()
        End Get
    End Property

    ''' <summary>
    ''' Returns the pad names that are in the eagle device and not yet connected
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetUnconnectedPadNames() As List(Of String)
        Dim Res As New List(Of String)
        For Each EaglePadName As String In m_PinConnection.PadNames
            If Not isEaglePadNameConnected(EaglePadName) Then
                Res.Add(EaglePadName)
            End If
        Next
        Return Res
    End Function

    ''' <summary>
    ''' Checks if an eagle pad name is connected to this device pin
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function isEaglePadNameConnected(ByVal Name As String) As Boolean
        For Each Pad As Pad In m_Pads
            If Pad.EaglePadName = Name Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Returns the gate name (A,B,C,...)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GateName() As String
        Get
            Return m_PinConnection.GateName
        End Get
    End Property

    ''' <summary>
    ''' Returns how many gates are used in the device of the pin
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GateCount() As Integer
        Get
            Return m_PinConnection.Device.DeviceSet.Gates.Count
        End Get
    End Property

    ''' <summary>
    ''' Returns the device this pin belongs to
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Device() As Device
        Get
            Return m_Device
        End Get
    End Property

    ''' <summary>
    ''' Returns a unique pad name for an Eagle Pad Name
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetUniquePadName(ByVal EaglePadName As String, Optional ByVal PadId As Integer = -1) As String
        Dim Name As String = m_Device.Name & "." & EaglePadName 'm_PinConnection.PadNames(Math.Min(PadIndex, m_PinConnection.PadNames.Length - 1))
        Dim NameIndex As Integer = Pads.Count()
        While m_PCB.LayerObjectNameExists(Name, PadId)
            Name = m_Device.Name & "." & EaglePadName & "-" & NameIndex 'm_PinConnection.PadNames(Math.Min(PadIndex, m_PinConnection.PadNames.Length - 1)) & "-" & NameIndex
            NameIndex += 1
        End While
        Return Name
    End Function


    ''' <summary>
    ''' If the name of the device was changed we must adjust the name of all pads!
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="OldName"></param>
    ''' <param name="NewName"></param>
    ''' <remarks></remarks>
    Private Sub m_Device_NameChanged(ByVal Sender As Device, ByVal OldName As String, ByRef NewName As String) Handles m_Device.NameChanged
        For Each Pad As Pad In m_Pads
            Pad.Name = GetUniquePadName(Pad.EaglePadName, Pad.id)
        Next
    End Sub

    ''' <summary>
    ''' Connects a pad to this device pin
    ''' </summary>
    ''' <param name="Pad">The pad class to connect</param>
    ''' <param name="EaglePadName">Name of the pad in the eagle lib</param>
    ''' <remarks></remarks>
    Public Sub AddPad(ByVal Pad As Pad, ByVal EaglePadName As String)
        If Pad.DevicePin IsNot Nothing Then
            Pad.DevicePin.Pads.Remove(Pad)
        End If
        Pad.DevicePin = Me
        Pad.EaglePadName = EaglePadName
        m_Pads.Add(Pad)
        RaiseEvent PadAdded(Me, Pad)
    End Sub

    ''' <summary>
    ''' Disconnects all pads from this pin
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ClearPads()
        For Each Pad As Pad In m_Pads
            Pad.DevicePin = Nothing
            RaiseEvent PadRemoved(Me, Pad)
        Next
        m_Pads.Clear()
    End Sub

    ''' <summary>
    ''' Removes a pad that is connected to this pin
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub RemovePad(ByVal Pad As Pad)
        Pad.DevicePin = Nothing
        m_Pads.Remove(Pad)
        RaiseEvent PadRemoved(Me, Pad)
    End Sub

    ''' <summary>
    ''' Called when the device is removed from the project, disconnect and remove all pads
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Remove()
        Dim PadsToRemove() As Pad = m_Pads.ToArray()
        If m_Pads.Count > 0 Then
            For Each pad As Pad In PadsToRemove
                m_PCB.RemoveObject(pad) 'will also remove from the collection
                m_Pads.Add(pad) 'add it back to the collection, this is needed for making undo work properly
            Next
        End If

        m_GateInstance = Nothing
    End Sub

    ''' <summary>
    ''' Returns the default type of pad that should be used for each pad
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DefaultPadType(ByVal PadName As String) As System.Type
        Get
            Return m_DefaultPadTypes(PadName)
        End Get
        Set(ByVal value As System.Type)
            m_DefaultPadTypes(PadName) = value
        End Set
    End Property

    Public Overridable ReadOnly Property GateInstance() As GateInstance
        Get
            If m_GateInstance Is Nothing Then m_GateInstance = m_Device.GetGateInstance(m_PinConnection.GateName)
            Return m_GateInstance
        End Get
    End Property

    Public Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Root.Attributes.Append(XMLDoc.CreateAttribute("pin_name")).Value = m_PinConnection.PinName
        Root.Attributes.Append(XMLDoc.CreateAttribute("gate_name")).Value = m_PinConnection.GateName
        Dim PadRoot As Xml.XmlNode = Root.AppendChild(XMLDoc.CreateElement("pads"))
        For Each Pad As Pad In m_Pads
            PadRoot.AppendChild(XMLDoc.CreateElement("pad")).Attributes.Append(XMLDoc.CreateAttribute("id")).Value = Pad.id
        Next
    End Sub

    Public Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim PinName As String = Root.Attributes("pin_name").Value
        Dim GateName As String = Root.Attributes("gate_name").Value

        m_PCB = PCB
        m_PinConnection = m_Device.EagleDevice.GetPinConnection(GateName, PinName)
        SetDefaultPadTypes()
        Dim PadNodes As Xml.XmlNodeList = Root.SelectNodes("./pads/pad")
        For Each PadNode As Xml.XmlNode In PadNodes
            Dim Id As Integer = PadNode.Attributes("id").Value
            Dim Pad As Pad = PCB.GetLayerObject(Id)
            If Pad IsNot Nothing Then 'in case the pad was not placed on a layer when document was saved
                m_Pads.Add(Pad)
                Pad.DevicePin = Me
            Else
                Debug.Print("Pad " & Id & " for device " & Me.Device.Name & "." & GateName & "." & PinName & " was not placed on a layer!")
            End If
        Next
    End Sub

End Class