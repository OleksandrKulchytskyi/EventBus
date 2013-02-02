using log4net;
using System;
using System.Diagnostics;

namespace EventBus.Logging
{
	public class Logger : ILog
	{
		internal ILog GetLoggerForCall()
		{
			log4net.Config.XmlConfigurator.Configure();

			return log4net.LogManager.GetLogger(new StackTrace().GetFrame(2).GetMethod().DeclaringType);
		}

		#region ILog Members

		public void Debug(object message, Exception exception)
		{
			GetLoggerForCall().Debug(message, exception);
		}

		public void Debug(object message)
		{
			GetLoggerForCall().Debug(message);
		}

		public void DebugFormat(IFormatProvider provider, string format, params object[] args)
		{
			GetLoggerForCall().DebugFormat(provider, format, args);
		}

		public void DebugFormat(string format, object arg0, object arg1, object arg2)
		{
			GetLoggerForCall().DebugFormat(format, arg0, arg1, arg2);
		}

		public void DebugFormat(string format, object arg0, object arg1)
		{
			GetLoggerForCall().DebugFormat(format, arg0, arg1);
		}

		public void DebugFormat(string format, object arg0)
		{
			GetLoggerForCall().DebugFormat(format, arg0);
		}

		public void DebugFormat(string format, params object[] args)
		{
			GetLoggerForCall().DebugFormat(format, args);
		}

		public void Error(object message, Exception exception)
		{
			GetLoggerForCall().Error(message, exception);
		}

		public void Error(object message)
		{
			GetLoggerForCall().Error(message);
		}

		public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
		{
			GetLoggerForCall().ErrorFormat(provider, format, args);
		}

		public void ErrorFormat(string format, object arg0, object arg1, object arg2)
		{
			GetLoggerForCall().ErrorFormat(format, arg0, arg1, arg2);
		}

		public void ErrorFormat(string format, object arg0, object arg1)
		{
			GetLoggerForCall().ErrorFormat(format, arg0, arg1);
		}

		public void ErrorFormat(string format, object arg0)
		{
			GetLoggerForCall().ErrorFormat(format, arg0);
		}

		public void ErrorFormat(string format, params object[] args)
		{
			GetLoggerForCall().ErrorFormat(format, args);
		}

		public void Fatal(object message, Exception exception)
		{
			GetLoggerForCall().Fatal(message, exception);
		}

		public void Fatal(object message)
		{
			GetLoggerForCall().Fatal(message);
		}

		public void FatalFormat(IFormatProvider provider, string format, params object[] args)
		{
			GetLoggerForCall().FatalFormat(provider, format, args);
		}

		public void FatalFormat(string format, object arg0, object arg1, object arg2)
		{
			GetLoggerForCall().FatalFormat(format, arg0, arg1, arg2);
		}

		public void FatalFormat(string format, object arg0, object arg1)
		{
			GetLoggerForCall().FatalFormat(format, arg0, arg1);
		}

		public void FatalFormat(string format, object arg0)
		{
			GetLoggerForCall().FatalFormat(format, arg0);
		}

		public void FatalFormat(string format, params object[] args)
		{
			GetLoggerForCall().FatalFormat(format, args);
		}

		public void Info(object message, Exception exception)
		{
			GetLoggerForCall().Info(message, exception);
		}

		public void Info(object message)
		{
			GetLoggerForCall().Info(message);
		}

		public void InfoFormat(IFormatProvider provider, string format, params object[] args)
		{
			GetLoggerForCall().InfoFormat(provider, format, args);
		}

		public void InfoFormat(string format, object arg0, object arg1, object arg2)
		{
			GetLoggerForCall().InfoFormat(format, arg0, arg1, arg2);
		}

		public void InfoFormat(string format, object arg0, object arg1)
		{
			GetLoggerForCall().InfoFormat(format, arg0, arg1);
		}

		public void InfoFormat(string format, object arg0)
		{
			GetLoggerForCall().InfoFormat(format, arg0);
		}

		public void InfoFormat(string format, params object[] args)
		{
			GetLoggerForCall().InfoFormat(format, args);
		}

		public bool IsDebugEnabled
		{
			get { return GetLoggerForCall().IsDebugEnabled; ; }
		}

		public bool IsErrorEnabled
		{
			get { return GetLoggerForCall().IsErrorEnabled; ; }
		}

		public bool IsFatalEnabled
		{
			get { return GetLoggerForCall().IsFatalEnabled; ; }
		}

		public bool IsInfoEnabled
		{
			get { return GetLoggerForCall().IsInfoEnabled; ; }
		}

		public bool IsWarnEnabled
		{
			get { return GetLoggerForCall().IsWarnEnabled; ; }
		}

		public void Warn(object message, Exception exception)
		{
			GetLoggerForCall().Warn(message, exception);
		}

		public void Warn(object message)
		{
			GetLoggerForCall().Warn(message);
		}

		public void WarnFormat(IFormatProvider provider, string format, params object[] args)
		{
			GetLoggerForCall().WarnFormat(provider, format, args);
		}

		public void WarnFormat(string format, object arg0, object arg1, object arg2)
		{
			GetLoggerForCall().WarnFormat(format, arg0, arg1, arg2);
		}

		public void WarnFormat(string format, object arg0, object arg1)
		{
			GetLoggerForCall().WarnFormat(format, arg0, arg1);
		}

		public void WarnFormat(string format, object arg0)
		{
			GetLoggerForCall().WarnFormat(format, arg0);
		}

		public void WarnFormat(string format, params object[] args)
		{
			GetLoggerForCall().WarnFormat(format, args);
		}

		#endregion ILog Members

		#region ILoggerWrapper Members

		log4net.Core.ILogger log4net.Core.ILoggerWrapper.Logger
		{
			get { return GetLoggerForCall().Logger; }
		}

		#endregion ILoggerWrapper Members
	}
}