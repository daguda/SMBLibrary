using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMBLibrary.Client
{
    public class SMBClientFileStream : System.IO.Stream
    {
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { return _length; } }
        public override long Position { get; set; }

        ISMBFileStore _fileStore;
        object _handle;
        long _length;

        public SMBClientFileStream(ISMBFileStore fileStore, object handle)
        {
            _fileStore = fileStore;
            _handle = handle;
            _fileStore.GetFileInformation(out var result, _handle, FileInformationClass.FileStandardInformation);
            _length = ((FileStandardInformation)result).EndOfFile;
        }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var myBuffer = new byte[count];
            if (count + offset + Position > Length)
                count = (int)(Length - (Position));
            var status = _fileStore.ReadFile(out myBuffer, _handle, Position, count);
            if (myBuffer != null)
            {
                myBuffer.CopyTo(buffer, 0);
                if (status == NTStatus.STATUS_SUCCESS)
                {
                    Position += count;
                    return count;
                }
            }
            throw new SMBClientException("Error on read", status);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var status = _fileStore.WriteFile(out var written, _handle, Position, buffer);
            if (status != NTStatus.STATUS_SUCCESS)
                throw new SMBClientException("Error on write", status);
            Position += written;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _fileStore.CloseFile(_handle);
        }
    }
}
