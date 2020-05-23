using Newtonsoft.Json;
using Quokka.RISCV.Integration.DTO;
using Quokka.RISCV.Integration.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Quokka.RISCV.Integration.Client
{
    public class RISCVIntegrationClient
    {
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
                var url = $"{context.Endpoint.URL}/Invoke";

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
                var url = $"{endpoint.URL}/asm";
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
    }
}
