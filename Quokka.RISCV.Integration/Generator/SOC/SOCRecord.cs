using System;
using System.Text;

namespace Quokka.RISCV.Integration.Generator.SOC
{
    public class SOCRecord
    {
        public int SegmentBits { get; set; } = 8;

        public string SoftwareName { get; set; }
        public string HardwareName { get; set; }
        public uint Segment { get; set; }
        public uint Depth { get; set; }
        public string Template { get; set; }
        public Type DataType { get; set; }

        public uint Width => SOCTypeLookups.DataSize(DataType);
        public string CType => SOCTypeLookups.CType(DataType);
    }
}
