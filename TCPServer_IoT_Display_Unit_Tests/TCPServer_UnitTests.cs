using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCPServer;
namespace TCPServer_IoT_Display_Unit_Tests
    {
    
        [TestClass]
        public class TCPServer_UnitTests 
        {
        [ClassInitialize]
        public static void Programinit(TestContext x)
        {
            
            
        }
        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                TCPServerProgram.InitProgram();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        [TestMethod]
            public void Test_DetermineResponse_ValidCMD()
            {
                
                List<string> ValidMessages = new List<string>();
                ValidMessages.Add("GetImage");
                ValidMessages.Add("GetID");
                ValidMessages.Add("SetID");
                ValidMessages.Add("quit");
                List<string> ValidRespones = new List<string>();
                ValidRespones.Add("SendImage");
                ValidRespones.Add("SendID");
                ValidRespones.Add("GetIP");
                ValidRespones.Add("quit");
                for (int i = 0; i < ValidMessages.Count; i++)
                {
                    Assert.AreEqual(ValidRespones[i], TCPServerProgram.DetermineResponse(ValidMessages[i]));
                }
            }
            [TestMethod]
            public void Test_DetermineResponse_InvalidCMD()
            {
            const string invalidcmd = "incorrect command here!";
            const string expectedstring = "-1";
            Assert.AreEqual(expectedstring, TCPServerProgram.DetermineResponse(invalidcmd));
            }
            [TestMethod]
            public void Test_SplitIPAndPort_ValidString()
            {
            //test IP and port.
            const string testIPandPort = "10.2.255.77:7777";
            const string Expectedsplit = "10.2.255.77";
            Assert.AreEqual(Expectedsplit,TCPServerProgram.SplitIPAndPort(testIPandPort));

            }
            [TestMethod]
            public void Test_SplitIPAndPort_InvalidString()
            {
            const string invalidstring1 = "INVALIDSTRING";
            const string invalidstring2 = "This.Is.Wrong.String";
            const string invalidstring3 = "492.92.22.95:7778";
            const string blank = "";
            Assert.AreEqual("-1", TCPServerProgram.SplitIPAndPort(invalidstring1));
            Assert.AreEqual("Non integer found while parsing", TCPServerProgram.SplitIPAndPort(invalidstring2));
            Assert.AreEqual("-2",TCPServerProgram.SplitIPAndPort(invalidstring3));
            Assert.AreEqual("-1", TCPServerProgram.SplitIPAndPort(blank));
            }
        //In other words the device has not been added yet.
            [TestMethod]
            public void Test_GetID_NewDevice()
            {
            const int expectedresponse= -2;
            const string testIP = "10.0.0.2:7779";
            const string IP = "10.0.0.2";
            Assert.AreEqual(expectedresponse, TCPServerProgram.GetID(testIP));
            }
            [TestMethod]
            public void Test_GetID_OldDevice()
            {
            const string testIP = "10.0.0.0:7778";
            const string IP = "10.0.0.0";
            Dictionary<string, int> IDTable = TCPServerProgram.GetIDTable();
            //add mock data and insert an entry into the table already.
            IDTable.Add(IP,1);
            const int expected = 1;
                int got=TCPServerProgram.GetID(testIP);
                Assert.AreEqual(expected, got);
            IDTable.Remove(IP);
            }
            [TestMethod]
            public void Test_GetID_Invalidstring()
            {
            const string invalidresponse = "INVALID";
            const int expectedresponse = -1;
            Assert.AreEqual(expectedresponse,TCPServerProgram.GetID(invalidresponse));
            }
            [TestMethod]
            public void Test_SetID_NewDevice()
            {
            
            const string testIP = "10.0.0.5:7778";
            const string IP = "10.0.0.5";
            Dictionary<string, int> IDTable = TCPServerProgram.GetIDTable();
            
            //success
            const int expectedresponse = 1;
            Assert.AreEqual(expectedresponse,TCPServerProgram.SetID(testIP));
            try
            {
                IDTable.Remove(IP);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
            [TestMethod]
            public void Test_SetID_OldDevice()
            {
            const string testIP = "10.0.0.6:7778";
            const string IP = "10.0.0.6";
            //already in table
            Dictionary<string, int> IDTable = TCPServerProgram.GetIDTable();
            IDTable.Add(IP,1);
            const int expectedresponse = -2;
            Assert.AreEqual(expectedresponse, TCPServerProgram.SetID(testIP));
            //remove IP
            IDTable.Remove(IP);
        }
            [TestMethod]
            public void Test_SetID_Invalidstring()
            {
            const string testIP = "Invalid";
            //success
            const int expectedresponse = -1;
            Assert.AreEqual(expectedresponse, TCPServerProgram.SetID(testIP));
        }
        [TestMethod]
        public void Test_SetID_MoreThanOneDevice()
        {
            const string testIP1 = "10.0.2.2:2222";
            const string testIP2 = "10.2.2.2:62000";
            const string testIP3 = "255.255.255.255:7859";
            const int ExpectedResponseSETID1 = 1;
            const int ExpectedResponseSETID2 = 2;
            const int ExpectedResponseSETID3 = 3;
            Assert.AreEqual(ExpectedResponseSETID1,TCPServerProgram.SetID(testIP1));
            Assert.AreEqual(ExpectedResponseSETID2,TCPServerProgram.SetID(testIP2));
            Assert.AreEqual(ExpectedResponseSETID3,TCPServerProgram.SetID(testIP3));

        }
        [TestMethod]
        public void Test_ConvertIDtoBytes()
        {
            //Can't have 0x80000000 or higher otherwise it's negative for signed ints
            uint id = 0x72F5D102;
            byte[] ExpectedByteOrder = new byte[] { 0x72, 0xF5, 0xD1, 0x02 };

            byte[] temp=TCPServerProgram.ConvertIDtoBytes((int) id);
            for (int i = 0; i < ExpectedByteOrder.Length; i++)
            {
                Assert.AreEqual(temp[i], ExpectedByteOrder[i]);
            }
        }
        [TestMethod]
        public void Test_GetImageFromTable_VALID()
        {
            const int image1id = 1;
            const int image2id = 2;
            byte[] image1;
            byte[] image2;
            byte[] expectedimage1 = TCPServerProgram.GetBitmap1();
            byte[] expectedimage2 = TCPServerProgram.GetBitmap2();
            //will get bitmap1 and bitmap2 respectively.
            image1 =TCPServerProgram.GetImageFromTable(image1id);
            image2=TCPServerProgram.GetImageFromTable(image2id);
            Assert.AreEqual(expectedimage1,image1);
            Assert.AreEqual(expectedimage2,image2);
        }
        [TestMethod]
        public void Test_GetImageFromTable_INVALID()
        {
            byte[] image;
            const int nonvalidID = 3;
            byte[] expectedimage = TCPServerProgram.GetBitmap1();
            image=TCPServerProgram.GetImageFromTable(nonvalidID);
            Assert.AreEqual(expectedimage,image);
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {

        }
    }
    

}
