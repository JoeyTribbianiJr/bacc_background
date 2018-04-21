using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WsUtils
{
    public static class LogHelper
    {
        static LogHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
            LogHelper.WriteLog(typeof(Object), "发送提前倒计时："); 
        }
        /// <summary>
        /// 输出日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ex"></param>
        #region static void WriteLog(Type t, Exception ex)

        //public static void WriteLog(Type t, Exception ex)
        //{
        //    log4net.ILog log = log4net.LogManager.GetLogger(t);
        //    log.Error("Error", ex);
        //}

        #endregion

        /// <summary>
        /// 输出日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        #region static void WriteLog(Type t, string msg)

        public static void WriteLog(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(msg);
        }

        #endregion


    }
}