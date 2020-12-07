using FinchAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;

namespace textToSpeech_FinchControl
{
    //*************************************//
    //Author: Samuel Gorcyca
    //Date:12/5/20
    //Assignment: CIT 110 Capstone
    //Program Description: Speech to Text AI
    //**************************************//
    internal class Program
    {
        private static ManualResetEvent recongnitionComplete;

        private static void Main(string[] args)
        {
            //
            //TODO: Investigate looping function of Speech Class
            //
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
            //
            //LED GRAMMAR ELEMENTS
            //
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("red")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("blue")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("green")));
            //
            //movement commands for the finch
            //
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("forward")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("backward")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("right")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("left")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("stop")));
            //
            //Function
            //
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("ghost")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("temperature F")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("temperature C")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("display temperature C")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("quit")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("continue")));
            recognizer.LoadGrammar(new Grammar(new GrammarBuilder("guess")));
            //
            //Calculator terms
            //
            //recognizer.loadgrammar(new grammar(new grammarbuilder("calculator")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("0")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("1")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("2")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("3")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("4")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("5")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("6")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("7")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("8")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("9")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("add")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("subtract")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("divide")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("multiply")));
            //recognizer.loadgrammar(new grammar(new grammarbuilder("number")));

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
                case "green":
                    robotFinch.setLED(0, 250, 0);
                    break;
                case "blue":
                    robotFinch.setLED(0, 0, 250);
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

                case "display temperature C":
                    DisplayTemperatureC();
                    break;

                case "guess":
                    GuessingGame(robotFinch);
                    break;

                //case "calculator":
                //  CalculatorFunction(warningVoice, sender, e);
                  //break;
                  //case "movement":
                //  robotMovement(robotFinch, warningVoice, sender, e);
                //  break;

                case "quit":
                    Console.Clear();
                    //
                    // TODO - fix bug requiring two consecutive "quit" commands
                    //
                    recongnitionComplete.Set();
                    recongnitionComplete.WaitOne(500);
                    recongnitionComplete.Set();
                    break;
                    //
                    //Movement Functions temporarily put here outside of their method
                    //requires a way to actively listen to commands
                    //Working on that
                    //
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
            Console.Clear();
            DisplayMainMenu();
        }

        #region CALCULATOR

        private static void CalculatorFunction(SpeechSynthesizer warningVoice, object sender, SpeechRecognizedEventArgs e)
        {
            DisplayCalculatorFunction();
            int num1 = 0;
            int num2 = 0;
            Console.CursorVisible = false;
            Console.WriteLine(e.Result.Text);
            Thread.Sleep(5000);
            switch (e.Result.Text)
            {
                case "multiply":
                    MultiplicationCalculator(num1, num2);
                    break;

                case "divide":
                    if (num2 == 0)
                    {
                        Console.WriteLine("ERROR");
                        Console.WriteLine("cannot divide by zero");
                    }
                    else
                    {
                        DivideCalculator(num1, num2);
                    }
                    break;

                case "subtract":
                    SubtractCalculator(num1, num2);
                    break;

                case "add":
                    AddCalculator(num1, num2);
                    break;

                case "numbers":
                    num1 = GetNumber(sender, e);
                    num2 = GetNumber(sender, e);
                    break;
            }
        }

        private static int GetNumber(object sender, SpeechRecognizedEventArgs e)
        {
            int num = 0;
            string userResponse = null;
            bool endNow = false;
            while (!endNow)
            {
                Console.WriteLine("Please enter in a series of numbers 9-0.");
                userResponse = e.Result.ToString();
                if (userResponse == "continue")
                {
                    endNow = true;
                }
                else
                {
                    try
                    {
                        int.TryParse(userResponse, out num);
                    }
                    catch
                    {
                        Console.WriteLine("That only works with numbers!");
                        Console.WriteLine("Please try again.");
                    };
                }
            }
            return num;
        }

        private static void MultiplicationCalculator(int num1, int num2)
        {
            int totalNum;
            totalNum = num1 * num2;
            Console.WriteLine($"The Answer is {totalNum}");
            Console.WriteLine($"The equation.");
            Console.WriteLine($"{totalNum} = {num1} * {num2}");
        }

        private static void DivideCalculator(int num1, int num2)
        {
            int totalNum;
            totalNum = num1 / num2;
            Console.WriteLine($"The Answer is {totalNum}");
            Console.WriteLine($"The equation.");
            Console.WriteLine($"{totalNum} = {num1} / {num2}");
        }

        private static void AddCalculator(int num1, int num2)
        {
            int totalNum;
            totalNum = num1 + num2;
            Console.WriteLine($"The Answer is {totalNum}");
            Console.WriteLine($"The equation.");
            Console.WriteLine($"{totalNum} = {num1} + {num2}");
        }

        private static void SubtractCalculator(int num1, int num2)
        {
            int totalNum;
            totalNum = num1 - num2;
            Console.WriteLine($"The Answer is {totalNum}");
            Console.WriteLine($"The equation.");
            Console.WriteLine($"{totalNum} = {num1} - {num2}");
        }

        #endregion CALCULATOR

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
            //
            //TODO Add in number of times data is gathered.
            //
            string dataPath = @"Data\TextFile1.txt";
            string dataTaken = $"Tempature is: {finchRobot.getTemperature()} Celsius\n At the time of {DateTime.Now}.";
            File.WriteAllText(dataPath, dataTaken);
            Console.WriteLine(dataTaken);
            Thread.Sleep(10000);
        }

        private static void DisplayTemperatureC()
        {
            string dataPath = @"Data\TextFile1.txt";
            string[] holding;
            holding = File.ReadAllLines(dataPath);
            Console.WriteLine(holding);
            Thread.Sleep(10000);
        }

        //private static void robotMovement(Finch robotFinch, SpeechSynthesizer warningVoice, object sender, SpeechRecognizedEventArgs e)
        //{
        //    Console.CursorVisible = false;
        //    //
        //    //TODO- figure out how to get text to speech to clear itself when entering a new method
        //    //
        //    Console.WriteLine();
        //    Thread.Sleep(5000);
        //    switch (e.Result.Text)
        //    {
        //        case "forward":
        //            Console.Clear();
        //            robotFinch.setMotors(150, 150);
        //            break;

        //        case "stop":
        //            Console.Clear();
        //            robotFinch.setMotors(0, 0);
        //            break;

        //        case "backward":
        //            Console.Clear();
        //            robotFinch.setMotors(-150, -150);
        //            break;

        //        case "right":
        //            Console.Clear();
        //            if (robotFinch.isObstacleRightSide() == true)
        //            {
        //                Console.WriteLine("Warning, obstruction on that side..ignoring command.");
        //                warningVoice.Speak("Warning, obstruction on that side..ignoring command.");
        //            }
        //            else
        //            {
        //                robotFinch.setMotors(150, -150);
        //                robotFinch.wait(3000);
        //                robotFinch.setMotors(0, 0);
        //            }
        //            break;

        //        case "left":
        //            Console.Clear();
        //            if (robotFinch.isObstacleLeftSide() == true)
        //            {
        //                Console.WriteLine("Warning, obstruction on that side..ignoring command.");
        //                warningVoice.Speak("Warning, obstruction on that side..ignoring command.");
        //            }
        //            else
        //            {
        //                robotFinch.setMotors(-150, 150);
        //                robotFinch.wait(3000);
        //                robotFinch.setMotors(0, 0);
        //            }
        //            break;
        //    }
        //}

        #endregion FUNCTIONS

        #region GUESSING GAME

        //
        //Guessing Game Functions within
        //
        //TODO - FIGURE OUT TEXT TO SPEECH RESET
        //
        private static void GuessingGame(Finch robotFinch)
        {
            //
            //Main Menu Display and rules
            //
            DisplayGuessingGameMenu();
            Console.WriteLine("");
            Thread.Sleep(5000);
            //
            //Initalizing variables
            //
            Random r = new Random();
            int x = r.Next(0, 2);
            Console.CursorVisible = true;
            string userResponse, robotResponse;
            string[] choices = new string[3];
            choices.SetValue("blue", 0);
            choices.SetValue("green", 1);
            choices.SetValue("red", 2);
            robotResponse = choices[x];
            //
            //Temporary fix to allow user to add input into the guessing game
            //
            Console.WriteLine("Enter in user response below.");
            userResponse = Console.ReadLine();
            //
            //This is a simple check for if the response is correct or not
            //
            if (userResponse == robotResponse)
            {
                Console.WriteLine("Congrats you have matched the robots response.");
                Console.WriteLine($"The robot said: {robotResponse}");
                SetFinchRobotLED(robotResponse, robotFinch);
                robotFinch.wait(4000);
                robotFinch.setLED(0, 0, 0);
            }
            if (userResponse != robotResponse)
            {
                Console.WriteLine("Robot has won");
                Console.WriteLine($"The robot had said {robotResponse}");
                Console.WriteLine("Better luck next time.");
                Thread.Sleep(5000);
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

        #endregion GUESSING GAME

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
            Console.WriteLine("\t1) Red");
            Console.WriteLine("\t2) Blue");
            Console.WriteLine("\t2) Green");
            Console.WriteLine("\t3) movement");
            Console.WriteLine("\t4) ghost");
            Console.WriteLine("\t5) temperature C");
            Console.WriteLine("\t6) temperature F");
            Console.WriteLine("\t7) guess");
            Console.WriteLine("\t8) forward");
            Console.WriteLine("\t9) backward");
            Console.WriteLine("\t10) left");
            Console.WriteLine("\t11) right");
            Console.WriteLine("\t12) stop");
            Console.WriteLine("quit to quit");
        }

        private static void Disclaimer()
        {
            SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Speak("Hello, pardon the intrusion.");
            Thread.Sleep(1000);
            speechSynthesizer.Speak("Some features of this program aren't available.");
            speechSynthesizer.Speak("Kind of like the lackluster menu interface.");
            Thread.Sleep(1000);
            speechSynthesizer.Speak("This is a mostly vocal based software and is not meant to use visual format.");
            speechSynthesizer.Speak("The commands are listed as they should be given.");
            speechSynthesizer.Speak("Certain commands will lead you to things that require input currently.");
            speechSynthesizer.Speak("Thank you for your patience, onto the program.");
        }

        private static void DisplayMovementMenu()
        {
            Console.Clear();
            DisplayHeader("Movement Menu");
            Console.WriteLine("\t1) Forward");
            Console.WriteLine("\t2) Backward");
            Console.WriteLine("\t3) Left");
            Console.WriteLine("\t4) Right");
            Console.WriteLine();
            Console.WriteLine("Quit to exit program.");
        }

        private static void DisplayGuessingGameMenu()
        {
            DisplayHeader("Guessing Game");
            Console.WriteLine("The rules are simply, the finch will light up with a random color.");
            Console.WriteLine("It will either be red, green or blue.");
            Console.WriteLine("Take a guess and if its correct then you will win!");
            Console.WriteLine("Good luck.");
            Console.WriteLine();
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