using CloudinaryDotNet;

namespace BookingBakery.Infrastructure.Helper
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }

    public class HelperCloudinary
    {
        public Cloudinary CloudinaryInstance { get; }

        public HelperCloudinary(CloudinarySettings config)
        {
            var account = new Account(
                config.CloudName,
                config.ApiKey,
                config.ApiSecret
            );

            CloudinaryInstance = new Cloudinary(account);
            CloudinaryInstance.Api.Secure = true;
        }
    }
}
