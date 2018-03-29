using System;
using System.Collections.Generic;
using System.Text;

namespace SMBLibrary.Client
{
    public class SMBClient : IDisposable
    {
        public Enums.SMBClientVersion Version { get; private set; }
        protected ISMBClient Client { get; private set; }

        public SMBClient(Enums.SMBClientVersion version)
        {
            Version = version;
            switch (Version)
            {
                case Enums.SMBClientVersion.Version1:
                    Client = new SMB1Client();
                    break;
                case Enums.SMBClientVersion.Version2:
                    Client = new SMB2Client();
                    break;
                default:
                    throw new NotImplementedException();                    
            }
        }

        public Enums.SMBClientStatus Login(System.Net.IPAddress address, string username, string password, string domain = "")
        {
            if (!Client.Connect(address, SMBTransportType.DirectTCPTransport))
            {
                return Enums.SMBClientStatus.CantConnect;
            }
            if (Client.Login(domain, username, password) != NTStatus.STATUS_SUCCESS)
            {
                return Enums.SMBClientStatus.LoginFailed;
            }
            return Enums.SMBClientStatus.LoggedIn;
        }

        public void Logoff()
        {
            Client.Logoff();            
        }

        ISMBFileStore _fileStore;

        public void OpenShare(string share)
        {            
            _fileStore = Client.TreeConnect(share, out var status);
            if (status != NTStatus.STATUS_SUCCESS)
                throw new SMBClientException("Open share failed") { NTStatus = status };
        }

        public void CloseShare()
        {
            _fileStore?.Disconnect();
            _fileStore = null;
        }

        public SMBClientStream GetFileReader(string path, SMBLibrary.ShareAccess shareAccess)
        {
            CheckFileStore();
            _fileStore.CreateFile(out object handle, out var fileStatus, path, AccessMask.GENERIC_READ, 0, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
            if (fileStatus != FileStatus.FILE_OPENED)
                throw new SMBClientException("Error in GetFileReader") { FileStatus = fileStatus };
            return new SMBClientStream(_fileStore, handle);
        }

        private void CheckFileStore()
        {
            if (_fileStore == null)
                throw new SMBClientException("call OpenShare first");
        }

        public List<ListEntry> ListContent(string path)
        {
            CheckFileStore();
            _fileStore.CreateFile(out object handle, out var fileStatus, path, AccessMask.GENERIC_READ, 0, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
            if (fileStatus != FileStatus.FILE_OPENED)
                throw new SMBClientException("Error in ListContent") { FileStatus = fileStatus };
            try
            {                
                _fileStore.QueryDirectory(out var items, handle, "*", FileInformationClass.FileDirectoryInformation);
                var lst = new List<ListEntry>();
                foreach (FileDirectoryInformation i in items)
                {
                    if (i.FileName == "." || i.FileName == "..")
                        continue;
                    var li = new ListEntry();
                    li.Name = i.FileName;
                    li.Size = i.AllocationSize;
                    li.Attributes = i.FileAttributes;
                    lst.Add(li);
                }
                return lst;
            }
            finally
            {
                _fileStore.CloseFile(handle);
            }
        }

        public List<string> Shares
        {
            get
            {
                var lst = Client.ListShares(out var status);
                if (status == NTStatus.STATUS_PENDING)
                {
                    lst = Client.ListShares(out status);
                }
                return lst;
            }
        }

        public void Dispose()
        {            
            Client?.Disconnect();
            Client = null;
        }
    }
}
