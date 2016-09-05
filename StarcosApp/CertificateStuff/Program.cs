using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.Asn1;
using MySql.Data.MySqlClient;

namespace CertificateStuff
{
    class Program
    {


        static void Main(string[] args)
        {
            var certObject = new Cryptography.Certificate();
            //AsymmetricKeyParameter privatekey = certObject.ReadCaPrivateKeyFromFile();
            // key.ReadPublicKeyFromCardResponse(param);
            //key.ReadCaPrivateKeyFromFile();
            //Key clientKey = new Key();
            //clientKey.ReadPublicKeyFromCardResponse(null);

            //Key caKey = new Key();
            //caKey.ReadCaPrivateKeyFromFile();


            //certObject.CreateCertificate(clientKey, caKey);
            //certObject.SaveCertificateToFile();
            String cert = @"-----BEGIN CERTIFICATE-----
MIIC2DCCAcCgAwIBAgIBETANBgkqhkiG9w0BAQsFADCBzjELMAkGA1UEBhMCUEwx
IzAhBgNVBAgMGldlc3Rlcm5wb21lcmFuaWFuIERpc3RyaWN0MREwDwYDVQQHDAhT
emN6ZWNpbjEyMDAGA1UECgwpV2VzdGVycG9tZXJhbmlhbiBVbml2ZXJpc3R5IG9m
IFRlY2hub2xvZ3kxDjAMBgNVBAsMBVdJWlVUMRowGAYDVQQDDBFtcG1iLmNsb3Vk
YXBwLm5ldDEnMCUGCSqGSIb3DQEJARYYaWJzaS5hbm9ueW1vdXNAZ21haWwuY29t
MB4XDTE2MDgyODE1MDIyNFoXDTE3MDgyODE1MDIyNFowFDESMBAGA1UEAwwJTWFj
aWVnIEJlMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCWM6VJ654Rf3M+Nozt
c9kk3IvfonXT4erpRL1jesHRpuUu5WRVq/rHNZm/0c5T8+ZY+N+KbcxMyZheZeqi
X4yjQ29sCNky9ylOMv5MgRWWt66wrs7CB8M2mAuQTe4lKZpWSpH+tsPAvTPUvV8z
DB//k9Pz7wAZMI/AfLgcCqqnSQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQCabca0
P6KEROEnQfz1Oz3jGpMAnlBKeptFKdR3a7THo3Ow7vV8qm8Xqu3yArWOOmQ1rtZa
d22QGjKKWSZd8upZ+5yg0rSqMHey17UOdtxOpP9ope1/sUraRko0u8oRME/xaF05
k60ia19kbLT04JDv7TVbkaH9lGBQIkgZ/+OEXJy9tKGOXkvJgNrt0hcYD7WXhj+l
6EpIJfFqpN5b8L8WxY+FnMaaRDToBdBIfhhlWWh5W/8r+1yymv0FTEAF99mTNU3l
+nPwuXPh9HZVIjoBk1piiEhrs/WL72MQX145uY/Ghvav99oU0AtcV2ly54TI7cqm
us/86QtwFQX5bh6X
-----END CERTIFICATE-----";
            MySQLDatabase mySqlDb = new MySQLDatabase("Database=starcosdb;Data Source=us-cdbr-azure-southcentral-f.cloudapp.net;User Id=bb73e9b3e5342d;Password=32da954d");
            mySqlDb.OpenDatabaseConnection();
            mySqlDb.ExecuteMysqlQuery("INSERT INTO users (nazwa, pesel, nr_albumu, certyfikat, data_urodzenia) VALUES('Maciej Bartłomiejczyk', '94011211434', '29690', '" + cert +"','1994-01-12')");
            mySqlDb.CloseDatabaseConnection();

        }
    }

    public class MySQLDatabase
    {
        private String _connectionString = null;
        private MySqlConnection _mySqlConnection = null;
        private MySqlCommand _mySqlCommand = null;
   
        public MySQLDatabase(String connectionString)
        {
            this._connectionString = connectionString;
        }

        public void OpenDatabaseConnection()
        {
            try
            {
                _mySqlConnection = new MySqlConnection();
                _mySqlConnection.ConnectionString = _connectionString;
                _mySqlConnection.Open();
            }
            catch(MySqlException e)
            {
                String error = e.Message;
            }
        }
        public void CloseDatabaseConnection()
        {
            try
            {
                if (_mySqlConnection != null)
                    _mySqlConnection.Close();
            }
            catch (MySqlException e)
            {
                String error = e.Message;
            }
        }
        public void ExecuteMysqlQuery(String queryString)
        {
            var state = _mySqlConnection.State.ToString();
            if (state == "Open")
            {
                try
                {
                    _mySqlCommand = new MySqlCommand();
                    _mySqlCommand.Connection = _mySqlConnection;
                    _mySqlCommand.CommandText = queryString;
                    _mySqlCommand.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    _mySqlConnection.Close();
                }
            }
        }
    }
    public static class Cryptography
    {

        public class Certificate
        {

            #region Fields
            private X509Certificate _certificate = null;
            #endregion


            #region Methods
            public System.Security.Cryptography.X509Certificates.X509Certificate2 LoadPKCS12FromFile()
            {
                string path = Directory.GetCurrentDirectory() + "\\CA\\cert_key.p12";
                string certificatePassword = "DiffieHellman";
                return new System.Security.Cryptography.X509Certificates.X509Certificate2(path, certificatePassword);
            }

            public void CreateCertificate(Key clientPublicKey, Key caPrivateKey)
            {
                DateTime startDate = DateTime.Now;
                DateTime expiryDate = startDate.AddDays(365);
                SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());
                AsymmetricKeyParameter publicKey = clientPublicKey.GetPublicAsAsymmetricKeyParam;
                System.Security.Cryptography.X509Certificates.X509Certificate2 caCert2 = LoadPKCS12FromFile();
                var utilities = DotNetUtilities.FromX509Certificate(caCert2);
                ISignatureFactory signatureAlgorithm = new Asn1SignatureFactory("SHA256WITHRSA", caPrivateKey.GetPrivateAsAsymmetricKeyParam, random);
                X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
                X509Name subjectName = new X509Name("C=Poland, O=West Pomeranian University of Technology, OU=IT department, ST=Szczecin, CN=bm29690.wi.zut.edu.pl, serialNumber=29690, Surname=Maciej Bartłomiejczyk");
                certGen.SetSerialNumber(BigInteger.ValueOf(DateTime.Now.Millisecond));
                certGen.SetIssuerDN(utilities.SubjectDN);
                certGen.SetNotBefore(startDate);
                certGen.SetNotAfter(expiryDate);
                certGen.SetSubjectDN(subjectName);
                certGen.SetPublicKey(publicKey);
                X509Certificate newCertificate = certGen.Generate(signatureAlgorithm);
                _certificate = newCertificate;
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
            #endregion


            #region Properties
            public X509Certificate GetCertificate
            {
                get
                {
                    return _certificate;
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
