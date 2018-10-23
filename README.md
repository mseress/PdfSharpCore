# PdfSharpCore

**PdfSharpCore** is an unofficial .NETCore/netstandard 2.0 revision of PdfSharp-MigraDoc, which is a .NET library for processing PDF.

**PdfSharpCore** works on Windows and **Linux**!

**PdfSharpCore** is keeping updating from https://sourceforge.net/projects/pdfsharp/.

## Update Notes
This repository: **PDFsharp-MigraDocFoundation-1_50-beta5 (2017-12-24)**.

_If you find the version is below than on the sorceforge, please let us know. Thank you!_

SourceForge Versions: https://sourceforge.net/projects/pdfsharp/files/pdfsharp/


## Nuget
- [PdfSharpCore.PdfSharp](https://www.nuget.org/packages/PdfSharpCore.PdfSharp)

- [PdfSharpCore.PdfSharp.Rendering](https://www.nuget.org/packages/PdfSharpCore.PdfSharp.Rendering)

- [PdfSharpCore.MigraDoc](https://www.nuget.org/packages/PdfSharpCore.MigraDoc)

- [PdfSharpCore.MigraDoc.Rendering](https://www.nuget.org/packages/PdfSharpCore.MigraDoc.Rendering)


## PDFsharp and MigraDoc
PDFsharp is a .NET library for creating and modifying Adobe PDF documents programmatically from any .NET language like C# or VB.NET. PDFsharp defines classes for the objects found in PDF files, so you never have to deal with IDs or references directly.
The downloads include MigraDoc Foundation, a .NET library for creating documents on the fly (supports PDF and RTF).

PDFsharp is the .NET library that easily creates and processes PDF documents on the fly from any .NET language. The same drawing routines can be used to create PDF documents, draw on the screen, or send output to any printer.

MigraDoc Foundation is the .NET library that easily creates documents based on an object model with paragraphs, tables, styles, etc. and renders them into PDF, XPS, or RTF.

### PDFsharp
PDFsharp is a .NET library for creating and modifying PDF documents.

Brought to you by: [pdfsharp](https://sourceforge.net/u/pdfsharp), [stefan_lange](https://sourceforge.net/u/userid-1360638)

### MigraDoc
MigraDoc Foundation - Creating documents on the fly

MigraDoc references PDFsharp as a submodule. 

### Use PDFsharp or MigraDoc?
* Use PDFsharp to create PDF files only, but be able to control every pixel and every line that is drawn.
* Use MigraDoc to create PDF and RTF files and to enjoy the comfort of a word processor.

## Samples

Democrates features of PdfSharpCore components of PdfSharp and MigraDoc.

Note1: 
You should import nuget package [System.Text.Encoding.CodePages](https://www.nuget.org/packages/System.Text.Encoding.CodePages/), and add lines in Program.cs:
```csharp
  static Program()
  {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
  }
```
Note2:
Font searching in (1) executing assembly location; (2) Windows/Linux fonts path; and (3) stsong.ttf in the embedded resource when font not found.

### 0-ProtectedPdf
Use **PdfSharp.Charting** to create a password protected pdf.

| Role | Password | Privilege|
|:---:|:----:|:---:|
|user | user | view, ...
|owner | owner | view, modify, print, change password, ... |

### 1-HelloWorld
Use **MigraDoc.Rendering** to create a simple pdf with one line text.

### 2-HelloMigraDoc
Use **MigraDoc.Rendering** to create a common pdf with paragraph styles and charting.

### 3-Invoice
Use **MigraDoc.Rendering** to create a invoice letter with header, footer, title, table, and feed data from a xml file

### 4-PdfSharpDemo
Use **MigraDoc.Rendering** to create a pdf that combines another pdf and appends some new content.


## Resources

The official project web site:  
http://pdfsharp.net/

The official peer-to-peer support forum:  
http://forum.pdfsharp.net/





