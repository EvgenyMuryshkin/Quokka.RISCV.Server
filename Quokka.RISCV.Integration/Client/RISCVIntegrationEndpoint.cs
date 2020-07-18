namespace Quokka.RISCV.Integration.Client
{
    public class RISCVIntegrationEndpoint
    {
        public string Host = "http://localhost";
        public int Port = 15000;

        public string HealthCheck => $"{Host}:{Port}/api/HealthCheck";
        public string RISCV => $"{Host}:{Port}/api/RISCV";

    }
}
