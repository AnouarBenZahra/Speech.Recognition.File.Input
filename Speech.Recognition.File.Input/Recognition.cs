using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speech.Recognition.File.Input
{
    public class Recognition
    {
        public async void Voice(string inputNameWave, SpeechConfig speechConfig)
        {
            string result = string.Empty;
            var stopRecognition = new TaskCompletionSource<int>();
            using (var inputVoice = AudioConfig.FromWavFileInput(inputNameWave))
            {
                using (var speechRecognizer = new SpeechRecognizer(speechConfig, inputVoice))
                {
                    speechRecognizer.Recognizing += (s, e) =>
                    {
                        result = e.Result.Text;
                    };
                    speechRecognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            result = e.Result.Text;
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            result = "Error";
                        }
                    };
                    speechRecognizer.Canceled += (s, e) =>
                    {
                        result = "Error" + e.Reason;

                        if (e.Reason == CancellationReason.Error)
                        {
                            result = "ErrorCode" + e.ErrorCode + e.ErrorDetails;
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    speechRecognizer.SessionStarted += (s, e) =>
                    {

                    };

                    speechRecognizer.SessionStopped += (s, e) =>
                    {
                        result = "Session stopped event";

                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition
                    await speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
