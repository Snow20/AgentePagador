using System;

namespace Agent.Core.Entities
{
    public class ParsedInvoice
    {
        public Guid VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public string ServiceMonth { get; set; } = string.Empty;
        public bool HasAmbiguousData { get; set; }
        public string AmbiguousField { get; set; } = string.Empty;
    }

    public class SystemRules
    {   
        public decimal HumanApprovalThreshold { get; set; }
    }

    public class RawInvoice
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;

        // Nuevas propiedades obligatorias para la evaluación jerárquica
        public decimal Amount { get; set; }
        public bool IsDuplicate { get; set; }
        public bool IsIllegal { get; set; }
        public bool NeedsReview { get; set; }
    }

    public class Vendor
    {
        public Guid Id { get; set; }
        public bool IsAllowed { get; set; }
        public string RegisteredIban { get; set; } = string.Empty;
    }
}