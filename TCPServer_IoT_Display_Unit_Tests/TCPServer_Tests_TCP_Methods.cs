using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCPServer;

namespace TCPServer_IoT_Display_Unit_Tests
{
    [TestClass]
    public class TCPServer_Tests_TCP_Methods
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private static Thread MainServer;
        private static Dictionary<string, int> IDTable;
        private static Dictionary<int, byte[]> ImageTable;
        const int port = 7777;
        private static void GetDictionaries()
        {
            IDTable=TCPServerProgram.GetIDTable();
            ImageTable=TCPServerProgram.GetImageTable();
        }
        private static void SendData(byte[] data)
        {

            stream.Write(data, 0, data.Length);
        }
        private static int ReceiveData(byte[] data)
        {
            return stream.Read(data, 0, data.Length);
        }
        [ClassInitialize]
        public static void Programinit(TestContext x)
        {
            try
            {
                //run server loop in seperate thread.
                MainServer = new Thread(TCPServerProgram.listenfordevice);
                MainServer.Start();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            }
        [TestInitialize]
        public void InitTCPClient()
        {
            //reinit dictionaries in main program.
            try
            {
                TCPServerProgram.InitProgram();
                GetDictionaries();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //establish TCP connection with server program
            try
            {
                //The server runs on the same machine and uses IPv4.  This is significantly faster (was taking 8 seconds before!).
                client = new TcpClient(AddressFamily.InterNetwork);
                client.Connect("localhost", port);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                //terminate program if we can't connect to server.
                Console.WriteLine(ex.Message);
                Console.WriteLine("Test Failed to initialize");
                //System.Environment.Exit(0);
            }
        }
        [TestMethod]
        public void TestServerLoop_SetID_Test()
        {
            const string validinput = "SetID";
            
            byte[] ExpectedResponse = { 0, 0, 0, 1 };
            byte[] buffer = new byte[256];
            byte[]data= System.Text.Encoding.ASCII.GetBytes(validinput);
            SendData(data);
            int numofbytes= ReceiveData(buffer);
            for (int i = 0; i < 4; i++)
            {
                Console.Write(buffer[i] + " ");
                Assert.AreEqual(ExpectedResponse[i], buffer[i]);
            }
        }
        [TestMethod]
        public void TestServerLoop_SetID_Test_MORETHANONE_ENTRY()
        {
            TCPServerProgram.SetID("255.255.255.255:9234");
            const string validinput = "SetID";
            byte[] ExpectedResponse = { 0, 0, 0, 2 };
            byte[] buffer = new byte[256];
            byte[] data = System.Text.Encoding.ASCII.GetBytes(validinput);
            SendData(data);
            int numofbytes = ReceiveData(buffer);
            SendData(Encoding.ASCII.GetBytes("GetID"));
            ReceiveData(buffer);
            for (int i = 0; i < 4; i++)
            {
                Console.Write(buffer[i] + " ");
                Assert.AreEqual(ExpectedResponse[i], buffer[i]);
            }
        }
        [TestMethod]
        public void TestServerLoop_GetID_Test()
        {
            const int success = 1;
            byte[] ExpectedResponseSuccess = { 0, 0, 0, 1 };
            string ExpectedResponseFailure = "FAIL";
            byte[]buffer=new byte[256];
            const string GetID = "GetID";
            const string SetID = "SetID";
            /*
             * This block is for when an ID is not present in the table.
             * */
            SendData(Encoding.ASCII.GetBytes(GetID));
            ReceiveData(buffer);
            string actualresponsefailure=Encoding.ASCII.GetString(buffer);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(ExpectedResponseFailure[i], actualresponsefailure[i]);
            }
            /*
             * This block is for when an ID is present.  We first set the ID and then request it.
             * */
            SendData(Encoding.ASCII.GetBytes(SetID));
            ReceiveData(buffer);
            SendData(Encoding.ASCII.GetBytes(GetID));
            ReceiveData(buffer);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(ExpectedResponseSuccess[i], buffer[i]);
            }

        }
        public void TestServerLoop_GetID_Test_MORETHANONE_ENTRY()
        {
            TCPServerProgram.SetID("255.255.255.255:9234");
            string validinput = "GetID";
            byte[] ExpectedResponse = { 0, 0, 0, 2 };
            byte[] buffer = new byte[256];
            byte[] data = System.Text.Encoding.ASCII.GetBytes(validinput);
            SendData(data);
            int numofbytes = ReceiveData(buffer);
            for (int i = 0; i < 4; i++)
            {
                Console.Write(buffer[i] + " ");
                Assert.AreEqual(ExpectedResponse[i], buffer[i]);
            }
        }
        [TestMethod]
        public void TestServerLoop_Invalid()
        {
            const string invalidinput = "NotValid";
            //failure byte
            byte ExpectedResponse = 0xFF;
            byte[] buffer = new byte[256];
            SendData(Encoding.ASCII.GetBytes(invalidinput));
            int numofbytes=ReceiveData(buffer);
            Assert.AreEqual(ExpectedResponse, buffer[0]);
        }
        [TestMethod]
        public void Testtemp()
        {

        }
        [TestCleanup]
        public void CleanupTCPClient()
        {
            try
            {
                string QuitString = "quit";
                SendData(Encoding.ASCII.GetBytes(QuitString));
                Console.WriteLine("Quit Server");
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                //dispose of client after finished with a test.
                client.Close();
                client.Dispose();
                //close stream.
                stream.Close();
            }
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            //Destroy thread after we're done testing.
            try 
            {
                MainServer.Abort();
            }
            catch (ThreadAbortException)
            {

            }
        }
    }
}
