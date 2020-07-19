using ICSharpCode.SharpZipLib.Zip;
using Quokka.RISCV.Integration.DTO;
using Quokka.RISCV.Integration.Generator;
using Quokka.RISCV.Integration.RuleHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Quokka.RISCV.Integration.Engine
{
    public class Toolchain : IDisposable
    {
        Guid _correlationId;

        public string RootPath { get; private set; }

        private List<RuleHandler> _rules = new List<RuleHandler>();

        public Toolchain(Guid correlationId)
        {
            Console.WriteLine($"======================================================");
            Console.WriteLine($"Toolchain request {correlationId}");

            _correlationId = correlationId;
            RootPath = Path.Combine(Path.GetTempPath(), _correlationId.ToString());
            Directory.CreateDirectory(RootPath);
        }

        public void SaveSnapshot(FSSnapshot fsSnashot)
        {
            new FSManager(RootPath).SaveSnapshot(fsSnashot);
        }

        public FSSnapshot LoadSnapshot(ExtensionClasses classes)
        {
            return new FSManager(RootPath)
                .LoadSnapshot(
                    classes, 
                    _rules
                        .SelectMany(r => r.MatchingFiles())
                        .ToHashSet());
        }

        public void SetupRules(IEnumerable<FileRule> rules)
        {
            DisposeRules();

            _rules = new RulesManager(RootPath).CreateRules(rules);
        }

        static void FSCall(CommandLineInfo commandLine)
        {
            Console.WriteLine($"Running [{commandLine.FileName} {commandLine.Arguments}]");

            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = commandLine.FileName,
                    Arguments = commandLine.Arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var process = new Process()
                {
                    StartInfo = psi
                };

                process.Start();

                string result = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine($"Completed with {process.ExitCode}");
                Console.WriteLine($"Stdout: {result}");
                Console.WriteLine($"Stderror {errors}");

                if (process.ExitCode != 0)
                    throw new Exception(errors);
            }
            catch(Exception ex)
            {
                throw new Exception($"Exception running {commandLine.FileName} {commandLine.Arguments}", ex);
            }
        }

        public void Invoke(IEnumerable<ToolchainOperation> operations)
        {
            var current = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(RootPath);

                foreach (var cmd in operations)
                {
                    switch (cmd)
                    {
                        case CommandLineInfo commandLine:
                        {
                            FSCall(commandLine);
                        }   break;

                        case ResetRules rst:
                        {
                            _rules.ForEach(r => r.Reset());
                        }   break;
                    }
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(current);
            }
        }

        #region IDisposable Support

        void DisposeRules()
        {
            if (_rules != null)
            {
                _rules.ForEach(r => r.Dispose());
            }

            _rules = null;
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeRules();
                    Directory.Delete(RootPath, true);
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public static InvokeResponse Invoke(InvokeRequest request)
        {
            var response = new InvokeResponse()
            {
                CorrelationId = request.CorrelationId
            };

            using (var toolchain = new Toolchain(request.CorrelationId))
            {
                toolchain.SaveSnapshot(request.Source);
                toolchain.SetupRules(request.ResultRules);
                toolchain.Invoke(request.Operations);

                response.Result = toolchain.LoadSnapshot(request.ExtensionClasses);
            }

            return response;
        }

        public static async Task<byte[]> Make(Stream zipStream, string target)
        {
            var resultId = Guid.NewGuid();
            var zipFolder = Path.Combine(Path.GetTempPath(), $"{resultId}");
            var zipFile = Path.Combine(Path.GetTempPath(), $"{resultId}.zip");
            var currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                await zipFile.CreateFromStreamAsync(zipStream);

                // extract to temp folder
                zipFile.ExtractZip(zipFolder);
                zipFile.DeleteFileIfExists();

                Directory.SetCurrentDirectory(zipFolder);
                var bashInvoke = new BashInvocation($"make {target}");
                FSCall(bashInvoke);

                // zip and return result
                zipFolder.CompressFolder(zipFile);
                return File.ReadAllBytes(zipFile);
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
                zipFolder.DeleteDirectoryIfExists();
                zipFile.DeleteFileIfExists();
            }
        }

        public static byte[] Asm(string asmSource)
        {
            var requestId = Guid.NewGuid();
            var tempPath = Path.Combine(Path.GetTempPath(), requestId.ToString());
            var currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.CreateDirectory(tempPath);
                var templates = IntegrationTemplatesLoader.AsmTemplates;
                var fs = new FSManager(tempPath);
                fs.SaveSnapshot(templates);
                File.WriteAllText(Path.Combine(tempPath, "test.S"), asmSource);
                Directory.SetCurrentDirectory(tempPath);
                var cmdInvocation = new BashInvocation("make bin");
                FSCall(cmdInvocation);
                return File.ReadAllBytes("firmware.bin");
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }
    }
}
