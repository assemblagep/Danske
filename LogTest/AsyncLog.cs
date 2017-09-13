namespace LogTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class AsyncLog : ILog
    {
        private Thread _runThread;
        private volatile System.Collections.Concurrent.ConcurrentQueue<LogLine> LogQueue = new System.Collections.Concurrent.ConcurrentQueue<LogLine>();
        private StringBuilder stringBuilder = new StringBuilder();
        private StreamWriter _writer;
        DateTime _curDate;
        private bool _exit;
        private bool _QuitWithFlush = false;


        public AsyncLog()
        {
            _curDate = DateTime.Now;
            NewLogFile();
            RunThread();
        }

        public AsyncLog(DateTime currentTime)
        {
            _curDate = currentTime;
            NewLogFile();
            RunThread();
        }

        private void RunThread()
        {
            this._runThread = new Thread(this.MainLoop);
            this._runThread.Start();
        }

        private void NewLogFile()
        {
            if (!Directory.Exists(@"C:\LogTest"))
                Directory.CreateDirectory(@"C:\LogTest");

            this._writer = File.AppendText(@"C:\LogTest\Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");
            this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
            this._writer.AutoFlush = true;
        }

        private void CheckDate()
        {
            if ((DateTime.Now - _curDate).Days != 0)
            {
                _curDate = DateTime.Now;
                NewLogFile();
            }
        }

        private void MainLoop()
        {
            while (!this._exit)
            {
                LogQueuedMessages();

                if (this._QuitWithFlush == true && this.LogQueue.Count == 0)
                    this._exit = true;
            }
            CloseWriter();
        }

        private void PrintLogLine(LogLine logLine)
        {
            stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            stringBuilder.Append("\t");
            stringBuilder.Append(logLine.LineText());
            stringBuilder.Append("\t");
            stringBuilder.Append(Environment.NewLine);

            this._writer.Write(stringBuilder.ToString());
            stringBuilder.Clear();
        }

        private void LogQueuedMessages()
        {
            LogLine outMessage = null;
            while (LogQueue.TryDequeue(out outMessage))
            { 
                PrintLogLine(outMessage);
                Thread.Sleep(40);
                if (this._exit == true)
                    break;
                CheckDate();
            }
        }

        private void CloseWriter()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
                _writer = null;
            }
        }

        public void StopWithoutFlush()
        {
            this._exit = true;
        }
  
        public void StopWithFlush()
        {
            this._QuitWithFlush = true;
        }

        public void Write(string text)
        {
             this.LogQueue.Enqueue(new LogLine() { Text = text, Timestamp = DateTime.Now });
        }
    }
}