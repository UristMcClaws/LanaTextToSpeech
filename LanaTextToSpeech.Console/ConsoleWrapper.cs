using System;
using System.Collections.Generic;
using System.Text;
using LanaTextToSpeech.PiperWrapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using LanaTextToSpeech.Sherpa;

namespace LanaTextToSpeech.Console;

internal class ConsoleWrapper
{
    private int audioIndex;
    private LanaPiperWrapper piperWrapper = new LanaPiperWrapper();

    public ConsoleWrapper(int audioIndex)
    {
        this.audioIndex = audioIndex;
    }

    public void PiperVersion()
    {
        System.Console.WriteLine("Testing the Piper wrapper");

        //piperWrapper.LoadModel(ModelKeys.librittsHigh).Wait();
        piperWrapper.LoadModel("vits-en_us_libritts-high").Wait();

        bool keepGoing = true;

        while (keepGoing)
        {
            keepGoing = AudioPiperLoop();
        }

        //System.Console.WriteLine();
        //System.Console.WriteLine("Please write the text you want to convert to speech (press Enter when done):");
        //string textToConvert = System.Console.ReadLine();

        //byte[] audioData = piperWrapper.GenerateAudio(textToConvert).Result;

        //piperWrapper.WriteAudio(textToConvert, "output.wav");

        System.Console.WriteLine();
        System.Console.WriteLine("Piper wrapper test completed.");
    }

    public void SherpaVersion()
    {
        System.Console.WriteLine("Testing the Sherpa wrapper");
        LanaSherpaEngine sherpaEngine = new LanaSherpaEngine("en_US-libritts-high");

        bool keepGoing = true;

        while (keepGoing)
        {
            keepGoing = AudioSherpaLoop(sherpaEngine);
        }

        //sherpaEngine.LoadModel("vits-piper-en_US-libritts-high").Wait();
        //bool keepGoing = true;
        //while (keepGoing)
        //{
        //    keepGoing = AudioLoop();
        //}
        System.Console.WriteLine();
        System.Console.WriteLine("Sherpa wrapper test completed.");
    }

    private bool AudioSherpaLoop(LanaSherpaEngine sherpaEngine)
    {
        bool KeepGoing = true;

        System.Console.WriteLine();
        System.Console.WriteLine("Please write the text you want to convert to speech (press Enter when done):");
        string textToConvert = System.Console.ReadLine() ?? string.Empty;

        if (textToConvert.Trim().Length == 0)
        {
            System.Console.WriteLine("No text provided. Exiting.");
            return false;
        }

        var audioData = sherpaEngine.GenerateAudio(textToConvert);
        sherpaEngine.PlayAudio(audioData);
        return true;

        //byte[] audioData = piperWrapper.GenerateAudio(textToConvert).Result;
        //PlayAudio(audioData);
        //return true;
    }

    private bool AudioPiperLoop()
    {
        bool KeepGoing = true;

        System.Console.WriteLine();
        System.Console.WriteLine("Please write the text you want to convert to speech (press Enter when done):");
        string textToConvert = System.Console.ReadLine();

        if (textToConvert == null || textToConvert.Trim().Length == 0)
        {
            System.Console.WriteLine("No text provided. Exiting.");
            return false;
        }

        byte[] audioData = piperWrapper.GenerateAudio(textToConvert).Result;
        PlayAudio(audioData);
        return true;
    }

    public void PlayAudio(byte[] audioData)
    {
        var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        using (MemoryStream ms = new MemoryStream(audioData))
        using (WaveFileReader waveReader = new WaveFileReader(ms))
        using (WasapiPlayer player = new WasapiPlayerBuilder()
                .WithDevice(devices[audioIndex])
                .WithEventSync()
                .Build())
        {
            player.Init(waveReader);
            player.Play();
            while (player.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(100);
            }
        }
    }
}