﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyWorZonesCreation.Models
{
    public class ResponseOFSC
    {
        public int statusCode { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public bool flag { get; set; }
    }
}
