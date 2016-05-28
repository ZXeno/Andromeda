using System;
using System.Xml;
using System.IO;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class XMLImport
    {
        public static XmlDocument GetXMLFileData(string path)
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
                Logger.Log($"Something went wrong in XMLImport. Exception information: {e.Message}");
                if (ResultConsole.Instance.IsInitialized)
                {
                    ResultConsole.Instance.AddConsoleLine($"Cannot open file: {path}");
                }
                return null;
            }
        }
    }
}
