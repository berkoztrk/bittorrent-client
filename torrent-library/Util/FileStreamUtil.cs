using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public static class FileStreamUtil
    {

        private const int FILE_SUPPORTS_SPARSE_FILES = 64;

        private const int FSCTL_SET_SPARSE = 0x000900c4;

        private const int FSCTL_SET_ZERO_DATA = 0x000980c8;

        public static void MakeSparse(this FileStream fileStream)
        {
            var bytesReturned = 0;
            var lpOverlapped = new NativeOverlapped();
            var result = DeviceIoControl(
                fileStream.SafeFileHandle,
                FSCTL_SET_SPARSE,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                ref bytesReturned,
                ref lpOverlapped);

            if (!result)
            {
                throw new Win32Exception();
            }
        }

        public static void SetSparseRange(this FileStream fileStream, long fileOffset, long length)
        {
            var fzd = new FILE_ZERO_DATA_INFORMATION();
            fzd.FileOffset = fileOffset;
            fzd.BeyondFinalZero = fileOffset + length;
            var lpOverlapped = new NativeOverlapped();
            var dwTemp = 0;

            var result = DeviceIoControl(
                fileStream.SafeFileHandle,
                FSCTL_SET_ZERO_DATA,
                ref fzd,
                Marshal.SizeOf(typeof(FILE_ZERO_DATA_INFORMATION)),
                IntPtr.Zero,
                0,
                ref dwTemp,
                ref lpOverlapped);
            if (!result)
            {
                throw new Win32Exception();
            }
        }

        public static bool SupportedOnVolume(string filename)
        {
            var targetVolume = Path.GetPathRoot(filename);
            var fileSystemName = new StringBuilder(300);
            var volumeName = new StringBuilder(300);
            uint lpFileSystemFlags;
            uint lpVolumeSerialNumber;
            uint lpMaxComponentLength;

            var result = GetVolumeInformationW(
                targetVolume,
                volumeName,
                (uint)volumeName.Capacity,
                out lpVolumeSerialNumber,
                out lpMaxComponentLength,
                out lpFileSystemFlags,
                fileSystemName,
                (uint)fileSystemName.Capacity);
            if (!result)
            {
                throw new Win32Exception();
            }

            return (lpFileSystemFlags & FILE_SUPPORTS_SPARSE_FILES) == FILE_SUPPORTS_SPARSE_FILES;
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            int dwIoControlCode,
            IntPtr InBuffer,
            int nInBufferSize,
            IntPtr OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            [In] ref NativeOverlapped lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            int dwIoControlCode,
            ref FILE_ZERO_DATA_INFORMATION InBuffer,
            int nInBufferSize,
            IntPtr OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            [In] ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", EntryPoint = "GetVolumeInformationW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumeInformationW(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpVolumeNameBuffer,
            uint nVolumeNameSize,
            out uint lpVolumeSerialNumber,
            out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpFileSystemNameBuffer,
            uint nFileSystemNameSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct FILE_ZERO_DATA_INFORMATION
        {
            public long FileOffset;

            public long BeyondFinalZero;
        }
    }
}
