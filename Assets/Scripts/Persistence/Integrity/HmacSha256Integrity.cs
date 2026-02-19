using System;
using System.Security.Cryptography;
using System.Text;
using IdleScaper.Persistence.Core;

namespace IdleScaper.Persistence.Integrity
{
    /// <summary>
    /// HMAC-SHA256 integrity provider. Detects tampering unless attackers extract the key.
    /// Use this for casual tamper detection; server authority is needed for strong security.
    /// </summary>
    public sealed class HmacSha256Integrity : ISaveIntegrity
    {
        private readonly byte[] keyBytes;

        /// <summary>
        /// Creates an integrity provider with a given secret key.
        /// </summary>
        public HmacSha256Integrity(string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("Secret key must be non-empty.", nameof(secretKey));
            }

            keyBytes = Encoding.UTF8.GetBytes(secretKey);
        }

        /// <inheritdoc />
        public string ComputeSignature(string payload)
        {
            var dataBytes = Encoding.UTF8.GetBytes(payload ?? string.Empty);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToBase64String(hash);
        }

        /// <inheritdoc />
        public bool VerifySignature(string payload, string signature)
        {
            if (string.IsNullOrEmpty(signature))
            {
                return false;
            }

            var expected = ComputeSignature(payload);

            // Constant-time compare on bytes.
            var a = Encoding.UTF8.GetBytes(expected);
            var b = Encoding.UTF8.GetBytes(signature);

            return CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}