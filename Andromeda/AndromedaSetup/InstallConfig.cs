using System;
using System.Collections.Generic;
using System.Xml;

namespace AndromedaSetup
{
    public class InstallConfig
    {
        public readonly List<InstallableItem> InstallableItems; 

        public InstallConfig()
        {
            InstallableItems = new List<InstallableItem>();
        }

        public bool LoadConfigFile(string path)
        {
            try
            {
                var xdoc = new XmlDocument();
                xdoc.Load(path);

                var xmlNode = xdoc.SelectSingleNode("files");
                if (xmlNode == null) {throw new Exception(); }

                var nodes = xmlNode.ChildNodes;

                foreach (XmlNode node in nodes)
                {
                    var item = node.SelectSingleNode("name").InnerText;
                    var source = node.SelectSingleNode("source").InnerText;
                    var destination = node.SelectSingleNode("destination").InnerText.Replace("{user}", Environment.UserName);

                    if (destination.EndsWith("\\"))
                    {
                        destination = destination.Remove(destination.Length - 1, 1);
                    }

                    InstallableItems.Add(new InstallableItem
                    {
                        Item = item,
                        Source = source,
                        Destination = destination
                    });
                }

                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to load setup.dat");
                return false;
            }
        }
    }
}