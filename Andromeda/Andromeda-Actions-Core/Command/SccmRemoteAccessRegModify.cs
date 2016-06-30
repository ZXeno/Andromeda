﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Andromeda.View;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.ViewModel;
using Microsoft.Win32;

namespace Andromeda_Actions_Core.Command
{
    public class SccmRemoteAccessRegModify : Action
    {
        private IRegistryServices _registry;

        private const string SccmRemoteControlRegistryPath = "SOFTWARE\\Microsoft\\SMS\\Client\\Client Components\\Remote Control";
        private const string RemoteAccessEnabledKeyName = "Enabled";
        private const string RequiresUserApprovalKeyName = "Permission Required";
        private const string ShowConnectionBannerKeyName = "RemCtrl Connection Bar";
        private const string ShowTaskbarIconKeyName = "RemCtrl Taskbar Icon";
        private const string AllowAccessOnUnattendedComputersKeyName = "Allow Remote Control of an unattended computer";
        private const string AllowLocalAdministratorsToRemoteControlKeyName = "Allow Local Administrators to do Remote Control";
        private const string AudibleSignalKeyName = "Audible Signal";

        private bool _remoteAccessEnabled;
        private bool _requiresUserApproval;
        private bool _showConnectionBanner;
        private bool _showTaskBarIcon;
        private bool _allowAccessOnUnattended;
        private bool _allowLocalAdministratorsToRemoteControl;
        private bool _audibleSignal = false;

        public SccmRemoteAccessRegModify(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IRegistryServices registryServices) : base(logger, networkServices, fileAndFolderServices)
        {
            _registry = registryServices;

            ActionName = "SCCM Remote Access Registry Hack";
            Description = "Changes the remote access options for SCCM remote control.";
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            var sccmRegHackContext = new SccmRegHackOptionViewModel();
            var fileCopyPrompt = new SccmRegHackOptionsPrompt
            {
                DataContext = sccmRegHackContext
            };
            fileCopyPrompt.ShowDialog();

            if (!sccmRegHackContext.Result)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                return;
            }

            _remoteAccessEnabled = sccmRegHackContext.RemoteAccessEnabled;
            _requiresUserApproval = sccmRegHackContext.RequiresUserApproval;
            _showConnectionBanner = sccmRegHackContext.ShowConnectionBanner;
            _showTaskBarIcon = sccmRegHackContext.ShowTaskbarIcon;
            _allowAccessOnUnattended = sccmRegHackContext.AllowAccessOnUnattended;
            _allowLocalAdministratorsToRemoteControl = sccmRegHackContext.AllowLocalAdministratorsToRemoteControl;


            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    _registry.WriteToSubkey(
                        device, 
                        RegistryHive.LocalMachine, 
                        SccmRemoteControlRegistryPath, 
                        RemoteAccessEnabledKeyName,
                        BoolToIntString(_remoteAccessEnabled).ToString(), 
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        RequiresUserApprovalKeyName,
                        BoolToIntString(_requiresUserApproval),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        ShowConnectionBannerKeyName,
                        BoolToIntString(_showConnectionBanner),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        ShowTaskbarIconKeyName,
                        BoolToIntString(_showTaskBarIcon),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        AllowAccessOnUnattendedComputersKeyName,
                        BoolToIntString(_allowAccessOnUnattended),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        AllowLocalAdministratorsToRemoteControlKeyName,
                        BoolToIntString(_allowLocalAdministratorsToRemoteControl),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        AudibleSignalKeyName,
                        BoolToIntString(_audibleSignal),
                        RegistryValueKind.DWord);
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

        private string BoolToIntString(bool value)
        {
            if (value) { return "1"; }

            return "0";
        }
    }
}