using EightAmps;
using NUnit.Framework;

namespace EightAmpsTest
{
    [TestFixture]
    class InfrareddyTest
    {
        private static string RC5_PRONTO = "0000 0073 0000 000C 0020 0020 0040 0020 0020 0020 0020 " +
                                  "0020 0020 0020 0020 0020 0020 0020 0020 0040 0040 0020 " + 
                                  "0020 0020 0020 0020 0020 0CC8";

        private FakeHidDevice device;
        private FakeHidStream stream;
        Infrareddy.RequestStatus status;

        private void completeHandler(Infrareddy.RequestStatus status)
        {
            this.status = status;
        }

        [SetUp]
        public void SetUp()
        {
            Infrareddy.ResetRequestTag();
            device = new FakeHidDevice();
            stream = new FakeHidStream(device);
        }

        [TestCase]
        public void TestIsInstantiable()
        {
            Infrareddy infrareddy = new Infrareddy(stream);
            Assert.NotNull(infrareddy);
        }

        [TestCase]
        public void TestEmitPronto()
        {
            Infrareddy infrareddy = new Infrareddy(stream);
            var status = infrareddy.EmitPronto("abcd", Infrareddy.NoRepeat);
            Assert.AreEqual(Infrareddy.RequestStatus.IrSuccess, status);
        }

        [TestCase]
        public void TestRequestKeyInitialValue()
        {
            var key = Infrareddy.NextRequestTag();
            Assert.AreEqual(0x00, key, "Expected first key to be 0");
        }
        
        [TestCase]
        public void TestRequestKeyWrapping()
        {
            var key = 0;
            while (key < 0xfffe)
            {
                key = Infrareddy.NextRequestTag();
            }
            Assert.AreEqual(0xfffe, key, "Expected valid key at 254");
            key = Infrareddy.NextRequestTag();
            Assert.AreEqual(0, key, "Expected overflow back to 0x00");
        }
        
        [TestCase]
        public void TestCommandToPackets()
        {
            var infrareddy = new Infrareddy(stream);
            var reports = infrareddy.CommandToReports(RC5_PRONTO, 0x23);

            Assert.AreEqual(3, reports.Length);
            var report = reports[0];
            Assert.AreEqual(0x03, report.id);
            Assert.AreEqual(0x23, report.tag);
            Assert.AreEqual(54, report.prot.chunkLen);
            Assert.AreEqual(0, report.prot.chunkOffset);
            Assert.AreEqual(139, report.prot.len);

            report = reports[1];
            Assert.AreEqual(0x03, report.id);
            Assert.AreEqual(0x23, report.tag);
            Assert.AreEqual(54, report.prot.chunkLen);
            Assert.AreEqual(54, report.prot.chunkOffset);
            Assert.AreEqual(139, report.prot.len);

            report = reports[2];
            Assert.AreEqual(0x03, report.id);
            Assert.AreEqual(0x23, report.tag);
            Assert.AreEqual(31, report.prot.chunkLen);
            Assert.AreEqual(108, report.prot.chunkOffset);
            Assert.AreEqual(139, report.prot.len);
        }

        [TestCase]
        public void TestPacketsToCommand()
        {
            var infrareddy = new Infrareddy(stream);
            var reports = infrareddy.CommandToReports(RC5_PRONTO, 0x23);
            string command = infrareddy.ReportsToCommand(reports);
            Assert.AreEqual(RC5_PRONTO, command);
        }
    }
}
