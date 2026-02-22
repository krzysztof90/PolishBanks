using ProtoBuf;
using System;
using System.IO;
using System.IO.Compression;

namespace BankService.Fingerprints
{
    public static class FingerprintManager
    {
        public static F CreateFingerprintFromBase64<F>(string base64) where F : FingerprintBase
        {
            byte[] deflated = Convert.FromBase64String(base64);

            byte[] protoBytes = InflateZlib(deflated);

            //File.WriteAllBytes("proto.bin", protoBytes);
            //protoc --decode_raw < proto.bin

            F fingerprint = ProtoBytesToFingerprint<F>(protoBytes);
            return fingerprint;
        }

        public static string CreateBase64FromFingerprint<F>(F fingerprint, CompressionLevel compressionLevel = CompressionLevel.Fastest) where F : FingerprintBase
        {
            byte[] protoBytes = FingerprintToProtoBytes(fingerprint);

            //byte[] deflated = ToZlib(protoBytes, compressionLevel);
            byte[] deflated = DeflateZlib(protoBytes, compressionLevel);

            string base64 = Convert.ToBase64String(deflated);
            return base64;
        }

        private static byte[] FingerprintToProtoBytes<F>(F fingerprint) where F : FingerprintBase
        {
            byte[] protoBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, fingerprint);
                protoBytes = ms.ToArray();
            }
            return protoBytes;
        }

        private static F ProtoBytesToFingerprint<F>(byte[] protoBytes) where F : FingerprintBase
        {
            F fingerprint;
            using (MemoryStream ms = new MemoryStream(protoBytes))
            {
                fingerprint = Serializer.Deserialize<F>(ms);
            }
            return fingerprint;
        }

        private static byte[] InflateZlib(byte[] deflated)
        {
            //using (MemoryStream input = new MemoryStream(compressed))
            using (MemoryStream input = new MemoryStream(deflated, 2, deflated.Length - 2))
            using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                deflate.CopyTo(output);

                byte[] protoBytes = output.ToArray();
                return protoBytes;
                //return Encoding.UTF8.GetString(protoBytes);
            }
        }

        private static byte[] DeflateZlib(byte[] data, CompressionLevel compressionLevel)
        {
            using (MemoryStream output = new MemoryStream())
            {
                output.WriteByte(0x78);
                output.WriteByte(0x9C);

                using (DeflateStream deflate = new DeflateStream(output, compressionLevel, leaveOpen: true))
                {
                    deflate.Write(data, 0, data.Length);
                }

                uint adler32 = Adler32(data);
                output.WriteByte((byte)(adler32 >> 24));
                output.WriteByte((byte)(adler32 >> 16));
                output.WriteByte((byte)(adler32 >> 8));
                output.WriteByte((byte)(adler32));

                return output.ToArray();
            }
        }

        private static byte[] ToZlib(byte[] data, CompressionLevel compressionLevel)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using (MemoryStream msOut = new MemoryStream())
            {
                byte[] deflated = ToDeflateRaw(data, compressionLevel);
                uint adler = Adler32(data);
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteByte(0x78);
                    ms.WriteByte(0x9C);
                    ms.Write(deflated, 0, deflated.Length);
                    ms.WriteByte((byte)((adler >> 24) & 0xFF));
                    ms.WriteByte((byte)((adler >> 16) & 0xFF));
                    ms.WriteByte((byte)((adler >> 8) & 0xFF));
                    ms.WriteByte((byte)(adler & 0xFF));
                    return ms.ToArray();
                }
            }
        }

        private static uint Adler32(byte[] data)
        {
            const uint MOD = 65521;
            uint s1 = 1, s2 = 0;
            for (int i = 0; i < data.Length; i++)
            {
                s1 = (s1 + data[i]) % MOD;
                s2 = (s2 + s1) % MOD;
            }
            return (s2 << 16) | s1;
        }

        private static byte[] ToDeflateRaw(byte[] data, CompressionLevel compressionLevel)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using (MemoryStream output = new MemoryStream())
            {
                //using (GZipStream gs = new GZipStream(msOut, compressionLevel, leaveOpen: true))
                using (DeflateStream ds = new DeflateStream(output, compressionLevel, leaveOpen: true))
                {
                    ds.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }
    }
}
