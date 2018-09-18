using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TSClientInfo
{

    public class UserLogins
    {

        [DllImport("wtsapi32.dll")]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        static extern Int32 WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
            [MarshalAs(UnmanagedType.U4)] Int32 Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
            System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType
        }
        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        //Structure for TS Client IP Address 
        [StructLayout(LayoutKind.Sequential)]
        private struct _WTS_CLIENT_ADDRESS
        {
            public int AddressFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Address;
        }

        public struct TsSession
        {
            public int SessionID;
            public string StationName;
            public string ConnectionState;
            public string UserName;
            public string DomainName;
            public string ClientName;
            public string IpAddress;
        }


        public static IntPtr OpenServer(String Name)
        {
            IntPtr server = WTSOpenServer(Name);
            return server;
        }
        public static void CloseServer(IntPtr ServerHandle)
        {
            WTSCloseServer(ServerHandle);
        }
        public static IEnumerable<UserInfo> GetUsers(String ServerName)
        {
            IntPtr serverHandle = IntPtr.Zero;
            List<String> resultList = new List<string>();
            serverHandle = OpenServer(ServerName);

            try
            {
                IntPtr SessionInfoPtr = IntPtr.Zero;
                IntPtr userPtr = IntPtr.Zero;
                IntPtr domainPtr = IntPtr.Zero;
                //IntPtr clientnamePtr = IntPtr.Zero;
                
                TsSession oTsSession = new TsSession();
                IntPtr addrPtr = IntPtr.Zero;
                uint returned = 0; ;

                Int32 sessionCount = 0;
                Int32 retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref SessionInfoPtr, ref sessionCount);
                Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                Int32 currentSession = (int)SessionInfoPtr;
                uint bytes = 0;

                if (retVal != 0)
                {
                    for (int i = 0; i < sessionCount; i++)
                    {
                        WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)currentSession, typeof(WTS_SESSION_INFO));
                        currentSession += dataSize;

                        WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out bytes);
                        WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out bytes);
                        //WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSClientAddress, out clientnamePtr, out bytes);

                        if (WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSClientAddress, out addrPtr, out returned) == true)
                        {
                            _WTS_CLIENT_ADDRESS obj = new _WTS_CLIENT_ADDRESS();
                            obj = (_WTS_CLIENT_ADDRESS)Marshal.PtrToStructure(addrPtr, obj.GetType());
                            oTsSession.IpAddress = obj.Address[2] + "." + obj.Address[3] + "." + obj.Address[4] + "." + obj.Address[5];

                            
                        }

                        yield return new UserInfo
                        {
                            Domain = Marshal.PtrToStringAnsi(domainPtr),
                            User = Marshal.PtrToStringAnsi(userPtr),
                            ClientIP = oTsSession.IpAddress,
                            SessionID = si.SessionID,
                            ClientName = si.pWinStationName 
                        };

                        WTSFreeMemory(userPtr);
                        WTSFreeMemory(domainPtr);
                        WTSFreeMemory(addrPtr);
                    }

                    WTSFreeMemory(SessionInfoPtr);
                }
            }
            finally
            {
                CloseServer(serverHandle);
            }
        }
    }
}
