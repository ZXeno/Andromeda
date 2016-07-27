using System;
using System.Xml;
using System.IO;
using System.Text;

namespace AndromedaCore.Infrastructure
{
    public class XmlServices : IXmlServices
    {
        /// <summary>
        /// Gets xml document data. Throws exception if it fails to load the data. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public XmlDocument GetXmlFileData(string path)
        {
            if (!File.Exists(path)) return null;

            var xdoc = new XmlDocument();
            try
            {
                xdoc.Load(path);
                return xdoc;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read file data at {path} with error {e.Message}");
            }
        }

        /// <summary>
        /// Creates a new XmlWriter with Indent=true and Encoding=Encoding.UTF8 and writes the document start.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public XmlWriter CreateXmlWriter(string filePath)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
            };

            var xwriter = XmlWriter.Create(filePath, settings);
            xwriter.WriteStartDocument();

            return xwriter;
        }

        /// <summary>
        /// Creates a new XML node and closes it in a document writer.
        /// </summary>
        /// <param name="xWriter"></param>
        /// <param name="elementName"></param>
        /// <param name="stringData"></param>
        public void CreateUnattributedElement(ref XmlWriter xWriter, string elementName, string stringData)
        {
            xWriter.WriteStartElement(elementName);
            xWriter.WriteString(stringData);
            xWriter.WriteEndElement();
        }

        /// <summary>
        /// Returns inner text from an XML node. 
        /// 
        /// Throws exception if node cannot be found.
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public string GetNodeData(XmlDocument xDoc, string xpath)
        {
            var resultNode = xDoc.SelectSingleNode(xpath);
            if (resultNode != null)
            {
                return resultNode.InnerText;
            }

            throw new Exception($"Error getting data from XML node {xpath}");
        }

        /// <summary>
        /// Writes the document end and closes the document.
        /// </summary>
        /// <param name="xwriter"></param>
        public void CloseXmlWriter(XmlWriter xwriter)
        {
            xwriter.WriteEndDocument();
            xwriter.Close();
        }
    }
}
