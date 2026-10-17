using System;
using System.Collections.Generic;
using System.Text;

namespace LanaTextToSpeech.PiperWrapper;

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