using System;
using System.Collections.Generic;
using System.Text;

namespace CoreClasses
{
    public record KeyPair(byte[] PublicKey, byte[] PrivateKey);
}
