Public Class ConnectionMatrix

    Public Enum ConnectionTypes As Integer
        ConnectionTypeNotConnected = 0
        ConnectionTypeUnknown = 2
        ConnectionTypeConnected = 1
    End Enum

    Public Connections As New Dictionary(Of Pad, Dictionary(Of Pad, ConnectionTypes))

    Public Event ConnectionsChanged(ByVal Sender As ConnectionMatrix, ByVal Connections As Dictionary(Of Pad, Dictionary(Of Pad, ConnectionTypes)))

    Public Sub New()
    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    ''' <summary>
    ''' Returns a Dictionary that maps Pad ids with pads that are currently added to the connection matrix
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getPadIDMap() As Dictionary(Of Integer, Pad)
        Dim Map As New Dictionary(Of Integer, Pad)
        For Each ConnectionRow As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
            Map.Add(ConnectionRow.Key.id, ConnectionRow.Key)
        Next
        Return Map
    End Function

    ''' <summary>
    ''' Returns a matrix of connectiontypes for each pad id
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function BackUp() As Dictionary(Of Integer, Dictionary(Of Integer, ConnectionTypes))
        Dim Cloned As New Dictionary(Of Integer, Dictionary(Of Integer, ConnectionTypes))
        For Each ConnectionRow As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
            Dim ClonedRow As New Dictionary(Of Integer, ConnectionTypes)
            For Each Connection As KeyValuePair(Of Pad, ConnectionTypes) In ConnectionRow.Value
                ClonedRow.Add(Connection.Key.id, Connection.Value)
            Next
            Cloned.Add(ConnectionRow.Key.id, ClonedRow)
        Next
        Return Cloned
    End Function

    ''' <summary>
    ''' Restores connections to a previous backup, leaving connections that are new as they are now. notice: that pads must exist
    ''' </summary>
    ''' <param name="StoredConnections"></param>
    ''' <remarks></remarks>
    Public Sub Restore(ByVal StoredConnections As Dictionary(Of Integer, Dictionary(Of Integer, ConnectionTypes)))
        Dim PadIDMap As Dictionary(Of Integer, Pad) = getPadIDMap()
        Dim RowPad As Pad, ColPad As Pad
        For Each ConnectionRow As KeyValuePair(Of Integer, Dictionary(Of Integer, ConnectionTypes)) In StoredConnections
            For Each Connection As KeyValuePair(Of Integer, ConnectionTypes) In ConnectionRow.Value
                RowPad = PadIDMap(ConnectionRow.Key)
                ColPad = PadIDMap(Connection.Key)
                Connections(RowPad)(ColPad) = Connection.Value
            Next
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Removes a pad from the matrix and all it's connection states
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub RemovePad(ByVal Pad As Pad)
        For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
            Row.Value.Remove(Pad) 'remove pad from each column
        Next
        Connections.Remove(Pad) 'remove pad from the rows
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Adds a new Pad to the connection matrix
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub AddPad(ByVal Pad As Pad)
        Dim Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes))
        Dim NewRow As New Dictionary(Of Pad, ConnectionTypes)

        For Each Row In Connections
            Row.Value.Add(Pad, ConnectionTypes.ConnectionTypeUnknown)
            NewRow.Add(Row.Key, ConnectionTypes.ConnectionTypeUnknown)
        Next

        Connections.Add(Pad, NewRow)
        Connections(Pad)(Pad) = ConnectionTypes.ConnectionTypeConnected 'connect to myself
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Gets all pads that are connected to the given pad by ConnectionType (default is normal connected), includes the pad itself
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <param name="ConnectionType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNet(ByVal Pad As Pad, Optional ByVal ConnectionType As ConnectionTypes = ConnectionTypes.ConnectionTypeConnected) As Net
        Dim Net As New Net
        Dim Row As Dictionary(Of Pad, ConnectionTypes)
        Dim Col As KeyValuePair(Of Pad, ConnectionTypes)

        Row = Connections(Pad)
        For Each Col In Row
            If Col.Value = ConnectionType Then
                Net.Pads.Add(Col.Key)
            End If
        Next

        Return Net
    End Function

    ''' <summary>
    ''' Returns all the nets (net = collection of connected pads/pins), slow function!
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNets() As List(Of Net)
        Dim PadsChecked As New List(Of Pad) 'contains all pads that we know are in some net
        Dim Nets As New List(Of Net)
        'get the net for all pads
        For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
            If Not PadsChecked.Contains(Row.Key) Then
                Dim CurrentNet As Net = GetNet(Row.Key, ConnectionTypes.ConnectionTypeConnected)
                'add the pads in this net to the padschecked list, we will not get a net for them anymore
                For Each Pad As Pad In CurrentNet.Pads
                    PadsChecked.Add(Pad)
                Next
                Nets.Add(CurrentNet)
            End If
        Next
        Return Nets
    End Function

    Public Sub ConnectPads(ByVal Pad1 As Pad, ByVal Pad2 As Pad)
        Dim NetConnected As Net
        Dim NetNotConnected As Net
        Dim RowPad As Pad
        Dim ColPad As Pad

        Connections(Pad1)(Pad1) = ConnectionTypes.ConnectionTypeConnected
        Connections(Pad2)(Pad2) = ConnectionTypes.ConnectionTypeConnected

        'first select all pads pad1 and pad2 are connected with
        NetConnected = GetNet(Pad1, ConnectionTypes.ConnectionTypeConnected) + GetNet(Pad2, ConnectionTypes.ConnectionTypeConnected)

        For Each RowPad In NetConnected.Pads 'iterate through all the connected pads and connect them with each other
            For Each ColPad In NetConnected.Pads
                Connections(RowPad)(ColPad) = ConnectionTypes.ConnectionTypeConnected
            Next
        Next

        'select all pads that are not connected to pad1 and pad2
        NetNotConnected = GetNet(Pad1, ConnectionTypes.ConnectionTypeNotConnected) + GetNet(Pad2, ConnectionTypes.ConnectionTypeNotConnected)

        For Each RowPad In NetNotConnected.Pads 'iterate through all the NOT connected pads and NOT connect them with the pads we are connected with, what's left over will be UNKNOWN connection
            For Each ColPad In NetConnected.Pads
                Connections(RowPad)(ColPad) = ConnectionTypes.ConnectionTypeNotConnected
                Connections(ColPad)(RowPad) = ConnectionTypes.ConnectionTypeNotConnected
            Next
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    Public Sub ConnectPads(ByVal Pads() As Pad)
        Dim Pad As Pad
        Dim FirstPad As Pad = Nothing
        For Each Pad In Pads
            If FirstPad Is Nothing Then
                FirstPad = Pad
            Else
                ConnectPads(FirstPad, Pad)
            End If
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Says that Pad1 and Pad2 and the pads they are connected with are NOT connected to each other, this eliminates further needed testing for connection between the 2 nets (for ex. all pads connected to GND and VCC will never be connected to each other)
    ''' </summary>
    ''' <param name="Pad1"></param>
    ''' <param name="Pad2"></param>
    ''' <remarks></remarks>
    Public Sub NotConnectPads(ByVal Pad1 As Pad, ByVal Pad2 As Pad)
        Dim NetConnected1 As Net
        Dim NetConnected2 As Net
        'Dim Pad1 As Pad, Pad2 As Pad

        If Not Pad1.Equals(Pad2) Then
            'get all pads that are connected
            NetConnected1 = GetNet(Pad1, ConnectionTypes.ConnectionTypeConnected)
            NetConnected2 = GetNet(Pad2, ConnectionTypes.ConnectionTypeConnected)

            For Each PadConnected1 As Pad In NetConnected1.Pads
                For Each padConnected2 As Pad In NetConnected2.Pads
                    Connections(PadConnected1)(padConnected2) = ConnectionTypes.ConnectionTypeNotConnected
                    Connections(padConnected2)(PadConnected1) = ConnectionTypes.ConnectionTypeNotConnected
                Next
            Next

            Connections(Pad1)(Pad1) = ConnectionTypes.ConnectionTypeConnected
            Connections(Pad2)(Pad2) = ConnectionTypes.ConnectionTypeConnected

            RaiseEvent ConnectionsChanged(Me, Connections)
        End If
    End Sub

    ''' <summary>
    ''' Says that the pads in the array and the pads they are connected with, are NOT connected to each other, this eliminates further needed testing for connection between the 2 nets (for ex. all pads connected to GND and +3.3V, +5V will never be connected to each other)
    ''' </summary>
    ''' <param name="Pads"></param>
    ''' <remarks></remarks>
    Public Sub NotConnectPads(ByVal Pads() As Pad)
        For Each Pad1 As Pad In Pads
            For Each pad2 As Pad In Pads
                NotConnectPads(Pad1, pad2)
            Next
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' The pad will be NOT connected to all other pads which are not checked
    ''' </summary>
    ''' <param name="Pad1">The pad to NOT connect to all other pads which have connectiontype: unknown</param>
    ''' <remarks></remarks>
    Public Sub NotConnectToAllPads(ByVal Pad1 As Pad)
        Dim AllPads As Dictionary(Of Pad, ConnectionTypes)
        AllPads = New Dictionary(Of Pad, ConnectionTypes)(Connections.Item(Pad1))
        For Each Pad As KeyValuePair(Of Pad, ConnectionTypes) In AllPads
            If Pad.Value = ConnectionTypes.ConnectionTypeUnknown Then
                NotConnectPads(Pad1, Pad.Key)
            End If
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Disconnects the pad from all pads it's connected with and NOT connected with (resets all connection states for the pad)
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub DisconnectPad(ByVal Pad As Pad)
        'Dim Row As Dictionary(Of Pad, ConnectionTypes)
        Dim Col As KeyValuePair(Of Pad, ConnectionTypes)
        Dim Pads As New List(Of Pad)
        'Row = 
        For Each Col In Connections(Pad)
            Pads.Add(Col.Key)
        Next
        For Each Pad1 As Pad In Pads
            Connections(Pad)(Pad1) = ConnectionTypes.ConnectionTypeUnknown
            Connections(Pad1)(Pad) = ConnectionTypes.ConnectionTypeUnknown
        Next
        Connections(Pad)(Pad) = ConnectionTypes.ConnectionTypeConnected
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Disconnects pads, changing the connection state to unknown
    ''' </summary>
    ''' <param name="Pads"></param>
    ''' <remarks></remarks>
    Public Sub DisconnectPads(ByVal Pads() As Pad)
        For Each pad As Pad In Pads
            DisconnectPad(pad)
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

    ''' <summary>
    ''' Returns next pad to check connection with based on the pad we want to test against, returns nothing if all are checked
    ''' </summary>
    ''' <param name="CheckWithPad"></param>
    ''' <param name="OrderByDistance">If set to true the nearest unchecked pad will be returned first, if set false the first found will be returned</param>
    ''' <param name="ExcludePads">Optional list containing pads that must be excluded from search</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNextToCheckPad(ByVal CheckWithPad As Pad, Optional ByVal OrderByDistance As Boolean = True, Optional ByVal ExcludePads As List(Of Pad) = Nothing) As Pad
        Dim Row As Dictionary(Of Pad, ConnectionMatrix.ConnectionTypes)
        Dim ClosestPad As Pad = Nothing
        Dim ClosestDistance As UInt32 = UInt32.MaxValue

        Row = Connections.Item(CheckWithPad)
        For Each Col As KeyValuePair(Of Pad, ConnectionTypes) In Row
            If Col.Value = ConnectionTypes.ConnectionTypeUnknown Then
                If ExcludePads Is Nothing OrElse ExcludePads.Contains(Col.Key) = False Then
                    If OrderByDistance Then 'check if we need to order by distance
                        Dim Distance As UInt32 = Functions.Distance(Col.Key.Center, CheckWithPad.Center)
                        If Distance < ClosestDistance Then
                            ClosestDistance = Distance
                            ClosestPad = Col.Key
                        End If
                    Else
                        Return Col.Key
                    End If
                End If
            End If
        Next
        Return ClosestPad
    End Function

    ''' <summary>
    ''' Returns the first found pad which has unknown connection states to another pad
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetFirstUnconnectedPad() As Pad
        For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
            For Each Col As KeyValuePair(Of Pad, ConnectionTypes) In Row.Value
                If Col.Value = ConnectionTypes.ConnectionTypeUnknown Then
                    Return Row.Key
                End If
            Next
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Performs a count of the number of connections that still have to be checked
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetUnknownConnections() As Integer
        Dim Count As Integer = 0
        For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
            For Each Col As KeyValuePair(Of Pad, ConnectionTypes) In Row.Value
                If Col.Value = ConnectionTypes.ConnectionTypeUnknown Then
                    Count += 1
                End If
            Next
        Next
        Return Count
    End Function

    ''' <summary>
    ''' Returns total number of connections on the PCB
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTotalConnections() As Integer
        Return Connections.Count ^ 2
    End Function


    ''' <summary>
    ''' Converts to XML document
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData">Zip file containing binary data</param>
    ''' <remarks></remarks>
    Public Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes))
        Dim Col As KeyValuePair(Of Pad, ConnectionTypes)
        'Dim ConnectionNode As Xml.XmlElement = Root.AppendChild(XMLDoc.CreateElement("connections"))

        For Each Row In Connections
            Dim RowConnection As Xml.XmlElement = Root.AppendChild(XMLDoc.CreateElement("row"))
            RowConnection.Attributes.Append(XMLDoc.CreateAttribute("pad_id")).Value = Row.Key.id
            For Each Col In Row.Value
                Dim ColConnection As Xml.XmlElement = RowConnection.AppendChild(XMLDoc.CreateElement("col"))
                ColConnection.Attributes.Append(XMLDoc.CreateAttribute("pad_id")).Value = Col.Key.id
                ColConnection.Attributes.Append(XMLDoc.CreateAttribute("con_type")).Value = Col.Value
            Next
        Next
    End Sub

    ''' <summary>
    ''' Converts from XML document
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim Rows As Xml.XmlNodeList
        Dim Row As Xml.XmlNode
        Dim Cols As Xml.XmlNodeList
        Dim Col As Xml.XmlNode
        Dim ConnectionRow As Dictionary(Of Pad, ConnectionTypes)
        Dim Pad As Pad
        Dim PadId As Integer
        Dim ConnectionType As ConnectionTypes


        Rows = Root.SelectNodes("row")

        For Each Row In Rows
            PadId = Row.Attributes("pad_id").Value
            Pad = CType(PCB.GetLayerObject(PadId), Pad)
            Cols = Row.SelectNodes("col")
            ConnectionRow = New Dictionary(Of Pad, ConnectionTypes)
            Connections.Add(Pad, ConnectionRow)
            For Each Col In Cols
                PadId = Col.Attributes("pad_id").Value
                Pad = CType(PCB.GetLayerObject(PadId), Pad)
                ConnectionType = Col.Attributes("con_type").Value
                ConnectionRow.Add(Pad, ConnectionType)
            Next
        Next
        RaiseEvent ConnectionsChanged(Me, Connections)
    End Sub

End Class
