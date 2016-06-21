﻿using System;

namespace JiraAssistant.Logic.Settings
{
    public class ReportsSettings : SettingsBase
    {
        public bool RemindAboutWorklog
        {
            get { return GetValue(defaultValue: false); }
            set { SetValue(value, defaultValue: false); }
        }

        public DateTime RemindAt
        {
            get { return GetValue(defaultValue: new DateTime(1, 1, 1, 16, 0, 0)); }
            set { SetValue(value, defaultValue: new DateTime(1, 1, 1, 16, 0, 0)); }
        }

        public DateTime LastLogWorkDisplayed
        {
            get { return GetValue(defaultValue: new DateTime(1900, 1, 1)); }
            set { SetValue(value, defaultValue: new DateTime(1900, 1, 1)); }
        }

        public bool MonitorIssuesUpdates
        {
            get { return GetValue(defaultValue: false); }
            set { SetValue(value, defaultValue: false); }
        }

        public bool ShowCreatedIssues
        {
            get { return GetValue(defaultValue: true); }
            set { SetValue(value, defaultValue: true); }
        }

        public bool ShowUpdatedIssues
        {
            get { return GetValue(defaultValue: true); }
            set { SetValue(value, defaultValue: true); }
        }

        public TimeSpan ScanForUpdatesInterval
        {
            get { return GetValue(defaultValue: TimeSpan.FromMinutes(5)); }
            set { SetValue(value, defaultValue: TimeSpan.FromMinutes(5)); }
        }

        public DateTime LastUpdatesScan
        {
            get { return GetValue(defaultValue: new DateTime(1900, 1, 1)); }
            set { SetValue(value, defaultValue: new DateTime(1900, 1, 1)); }
        }

        public string ProjectsList
        {
            get { return GetValue(defaultValue: ""); }
            set { SetValue(value, defaultValue: ""); }
        }

        public bool SkipOwnChanges
        {
            get { return GetValue(defaultValue: true); }
            set { SetValue(value, defaultValue: true); }
        }
    }
}
