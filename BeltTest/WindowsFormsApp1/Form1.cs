﻿using ClassLibrary1;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private const string BoundsRegistryKey = @"HKEY_CURRENT_USER\SOFTWARE\BeltTest";
        private const string BoundsRegistryName = "Form1Location";

        public Form1()
        {
            InitializeComponent();
            this.AddShieldToSaveButton();

            this.EditionValue.Text = Class1.GetEdition();
            this.CustomerValue.Text = Class1.GetCustomer();
        }

        protected override void OnLoad(EventArgs e)
        {
            var value = (string)Registry.GetValue(BoundsRegistryKey, BoundsRegistryName, null);
            var split = value?.Split(',');

            if (split?.Length == 5)
            {
                var numbers = split.Select(Int32.Parse).ToArray();

                this.DesktopBounds = new Rectangle(numbers[1], numbers[2], numbers[3], numbers[4]);
                this.WindowState = (FormWindowState)numbers[0];
            }

            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var bounds = this.WindowState == FormWindowState.Normal ? 
                this.DesktopBounds : 
                this.RestoreBounds;

            var value = String.Join(",", (int)this.WindowState, bounds.X, 
                bounds.Y, bounds.Width, bounds.Height);

            Registry.SetValue(BoundsRegistryKey, BoundsRegistryName, value, RegistryValueKind.String);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.CustomerValue.Text))
            {
                var executable = typeof(Form1).Assembly.Location;

                var startInfo = new ProcessStartInfo(executable, $"-customer \"{this.CustomerValue.Text}\"")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0) 
                    {
                        this.CustomerValue.Text = Class1.GetCustomer();
                    }
                    else
                    {
                        MessageBox.Show($"Something went wrong, error code: {process.ExitCode}");
                    }
                }
            }
        }


        private void AddShieldToSaveButton()
        {
            SendMessage(this.SaveButton.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        [DllImport("user32")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);
        internal const int BCM_SETSHIELD = 5644;
    }
}
