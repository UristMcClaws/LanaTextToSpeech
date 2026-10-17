using PiperSharp;
using PiperSharp.Models;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LanaTextToSpeech.PiperWrapper;

public class PiperWrapper
{
    private PiperProvider piperProvider;
    private string directory;

    public PiperWrapper(string? directory = null, string? modelKey = null)
    {
        if (directory == null)
        {
            directory = Directory.GetCurrentDirectory();
        }
        this.directory = directory;
        InitializePiper().Wait();
        LoadModel(modelKey ?? "en_US-lessac-medium").Wait();
    }

    /// <summary>
    /// Download and extract the Piper executable into the working directory when it is not already present.
    /// </summary>
    /// <remarks>Performs no action if the Piper executable already exists. May throw I/O or network-related
    /// exceptions if download or extraction fails.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task InitializePiper()
    {
        // Download and extract the Piper executable to your working directory if the Piper executable is not already present
        if (!File.Exists(Path.Join(directory, "piper", "piper.exe")))
        {
            await PiperDownloader.DownloadPiper().ExtractPiper(directory);
        }
    }

    /// <summary>
    /// Loads the voice model identified by modelKey asynchronously, downloading it if missing, and initializes the
    /// Piper provider.
    /// </summary>
    /// <remarks>If models/{modelKey}/{modelKey}.onnx is not present, the model is downloaded into the local
    /// models directory. The PiperProvider is then configured with the downloaded or existing model and the piper
    /// executable under the application's piper directory.</remarks>
    /// <param name="modelKey">Key that identifies the voice model to load.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public async Task LoadModel(string modelKey)
    {
        // Fetch a specific voice model (e.v., a medium-quality US English voice) if the model is not already present
        VoiceModel? model = null;
        string modelPath = Path.Join(directory, "models", modelKey);
        if (Directory.Exists(modelPath))
        {
            model = await VoiceModel.LoadModel(modelPath);
        }
        else
        {
            model = await PiperDownloader.GetModelByKey(modelKey);
            model = await model.DownloadModel(Path.Join(directory, "models"));
        }

        //Load the provider with the model and the piper executable
        piperProvider = new PiperProvider(new PiperConfiguration()
        {
            ExecutableLocation = Path.Join(directory, "piper", "piper.exe"),
            Model = model
        });
    }

    public async Task LoadModel(ModelKeys modelKey)
    {
        await LoadModel(HuggingFaceModelList.ModelKeys[(int)modelKey]);
    }

    /// <summary>
    /// Generates audio from text and returns it as a byte array.
    /// </summary>
    /// <param name="text">The text to convert to audio.</param>
    /// <param name="outputType">The type of audio output to generate.</param>
    /// <returns>A task that represents the asynchronous operation and returns the generated audio as a byte array.</returns>
    public async Task<byte[]> GenerateAudio(string text, AudioOutputType outputType = AudioOutputType.Wav)
    {
        try
        {
            byte[] audioBytes = await piperProvider.InferAsync(text, outputType);
            return audioBytes ?? Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PiperWrapper] GenerateAudio failed: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Synthesizes speech from the provided text and writes the resulting audio to a file.
    /// </summary>
    /// <remarks>Overwrites any existing file at the specified path. Propagates exceptions from audio
    /// generation or file I/O.</remarks>
    /// <param name="text">Text to synthesize into audio.</param>
    /// <param name="outputFileName">Path of the file to create or overwrite with the generated audio.</param>
    /// <param name="outputType">Desired audio format for the output file.</param>
    /// <returns>A Task that completes when the audio has been written to disk.</returns>
    //public async Task WriteAudio(string text, string outputFileName, AudioOutputType outputType = AudioOutputType.Wav)
    //{
    //    //await File.WriteAllBytesAsync(outputFileName, await GenerateAudio(text, outputType));

    //    byte[] audioBytes = await GenerateAudio(text, outputType);
    //    await File.WriteAllBytesAsync(outputFileName, audioBytes);
    //}

    public async Task WriteAudio(string text, string outputFileName, AudioOutputType outputType = AudioOutputType.Wav)
    {
        try
        {
            byte[] audioBytes = GenerateAudio(text, outputType).Result;
            await File.WriteAllBytesAsync(outputFileName, audioBytes);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"WriteAudio failed: {ex}");
            throw;
        }
    }
}

public static class HuggingFaceModelList
{
    public static List<string> ModelKeys = new List<string>
    {
        "en_GB-alan-low",
        "en_GB-alan-medium",
        "en_GB-alba-medium",
        "en_GB-aru-medium",
        "en_GB-cori-high",
        "en_GB-cori-medium",
        "en_GB-jenny_dioco-medium",
        "en_GB-northern_english_male-medium",
        "en_GB-semain-medium",
        "en_GB-southern_english_female-low",
        "en_GB-vctk-medium",
        "en_US-amy-low",
        "en_US-amy-medium",
        "en_US-arctic-medium",
        "en_US-bryce-medium",
        "en_US-danny-low",
        "en_US-hfc_female-medium",
        "en_US-hfc_male-medium",
        "en_US-joe-medium",
        "en_US-john-medium",
        "en_US-kathleen-low",
        "en_US-kristin-medium",
        "en_US-kusal-medium",
        "en_US-l2artic-medium",
        "en_US-lessac-high",
        "en_US-lessac-low",
        "en_US-lessac-medium",
        "en_US-libritts-high",
        "en_US-libritts_r-medium",
        "en_US-ljspeech-high",
        "en_US-ljspeech-medium",
        "en_US-mike-medium",
        "en_US-norman-medium",
        "en_US-reza_ibrahim-medium",
        "en_US-ryan-high",
        "en_US-ryan-low",
        "en_US-ryan-medium",
        "en_US-sam-medium"
    };
}

public enum ModelKeys
{
    alanLow,
    alanMedium,
    albaMedium,
    aruMedium,
    coriHigh,
    coriMedium,
    jennyDiocoMedium,
    northernEnglishMaleMedium,
    semainMedium,
    southernEnglishFemaleLow,
    vctkMedium,
    amyLow,
    amyMedium,
    arcticMedium,
    bryceMedium,
    dannyLow,
    hfcFemaleMedium,
    hfcMaleMedium,
    joeMedium,
    johnMedium,
    kathleenLow,
    kristinMedium,
    kusalMedium,
    l2ArticMedium,
    lessacHigh,
    lessacLow,
    lessacMedium,
    librittsHigh,
    librittsRMedium,
    ljspeechHigh,
    ljspeechMedium,
    mikeMedium,
    normanMedium,
    rezaIbrahimMedium,
    ryanHigh,
    ryanLow,
    ryanMedium,
    samMedium
}