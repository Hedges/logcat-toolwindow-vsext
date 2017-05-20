﻿//------------------------------------------------------------------------------
// <copyright file="LogcatOutputToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace LogcatToolWin
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for LogcatOutputToolWindowControl.
    /// </summary>
    public partial class LogcatOutputToolWindowControl : UserControl
    {
        AdbAgent adb = new AdbAgent();
        /// <summary>
        /// Initializes a new instance of the <see cref="LogcatOutputToolWindowControl"/> class.
        /// </summary>
        public LogcatOutputToolWindowControl()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnLoadedHandler);
        }

        void OnLoadedHandler(object sender, RoutedEventArgs ev)
        {
            AdbAgent.OnDeviceChecked += OnDeviceChecked;
            adb.CheckAdbDevice();
        }

        void OnDeviceChecked(string device_name, bool is_ready)
        {
            string msg = device_name;
            if (is_ready)
            {
                msg += " (Online)";
            }
            else
            {
                msg += " (Offline)";
            }
            this.Dispatcher.Invoke(() => { DeviceStateLabel.Content = msg; });
            //Instance.DeviceStateLabel.Content = "Device"; // device_name as object;

        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "LogcatOutputToolWindow");
        }
    }
}