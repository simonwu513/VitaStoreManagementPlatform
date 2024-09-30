namespace StoreManagementWebsite.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ErrorMessage { get; set; }

        public string? ErrorResolution { get; set; }

        public List<string>? ErrorItems { get; set; }


    }
}
