/*
 * This class handles importing XML files and returning them in a parsable format to other classes.
 * 
 */

using System;
using System.Windows;
using System.Xml;
using System.IO;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class XMLImport
    {

        public static XmlDocument GetXMLFileData(string path)
        {
            XmlDocument xdoc;
            if (File.Exists(path))
            {
                xdoc = new XmlDocument();
                try 
                { 
                    xdoc.Load(path);
                    return xdoc;
                }
                catch (Exception e) 
                {
                    Logger.Log("Error! Something went wrong in XMLImport. Exception information: \n" + e.ToString());
                    if (ResultConsole.Instance.IsInitialized)
                    {
                        ResultConsole.Instance.AddConsoleLine("Cannot open file: " + path);
                    }
                    xdoc = null;
                    return xdoc;
                }
            }
            else
            {
                return null;
            }
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

    }
}
