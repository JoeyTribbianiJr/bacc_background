using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Bacc_background
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
            };
        }

        /// <summary>  
        /// UI线程抛出全局异常事件处理  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("UI线程全局异常");
            }
        }

        /// <summary>  
        /// 非UI线程抛出全局异常事件处理  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("非UI线程全局异常");
            }
        }
    }
}
