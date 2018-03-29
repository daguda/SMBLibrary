using System;
using System.Collections.Generic;
using System.Text;

namespace SMBLibrary.Client
{
    public class ListEntry
    {
        public string Name { get; set; }
        public long Size { get; set; }        
        public FileAttributes Attributes { get; set; }
    }
}
