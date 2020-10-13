FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy PDF-Tron specific
RUN mkdir -p ./PdfTron/
COPY ./PdfTron/linux/PDFNetDotNetCore.dll ./PdfTron/.

# Copy everything else and build
COPY . ./
RUN dotnet restore
RUN dotnet test
RUN dotnet publish pdftron-support.sln -c Release -o out
