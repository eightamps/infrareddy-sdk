using HidSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EightAmpsTest
{
    class FakeHidStream : HidStream
    {
        private int readTimeout;
        private int writeTimeout;

        public FakeHidStream(HidDevice device) : base(device) {}

        public override bool CanRead => base.CanRead;

        public override bool CanSeek => base.CanSeek;

        public override bool CanWrite => base.CanWrite;

        public override bool CanTimeout => base.CanTimeout;

        public override long Length => base.Length;

        public override long Position { get => base.Position; set => base.Position = value; }
        public override int ReadTimeout {
            get { return readTimeout; }
            set { readTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return writeTimeout; }
            set { writeTimeout = value; }
        }

        public override void GetFeature(byte[] buffer, int offset, int count)
        {
        }

        public override void SetFeature(byte[] buffer, int offset, int count)
        {

        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return null;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            base.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            base.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return base.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return base.FlushAsync(cancellationToken);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override object InitializeLifetimeService()
        {
            return base.InitializeLifetimeService();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override int Read(Span<byte> buffer)
        {
            return base.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return base.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return base.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            base.SetLength(value);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // throw new NotImplementedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            base.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return base.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
        }

        [Obsolete]
        protected override WaitHandle CreateWaitHandle()
        {
            return base.CreateWaitHandle();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        [Obsolete]
        protected override void ObjectInvariant()
        {
            base.ObjectInvariant();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
        }

        protected override void OnInterruptRequested()
        {
            base.OnInterruptRequested();
        }
    }
}
