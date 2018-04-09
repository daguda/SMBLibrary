using System;
using System.Collections.Generic;
using System.Text;

namespace SMBLibrary.Client
{
    public class SMBClientException : ApplicationException
    {
        public NTStatus NTStatus { get; set; }
        public FileStatus FileStatus { get; set; }

        public SMBClientException(string message) : base(message)
        {
        }

        public SMBClientException(string message, NTStatus ntStatus) : base(string.Format("{0} ({1})", message , ntStatus))
        {
            NTStatus = ntStatus;
        }

        public SMBClientException(string message, FileStatus fileStatus) : base(string.Format("{0} ({1})", message, fileStatus))
        {
            FileStatus = fileStatus;
        }
    }
}
