using SharedKernel.Extensions;
using System.Diagnostics;
using Xunit;

namespace SharedKernelTests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void HashPerformanceTest()
        {

            var testString = CreateRandomString(10000);

            // 1. Warm up the methods (JIT compilation)
            var hash = testString.GetHash();
            var hash2 = testString.GetHashFast();

            var iterations = 100_000;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) testString.GetHash();
            sw.Stop();
            var elapsed1 = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            for (int i = 0; i < iterations; i++) testString.GetHashFast();
            sw.Stop();
            var elapsed2 = sw.Elapsed.TotalMilliseconds;

            Assert.Equal(hash, hash2);
            Assert.True(elapsed2 < elapsed1, $"GetHash2 should be faster than GetHash. Elapsed1: {elapsed1}, Elapsed2: {elapsed2}");
        }

        public string CreateRandomString(int length = 10000)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return string.Create(length, chars, (span, alphabet) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    // Using RandomNumberGenerator for higher quality randomness 
                    // or Random.Shared.Next for pure speed
                    span[i] = alphabet[Random.Shared.Next(alphabet.Length)];
                }
            });
        }
    }
}
