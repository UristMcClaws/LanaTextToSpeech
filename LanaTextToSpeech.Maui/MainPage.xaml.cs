using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using LanaTextToSpeech.Sherpa; // ensure project reference
using System.Threading.Tasks;
using SherpaOnnx;

namespace LanaTextToSpeech.Maui
{
    public partial class MainPage : ContentPage
    {
        private LanaSherpaEngine engine;
        private OfflineTtsGeneratedAudio currentAudio;
        private Models model;
        private bool isGenerating = false;
        private ObservableCollection<string> History = new ObservableCollection<string>();
        private string lastModelKey = null;
        private int? lastVoice = null;

        public MainPage()
        {
            InitializeComponent();

            // React to model selection so voices can be populated based on the selected model
            ModelsPicker.SelectedIndexChanged += ModelsPicker_SelectedIndexChanged;

            // Detect Enter (Completed) on the single-line Entry to auto-generate + play
            InputEditor.Completed += InputEditor_Completed;

            // Hook up history view and load models (SelectedIndexChanged handler will populate voices)
            HistoryList.ItemsSource = History;

            LoadModels();
        }

        private void ModelsPicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (ModelsPicker.SelectedItem is Models selectedModel)
            {
                model = selectedModel;
                LoadVoices(selectedModel.NumberOfSpeakers);
            }
            else
            {
                // fallback to default voice count
                LoadVoices();
            }
        }

        private void LoadModels()
        {
            try
            {
                // Use the compiled list of models from the Sherpa project instead of scanning disk
                var models = ModelList.GetModels() ?? new List<Models>();

                // Show the friendly Name in the Picker and keep the Models objects as the ItemsSource
                ModelsPicker.ItemDisplayBinding = new Binding("Name");
                ModelsPicker.ItemsSource = models;

                if (models.Any())
                {
                    ModelsPicker.SelectedIndex = 0;
                    model = models[0];
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading models: {ex.Message}";
            }
        }

        private void LoadVoices(int numberOfSpeakers = 20)
        {
            try
            {
                if (numberOfSpeakers <= 0) numberOfSpeakers = 20;

                // Create items 0..(numberOfSpeakers-1)
                VoicesPicker.ItemsSource = Enumerable.Range(0, numberOfSpeakers)
                                                     .Select(i => i.ToString())
                                                     .ToList();

                if (numberOfSpeakers > 0)
                    VoicesPicker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading voices: {ex.Message}";
            }
        }

        private async void GenerateButton_Clicked(object sender, EventArgs e)
        {
            if (ModelsPicker.SelectedItem == null)
            {
                await DisplayAlert("Model required", "Please select a model.", "OK");
                return;
            }

            string modelKey;
            if (ModelsPicker.SelectedItem is Models selectedModel)
            {
                model = selectedModel;
                modelKey = selectedModel.Key;
            }
            else
            {
                modelKey = ModelsPicker.SelectedItem?.ToString() ?? string.Empty;
            }

            int voice = VoicesPicker.SelectedItem != null ? int.Parse(VoicesPicker.SelectedItem.ToString()) : 0;
            string text = InputEditor.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                await DisplayAlert("Text required", "Please enter text to generate audio.", "OK");
                return;
            }

            try
            {
                await GenerateAudioAsync(modelKey, voice, text);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                StatusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async Task GenerateAudioAsync(string modelKey, int voice, string text)
        {
            if (isGenerating) return;
            isGenerating = true;
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => StatusLabel.Text = "Generating...");

                engine = new LanaSherpaEngine(modelKey);
                var generated = await Task.Run(() => engine.GenerateAudio(text, voice));
                currentAudio = generated;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    StatusLabel.Text = "Audio generated.";
                    PlayButton.IsEnabled = currentAudio != null;
                    SaveButton.IsEnabled = currentAudio != null;
                });

                // Append to history (timestamp + short summary) — do this after UI update on main thread
                var summary = $"Generated: \"{(text.Length > 80 ? text.Substring(0, 80) + "..." : text)}\"";
                AddHistoryEntry(summary, modelKey, voice);
            }
            finally
            {
                isGenerating = false;
            }
        }

        // Add a timestamped history entry; include model/voice only when different from last recorded.
        private void AddHistoryEntry(string baseText, string modelKey = null, int? voice = null)
        {
            string suffix = string.Empty;
            if (!string.IsNullOrEmpty(modelKey))
            {
                if (lastModelKey == null || lastModelKey != modelKey || lastVoice != voice)
                {
                    suffix = $" (Model={modelKey}, Voice={voice})";
                    lastModelKey = modelKey;
                    lastVoice = voice;
                }
            }

            var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {baseText}{suffix}";
            MainThread.BeginInvokeOnMainThread(() => History.Insert(0, entry));
        }

        // Entry.Completed handler triggers when the Return/Done key is pressed on the keyboard.
        private async void InputEditor_Completed(object sender, EventArgs e)
        {
            if (isGenerating) return;

            string text = InputEditor.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return;

            string modelKey;
            if (ModelsPicker.SelectedItem is Models selectedModel)
            {
                model = selectedModel;
                modelKey = selectedModel.Key;
            }
            else
            {
                modelKey = ModelsPicker.SelectedItem?.ToString() ?? string.Empty;
            }

            int voice = VoicesPicker.SelectedItem != null ? int.Parse(VoicesPicker.SelectedItem.ToString()) : 0;

            try
            {
                await GenerateAudioAsync(modelKey, voice, text);
                // Update UI and clear input before starting playback
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    StatusLabel.Text = "Playing...";
                    // Clear the input after triggering generate+play
                    InputEditor.Text = string.Empty;
                });

                // Run playback on background thread and await completion so we can update status when done
                await Task.Run(() => engine?.PlayAudio(currentAudio));

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    StatusLabel.Text = "Finished playing.";
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() => StatusLabel.Text = $"Error: {ex.Message}");
            }
        }

        private async void PlayButton_Clicked(object sender, EventArgs e)
        {
            if (engine == null || currentAudio == null)
            {
                await DisplayAlert("No audio", "Generate audio first.", "OK");
                return;
            }

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => StatusLabel.Text = "Playing...");
                int? playVoice = VoicesPicker.SelectedItem != null ? int.Parse(VoicesPicker.SelectedItem.ToString()) : (int?)null;

                await Task.Run(() => engine.PlayAudio(currentAudio));

                await MainThread.InvokeOnMainThreadAsync(() => StatusLabel.Text = "Finished playing.");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Play error", ex.Message, "OK");
                StatusLabel.Text = $"Play error: {ex.Message}";
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (engine == null || currentAudio == null)
            {
                await DisplayAlert("No audio", "Generate audio first.", "OK");
                return;
            }

            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string outPath = Path.Combine(docs, $"lana_tts_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                // Save using engine helper
                await Task.Run(() => engine.SaveAudioToFile(currentAudio, outPath));
                await DisplayAlert("Saved", $"File saved to:\n{outPath}", "OK");
                StatusLabel.Text = $"Saved: {outPath}";
                try { History.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Saved file: {outPath}"); } catch { }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Save error", ex.Message, "OK");
                StatusLabel.Text = $"Save error: {ex.Message}";
            }
        }
    }
}