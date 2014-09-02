﻿/*
 * This class handles importing XML files and returning them in a parsable format to other classes.
 * 
 */

using System;
using System.Windows;
using System.Text;
using System.Xml;
using System.IO;

namespace Andromeda
{
    public class XMLImport
    {

        public static XmlDocument GetXMLFileData(string path)
        {
            XmlDocument xdoc;
            if (FileExists(path))
            {
                xdoc = new XmlDocument();
                try { xdoc.Load(path); }
                catch (Exception e) 
                {
                    MessageBox.Show("Error! Something went wrong. Exception information: " + e.ToString());
                    App.Current.Shutdown();
                }
                return xdoc;
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
