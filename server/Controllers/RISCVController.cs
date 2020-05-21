using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quokka.RISCV.Integration.DTO;
using Quokka.RISCV.Integration.Engine;

namespace server.Controllers
{
    [Route("api/[controller]")]
    public class RISCVController : Controller
    {
        [HttpPost("[action]")]
        public ActionResult Invoke([FromBody] BaseRequest request)
        {
            try
            {
                switch (request)
                {
                    case InvokeRequest invoke:
                        return Ok(Toolchain.Invoke(invoke));
                    default:
                        return BadRequest($"Unsupported request type: {request.GetType()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Asm()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var asmSource = await reader.ReadToEndAsync();

                return File(Toolchain.Asm(asmSource), "application/octet-stream");
            }
        }
    }
}
