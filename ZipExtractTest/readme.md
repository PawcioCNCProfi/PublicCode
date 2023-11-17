So I'm using following method to extract an archive.

```C#
public static void extractArchive(string zipFile, string destination) {
    destination = Path.GetFullPath(destination);
    if (!destination.EndsWith(Path.DirectorySeparatorChar.ToString()))
        destination += Path.DirectorySeparatorChar;

    using (var arc = ZipFile.OpenRead(zipFile)) {
        foreach (var e in arc.Entries) {
            var des = Path.GetFullPath(
                Path.Combine(destination + e.FullName));
            if (des.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                if (!Directory.Exists(des)) Directory.CreateDirectory(des);
                continue;
            }
            if (File.Exists(des) || !des.StartsWith(destination)) continue;
            Console.WriteLine($@"extracting  ""{e}"" to ""{des}""");
            e.ExtractToFile(des);
        }
    }
}
```

It works fine when I extract an archive packed manually using **WinRAR** but it throws an exception when the archive is packed using **MS Build** task:

```xml
<Target Name="AfterBuild">
  <Message Text="Second occurrence" />
  <ZipDirectory Overwrite="true" SourceDirectory="$(TargetDir)\archive\" DestinationFile="$(TargetDir)\VS_MSBS_task.zip" />
</Target>
```

```C#
static void Main(string[] args) {

    var dest = Directory.GetCurrentDirectory();
    dest = Path.Combine(dest, "output");

    zip("Winrar zip", "Winrar.zip",  dest);
    zip("VisualStudio zip", "VS_MSBS_task.zip",  dest); //exception


    Console.WriteLine("Done");
    Console.ReadKey();
}

private static void zip(string name, string zipFile, string dest) {
    Console.WriteLine($"-----------------{name}--------------------------------------------------------");
    if(Directory.Exists(dest))
        Directory.Delete(dest, true);
    Directory.CreateDirectory(dest);
    try {
        extractArchive(zipFile, dest);
    }catch (Exception ex) {
        Console.WriteLine(ex);
    }
    Console.WriteLine($"-----------------FINISHED------------------------------------------------------");
}
```

With "VS_MSBS_task.zip" I get:

 **System.IO.DirectoryNotFoundException:** 'Could not find a part of the path "path\to\file\in\subfolder.extension".'' at:

```C#
//ZipFileExtensions.cs:
public static void ExtractToFile(this ZipArchiveEntry source, string destinationFileName, bool overwrite)
{
    //...
    using (Stream destination = File.Open(destinationFileName, mode, FileAccess.Write, FileShare.None))
    {
        //..
    }
    //..
}
```

The input for the method looks exactly the same in both cases so I suppose this is a bug in `System.IO.Compression` or am I missing something?

Here is the console output:

```
-----------------Winrar zip--------------------------------------------------------
extracting  "sub1/fileInSubFolder.cs" to "C:\...\ZipExtractTest\bin\Debug\output\sub1\fileInSubFolder.cs"
extracting  "text.txt" to "C:\...\ZipExtractTest\bin\Debug\output\text.txt"
-----------------FINISHED------------------------------------------------------
-----------------VisualStudio zip--------------------------------------------------------
extracting  "text.txt" to "C:\...\ZipExtractTest\bin\Debug\output\text.txt"
extracting  "sub1/fileInSubFolder.cs" to "C:\...\ZipExtractTest\bin\Debug\output\sub1\fileInSubFolder.cs"
System.IO.DirectoryNotFoundException: Could not find a part of the path 'C:\...\ZipExtractTest\bin\Debug\output\sub1\fileInSubFolder.cs'.
   at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
   at System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath, Boolean checkHost)
   at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
   at System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry source, String destinationFileName, Boolean overwrite)
   at System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry source, String destinationFileName)
   at ZipExtractTest.Program.extractArchive(String zipFile, String destination) in C:\...\ZipExtractTest\Program.cs:line 48
   at ZipExtractTest.Program.zip(String name, String zipFile, String dest) in C:\...\ZipExtractTest\Program.cs:line 26
-----------------FINISHED------------------------------------------------------
Done
```


