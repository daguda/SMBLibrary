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
    }
}
