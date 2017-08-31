using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using AndromedaActions.View;
using AndromedaActions.ViewModel;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.ViewModel;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class DeleteOldProfiles : Action
    {
        private readonly IWmiServices _wmi;
        private readonly IWindowService _windowService;
        private const string ProfileQuery = "SELECT * FROM Win32_UserProfile WHERE LocalPath LIKE '%Users%'";

        private List<string> _parsedListCache;
        private int _dayCountCache = 0;

        public DeleteOldProfiles(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices, IWindowService windowService) : base(logger, networkServices, fileAndFolderServices)
        {
            _wmi = wmiServices;
            _windowService = windowService;

            ActionName = "Delete Old User Profiles";
            Description = "Deletes profiles older than a provided date.";
            Category = "Windows Management";

            UiCallback = CallbackMethod;
            HasUserInterfaceElement = true;
        }

        public override void OpenUserInterfaceElement(string rawDeviceList)
        {
            _parsedListCache = ParseDeviceList(rawDeviceList);
            var deleteProfilesContext = new DeleteOldProfilesPromptViewModel();
            _windowService.ShowDialog<DeleteOldProfilesPrompt>(deleteProfilesContext);

            if (!deleteProfilesContext.Result)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                CancellationToken.Cancel();
            }

            if (deleteProfilesContext.DayCount < 0)
            {
                var msg = $"Action {ActionName} aborted: cannot delete profiles from < 0 days..";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                CancellationToken.Cancel();
            }

            _dayCountCache = deleteProfilesContext.DayCount;
        }

        private void CallbackMethod()
        {
            var failedlist = new List<string>();

            try
            {
                Parallel.ForEach(_parsedListCache, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    var remote = _wmi.ConnectToRemoteWmi(device, _wmi.RootNamespace, new ConnectionOptions());
                    if (remote != null)
                    {
                        var searcher = new ManagementObjectSearcher(remote, new ObjectQuery(ProfileQuery));
                        var collection = searcher.Get();
                        var profiles = new List<ManagementObject>();

                        foreach (ManagementObject profileObject in collection)
                        {
                            CancellationToken.Token.ThrowIfCancellationRequested();

                            var queryObjDate = profileObject?["LastUseTime"];
                            if (queryObjDate == null) { continue; }

                            var date = ManagementDateTimeConverter.ToDateTime(queryObjDate.ToString());

                            if (DateTime.Now.DayOfYear - date.DayOfYear >= _dayCountCache)
                            {
                                profiles.Add(profileObject);
                            }
                        }

                        var countText = $"{profiles.Count} profile{(profiles.Count == 1 ? String.Empty : "s")} slated for deletion.\n" +
                                        $"Deleting profiles older than {_dayCountCache} days on device {device}.";

                        ResultConsole.AddConsoleLine(countText);

                        try
                        {
                            Parallel.ForEach(profiles, (queryObj) =>
                            {
                                CancellationToken.Token.ThrowIfCancellationRequested();

                                var path = queryObj["LocalPath"];

                                queryObj.Delete();
                                queryObj.Dispose();

                                ResultConsole.AddConsoleLine($"Delete {path} from device {device} done.");

                            });
                        }
                        catch (Exception e)
                        {
                            Logger.LogError($"Deleting profiles on device {device} returned an error. {e.Message}", e);
                        }
                    }
                });
            }
            catch (OperationCanceledException e)
            {
                ResetCancelToken(ActionName, e);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }


        public override void RunCommand(string rawDeviceList)
        {
            throw new NotImplementedException($"{ActionName} has a user interface element and does utilize the RunCommand method interface.");
        }
    }
}