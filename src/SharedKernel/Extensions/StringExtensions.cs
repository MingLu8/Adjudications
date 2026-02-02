using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Extensions;

public static class StringExtensions
{
    public static string GetHash(this string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        using var hash = SHA256.Create();
        var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(bytes);
    }

    public static string GetHashFast(this string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // 1. Prepare the input bytes (Stack for small strings, Pool for large)
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(input.Length);
        byte[]? rentedArray = null;
        var sourceBuffer = maxByteCount <= 512
            ? stackalloc byte[512]
            : (rentedArray = ArrayPool<byte>.Shared.Rent(maxByteCount));

        try
        {
            var actualByteCount = Encoding.UTF8.GetBytes(input, sourceBuffer);
            var inputSpan = sourceBuffer.Slice(0, actualByteCount);

            // 2. Compute SHA-256 into a stack buffer
            Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(inputSpan, hashBytes);

            // 3. The Magic: string.Create
            // We know SHA-256 always produces 32 bytes = 64 hex characters
            return string.Create(64, hashBytes, (chars, hash) =>
            {
                const string hexAlphabet = "0123456789ABCDEF";
                for (int i = 0; i < hash.Length; i++)
                {
                    // Write directly into the string's memory buffer
                    chars[i * 2] = hexAlphabet[hash[i] >> 4];
                    chars[i * 2 + 1] = hexAlphabet[hash[i] & 0xF];
                }
            });
        }
        finally
        {
            if (rentedArray != null) ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }
}

