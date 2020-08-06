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
                { typeof(sbyte),     "char" },
                { typeof(byte),     "unsigned char" },
                { typeof(short),    "short" },
                { typeof(ushort),   "unsigned short" },
                { typeof(int),      "int" },
                { typeof(uint),     "unsigned int" },
            };

            if (!sizes.ContainsKey(data))
                throw new Exception($"Unsupported data type: {data}");

            return sizes[data];
        }
    }
}
