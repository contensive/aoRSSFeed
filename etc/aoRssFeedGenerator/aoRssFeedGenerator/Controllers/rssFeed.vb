﻿
Option Explicit On
Option Strict On

Imports System.Xml
Imports System.Xml.Schema
Imports System.IO
Imports System.Text
Imports Contensive.aoRssFeedGenerator

''' <summary>
''' Creates a simple RSS feed for blogs/articles etc
''' </summary>
''' <remarks></remarks>
Public Class RSSFeed
#Region "Properties and Definitions"
    ''' <summary>
    ''' A custom string writer which outputs in UTF8 rather than UTF16
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class UTF8StringWriter
        Inherits StringWriter
        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return System.Text.Encoding.UTF8
            End Get
        End Property
        Public Sub New(stringBuilder As StringBuilder)
            MyBase.New(stringBuilder)
        End Sub
        Public Sub New(stringBuilder As StringBuilder, format As System.IFormatProvider)
            MyBase.New(stringBuilder, format)
        End Sub
    End Class

    Private _document As XmlDocument
    ''' <summary>
    ''' Returns the XML document
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private ReadOnly Property Document As XmlDocument
        Get
            Return _document
        End Get
    End Property

    ''' <summary>
    ''' Returns a UTF8 string representation of the XML document
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shadows ReadOnly Property ToString As String
        Get
            Dim b = New StringBuilder
            Using w = New UTF8StringWriter(b)
                Document.Save(w)
            End Using
            Return b.ToString
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Initialise the XML document and create the opening rss tags
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

        _document = New XmlDocument
        Document.AppendChild(Document.CreateNode(XmlNodeType.XmlDeclaration, Nothing, Nothing))

        'Create the root element and add any name spaces
        Dim rootelement = Document.CreateElement("rss")
        rootelement.SetAttribute("version", "2.0")

        Document.AppendChild(rootelement)

        'Add the name spaces we use
        AddNamespace("dc", "http://purl.org/dc/elements/1.1/")
        AddNamespace("content", "http://purl.org/rss/1.0/modules/content/")

    End Sub

    'Friend Sub WriteRSSItem(name As String, link As String, createdBy As Integer, rSSDescription As String, ccguid As String)
    '    Throw New NotImplementedException()
    'End Sub

    ''' <summary>
    ''' Adds a prefix and url as a namespace to the rss root element
    ''' </summary>
    ''' <param name="prefix">The prefix for the namespace</param>
    ''' <param name="url">The reference url</param>
    ''' <remarks></remarks>
    Public Sub AddNamespace(prefix As String, url As String)
        Document.DocumentElement.SetAttribute("xmlns:" & prefix, url)
    End Sub

    ''' <summary>
    ''' Creates the channel in the RSS feed
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateChannel(channelArgument As channelArgumentsClass)
        'First check we haven't already created a chnanel, as there should only be one in the feed
        Dim channels = Document.GetElementsByTagName("channel")
        If channels.Count > 0 Then
            Throw New ArgumentException("Channel " & channels(0).Name & " has already been created")
        End If
        Dim mainchannel = channels(0)
        'Create the channel element
        mainchannel = Document.CreateElement("channel")
        mainchannel.AppendChild(CreateTextElement("title", channelArgument.title))
        mainchannel.AppendChild(CreateTextElement("link", channelArgument.link))
        mainchannel.AppendChild(CreateTextElement("language", channelArgument.language))
        mainchannel.AppendChild(CreateTextElement("copyright", channelArgument.copyRight))
        mainchannel.AppendChild(CreateTextElement("itunes:subtitle", channelArgument.itunessubtitle))
        mainchannel.AppendChild(CreateTextElement("itunes:author", channelArgument.itunesauthor))
        mainchannel.AppendChild(CreateTextElement("itunes:summary", channelArgument.itunessummary))
        mainchannel.AppendChild(CreateTextElement("description", channelArgument.description))
        mainchannel.AppendChild(CreateTextElement("itunes:keywords", channelArgument.ituneskeywords))
        mainchannel.AppendChild(CreateTextElement("itunes:explicit", channelArgument.itunesexplicit))
        mainchannel.AppendChild(CreateTextElement("itunes:new-feed-url", channelArgument.itunesnewfeedurl))
        Dim ownerName As XmlNode = CreateTextElement("owner-name", channelArgument.ownerName)
        Dim ownerEmail As XmlNode = CreateTextElement("owner-email", channelArgument.ownerEmail)
        Dim ownerNode As XmlNode = Document.CreateElement("owner")
        ownerNode.AppendChild(ownerName)
        ownerNode.AppendChild(ownerEmail)
        mainchannel.AppendChild(ownerNode)
        mainchannel.AppendChild(CreateTextElement("itunes:image", channelArgument.feedimage))
        mainchannel.AppendChild(CreateTextElement("atom:link", ""))

        'mainchannel.AppendChild(CreateTextElement("itunes:owner", mainchannel.AppendChild(CreateTextElement("itunes:name", channelArgument.ownerName)) & mainchannel.AppendChild(CreateTextElement("itunes:email", channelArgument.ownerEmail)) ))

        'mainchannel.AppendChild(CreateTextElement("lastBuildDate", channelArgument.dateLastChanged.ToString("r")))
        'Add any categories the channel belogs to
        If Not channelArgument.categories Is Nothing Then
            'Write the categories
            For Each item As KeyValuePair(Of String, String) In channelArgument.categories
                Dim cat = CreateTextElement("category", item.Key)
                cat.SetAttribute("domain", item.Value)
                mainchannel.AppendChild(cat)
            Next
        End If
        Document.DocumentElement.AppendChild(mainchannel)
    End Sub



    ''' <summary>
    ''' Writes an item to the channel
    ''' </summary>
    ''' <param name="Title">The title of the item</param>
    ''' <param name="Link">A link to the full post</param>
    ''' <param name="itunesauthor">The name of the author</param>
    ''' <param name="pubDate">The date it was published</param>
    ''' <param name="Description">A description of the items</param>
    ''' <param name="Guid">A unique, usually GUID identifier</param>
    ''' <param name="Content">Add the whole article content</param>
    ''' <param name="Categories">Any categories to add, in the format of Name, Url</param>
    ''' <remarks></remarks>
    Public Sub WriteRSSItem(Title As String,
                            Link As String,
                            Author As String,
                             subtitle As String,
                            summery As String,
                            enclosure As String,
                             category As String,
                            Guid As String,
                            explicit As String,
                            pubDate As DateTime,
                            keywords As String,
                             itunesimage As String,
                            Optional Content As String = Nothing,
                            Optional Categories() As KeyValuePair(Of String, String) = Nothing)

        'First check we haven't already created a chnanel, as there should only be one in the feed
        Dim channels = Document.GetElementsByTagName("channel")
        If channels.Count = 0 Then
            Throw New ArgumentException("Please create a channel first by calling CreateChannel")
        End If

        Dim mainchannel = channels(0)

        'Create an item
        Dim thisitem = Document.CreateElement("item")
        thisitem.AppendChild(CreateTextElement("title", Title))
        thisitem.AppendChild(CreateTextElement("link", Link))
        thisitem.AppendChild(CreateTextElement("Author", Author))
        thisitem.AppendChild(CreateTextElement("subtitle", subtitle))
        thisitem.AppendChild(CreateTextElement("summery", summery))
        thisitem.AppendChild(CreateTextElement("enclosure", enclosure))
        thisitem.AppendChild(CreateTextElement("category", category))
        thisitem.AppendChild(CreateTextElement("explicit", explicit))
        thisitem.AppendChild(CreateTextElement("itunes:keywords", keywords))
        'keywords
        thisitem.AppendChild(CreateTextElement("guid",
                                               Guid,
                                               {New KeyValuePair(Of String, String)("isPermaLink", "false")}
                                               ))
        thisitem.AppendChild(CreateTextElement("pubDate", pubDate.ToString("r")))
        thisitem.AppendChild(CreateTextElement("itunes:image", itunesimage))

        'Write the author node
        Dim creatorNode = Document.CreateNode(XmlNodeType.Element,
                                              "dc",
                                              "creator",
                                              Document.GetElementsByTagName("rss")(0).GetNamespaceOfPrefix("dc"))
        creatorNode.InnerText = Author
        thisitem.AppendChild(creatorNode)

        'Write the description



        'Write the content node
        If Not Content Is Nothing Then
            Dim contentNode = Document.CreateNode(XmlNodeType.Element,
                              "content",
                              "encoded",
                              Document.GetElementsByTagName("rss")(0).GetNamespaceOfPrefix("content"))
            contentNode.InnerText = Content
            thisitem.AppendChild(contentNode)
        End If

        If Not Categories Is Nothing Then
            'Write the categories
            For Each item In Categories
                Dim cat = CreateTextElement("category", item.Key)
                cat.SetAttribute("domain", item.Value)
                thisitem.AppendChild(cat)
            Next
        End If

        'Append the element
        mainchannel.AppendChild(thisitem)
    End Sub


#Region "Helper Functions"

    ''' <summary>
    ''' Creates a simple text element with the given name and inner text value
    ''' </summary>
    ''' <param name="Name">The name of the element</param>
    ''' <param name="Value">The value of it</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CreateTextElement(Name As String,
                                       Value As String) As XmlElement
        CreateTextElement = Document.CreateElement(Name)
        CreateTextElement.InnerText = Value
    End Function

    ''' <summary>
    ''' Creates a simple text element with the given name and inner text value, whilst also settings any passed attributes
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <param name="Value"></param>
    ''' <param name="Attributes"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CreateTextElement(Name As String,
                                       Value As String,
                                       Attributes() As KeyValuePair(Of String, String)) As XmlElement
        Dim element = CreateTextElement(Name, Value)
        For Each item In Attributes
            element.SetAttribute(item.Key, item.Value)
        Next
        Return element
    End Function
    '
    Public Class channelArgumentsClass
        Public Property title As String
        Public Property link As String
        Public Property language As String
        Public Property copyRight As String
        Public Property itunessubtitle As String
        Public Property description As String
        Public Property itunesauthor As String
        Public Property itunessummary As String
        Public Property ituneskeywords As String
        Public Property itunesexplicit As String
        Public Property itunesnewfeedurl As String
        'Public Property dateLastChanged As Date
        Public Property categories As New Dictionary(Of String, String)
        Public Property ownerName As String
        Public Property ownerEmail As String
        Public Property feedimage As String
        Public Property atomlink As String
    End Class
    '
    '
    Public Class podcastArgumentsClass
        Public Property name As String
        Public Property link As String
        Public Property createdBy As String
        Public Property language As String
        Public Property copyRight As String
        Public Property itunessubtitle As String
        Public Property description As String
        Public Property itunesauthor As String
        Public Property ccguid As String

    End Class

#End Region

End Class
