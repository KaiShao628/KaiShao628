using System.Text.Json;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Missing image path.");
    return 1;
}

var imagePath = args[0];
if (!File.Exists(imagePath))
{
    Console.Error.WriteLine($"Image not found: {imagePath}");
    return 2;
}

try
{
    var file = await StorageFile.GetFileFromPathAsync(imagePath).AsTask();
    using var stream = await file.OpenAsync(FileAccessMode.Read).AsTask();
    var decoder = await BitmapDecoder.CreateAsync(stream).AsTask();
    using var bitmap = await decoder.GetSoftwareBitmapAsync().AsTask();
    var engine = OcrEngine.TryCreateFromUserProfileLanguages();

    if (engine is null)
    {
        Console.Error.WriteLine("OCR engine unavailable.");
        return 3;
    }

    var result = await engine.RecognizeAsync(bitmap).AsTask();
    var payload = JsonSerializer.Serialize(new
    {
        text = result.Text ?? string.Empty
    });

    Console.Out.WriteLine(payload);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 4;
}
