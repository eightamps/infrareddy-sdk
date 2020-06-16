using System;
using System.Threading.Tasks;
using HidSharp;

namespace EightAmps
{
    public class EmitCommand : ICommand
    {
        private CompleteHandler handler;
        private byte repeats;
        private string value;

        public int Retries { get; set; }
        public struct Payload
        {
            // public string Status;
            public string Value;
            public byte Repeats;

            public Payload(string value, byte repeats)
            {
                this.Value = value;
                this.Repeats = repeats;
            }
        }

        public delegate void CompleteHandler(EmitCommand command, Payload payload);

        public EmitCommand(string value, byte repeats, CompleteHandler handler)
        {
            this.Retries = 0;
            this.handler = handler;
            this.repeats = repeats;
            this.value = value;
        }

        public void Execute()
        {
            Console.WriteLine("EmitCommand.Execute");
            // 1) Ensure the hidStream is still connected, fail if not.
            // 2) Send the payload.Value to the hidStream.
            // 3) Wait for response
            // 4) Call the provided handler once response is received.

            // var buffer = new byte[1];
            // buffer[0] = 0x01;
            // Console.WriteLine("Sending buffer to hid stream");
            // hidStream.SetFeature(buffer);
            // Task task = hidStream.ReadAsync(buffer, 0, 1);
            // task.Wait();
            // Console.WriteLine("Async wait is over {0}", buffer[0]);
// 
            Payload payload = new Payload(this.value, this.repeats);
            this.handler(this, payload);
        }
    }
}