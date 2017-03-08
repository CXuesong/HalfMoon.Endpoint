using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace HalfMoon.Endpoint.AspNetCore.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _Env;

        public HomeController(IHostingEnvironment env)
        {
            _Env = env;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            StringBuilder sb;
            using (var fs = _Env.WebRootFileProvider.GetFileInfo("default.html").CreateReadStream())
            using (var reader = new StreamReader(fs))
                sb = new StringBuilder(await reader.ReadToEndAsync());
            using (var proc = Process.GetCurrentProcess())
            {
                sb.Replace("$WORKING_SET$", proc.WorkingSet64.ToString("#,#"));
                sb.Replace("$PEAK_WORKING_SET$", proc.PeakWorkingSet64.ToString("#,#"));
            }
            sb.Replace("$GC_COUNTERS$",
                string.Join(",", Enumerable.Range(0, GC.MaxGeneration + 1).Select(GC.CollectionCount)));
            return new ContentResult {Content = sb.ToString(), ContentType = "text/html"};
        }
    }
}
