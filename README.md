# PDFTron support

## Using dotnet directly: On windows/linux:

$> cd Source
$> dotnet build
$> dotnet test

--> Fails due to unexpected "e_untrusted" state
```
[xUnit.net 00:00:01.54]     Agreeable.pdf.Tests.PdfTronSignTests.AdhocTest [FAIL]
  Con error Agreeable.pdf.Tests.PdfTronSignTests.AdhocTest [876 ms]
  Mensaje de error:
   Did not expect status to be equal to e_untrusted.
```

## Using docker (linux)

$> cd Source
$> docker build .

This build failes due to cert count being 0, should be 1 at least, right ?
