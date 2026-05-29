namespace TD.Lib.Common
{
    public class Dropdown
    {
        public dynamic Value { get; set; }
        public string Text { get; set; }
    }

    public class DropdownV2 : Dropdown
    {
        public dynamic Value { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }
    }

    public class DropdownCustomer : DropdownV2
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime? DateBirth { get; set; }
        public int? Gender { get; set; }
        public int? WageType { get; set; }
        public double? WageValue { get; set; }
        public double? OffPercent { get; set; }
    }

    public class DropdownSupplier : DropdownV2
    {
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class DropdownBankAccount : Dropdown
    {
        public string BankName { get; set; }
        public string AccountName { get; set; }
    }

    public class DropdownInvoice : Dropdown
    {
        public Guid IdLine { get; set; }
        public Guid IdSupplier { get; set; }
        public string LineName { get; set; }
        public string SupplierName { get; set; }
        public DateTime? NotifyDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public double? KgSupplier { get; set; }
        public double? KgReceived { get; set; }
        public double? KgDifference { get; set; }
    }

    public class DropdownCashCategory : DropdownV2
    {
        public int TransactionType { get; set; }
        public int PartnerType { get; set; }
    }

    public class DropdownBrand : DropdownV2
    {
        public bool IsApplyOff { get; set; }
    }
}
