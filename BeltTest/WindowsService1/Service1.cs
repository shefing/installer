﻿using System;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        private readonly System.Timers.Timer _timer;
        private string _path;
        private int _count;

        public Service1()
        {
            InitializeComponent();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += this.OnTimer;
        }

        protected override void OnStart(string[] args)
        {
            InitializePath();

            UpdateFile($"Start count: {_count}");

            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();

            UpdateFile($"Stop count: {_count}");
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateFile($"Running count: {++_count:000}");
        }

        private void InitializePath()
        {
            try
            {
                var path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\BeltTest", "CountFilePath", null) as string;

                _path = Path.GetFullPath(path);
            }
            catch
            {
            }

            if (String.IsNullOrEmpty(_path))
            {
                _path = Path.Combine(AppContext.BaseDirectory, "WindowsService1.txt");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_path));
        }

        private void UpdateFile(string text)
        {
            var bytes = Encoding.UTF8.GetBytes($"[{DateTime.Now:u}] {text}");

            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    using (var file = File.Open(_path, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        file.Write(bytes, 0, bytes.Length);
                    }

                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
