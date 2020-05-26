using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateBigInteger
{
	class Program
	{
	    private static int _bitWidth = 256;
	    //private static int bitWidth = 8;
	    private static int _generator = 13;

		static void Main(string[] args)
		{
			GenerateKeys();

			Console.WriteLine("Press any key to exit...");
			Console.Read();
		}

        //In case I have to figure it out again:
        //The Client will calculate its own random # in the range of 0 to p-1.
        //The Client will calculate its own public key (v) using g^y mod p.
        //The Client will caluclate the secret key using [server public key]^y mod p.

        //The Client sends its public key with the message and uses its secret key to encrypt the message.
        //The Server calculates the secret key using v [client public key]^u [server private key] mod p.

	    private static void GenerateKeys()
		{
		    Console.WriteLine("Bit width: {0}", _bitWidth);
            
            var g = GetGenerator();

            var p = GetRandomPrime();
            //var p = new BigInteger(StringToByteArray("B2C3C2A6CDFC59C14EC4F364D3FD2CA5311675F498E1E4ED8341EE37F9642F3F"));
			Console.WriteLine("Generated prime big integer for modulus (p): \n{0}", p.ToHexString());

		    var randomNumber = GetRandomBigInteger(p);
			Console.WriteLine("Generated random big integer (x): \n{0}", randomNumber.ToHexString());

            var serverPrivateKey = g.modPow(randomNumber, p);
			Console.WriteLine("Generated server private key (u): \n{0}", serverPrivateKey.ToHexString());
			
            var serverPublicKey = g.modPow(serverPrivateKey, p);
			Console.WriteLine("Generated server public key: \n{0}", serverPublicKey.ToHexString());
		}

	    private static BigInteger GetGenerator()
	    {
	        return new BigInteger(_generator);
	    }

	    private static BigInteger GetRandomPrime()
	    {
	        var random = new Random();
	        return BigInteger.genPseudoPrime(_bitWidth, 30, random);
	    }

	    private static BigInteger GetRandomBigInteger(BigInteger p)
	    {
	        BigInteger randomNumber = null;

	        while ((object)randomNumber == null || randomNumber >= p)
	        {
	            var random = new Random();
	            randomNumber = new BigInteger();
	            randomNumber.genRandomBits(_bitWidth, random);
	        }

	        return randomNumber;
	    }

        private static byte[] StringToByteArray( string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < hexString.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return bytes;
        }
	}
}
