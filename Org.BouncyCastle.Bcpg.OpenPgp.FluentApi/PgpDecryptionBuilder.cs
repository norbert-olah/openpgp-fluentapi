﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Org.BouncyCastle.Bcpg.OpenPgp.FluentApi
{
    public class PgpDecryptionBuilder
    {
        private Stream InStream;
        private Stream OutStream;

        private List<PrivateKeyInfo> PrivateKeys;
        private List<Stream> SignatureKeys;
        private bool VerifySignature;

        public PgpDecryptionBuilder()
        {
            OutStream = new MemoryStream();
            PrivateKeys = new List<PrivateKeyInfo>();
            SignatureKeys = new List<Stream>();
            VerifySignature = false;
        }

        public PgpDecryptionBuilder Decrypt(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentNullException("inputFilePath");

            try
            {
                if (!File.Exists(inputFilePath))
                    throw new ArgumentException("inputFilePath", "file does not exists");
            }
            catch(Exception ex)
            {
                throw new ArgumentException("inputFilePath", "error while checking file", ex);
            }

            var fileInfo = new FileInfo(inputFilePath);
            var inputFileStream = File.OpenRead(inputFilePath);

            return Decrypt(fileInfo.OpenRead());
        }

        public PgpDecryptionBuilder Decrypt(Stream inputFileStream)
        {
            if (InStream != null)
                throw new InvalidOperationException("stream to encrypt already specified");

            if (inputFileStream == null)
                throw new ArgumentNullException("inputFileStream");

            if (!inputFileStream.CanRead)
                throw new ArgumentException("stream is not readable", "inputFileStream");

            if (inputFileStream.CanSeek)
                inputFileStream.Seek(0, SeekOrigin.Begin);

            InStream = inputFileStream ?? throw new ArgumentNullException("inputFileStream");

            
            return this;
        }

        public PgpDecryptionBuilder WriteOutputTo(string outfilePath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(outfilePath))
                throw new ArgumentNullException("outfilePath");

            try
            {
                if (File.Exists(outfilePath) && !overwrite)
                    throw new ArgumentException("outfilePath", "out file already exists");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("outfilePath", "error while checking file", ex);
            }

            var stream = File.Open(outfilePath, FileMode.OpenOrCreate);
            
            return WriteOutputTo(stream);
        }

        public PgpDecryptionBuilder WriteOutputTo(Stream outStream)
        {
            //Console.WriteLine(outStream == null ? "null stream" : "not null stream");
            //if (OutStream != null)
            //    throw new InvalidOperationException("stream to encrypt already specified"); always, because initially set as new memorystream

            OutStream = outStream ?? throw new ArgumentNullException("outStream");

            return this;
        }

        public PgpDecryptionBuilder WithPrivateKey(string privateKeyPath, string password)
        {
            if (string.IsNullOrEmpty(privateKeyPath))
                throw new ArgumentNullException("privateKeyPath");

            if (!Uri.IsWellFormedUriString(privateKeyPath, UriKind.RelativeOrAbsolute))
                throw new ArgumentException("privateKeyPath", "malformed file path");

            if (!File.Exists(privateKeyPath))
                throw new ArgumentException("privateKeyPath", "file does not exists");

            var privateKeyStream = File.OpenRead(privateKeyPath);

            return WithPrivateKey(privateKeyStream, password);
        }

        public PgpDecryptionBuilder WithPrivateKey(Stream privateKeyStream, string password)
        {
            if (privateKeyStream == null)
                throw new ArgumentNullException("privateKeyStream");

            if (!privateKeyStream.CanRead)
                throw new ArgumentException("stream is not readable", "privateKeyStream");

            if (privateKeyStream.CanSeek)
                privateKeyStream.Seek(0, SeekOrigin.Begin);

            var privateKeyInfo = new PrivateKeyInfo
            {
                PrivateKeyStream = privateKeyStream ?? throw new ArgumentNullException("privateKeyStream"),
                PrivateKeyPassword = password
            };

            PrivateKeys.Add(privateKeyInfo);

            return this;
        }

        public PgpDecryptionBuilder VerifySignatureUsingKey(string signatureKeyPath)
        {
            if (string.IsNullOrEmpty(signatureKeyPath))
                throw new ArgumentNullException("signatureKeyPath");

            if (!Uri.IsWellFormedUriString(signatureKeyPath, UriKind.RelativeOrAbsolute))
                throw new ArgumentException("signatureKeyPath", "malformed file path");

            if (!File.Exists(signatureKeyPath))
                throw new ArgumentException("signatureKeyPath", "file does not exists");

            var signatureKeyStream = File.OpenRead(signatureKeyPath);

            return VerifySignatureUsingKey(signatureKeyStream);
        }

        public PgpDecryptionBuilder VerifySignatureUsingKey(Stream signatureKeyStream)
        {
            if (signatureKeyStream == null)
                throw new ArgumentNullException("signatureKeyStream");

            if (!signatureKeyStream.CanRead)
                throw new ArgumentException("stream is not readable", "signatureKeyStream");

            if (signatureKeyStream.CanSeek)
                signatureKeyStream.Seek(0, SeekOrigin.Begin);

            SignatureKeys.Add(signatureKeyStream ?? throw new ArgumentNullException("signatureKeyStream"));
            VerifySignature = true;

            return this;
        }

        public PgpDecryptionTask Build() => new PgpDecryptionTask
        {
            InStream = InStream,
            OutStream = OutStream,
            PrivateKeys = PrivateKeys,
            SignatureKeys = SignatureKeys,
            CheckSignature = VerifySignature
        };

    }

    class PrivateKeyInfo
    {
        internal Stream PrivateKeyStream { get; set; }
        internal string PrivateKeyPassword { get; set; }
    }
}
