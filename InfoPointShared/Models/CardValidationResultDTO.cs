using System;
using System.Collections.Generic;
using System.Text;

namespace InfoPoint.Models
{
    public class CardValidationResult
    {
        public bool IsValid { get; set; }
        public string? ClientName { get; set; }
        public string? CardNumber { get; set; }
        public bool IsActive { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
