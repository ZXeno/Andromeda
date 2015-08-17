using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Andromeda
{
    public interface IConfigurationManager
    {
        void LoadConfig();

        void CreateNewConfigFile();

        void UpdateConfigDocument(IConfiguration configdat);

    }
}