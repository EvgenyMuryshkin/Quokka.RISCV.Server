using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Quokka.RISCV.Integration.DTO;
using Quokka.RISCV.Integration.Engine;
using Quokka.RISCV.Integration.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quokka.RISCV.Integration.Client
{
    public class RISCVIntegrationClient
    {
        public static string LocalToolchainLocation()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var locations = path.Split(new char[] { ':', ';' });

            var localNames = new string[] { "riscv32-unknown-elf-gcc", "riscv32-unknown-elf-gcc.exe" };

            foreach (var location in locations)
            {
                foreach (var name in localNames)
                {
                    var gccPath = Path.Combine(location, name);
                    if (File.Exists(gccPath))
                        return location;
                }
            }

            return null;
        }

        public static async Task<RISCVIntegrationContext> Run(RISCVIntegrationContext context)
        {
            using (var client = new HttpClient() )
            {
                var request = new InvokeRequest()
                {
                    ExtensionClasses = context.ExtensionClasses,
                    Source = context.SourceSnapshot,
                    Operations = context.Operations,
                    ResultRules = context.ResultFileRules
                };

                var requestPayload = JsonHelper.Serialize(request);

                var test = JsonHelper.Deserialize<InvokeRequest>(requestPayload);

                var requestContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                var url = $"{context.Endpoint.RISCV}/Invoke";

                var response = await client.PostAsync(url, requestContent);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var exceptionPayload = await response.Content.ReadAsStringAsync();

                    throw new Exception($"Failed to post data to '{url}': {exceptionPayload}");
                }

                var responsePayload = await response.Content.ReadAsStringAsync();
                var invokeResponse = JsonHelper.Deserialize<InvokeResponse>(responsePayload);

                return context.WithResultSnapshot(invokeResponse.Result);
            }
        }

        public static IEnumerable<uint> ToInstructions(IEnumerable<byte> bytes)
        {
            while(bytes.Any())
            {
                yield return BitConverter.ToUInt32(bytes.Take(4).ToArray(), 0);
                bytes = bytes.Skip(4);
            }
        }

        public static async Task<uint[]> Asm(RISCVIntegrationEndpoint endpoint, string asmSource)
        {
            using (var client = new HttpClient())
            {
                var url = $"{endpoint.RISCV}/asm";
                var response = await client.PostAsync(url, new StringContent(asmSource));
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new Exception(message);
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                return ToInstructions(content).ToArray();
            }
        }

        public static async Task Make(RISCVIntegrationContext context)
        {
            if (HasLocalToolchain)
            {
                var currentDirectory = Directory.GetCurrentDirectory();

                try
                {
                    Directory.SetCurrentDirectory(context.RootFolder);
                    await Toolchain.Make(context.MakeTarget);
                }
                finally
                {
                    Directory.SetCurrentDirectory(currentDirectory);
                }
            }
            else
            {
                var zipFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                try
                {
                    using (var client = new HttpClient())
                    {
                        var url = $"{context.Endpoint.RISCV}/make/{context.MakeTarget}";
                        var zfe = new ZipEntryFactory { IsUnicodeText = true };
                        context.RootFolder.CompressFolder(zipFile);
                        var content = File.ReadAllBytes(zipFile);

                        var response = await client.PostAsync(url, new StreamContent(new MemoryStream(content)));
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            var exception = await response.Content.ReadAsStringAsync();
                            throw new Exception(exception);

                        }
                        content = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(zipFile, content);
                        zipFile.ExtractZip(context.RootFolder);
                    }
                }
                finally
                {
                    zipFile.DeleteFileIfExists();
                }
            }
        }

        public static async Task<bool> HealthCheck(RISCVIntegrationEndpoint endpoint)
        {
            var url = $"{endpoint.HealthCheck}/IsAlive";
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                Console.WriteLine($"RISCV toolchain is not available: {url}");
                return false;
            }
        }

        public static bool HasLocalToolchain
        {
            get
            {
                // perform API call on Windows
                return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }
        }
    }
}
