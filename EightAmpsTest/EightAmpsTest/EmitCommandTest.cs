using NUnit.Framework;
using EightAmps;
using HidSharp;

namespace EightAmpsTest
{
    [TestFixture]
    public class EmitCommandTest
    {
        private EmitCommand.Payload lastPayload;
        private HidDevice device;
        private HidStream stream;

        private void completeHandler(EmitCommand command, EmitCommand.Payload payload)
        {
            this.lastPayload = payload;
        }

        [SetUp]
        public void SetUp()
        {
            lastPayload = new EmitCommand.Payload("", 0x00);

            device = new FakeHidDevice();
            stream = new FakeHidStream(device);
        }

        [TestCase]
        public void TestIsInstantiable()
        {
            ICommand command = new EmitCommand("abcd", Infrareddy.NoRepeat, completeHandler);
            Assert.NotNull(command);
        }

        [TestCase]
        public void TestIsExecutable()
        {
            ICommand command = new EmitCommand("abcd", Infrareddy.NoRepeat, completeHandler);
            command.Execute();
            Assert.NotNull(lastPayload);
            Assert.AreEqual("abcd", lastPayload.Value);
            Assert.AreEqual(Infrareddy.NoRepeat, lastPayload.Repeats);
        }
    }
}
