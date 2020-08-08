using System;
using System.Collections.Generic;

namespace Quokka.RISCV.Integration.Generator.SOC
{
    public class SOCTypeLookups
    {
        public static uint DataSize(Type data)
        {
            var sizes = new Dictionary<Type, uint>()
            {
                { typeof(char),     16 },
                { typeof(sbyte),    8 },
                { typeof(byte),     8 },
                { typeof(short),    16 },
                { typeof(ushort),   16 },
                { typeof(int),      32 },
                { typeof(uint),     32 },
                { typeof(long),      64 },
                { typeof(ulong),     64 },
            };

            if (!sizes.ContainsKey(data))
                throw new Exception($"Unsupported data type: {data}");

            return sizes[data];
        }

        public static string CType(Type data)
        {
            var sizes = new Dictionary<Type, string>()
            {
                { typeof(char),     "wchar_t" },
                { typeof(sbyte),     "int8_t" },
                { typeof(byte),     "uint8_t" },
                { typeof(short),    "int16_t" },
                { typeof(ushort),   "uint16_t" },
                { typeof(int),      "int32_t" },
                { typeof(uint),     "uint32_t" },
                { typeof(long),      "int64_t" },
                { typeof(ulong),     "uint64_t" },
            };

            if (!sizes.ContainsKey(data))
                throw new Exception($"Unsupported data type: {data}");

            return sizes[data];
        }
    }
}
