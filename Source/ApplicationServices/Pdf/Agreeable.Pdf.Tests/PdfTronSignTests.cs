using System;
using Xunit;
using pdftron.PDF;
using System.IO;
using pdftron;
using X509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;

namespace Agreeable.pdf.Tests
{
    public class PdfTronSignTests
    {
        private const string SamplePdf = "sample.pdf";

        private const string CertPassword = "development";

        public PdfTronSignTests()
        {
            PDFNet.Initialize();
        }


        // This is a manual test that does similar thing but in a single place for fast debug/testing
        // It does not work yet: https://support.pdftron.com/support/tickets/17113
        // Validation of our signature comes out as "e_untrusted" when verifying.
        [Fact]
        public void AdhocTest()
        {
            PDFDoc doc = new PDFDoc(GetTestPdf(SamplePdf));

            // Ad-hoc field added for signing the PDF
            var signatureField = doc.FieldCreate("sample-field-name", Field.Type.e_signature, "signer name");
            signatureField.SetValue("Signature Name");

            var digitalSignatureField = new DigitalSignatureField(signatureField);

            // Before the rest of the lines or else it fails due to dictionary being empty
            digitalSignatureField.SignOnNextSave(GetCertificatePath("pdf-signing.pfx"), CertPassword);

            digitalSignatureField.SetReason("reason");
            digitalSignatureField.SetContactInfo("info@myemail.com");
            digitalSignatureField.SetLocation("location");
            digitalSignatureField.SetFieldPermissions(DigitalSignatureField.FieldPermissions.e_include, new string[0]);
            digitalSignatureField.SetDocumentPermissions(DigitalSignatureField.DocumentPermissions
                .e_formfilling_signing_allowed);

            // Save file
            var temporaryFile = Path.GetTempFileName();
            doc.Save(temporaryFile, pdftron.SDF.SDFDoc.SaveOptions.e_incremental);

            // VALIDATE
            var result = new PDFDoc(temporaryFile);

            var verificationOptions = new VerificationOptions(VerificationOptions.SignatureVerificationSecurityLevel
                .e_compatibility_and_archiving);
            // Using filepath/password directly makes it fail on adding trusted cert
            // THIS ONE FAILS: 
            // verificationOptions.AddTrustedCertificate(GetCertificatePath("pdf-signing.crt"));
            var x509 = new X509Certificate(GetCertificatePath("pdf-signing.pfx"), CertPassword);
            verificationOptions.AddTrustedCertificate(x509.GetRawCertData());

            DigitalSignatureFieldIterator signatureFieldIterator = result.GetDigitalSignatureFieldIterator();
            for (; signatureFieldIterator.HasNext(); signatureFieldIterator.Next())
            {
                var dsField = signatureFieldIterator.Current();
                var verificationResult = dsField.Verify(verificationOptions);

                var status = verificationResult.GetTrustStatus();
                var certCount = dsField.GetCertCount();

                Console.WriteLine($"Verification status {status}");
                Console.WriteLine($"Digest status {verificationResult.GetDigestStatus()}");
                Console.WriteLine($"Digest document status {verificationResult.GetDocumentStatus()}");
                Console.WriteLine($"Verification status {verificationResult.GetVerificationStatus()}");

                Console.WriteLine($"Cert count: {certCount}");
                Console.WriteLine($"Signature Name{dsField.GetSignatureName()}");
               
                var sigTime = dsField.GetSigningTime();
                Console.WriteLine($"Signing Time: {sigTime.day}/{sigTime.month}/{sigTime.year} {sigTime.hour}:{sigTime.minute}.{sigTime.second}");

                Assert.True(status != VerificationResult.TrustStatus.e_untrusted, "Unexpected status e_untrusted");
                Assert.True(certCount > 0, "DigitalSignatureField should have a certificate");
            }
        }

        private string GetCertificatePath(string fileName) => Path.Combine(Directory.GetCurrentDirectory(), fileName);

        private Stream GetTestPdf(string fileName)
        {
            return File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), fileName));
        }
    }
}
