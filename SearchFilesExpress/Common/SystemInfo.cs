using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SearchFiles.Common
{
    public class eCodified_SystemInfo
    {
        public ECpuArchitecture CpuArch { get { return cpuArch; } }
        public ECpuType CpuType { get { return cpuType; } }
        public uint NumberOfCpus { get { return sysInfo.dwNumberOfProcessors; } }
        public uint PageSize { get { return sysInfo.dwPageSize; } }
        public IntPtr MinAppAddress { get { return sysInfo.lpMinimumApplicationAddress; } }
        public IntPtr MaxAppAddress { get { return sysInfo.lpMaximumApplicationAddress; } }
        public UIntPtr ActiveCpuMask { get { return sysInfo.dwActiveProcessorMask; } }

        public eCodified_SystemInfo()
        {
            try
            {
                sysInfo.dwNumberOfProcessors = 0;
                cpuArch = ECpuArchitecture.UNKNOWN;
                cpuType = ECpuType.UNKNOWN;

                GetNativeSystemInfo(ref sysInfo);
                cpuArch = GetProcessorArchitecture();
                cpuType = GetProcessorType();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        public bool IsHighPerf()
        {
            return NumberOfCpus >= 5 && CpuArch != ECpuArchitecture.ARM;
        }

        public bool IsMediumPerf()
        {
            return NumberOfCpus == 4;
        }

        public bool IsLowPerf()
        {
            return NumberOfCpus <= 2 && (CpuArch == ECpuArchitecture.ARM || CpuArch == ECpuArchitecture.UNKNOWN);
        }
        
        public override string ToString()
        {
            string sysInfo = "";
            try
            {
                sysInfo += "CPU Architecture: " + CpuArchString();
                //sysInfo += "CPU Type: " + CpuTypeString();
                sysInfo += "Number of CPUs: " + NumberOfCpus.ToString() + "\r\n";
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
            return sysInfo;
        }

        public enum ECpuArchitecture : ushort
        {
            INTEL = 0,
            MIPS = 1,
            ALPHA = 2,
            PPC = 3,
            SHX = 4,
            ARM = 5,
            IA64 = 6,
            ALPHA64 = 7,
            MSIL = 8,
            AMD64 = 9,
            IA32_ON_WIN64 = 10,
            UNKNOWN = 0xFFFF
        }

        public string CpuArchString()
        {
            string procArchStr = "";
            try
            {
                switch (cpuArch)
                {
                    case ECpuArchitecture.INTEL:
                        procArchStr += "X86 (Intel 32-bit)";
                        break;
                    case ECpuArchitecture.MIPS:
                        procArchStr += "MIPS";
                        break;
                    case ECpuArchitecture.ALPHA:
                        procArchStr += "ALPHA (Alpha 32-bit)";
                        break;
                    case ECpuArchitecture.PPC:
                        procArchStr += "PPC (IBM Power PC)";
                        break;
                    case ECpuArchitecture.SHX:
                        procArchStr += "SHX";
                        break;
                    case ECpuArchitecture.ARM:
                        procArchStr += "ARM";
                        break;
                    case ECpuArchitecture.IA64:
                        procArchStr += "IA64 (Intel Itanium 64-bit)";
                        break;
                    case ECpuArchitecture.ALPHA64:
                        procArchStr += "ALPHA64 (Alpha 64-bit)";
                        break;
                    case ECpuArchitecture.MSIL:
                        procArchStr += "MSIL";
                        break;
                    case ECpuArchitecture.AMD64:
                        procArchStr += "X64 (AMD or Intel 64-bit)";
                        break;
                    case ECpuArchitecture.IA32_ON_WIN64:
                        procArchStr += "IA32_on_WIN64";
                        break;
                    case ECpuArchitecture.UNKNOWN:
                    default:
                        procArchStr += "UNKNOWN";
                        break;
                }
                procArchStr += "\r\n";
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
            return procArchStr;
        }

        public enum ECpuType : uint
        {
            INTEL_386,
            INTEL_486,
            INTEL_PENTIUM,
            INTEL_IA64 = 2200,
            AMD_X8664 = 8664,
            ARM = 5, // ???
            UNKNOWN = 0xFFFF
        }

        public string CpuTypeString()
        {
            string procTypeStr = "";
            try
            {
                switch (cpuType)
                {
                    case ECpuType.INTEL_386:
                        procTypeStr += "Intel 386";
                        break;
                    case ECpuType.INTEL_486:
                        procTypeStr += "Intel 486";
                        break;
                    case ECpuType.INTEL_PENTIUM:
                        procTypeStr += "Intel Pentium";
                        break;
                    case ECpuType.INTEL_IA64:
                        procTypeStr += "Intel IA64";
                        break;
                    case ECpuType.AMD_X8664:
                        procTypeStr += "X86 / X64";
                        break;
                    case ECpuType.ARM:
                        procTypeStr += "ARM";
                        break;
                    case ECpuType.UNKNOWN:
                    default:
                        procTypeStr += "UNKNOWN";
                        break;
                }
                procTypeStr += "\r\n";
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
            return procTypeStr;
        }

        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(ref _SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct _SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        private _SYSTEM_INFO sysInfo;
        private ECpuArchitecture cpuArch;
        private ECpuType cpuType;

        private ECpuArchitecture GetProcessorArchitecture()
        {
            try
            {
                return Enum.IsDefined(typeof(ECpuArchitecture), sysInfo.wProcessorArchitecture)
                    ? (ECpuArchitecture)sysInfo.wProcessorArchitecture
                    : ECpuArchitecture.UNKNOWN;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
            return ECpuArchitecture.UNKNOWN;
        }

        private ECpuType GetProcessorType()
        {
            try
            {
                return Enum.IsDefined(typeof(ECpuType), sysInfo.dwProcessorType)
                    ? (ECpuType)sysInfo.dwProcessorType
                    : ECpuType.UNKNOWN;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
            return ECpuType.UNKNOWN;
        }
    }
}
