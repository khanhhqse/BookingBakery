using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BookingBakery.Application.Common
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(StringComparer.Ordinal);
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(StringComparer.Ordinal);

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var queryString = new StringBuilder();
            var rawData = new StringBuilder();

            foreach (var kv in _requestData)
            {
                if (queryString.Length > 0)
                {
                    queryString.Append("&");
                    rawData.Append("&");
                }
                queryString.Append(UrlEncode(kv.Key) + "=" + UrlEncode(kv.Value));
                rawData.Append(UrlEncode(kv.Key) + "=" + UrlEncode(kv.Value));
            }

            string secureHash = HmacSha512(vnpHashSecret, rawData.ToString());
            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + secureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rawData = new StringBuilder();
            foreach (var kv in _responseData)
            {
                if (kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    if (rawData.Length > 0)
                    {
                        rawData.Append("&");
                    }
                    rawData.Append(UrlEncode(kv.Key) + "=" + UrlEncode(kv.Value));
                }
            }

            string myChecksum = HmacSha512(secretKey, rawData.ToString());
            return myChecksum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (byte theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        private string UrlEncode(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            StringBuilder sb = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            foreach (byte b in bytes)
            {
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || 
                    b == '-' || b == '_' || b == '.' || b == '~')
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", b);
                }
            }
            return sb.ToString();
        }
    }
}
