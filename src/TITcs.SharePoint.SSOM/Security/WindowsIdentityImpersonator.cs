using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace TITcs.SharePoint.SSOM.Security
{
    /// Impersonates a windows identity.
    /// Based on: http://msdn.microsoft.com/en-us/library/w070t6ka.aspx
    internal class WindowsIdentityImpersonator : IDisposable
    {
        #region fields and properties

        WindowsIdentity _newId;
        SafeTokenHandle _safeTokenHandle;
        WindowsImpersonationContext _impersonatedUser;

        public WindowsIdentity Identity { get { return _newId; } }

        #endregion

        #region events and methods

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public WindowsIdentityImpersonator(string Domain, string Username, string Password)
        {
            bool returnValue = LogonUser(Username, Domain, Password, 2, 0, out _safeTokenHandle);

            if (returnValue == false)
            {
                throw new UnauthorizedAccessException("Could not login as " + Domain + "\\" + Username + ".",
                    new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        public void BeginImpersonate()
        {
            _newId = new WindowsIdentity(_safeTokenHandle.DangerousGetHandle());
            _impersonatedUser = _newId.Impersonate();
        }

        public void EndImpersonate()
        {
            if (_newId != null)
            {
                _newId.Dispose();
            }
            if (_impersonatedUser != null)
            {
                _impersonatedUser.Dispose();
            }
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        #region IDisposable

        public void Dispose()
        {
            this.EndImpersonate();

            if (_safeTokenHandle != null)
            {
                _safeTokenHandle.Dispose();
            }
        }

        #endregion

        #endregion
    }

    #region SafeTokenHandle

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle() : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }

    #endregion
}