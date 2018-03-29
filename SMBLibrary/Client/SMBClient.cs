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

        public void Dispose()
        {
            Client?.Disconnect();
            Client = null;
        }
    }
}
