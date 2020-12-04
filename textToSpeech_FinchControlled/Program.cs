using FinchAPI;
using System;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;

namespace textToSpeech_FinchControl
{
    internal class Program
    {
        private static ManualResetEvent recongnitionComplete;

        private static void Main(string[] args)
        {
            DisplayMainMenu();
            Finch myFinch = new Finch();
            Console.CursorVisible = false;
            SpeechRecognition();
        }

        /// <summary>
        /// recognize speech
        /// adapted from the following CodeProject article
        /// https://www.codeproject.com/Articles/483347/Speech-recognition-speech-to-text-text-to-speech-a#speechrecognitionincsharp
        /// </summary>
        private static void SpeechRecognition()
        {
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
            recongnitionComplete = new ManualResetEvent(false);

            //
            // load grammar elements
            //
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("red")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("blue")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("forward")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("backward")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("right")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("left")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("stop")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("ghost")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("temperature F")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("temperature C")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("quit")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("movement")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("continue")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("guess")));
            //
            // add event handler to manage recognized speech
            //
            recognizer.SpeechRecognized += RecognizerSpeechRecognized;

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            recongnitionComplete.WaitOne();
            recognizer.Dispose();
        }

        /// <summary>
        /// event handler to manage recognized speech
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RecognizerSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SpeechSynthesizer warningVoice = new SpeechSynthesizer();
            Finch robotFinch = new Finch();
            robotFinch.connect();
            Console.CursorVisible = false;
            Console.WriteLine(e.Result.Text);
            switch (e.Result.Text)
            {
                case "red":
                    robotFinch.setLED(250, 0, 0);
                    break;

                case "blue":
                    robotFinch.setLED(0, 0, 250);
                    break;

                case "movement":
                    robotMovement(robotFinch, warningVoice, sender, e);
                    break;

                case "ghost":
                    BuildAGhost(warningVoice);
                    break;

                case "temperature C":
                    WriteTempatureC(robotFinch);
                    break;

                case "temperature F":
                    Console.WriteLine((robotFinch.getTemperature() * 9 / 5) + 32);
                    break;

                case "guess":
                    GuessingGame(robotFinch, warningVoice, sender, e);
                    break;
                case "calculator":
                    DisplayCalculatorFunction(warningVoice, sender, e);
                    break;
                case "quit":
                    Console.Clear();
                    //
                    // TODO - fix bug requiring two consecutive "quit" commands
                    //
                    recongnitionComplete.Set();
                    recongnitionComplete.WaitOne(500);
                    recongnitionComplete.Set();
                    break;
            }
        }
        #region CALCULATOR
        private static void DisplayCalculatorFunction(SpeechSynthesizer warningVoice, object sender, SpeechRecognizedEventArgs e)
        {
        }
        #endregion
        #region FUNCTIONS

        private static void BuildAGhost(SpeechSynthesizer warningVoice)
        {
            Console.WriteLine("  _____ ");
            Console.WriteLine(" /     \\");
            Console.WriteLine("/ 0   o|");
            Console.WriteLine("|  _   |");
            Console.WriteLine("| |_|  |");
            Console.WriteLine("|      |");

            warningVoice.Speak("Boo....I am a spooky ghost");
        }

        private static void WriteTempatureC(Finch finchRobot)
        {
            string dataPath = @"Text\TextFile1.txt";
            string dataTaken = $"Tempature is: {finchRobot.getTemperature()} Celsius\n At the time of {DateTime.Now}.";
            using (StreamWriter sw = new StreamWriter(dataPath, true))
            {
                sw.WriteLine(dataTaken);
            }
        }

        private static void DisplayTemperatureC(Finch finchRobot, object sender, SpeechRecognizedEventArgs e)
        {
            string dataPath = @"Text\TextFile1.txt";
            File.OpenRead(dataPath);
        }

        private static void robotMovement(Finch robotFinch, SpeechSynthesizer warningVoice, object sender, SpeechRecognizedEventArgs e)
        {
            Console.CursorVisible = false;
            Console.WriteLine(e.Result.Text);
            Thread.Sleep(5000);
            switch (e.Result.Text)
            {
                case "forward":
                    Console.Clear();
                    robotFinch.setMotors(150, 150);
                    break;

                case "stop":
                    Console.Clear();
                    robotFinch.setMotors(0, 0);
                    break;

                case "backward":
                    Console.Clear();
                    robotFinch.setMotors(-150, -150);
                    break;

                case "right":
                    Console.Clear();
                    if (robotFinch.isObstacleRightSide() == true)
                    {
                        Console.WriteLine("Warning, obstruction on that side..ignoring command.");
                        warningVoice.Speak("Warning, obstruction on that side..ignoring command.");
                    }
                    else
                    {
                        robotFinch.setMotors(150, -150);
                        robotFinch.wait(3000);
                        robotFinch.setMotors(0, 0);
                    }
                    break;

                case "left":
                    Console.Clear();
                    if (robotFinch.isObstacleLeftSide() == true)
                    {
                        Console.WriteLine("Warning, obstruction on that side..ignoring command.");
                        warningVoice.Speak("Warning, obstruction on that side..ignoring command.");
                    }
                    else
                    {
                        robotFinch.setMotors(-150, 150);
                        robotFinch.wait(3000);
                        robotFinch.setMotors(0, 0);
                    }
                    break;
            }
        }

        #endregion FUNCTIONS
        #region GUESSING GAME
        //
        //Guessing Game Functions within
        //
        private static void GuessingGame(Finch robotFinch, SpeechSynthesizer warningVoice, object sender, SpeechRecognizedEventArgs e)
        {
            //
            //Main Menu Display and rules
            //
            DisplayGuessingGameMenu();
            Console.WriteLine("");
            //
            //Initalizing variables
            //
            Random r = new Random();
            int x = r.Next(0, 2);
            Console.CursorVisible = false;
            string userResponse, robotResponse;
            Console.WriteLine(e.Result.Text);
            string[] choices = new string[3];
            choices.SetValue("blue", 0);
            choices.SetValue("green", 1);
            choices.SetValue("red", 2);
            userResponse = e.Result.Text;
            robotResponse = choices[x];
            //
            //This is a simple check for if the response is correct or not
            //
            if(userResponse == robotResponse)
            {
                Console.WriteLine("Congrats you have matched the robots response.");
                Console.WriteLine($"The robot said: {robotResponse}");
                SetFinchRobotLED(robotResponse, robotFinch);
                robotFinch.wait(4000);
                robotFinch.setLED(0, 0, 0);
            }
            if(userResponse != robotResponse)
            {
                Console.WriteLine("Robot has won");
                Console.WriteLine($"The robot had said {robotResponse}");
                Console.WriteLine("Better luck next time.");
            }
        }
        //
        //Loop to set the finch LED in response to the answer given
        //
        private static void SetFinchRobotLED(string ledSet, Finch robotFinch)
        {
            if (ledSet == "red")
            {
                robotFinch.setLED(255, 0, 0);
            }
            if (ledSet == "green")
            {
                robotFinch.setLED(0, 255, 0);
            }
            if (ledSet == "blue")
            {
                robotFinch.setLED(0, 0, 255);
            }
        }
        #endregion
        #region DISPLAY

        private static void DisplayHeader(string displayHeader)
        {
            Console.WriteLine($"\t\t{displayHeader}");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void DisplayMainMenu()
        {
            DisplayHeader("Main Menu");
            Console.WriteLine("\ta) Red");
            Console.WriteLine("\tb) Blue");
            Console.WriteLine("\tc) movement");
            Console.WriteLine("\td) ghost");
            Console.WriteLine("\te) temperature C");
            Console.WriteLine("\tf) temperature F");
            Console.WriteLine("\tg) guess");
            Console.WriteLine("quit to quit");
        }

        private static void DisplayGuessingGameMenu()
        {
            DisplayHeader("Guessing Game");
            Console.WriteLine("The rules are simply, the finch will light up with a random color.");
            Console.WriteLine("It will either be red, green or blue.");
            Console.WriteLine("Take a guess and if its correct then you will win!");
            Console.WriteLine("Good luck.");
            Console.WriteLine();
            Console.WriteLine("Say continue to proceed.");
        }
        private static void DisplayCalculatorFunction()
        {
            DisplayHeader("Calculator");
            Console.WriteLine("1)Addition");
            Console.WriteLine("2)Subtraction");
            Console.WriteLine("3)Multiply");
            Console.WriteLine("4)Division");
            Console.WriteLine("5)Two numbers to enter.");
            Console.WriteLine("Continue.");
        }
        #endregion DISPLAY
    }
}