using System;
using System.Collections.Generic;
using System.Text;

namespace InfoPointShared.Models
{
    public class CardValidationRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string CardNumber { get; set; } = string.Empty;
    }

}
