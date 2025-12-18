using System;
using System.Collections.Generic;

namespace InfoPoint.Models
{
    /// <summary>
    /// Datele clientului returnate la scanarea cardului de fidelitate
    /// </summary>
    public class ClientDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Codul EAN13 de pe cardul fizic
        /// </summary>
        public string CardNumber { get; set; } = string.Empty;

        /// <summary>
        /// Prenume client
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Nume client
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Nume complet formatat pentru afișare în UI
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Clientul este activ în sistem (card neblocat)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Cardul expiră la această dată (null dacă nu expiră)
        /// </summary>
        public DateTime? CardExpiryDate { get; set; }

        /// <summary>
        /// Puncte de fidelitate acumulate
        /// </summary>
        public decimal LoyaltyPoints { get; set; }

        /// <summary>
        /// Nivel client (Bronze, Silver, Gold, Platinum)
        /// </summary>
        public string LoyaltyTier { get; set; } = "Bronze";

        /// <summary>
        /// Discount general pentru client (ex: -5% pentru toate achizițiile)
        /// </summary>
        public decimal GeneralDiscountPercent { get; set; }

        /// <summary>
        /// Discounturi personalizate pe produse specifice
        /// </summary>
        public List<PersonalDiscountDto> PersonalDiscounts { get; set; } = new();

        /// <summary>
        /// Istoric ultimelor achiziții (max 10)
        /// </summary>
        public List<PurchaseHistoryDto> RecentPurchases { get; set; } = new();

        /// <summary>
        /// Produse recomandate pentru acest client
        /// </summary>
        public List<int> RecommendedProductIds { get; set; } = new();

        /// <summary>
        /// Data înregistrării în sistem
        /// </summary>
        public DateTime MemberSince { get; set; } = DateTime.Now.AddYears(-1);

        /// <summary>
        /// Ultima vizită în farmacie
        /// </summary>
        public DateTime? LastVisit { get; set; }

        /// <summary>
        /// Număr total de achiziții
        /// </summary>
        public int TotalPurchases { get; set; }

        /// <summary>
        /// Valoare totală cheltuită
        /// </summary>
        public decimal TotalSpent { get; set; }

        /// <summary>
        /// Categorie preferată de produse
        /// </summary>
        public string PreferredCategory { get; set; } = string.Empty;

        /// <summary>
        /// Brand preferat
        /// </summary>
        public string PreferredBrand { get; set; } = string.Empty;

        /// <summary>
        /// Feedback rating (1-5)
        /// </summary>
        public double? SatisfactionRating { get; set; }

        /// <summary>
        /// Email pentru marketing (cu consimțământ)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Telefon pentru notificări
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Farmacia unde s-a înregistrat
        /// </summary>
        public int HomePharmacyId { get; set; }
    }

    /// <summary>
    /// Discount personalizat pentru un anumit produs
    /// </summary>
    public class PersonalDiscountDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal SpecialPrice { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Reason { get; set; } = string.Empty; // "Recomandare", "Aniversare", etc.
    }

    /// <summary>
    /// Înregistrare din istoricul de achiziții
    /// </summary>
    public class PurchaseHistoryDto
    {
        public DateTime PurchaseDate { get; set; }
        public int PharmacyId { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool UsedLoyaltyPoints { get; set; }
        public decimal PointsEarned { get; set; }
        public string? PrescriptionNumber { get; set; }
    }

    /// <summary>
    /// Răspuns la validarea unui card
    /// </summary>
    public class CardValidationResponseDto
    {
        public bool Success { get; set; }
        public ClientDto? Client { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;

        // Helper properties pentru UI
        public bool IsValid => Success && Client?.IsActive == true;
        public bool IsExpired => Client?.CardExpiryDate.HasValue == true &&
                                 Client.CardExpiryDate.Value < DateTime.Now;
    }
}
