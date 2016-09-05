using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcosApp.Model
{
    public static class Cryptography
    {

        public class Certificate
        {

            #region Fields
            private X509Certificate _certificate = null;
            private String _certificateString = null;
            private X509Name _certificateDN = null;
            #endregion


            #region Methods
            public System.Security.Cryptography.X509Certificates.X509Certificate2 LoadPKCS12FromFile()
            {
                string path = Directory.GetCurrentDirectory() + "\\CA\\cert_key.p12";
                string certificatePassword = "DiffieHellman";
                return new System.Security.Cryptography.X509Certificates.X509Certificate2(path, certificatePassword);
            }

            public void CreateSubjectX509Name(String name, String surname, String serialNumber)
            {
                String param, CN, Surname;
                param = "C=Poland, O=West Pomeranian University of Technology, OU=IT department, ST=Szczecin, ";
                CN = (surname[0].ToString() + name[0].ToString()).ToLower() + serialNumber + ".wi.zut.edu.pl";
                Surname = name + " " + surname;
                param = param + "CN=" + CN + ", serialNumber=" + serialNumber + ", Surname=" + Surname;
                _certificateDN = new X509Name(param);
            }


            public void CreateCertificate(Key clientPublicKey, Key caPrivateKey, System.Security.Cryptography.X509Certificates.X509Certificate2 caCert)
            {
                if (_certificateDN != null)
                {
                    DateTime startDate = DateTime.Now;
                    DateTime expiryDate = startDate.AddDays(365);
                    SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());
                    AsymmetricKeyParameter publicKey = clientPublicKey.GetPublicAsAsymmetricKeyParam;
                    var utilities = DotNetUtilities.FromX509Certificate(caCert);
                    ISignatureFactory signatureAlgorithm = new Asn1SignatureFactory("SHA256WITHRSA", caPrivateKey.GetPrivateAsAsymmetricKeyParam, random);
                    X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
                    X509Name subjectName = _certificateDN;
                    certGen.SetSerialNumber(BigInteger.ValueOf(DateTime.Now.Millisecond));
                    certGen.SetIssuerDN(utilities.SubjectDN);
                    certGen.SetNotBefore(startDate);
                    certGen.SetNotAfter(expiryDate);
                    certGen.SetSubjectDN(subjectName);
                    certGen.SetPublicKey(publicKey);
                    X509Certificate newCertificate = certGen.Generate(signatureAlgorithm);
                    _certificate = newCertificate;
                    SaveAsString();
                }    
            }

            public void SaveCertificateToFile()
            {
                string pathAndName = Directory.GetCurrentDirectory() + "\\Certificate\\";
                pathAndName += _certificate.SubjectDN.ToString() + ".pem";
                TextWriter tW = new StringWriter();
                PemWriter pW = new PemWriter(tW);
                pW.WriteObject(_certificate);
                var certString = tW.ToString();
                using (StreamWriter outputFile = new StreamWriter(pathAndName, true))
                {
                    outputFile.Write(certString);
                }
            }

            private void SaveAsString()
            {
                TextWriter tW = new StringWriter();
                PemWriter pW = new PemWriter(tW);
                pW.WriteObject(_certificate);
                _certificateString = tW.ToString();
            }
            #endregion


            #region Properties
            public X509Certificate GetCertificate
            {
                get
                {
                    return _certificate;
                }
            }

            public String GetCertificateAsString
            {
                get
                {
                    return _certificateString;
                }
            }
            #endregion

        }


        public class Key
        {
            #region Fields
            private string _publicKeyString;
            private AsymmetricKeyParameter _publicKeyAsymmetricKeyParameter;
            private AsymmetricKeyParameter _privateKeyAsymmetricKeyParameter;

            #endregion

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

            public void ReadPublicKeyFromCardResponse(String key)
            {
                string prefix, suffix, ASN1, publicKeyBase64;
                byte[] publicKeyByte;

                prefix = "30 81 9F 30 0D 06 09 2A 86 48 86 F7 0D 01 01 01 05 00 03 81 8D 00 30 81 89 02 81 81 00 ";
                suffix = " 02 03 01 00 01";
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

            public AsymmetricKeyParameter GetPrivateAsAsymmetricKeyParam
            {
                get
                {
                    return _privateKeyAsymmetricKeyParameter;
                }
            }
        }

    }
}
