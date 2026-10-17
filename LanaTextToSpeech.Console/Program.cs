using LanaTextToSpeech.PiperWrapper;
using PiperSharp;

Console.WriteLine("Testing the Piper wrapper");

PiperWrapper piperWrapper = new PiperWrapper();
piperWrapper.LoadModel(ModelKeys.librittsHigh).Wait();

Console.WriteLine();
Console.WriteLine("Please write the text you want to convert to speech (press Enter when done):");
string textToConvert = Console.ReadLine();

byte[] test = piperWrapper.GenerateAudio(textToConvert).Result;
piperWrapper.WriteAudio(textToConvert, "output.wav");

Console.WriteLine();
Console.WriteLine("Piper wrapper test completed.");