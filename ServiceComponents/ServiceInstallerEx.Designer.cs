using System;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Collections;
using System.Diagnostics;


namespace WCFExample.ServiceComponents
{
    /// <summary>
    /// Class to extend Service installer.
    /// To adjust privileges for shutdown see:
    /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/shutting_down.asp
    /// </summary>
    public partial class ServiceInstallerEx
    {
        private const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const int SERVICE_ALL_ACCESS = 0xF01FF;
        private const int SERVICE_CONFIG_DESCRIPTION = 0x1;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
        private const int SERVICE_NO_CHANGE = -1;
        private const int ERROR_ACCESS_DENIED = 5;

        private const int TOKEN_ADJUST_PRIVILEGES = 32;
        private const int TOKEN_QUERY = 8;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        private const int SE_PRIVILEGE_ENABLED = 2;

        private string description = "";					
        private int failResetTime = SERVICE_NO_CHANGE;		
        private string failRebootMsg = "";					
        private string failRunCommand = "";					
        private bool setDescription = false;			
        private bool setFailActions = false;
        private bool startOnInstall = false;
        private int startTimeout = 15000;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ArrayList FailureActions;

        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public int FailCountResetTime
        {
            set
            {
                failResetTime = value;
                setFailActions = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public string FailRebootMsg
        {
            set
            {
                failRebootMsg = value;
                setFailActions = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public string FailRunCommand
        {
            set
            {
                failRunCommand = value;
                setFailActions = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public bool StartOnInstall
        {
            set
            {
                this.startOnInstall = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public int StartTimeout
        {
            set
            {
                this.startTimeout = value;
            }
        }

        #endregion

        #region Win32 Interop

        /// <summary>
        /// Structure for settting the description of the service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_DESCRIPTION
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public string lpDescription;
        }

        /// <summary>
        /// Structure for settting the failure actions of a service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_FAILURE_ACTIONS
        {

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public int dwResetPeriod;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public string lpRebootMsg;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public string lpCommand;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public int cActions;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public int lpsaActions;

        }

        /// <summary>
        /// Open the service control manager
        /// </summary>
        /// <param name="lpMachineName"></param>
        /// <param name="lpDatabaseName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll")]
        public static extern
            IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        /// <summary>
        /// Open a service instance
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <param name="lpServiceName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll")]
        public static extern IntPtr
            OpenService(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

        /// <summary>
        /// Lock the service database to perform write operations.
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll")]
        public static extern IntPtr
            LockServiceDatabase(IntPtr hSCManager);

        /// <summary>
        /// Change the service config for the failure actions.
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwInfoLevel"></param>
        /// <param name="lpInfo"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        public static extern bool
            ChangeServiceFailureActions(IntPtr hService, int dwInfoLevel,
            [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS lpInfo);

        /// <summary>
        /// Change the service config for the service description
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwInfoLevel"></param>
        /// <param name="lpInfo"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        public static extern bool
            ChangeServiceDescription(IntPtr hService, int dwInfoLevel,
            [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION lpInfo);

        /// <summary>
        /// Close a service related handle.
        /// </summary>
        /// <param name="hSCObject"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll")]
        public static extern bool
            CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// Unlock the service database.
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll")]
        public static extern bool
            UnlockServiceDatabase(IntPtr hSCManager);

        [DllImport("kernel32.dll")]
        public static extern int
            GetLastError();



        #endregion

        #region Win32 Shutdown Privilege Interop

        /// <summary>
        /// Structure for setting shutdown privileges
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public long Luid;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public int Attributes;
        }

        /// <summary>
        /// Structure for setting shutdown privileges
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TOKEN_PRIVILEGES
        {

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public int PrivilegeCount;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public LUID_AND_ATTRIBUTES Privileges;

        }

        /// <summary>
        /// Adjust token privileges. Required when specifying the reboot option
        /// </summary>
        /// <param name="TokenHandle"></param>
        /// <param name="DisableAllPrivileges"></param>
        /// <param name="NewState"></param>
        /// <param name="BufferLength"></param>
        /// <param name="PreviousState"></param>
        /// <param name="ReturnLength"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "4#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "5#"), DllImport("advapi32.dll")]
        public static extern bool
            AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength,
           IntPtr PreviousState, ref int ReturnLength);


        /// <summary>
        /// Gets privilege code for specified name
        /// </summary>
        /// <param name="lpSystemName"></param>
        /// <param name="lpName"></param>
        /// <param name="lpLuid"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll")]
        public static extern bool
            LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        /// <summary>
        /// Opens token for security and privilege for a specidied process hanadle
        /// </summary>
        /// <param name="ProcessHandle"></param>
        /// <param name="DesiredAccess"></param>
        /// <param name="TokenHandle"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "2#"), DllImport("advapi32.dll")]
        public static extern bool
            OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        /// <summary>
        /// Get current process handle
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr
            GetCurrentProcess();

        /// <summary>
        /// Close handle
        /// </summary>
        /// <param name="hndl"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool
            CloseHandle(IntPtr hndl);

        

        #endregion


        private string logMsgBase = string.Empty;

        private void LogInstallMessage(EventLogEntryType logLevel, string msg)
        {
            Console.WriteLine(msg);

            try
            {
                EventLog.WriteEntry(base.ServiceName, msg, logLevel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private bool GrandShutdownPrivilege()
        {

            bool retRslt = false;

            IntPtr hToken = IntPtr.Zero;
            IntPtr myProc = IntPtr.Zero;

            TOKEN_PRIVILEGES tkp = new TOKEN_PRIVILEGES();

            long Luid = 0;
            int retLen = 0;

            try
            {

                myProc = GetCurrentProcess();
                bool rslt = OpenProcessToken(myProc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref hToken);
                if (!rslt) return retRslt;

                LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref Luid);

                tkp.PrivilegeCount = 1;
                tkp.Privileges.Luid = Luid;
                tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

                rslt = AdjustTokenPrivileges(hToken, false, ref tkp, 0, IntPtr.Zero, ref retLen);

                if (GetLastError() != 0)
                {

                    throw new Exception("Failed to grant shutdown privilege");

                }

                retRslt = true;

            }
            catch (Exception ex)
            {

                LogInstallMessage(EventLogEntryType.Error, logMsgBase + ex.Message);

            }
            finally
            {

                if (hToken != IntPtr.Zero)
                {

                    CloseHandle(hToken);

                }
            }

            return retRslt;

        }

        private void UpdateServiceConfig(object sender, InstallEventArgs e)
        {
            this.setFailActions = false;

            int numActions = FailureActions.Count;

            if (numActions > 0)
            {
                setFailActions = true;
            }

            if (!(this.setDescription || this.setFailActions)) return;

            IntPtr scmHndl = IntPtr.Zero;
            IntPtr svcHndl = IntPtr.Zero;
            IntPtr tmpBuf = IntPtr.Zero;
            IntPtr svcLock = IntPtr.Zero;

            bool rslt = false;

            try
            {
                // Open the service control manager
                scmHndl = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);

                if (scmHndl.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error, logMsgBase + "Failed to Open Service Control Manager");
                    return;
                }

                // Lock the Service Database
                svcLock = LockServiceDatabase(scmHndl);

                if (svcLock.ToInt32() <= 0)
                {

                    LogInstallMessage(EventLogEntryType.Error, logMsgBase + "Failed to Lock Service Database for Write");
                    return;
                }

                // Open the service
                svcHndl = OpenService(scmHndl, base.ServiceName, SERVICE_ALL_ACCESS);

                if (svcHndl.ToInt32() <= 0)
                {

                    LogInstallMessage(EventLogEntryType.Information, logMsgBase + "Failed to Open Service ");
                    return;
                }

                // Set service failure actions. 
                // NB: The API lets you set as many as needed, but the Service Control Manager GUI 
                // only displays first 3.
                // Also the API allows granularity of seconds whereas GUI only shows days and minutes.

                if (this.setFailActions)
                {

                    int[] actions = new int[numActions * 2];
                    int currInd = 0;
                    bool needShutdownPrivilege = false;

                    foreach (FailureAction fa in FailureActions)
                    {

                        actions[currInd] = (int)fa.Type;
                        actions[++currInd] = fa.Delay;
                        currInd++;

                        if (fa.Type == RecoverAction.Reboot)
                            needShutdownPrivilege = true;

                    }

                    // If need shutdown privilege, then grant it to this process
                    if (needShutdownPrivilege)
                    {
                        rslt = this.GrandShutdownPrivilege();
                        if (!rslt) 
                            return;
                    }

                    // Need to pack 8 bytes per struct
                    tmpBuf = Marshal.AllocHGlobal(numActions * 8);

                    // Move array into marshallable pointer
                    Marshal.Copy(actions, 0, tmpBuf, numActions * 2);

                    // Set the SERVICE_FAILURE_ACTIONS struct
                    SERVICE_FAILURE_ACTIONS sfa = new SERVICE_FAILURE_ACTIONS();

                    sfa.cActions = numActions;
                    sfa.dwResetPeriod = this.failResetTime;
                    sfa.lpCommand = this.failRunCommand;
                    sfa.lpRebootMsg = this.failRebootMsg;
                    sfa.lpsaActions = tmpBuf.ToInt32();


                    // Call the ChangeServiceFailureActions() abstraction of ChangeServiceConfig2()
                    rslt = ChangeServiceFailureActions(svcHndl, SERVICE_CONFIG_FAILURE_ACTIONS, ref sfa);

                    //Check the return
                    if (!rslt)
                    {
                        int err = GetLastError();

                        if (err == ERROR_ACCESS_DENIED)
                            throw new Exception(logMsgBase + "Access Denied while setting Failure Actions");
                    }

                    // Free the memory
                    Marshal.FreeHGlobal(tmpBuf); tmpBuf = IntPtr.Zero;

                    LogInstallMessage(EventLogEntryType.Information, logMsgBase + "Successfully configured Failure Actions");

                }

                // Need to set the description field?
                if (this.setDescription)
                {

                    SERVICE_DESCRIPTION sd = new SERVICE_DESCRIPTION();
                    sd.lpDescription = this.description;

                    // Call the ChangeServiceDescription() abstraction of ChangeServiceConfig2()
                    rslt = ChangeServiceDescription(svcHndl, SERVICE_CONFIG_DESCRIPTION, ref sd);

                    // Error setting description?
                    if (!rslt)
                        throw new Exception(logMsgBase + "Failed to set description");

                    LogInstallMessage(EventLogEntryType.Information, logMsgBase + "Successfully set description");

                }

            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, ex.Message);
            }
            finally
            {

                if (scmHndl != IntPtr.Zero)
                {
                    // Unlock the service database
                    if (svcLock != IntPtr.Zero)
                    {
                        UnlockServiceDatabase(svcLock);
                        svcLock = IntPtr.Zero;
                    }

                    // Close the service control manager handle
                    CloseServiceHandle(scmHndl);
                    scmHndl = IntPtr.Zero;
                }

                // Close the service handle
                if (svcHndl != IntPtr.Zero)
                {
                    CloseServiceHandle(svcHndl);
                    svcHndl = IntPtr.Zero;
                }

                // Free the memory
                if (tmpBuf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tmpBuf);
                    tmpBuf = IntPtr.Zero;
                }


            } 

        }

        /// <summary>
        /// Start the service automatically after installation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartIfNeeded(object sender, InstallEventArgs e)
        {

            if (!this.startOnInstall)
                return;

            try
            {
                TimeSpan waitTo = new TimeSpan(0, 0, this.startTimeout);

                ServiceController sc = new ServiceController(base.ServiceName);

                // Start the service and wait for it to start
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, waitTo);
                sc.Close();

                LogInstallMessage(EventLogEntryType.Information, logMsgBase + " Service Started");

            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, logMsgBase + ex.Message);
            }

        }

    }

    /// <summary>
    /// Class to represent a failure action which consists of a recovery
    /// </summary>
    public class FailureAction
    {

        private RecoverAction type = RecoverAction.None;
        private int delay = 0;
        
        public FailureAction(){}

        public FailureAction(RecoverAction actionType, int actionDelay)
        {

            this.type = actionType;
            this.delay = actionDelay;

        }

        public RecoverAction Type
        {

            get { return type; }

            set
            {
                type = value;
            }

        }

        public int Delay
        {

            get { return delay; }

            set
            {
                delay = value;
            }

        }

    }



}

