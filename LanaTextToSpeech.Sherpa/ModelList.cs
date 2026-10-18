using System;
using System.Collections.Generic;
using System.Text;

namespace LanaTextToSpeech.Sherpa
{
    public static class ModelList
    {
        public static List<Models> GetModels()
        {
            return new List<Models>
            {
                new Models { Name = "English (US) - LibriTTS High Quality", Key = "en_US-libritts-high", FolderName = "en_US-libritts-high", NumberOfSpeakers = 904 },
            };
        }
    }
}