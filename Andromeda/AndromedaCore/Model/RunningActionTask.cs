using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndromedaCore.Model
{
    public class RunningActionTask
    {
        public Task ThisActionsTask { get; set; }
        public IAction RunningAction { get; set; }
        public string RunningActionName { get; set; }
        public int ThreadId { get; set; }
        public List<String> DeviceList { get; private set; }
        private string _rawDeviceString;
        public string RawDeviceListString
        {
            get => _rawDeviceString;
            set
            {
                _rawDeviceString = value;
                ParseDeviceString(_rawDeviceString);
            }
        }

        private void ParseDeviceString(string deviceString)
        {
            var devList = new List<string>(deviceString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            var resultList = new List<string>();

            foreach (var d in devList)
            {
                var t = d;

                t = new string(t.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

                resultList.Add(t);
            }

            DeviceList = resultList;
        }
    }
}