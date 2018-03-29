using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMBLibrary.Client
{
    public class SMBClientStream : System.IO.Stream
    {
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        ISMBFileStore _fileStore;
        object _handle;

        public SMBClientStream(ISMBFileStore fileStore, object handle)
        {
            _fileStore = fileStore;
            _handle = handle;
        }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var myBuffer = new byte[count];
            var status = _fileStore.ReadFile(out myBuffer, _handle, offset, count);
            myBuffer.CopyTo(buffer,0);
            if (status == NTStatus.STATUS_SUCCESS)
                return count;
            throw new SMBClientException("Error on read") { NTStatus = status };
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _fileStore.WriteFile(out var written, _handle, offset, buffer);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _fileStore.CloseFile(_handle);
        }
    }
}
