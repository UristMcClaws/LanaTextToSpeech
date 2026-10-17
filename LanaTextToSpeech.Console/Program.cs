using LanaTextToSpeech.PiperWrapper;
using PiperSharp;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices.Marshalling;
using LanaTextToSpeech.Console;

//var enumerator = new MMDeviceEnumerator();
//var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

//for (int i = 0; i < devices.Count; i++)
//{
//    var device = devices[i];
//    Console.WriteLine($"Device {i}: {device.FriendlyName}");
//}

ConsoleWrapper consoleWrapper = new ConsoleWrapper(0);

consoleWrapper.SherpaVersion();