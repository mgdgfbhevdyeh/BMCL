﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Markup;
using BMCLV2.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BMCLV2
{
    //分支测试
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        private static bool _skipPlugin;

        public static EventWaitHandle ProgramStarted;

        public static bool SkipPlugin => _skipPlugin;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, Process.GetCurrentProcess().ProcessName, out createNew);
            if (!createNew)
            {
                ProgramStarted.Set();
                Environment.Exit(3);
                return;
            }
            if (e.Args.Length == 0)   // 判断debug模式
                Logger.Debug = false;
            else
                if (Array.IndexOf(e.Args, "-Debug") != -1)
                    Logger.Debug = true;
            Logger.Start();
#if DEBUG
#else
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            if (Array.IndexOf(e.Args, "-Update") != -1)
            {
                var index = Array.IndexOf(e.Args, "-Update");
                if (index < e.Args.Length - 1)
                {
                    if (!e.Args[index + 1].StartsWith("-"))
                    {
                        DoUpdate(e.Args[index + 1]);
                    }
                    else
                    {
                        DoUpdate();
                    }
                }
            }
            if (Array.IndexOf(e.Args, "-SkipPlugin") != -1)
            {
                App._skipPlugin = true;
            }
            WebRequest.DefaultWebProxy = null;  //禁用默认代理
            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var crash = new CrashHandle(e.ExceptionObject as Exception);
            crash.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Logger.Stop();
        }

        public static void AboutToExit()
        {
            Logger.Stop();
        }

// ReSharper disable once UnusedMember.Local
// ReSharper disable once UnusedParameter.Local
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (e.Exception is XamlParseException)
            {
                if (e.Exception.InnerException != null)
                {
                    if (e.Exception.InnerException is FileLoadException)
                    {
                        //TODO 资源加载
                        return;
                    }
                }
            }
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }

        private void DoUpdate()
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            var time = 0;
            while (time < 10)
            {
                try
                {
                    File.Copy(processName, "BMCL.exe", true);
                    Process.Start("BMCL.exe", "-Update " + processName);
                    Application.Current.Shutdown(0);
                    return;
                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                }
                finally
                {
                    time ++;
                }
            }
            MessageBox.Show("自动升级失败，请手动使用" + processName + "替代旧版文件");
            MessageBox.Show("自动升级失败，请手动使用" + processName + "替代旧版文件");
        }

        private void DoUpdate(string fileName)
        {
            File.Delete(fileName);
        }
    }
}
