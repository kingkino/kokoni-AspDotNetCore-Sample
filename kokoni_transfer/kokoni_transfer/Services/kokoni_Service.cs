using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kokoni_transfer.Services
{
    public class kokoni_Service
    {
        private HttpContext context;
        public kokoni_Service(IHttpContextAccessor contextAccessor)
        {
            this.context = contextAccessor.HttpContext;
        }
    }
}
