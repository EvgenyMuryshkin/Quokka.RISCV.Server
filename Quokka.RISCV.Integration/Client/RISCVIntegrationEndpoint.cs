namespace Quokka.RISCV.Integration.Client
{
    public class RISCVIntegrationEndpoint
    {
        public string Host = "http://localhost";
        public int Port = 15000;

        public string URL => $"{Host}:{Port}/api/riscv";
    }
}
