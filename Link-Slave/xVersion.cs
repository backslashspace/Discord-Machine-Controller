using System;

namespace Link_Slave
{
    internal readonly struct xVersion
    {
        internal xVersion(UInt16 major, UInt16 minor, UInt16 build, UInt16 revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        internal xVersion(Version version)
        {
            Major = unchecked((UInt16)version.Major);
            Minor = unchecked((UInt16)version.Minor);
            Build = unchecked((UInt16)version.Build);
            Revision = unchecked((UInt16)version.Revision);
        }

        internal readonly UInt16 Major;
        internal readonly UInt16 Minor;
        internal readonly UInt16 Build;
        internal readonly UInt16 Revision;

        public override String ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Revision}";
        }

        internal static xVersionCompareResult Compare(ref xVersion v1, ref xVersion v2)
        {
            if (v1.Major != v2.Major)
            {
                if (v1.Major > v2.Major)
                {
                    return xVersionCompareResult.IsGreater;
                }

                return xVersionCompareResult.IsLess;
            }

            if (v1.Minor != v2.Minor)
            {
                if (v1.Minor > v2.Minor)
                {
                    return xVersionCompareResult.IsGreater;
                }

                return xVersionCompareResult.IsLess;
            }

            if (v1.Build != v2.Build)
            {
                if (v1.Build > v2.Build)
                {
                    return xVersionCompareResult.IsGreater;
                }

                return xVersionCompareResult.IsLess;
            }

            if (v1.Revision != v2.Revision)
            {
                if (v1.Revision > v2.Revision)
                {
                    return xVersionCompareResult.IsGreater;
                }

                return xVersionCompareResult.IsLess;
            }

            return xVersionCompareResult.IsEqual;
        }

        internal static Byte[] GetBytes(ref xVersion version)
        {
            Byte[] output = new Byte[8];

            Buffer.BlockCopy(BitConverter.GetBytes(version.Major), 0, output, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(version.Minor), 0, output, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(version.Build), 0, output, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(version.Revision), 0, output, 6, 2);

            return output;
        }

        internal static xVersion GetXVersion(ref Byte[] version)
        {
            xVersion result = new
                (
                    BitConverter.ToUInt16(version, 0),
                    BitConverter.ToUInt16(version, 2),
                    BitConverter.ToUInt16(version, 4),
                    BitConverter.ToUInt16(version, 6)
                );

            return result;
        }
    }

    internal enum xVersionCompareResult
    {
        IsEqual = 0,
        IsGreater = 1,
        IsLess = 2,
    }
}