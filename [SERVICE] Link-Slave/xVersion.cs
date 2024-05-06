using System;

namespace Link_Slave
{
    internal readonly struct xVersion
    {
        internal xVersion(UInt32 major, UInt32 minor, UInt32 build, UInt32 revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        internal xVersion(Version version)
        {
            Major = unchecked((UInt32)version.Major);
            Minor = unchecked((UInt32)version.Minor);
            Build = unchecked((UInt32)version.Build);
            Revision = unchecked((UInt32)version.Revision);
        }

        internal readonly UInt32 Major;
        internal readonly UInt32 Minor;
        internal readonly UInt32 Build;
        internal readonly UInt32 Revision;

        public override String ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Revision}";
        }

        internal static xVersionCompareResult Compare(ref readonly xVersion v1, ref readonly xVersion v2)
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

        internal static Byte[] GetBytes(ref readonly xVersion version)
        {
            Byte[] output = new Byte[16];

            Buffer.BlockCopy(BitConverter.GetBytes(version.Major), 0, output, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(version.Minor), 0, output, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(version.Build), 0, output, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(version.Revision), 0, output, 12, 4);

            return output;
        }

        internal static xVersion GetXVersion(ref readonly Byte[] version)
        {
            if (version.Length != 16)
            {
                throw new ArgumentException($"Unexpected length: 16 but was {version.Length}");
            }

            xVersion result = new
                (
                    BitConverter.ToUInt32(version, 0),
                    BitConverter.ToUInt32(version, 4),
                    BitConverter.ToUInt32(version, 8),
                    BitConverter.ToUInt32(version, 12)
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