using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoPointUI.Helpers
{
    public static class LoyaltyCardValidator
    {
        /// <summary>
        /// Verifică dacă un cod de bare EAN-13 este valid.
        /// </summary>
        public static bool IsValid(string code)
        {
            // Protecție: null sau gol
            if (string.IsNullOrWhiteSpace(code))
                return false;

            // Lungime fixă: 13 caractere
            if (code.Length != 13)
                return false;

            // Doar cifre
            foreach (char c in code)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // Calcul checksum EAN-13
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = code[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            int lastDigit = code[12] - '0';

            return checkDigit == lastDigit;
        }
    }
}
