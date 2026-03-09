namespace sircceli;

using System;
using System.Security.Cryptography;

public class BadCode
{
    public void Foo()
    {
        // Bad: Using System.Random for cryptographic key generation
        var rng = new System.Random();
        byte[] key = new byte[16];
        rng.NextBytes(key);
        
        using (var aes = Aes.Create())
        {
            aes.Key = key;  // SECURITY ISSUE: Weak RNG used for crypto key
        }
    }
}
