using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestTesseract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTesseract.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        [TestMethod()]
        public void RemoveBackGroudTest()
        {
            Program.RemoveBackGroud(@".\TestData\testPNG2.png");
        }
    }
}