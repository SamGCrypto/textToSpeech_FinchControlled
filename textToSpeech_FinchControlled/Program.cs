using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using System.Windows;
using FinchAPI;
using System.Threading;

namespace textToSpeech_FinchControlled
{
    class Program
    {
        static ManualResetEvent recongnitionComplete;
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            SpeechRecognition();
            Finch myFinch = new Finch();
        }

        /// <summary>
        /// recognize speech
        /// adapted from the following CodeProject article
        /// https://www.codeproject.com/Articles/483347/Speech-recognition-speech-to-text-text-to-speech-a#speechrecognitionincsharp
        /// </summary>
        static void SpeechRecognition()
        {
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
            recongnitionComplete = new ManualResetEvent(false);

            //
            // load grammar elements
            //
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("forward")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("backwards")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("right")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("left")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("quit")));

            //
            // add event handler to manage recognized speech
            //

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            recongnitionComplete.WaitOne();
            recognizer.Dispose();
        }
        //
        //Control the Finch with your voice
        //
        static void ControlFinch(object sender, SpeechRecognizedEventArgs e, Finch myFinch) {
            Console.CursorVisible = false;


            switch (e.Result.Text)
            {
                case "forward":
                    myFinch.setMotors(150, 150);
                    break;
                case "backward":
                    myFinch.setMotors(-150, -150);
                    break;
                case "left":
                    myFinch.setMotors(-150, 150);
                    break;
                case "right":
                    myFinch.setMotors(150, -150);
                    break;
                case "quit":
                    recongnitionComplete.Set();
                    recongnitionComplete.WaitOne(500);
                    recongnitionComplete.Set();
                    break;

                default:
                    break;
            }
        
        
        }



    }
}
