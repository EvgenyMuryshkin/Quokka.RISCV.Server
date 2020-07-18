using Quokka.RISCV.Integration.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quokka.RISCV.Integration.Generator.SOC
{
    public class SOCGenerator
    {
        public FSTextFile SOCImport(IEnumerable<SOCRecord> data)
        {
            var content = new StringBuilder();
            foreach (var item in data)
            {
                var address = item.Segment << (32 - item.SegmentBits);

                if (item.Depth == 0)
                {
                    content.AppendLine($"#define {item.SoftwareName} (*(volatile {item.CType}*)0x{address.ToString("X8")})");
                }
                else
                {
                    content.AppendLine($"#define {item.SoftwareName} ((volatile {item.CType}*)0x{address.ToString("X8")})");
                    content.AppendLine($"#define {item.SoftwareName}_size {item.Depth.ToString()}");
                }
            }

            return new FSTextFile()
            {
                Name = "soc.h",
                Content = content.ToString()
            };
        }
    }
}
