using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Bcpg.OpenPgp;
using StarcosApp.sources;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Utilities.Encoders;
using static CertificateStuff.Cryptography;

namespace CertificateStuff
{
    class Program
    {


        static void Main(string[] args)
        {
            var certObject = new Cryptography.Certificate();
            //AsymmetricKeyParameter privatekey = certObject.ReadCaPrivateKeyFromFile();
            Key key = new Key();
            byte[] param = null;
            key.ReadPublicKeyFromCardResponse(param);
            key.ReadCaPrivateKeyFromFile();
            
        }
    }

    public static class Cryptography
    {

        public class Certificate
        {
            private X509Certificate2 _certificate;

            public void LoadPKCS12FromFile()
            {
                string path = Directory.GetCurrentDirectory() + "\\CA\\cert_key.p12";
                string certificatePassword = "DiffieHellman";
                _certificate = new X509Certificate2(path, certificatePassword);           
            }

            public X509Certificate2 GetCertificate
            {
                get
                {
                    return _certificate;
                }
            }
        }


        public class Key
        {
            private string _publicKeyString;
            private AsymmetricKeyParameter _publicKeyAsymmetricKeyParameter;
            private AsymmetricKeyParameter _privateKeyAsymmetricKeyParameter;
            private static byte[] ConvertFromStringToHex(string inputHex)
            {
                inputHex = inputHex.Replace("-", "");

                byte[] resultantArray = new byte[inputHex.Length / 2];
                for (int i = 0; i < resultantArray.Length; i++)
                {
                    resultantArray[i] = Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
                }
                return resultantArray;
            }

            public void ReadCaPrivateKeyFromFile()
            {
                string path = Directory.GetCurrentDirectory() + "\\CA\\private.key";
                AsymmetricCipherKeyPair keyPair = null;
                
                using (StreamReader reader = File.OpenText(path))
                {
                    keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
                }
                _privateKeyAsymmetricKeyParameter = keyPair.Private;
            }

            public void ReadPublicKeyFromCardResponse(byte[] arg)
            {
                string prefix, suffix, key, ASN1, publicKeyBase64;
                byte[] publicKeyByte;

                prefix = "30 81 9F 30 0D 06 09 2A 86 48 86 F7 0D 01 01 01 05 00 03 81 8D 00 30 81 89 02 81 81 00 ";
                suffix = " 02 03 01 00 01";
                key = "96 33 A5 49 EB 9E 11 7F 73 3E 36 8C ED 73 D9 24 DC 8B DF A2 75 D3 E1 EA E9 44 BD 63 7A C1 D1 A6 E5 2E E5 64 55 AB FA C7 35 99 BF D1 CE 53 F3 E6 58 F8 DF 8A 6D CC 4C C9 98 5E 65 EA A2 5F 8C A3 43 6F 6C 08 D9 32 F7 29 4E 32 FE 4C 81 15 96 B7 AE B0 AE CE C2 07 C3 36 98 0B 90 4D EE 25 29 9A 56 4A 91 FE B6 C3 C0 BD 33 D4 BD 5F 33 0C 1F FF 93 D3 F3 EF 00 19 30 8F C0 7C B8 1C 0A AA A7 49";

                ASN1 = (prefix + key + suffix).Replace(" ", "");
                publicKeyByte = ConvertFromStringToHex(ASN1);
                publicKeyBase64 = Convert.ToBase64String(publicKeyByte);
                _publicKeyAsymmetricKeyParameter = PublicKeyFactory.CreateKey(publicKeyByte);
                _publicKeyString = publicKeyBase64;
            }
            public string GetPublicAsString
            {
                get
                {
                    return _publicKeyString;
                }
            }
            public AsymmetricKeyParameter GetPublicAsAsymmetricKeyParam
            {
                get
                {
                    return _publicKeyAsymmetricKeyParameter;
                }
            }
        }

    }
}
