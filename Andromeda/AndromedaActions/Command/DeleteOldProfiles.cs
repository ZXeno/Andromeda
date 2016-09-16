using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using AndromedaActions.View;
using AndromedaActions.ViewModel;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class DeleteOldProfiles : Action
    {
        private readonly IWmiServices _wmi;
        private const string ProfileQuery = "SELECT * FROM Win32_UserProfile WHERE LocalPath LIKE '%Users%'";

        public DeleteOldProfiles(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) : base(logger, networkServices, fileAndFolderServices)
        {
            _wmi = wmiServices;

            ActionName = "Delete Old User Profiles";
            Description = "Deletes profiles older than a provided date.";
            Category = "Windows Management";
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            var deleteProfilesContext = new DeleteOldProfilesPromptViewModel();
            var deleteProfilesPrompt = new DeleteOldProfilesPrompt
            {
                DataContext = deleteProfilesContext
                
            };
            deleteProfilesPrompt.ShowAsTopmostDialog();

            if (!deleteProfilesContext.Result)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                return;
            }

            if (deleteProfilesContext.DayCount < 0)
            {
                var msg = $"Action {ActionName} aborted: cannot delete profiles from < 0 days..";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                return;
            }

            int daysOlderThan = deleteProfilesContext.DayCount;

            try
            {
                Parallel.ForEach(devlist, (device) =>
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

                            if (DateTime.Now.DayOfYear - date.DayOfYear >= daysOlderThan)
                            {
                                profiles.Add(profileObject);
                            }
                        }

                        var countText = $"{profiles.Count} profile{(profiles.Count == 1 ? String.Empty : "s")} slated for deletion.\n" + 
                            $"Deleting profiles older than {daysOlderThan} days on device {device}.";

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
    }
}