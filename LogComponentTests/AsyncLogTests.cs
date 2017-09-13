using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LogTest.Tests
{
    [TestClass()]
    public class AsyncLogTests
    {
        private ILog logger;
        private String[] FileText;
        private System.IO.DirectoryInfo di;
        String DirectoryPass = @"C:\LogTest";

        [TestInitialize]
        public void TestInit()
        {
            di = new DirectoryInfo(DirectoryPass);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        [TestMethod()]
        public void AsyncLogWriteOneString()
        {
            logger = new AsyncLog();
            logger.Write("Test string");
            logger.StopWithFlush();

            System.Threading.Thread.Sleep(100);

            foreach (String filename in Directory.GetFiles(DirectoryPass))
            {
                FileText = File.ReadAllLines(filename);
            }

            Assert.IsTrue(FileText[1].ToString().Contains("Test string"));
        }

        [TestMethod()]
        public void AsyncLogStopWithFlushTest()
        {
            logger = new AsyncLog();

            for (int i = 0; i < 5; i++)
            {
                logger.Write("Number with Flush: " + i.ToString());
            }
            logger.StopWithFlush();

            System.Threading.Thread.Sleep(300);
            
            foreach (String filename in Directory.GetFiles(DirectoryPass))
            {
                FileText = File.ReadAllLines(filename);
            }
            
            Assert.IsTrue(FileText.Length == 6);
        }

        [TestMethod()]
        public void AsyncLogStopWithoutFlushTest()
        {
            logger = new AsyncLog();

            for (int i = 0; i < 10; i++)
            {
                logger.Write("Number without Flush: " + i.ToString());
                System.Threading.Thread.Sleep(30);
            }
            
            logger.StopWithoutFlush();

            System.Threading.Thread.Sleep(100);

            foreach (String filename in Directory.GetFiles(DirectoryPass))
            {
                FileText = File.ReadAllLines(filename);
            }

            Assert.IsTrue(FileText.Length < 11);
        }

        [TestMethod()]
        public void AsyncLogMidnightTest()
        {
            logger = new AsyncLog(DateTime.Now.AddDays(-1));

            for (int i = 0; i < 5; i++)
            {
                logger.Write("Number with Flush: " + i.ToString());
                System.Threading.Thread.Sleep(30);
            }

            logger.StopWithFlush();
            System.Threading.Thread.Sleep(100);

            int FilesCount = Directory.GetFiles(DirectoryPass).Length;

            Assert.IsTrue(FilesCount == 2);
        }
        [TestCleanup]
        public void TestClean()
        {
            ILog logger = null;

            if (FileText != null)
                Array.Clear(FileText, 0, FileText.Length);
        }
    }
}