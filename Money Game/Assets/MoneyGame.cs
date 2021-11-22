using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class MoneyGame : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] Buttons;
   public TextMesh DisplayText;
   public TextMesh[] ButtonText;

   public GameObject[] LEDs;
   public Material Lit;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   string[] LyricIndicator = { "sea shore", "she sore", "she sure", "seesaw", "seizure", "shell sea", "steep store", "sheer sort", "seed spore", "sieve horn", "steel sword" };
   string[] Row = { "she sells", "she shells", "sea shells", "sea sells" };
   string[] Column = { "sea\nshells", "she\nshells", "sea\nsells", "she\nsells" };
   string[,] Order = new string[4, 4] {
      { "1542413", "145322", "5315524", "1523351", },
      { "544343", "322431", "545225", "43233", },
      { "35335", "453151", "31415", "123531", },
      { "3435545", "42135", "3515413", "51142" }
   };
   List<List<string>> Lyrics = new List<List<string>>() { };
   
   int[] Answer = { 0, 0, 0 };
   int[] ButtonOrder = new int[5];
   int[] OrderToTheButtonOrder = new int[5];
   string[] Options = new string[5];
   string PhraseForDisplay = "";
   string AnswerOrder;

   int Iteration;
   int Stage;

   bool Animating;

   void Awake () {
      ModuleId = ModuleIdCounter++;

      foreach (KMSelectable Button in Buttons) {
         Button.OnInteract += delegate () { ButtonPress(Button); return false; };
      }
      Debug.LogFormat("<Money Game> Version number: 1.0");
   }

   void ButtonPress (KMSelectable Button) {
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
      Button.AddInteractionPunch();
      if (ModuleSolved || Animating) {
         return;
      }
      for (int i = 0; i < 5; i++) {
         if (Button == Buttons[i]) {
            if (AnswerOrder[Iteration].ToString() == OrderToTheButtonOrder[i].ToString()) {
               Iteration++;
               Debug.LogFormat("<Money Game #{0}> You pressed {1}. Good.", ModuleId, ButtonText[i].text);
               if (Iteration == AnswerOrder.Length) {
                  StageIncrease();
               }
            }
            else {
               Debug.LogFormat("<Money Game #{0}> You pressed {1}. Poor.", ModuleId, ButtonText[i].text);
               GetComponent<KMBombModule>().HandleStrike();
               GenerateStage();
               StartCoroutine(NewStageAnimation(true));
            }
         }
      }
   }

   void Start () {
      Audio.PlaySoundAtTransform("StartNoise", transform);
      //Instantiates the lyrics and removes all duplicates.
      Lyrics.Add(new List<string> { "She", "sells", "sea", "shells", "on", "the", "sea", "shore", "but", "the", "value", "of", "these", "shells", "will", "fall", "due", "to", "the", "laws", "of", "supply", "and", "demand", "No", "one", "wants", "to", "buy", "shells", "cus", "there\'s", "loads", "on", "the", "sand" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "You", "must", "create", "a", "sense", "of", "scarcity", "Shells", "will", "sell", "much", "better", "if", "the", "people", "think", "they\'re", "rare", "you", "see", "bare", "with", "me", "and", "take", "as", "many", "shells", "as", "you", "can", "find", "and", "hide", "\'em", "on", "an", "island", "stockpile", "\'em", "high", "until", "they\'re", "rarer", "than", "a", "diamond" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "You", "gotta", "make", "the", "people", "think", "that", "they", "want", "\'em", "Really", "want", "\'em", "Really", "fuckin", "want", "\'em", "Hit", "\'em", "like", "Bronson", "Influencers", "product", "placement", "featured", "prime", "time", "entertainment", "If", "you", "haven\'t", "got", "a", "shell", "then", "you\'re", "just", "a", "fucking", "waste", "man" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "It\'s", "Monopoly", "invest", "inside", "some", "property", "start", "a", "corporation", "make", "a", "logo", "do", "it", "properly", "These", "shells", "must", "sell", "that", "will", "be", "your", "new", "philosophy", "Swallow", "all", "your", "morals", "they\'re", "a", "poor", "man\'s", "quality" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "Expand", "Expand", "Expand", "Clear", "forest", "Make", "land", "Fresh", "blood", "On", "hands" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "Why", "just", "shells", "Why", "limit", "yourself", "She", "sells", "sea", "shells", "sell", "oil", "as", "well" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "Guns", "sell", "stocks", "sell", "diamonds", "sell", "rocks", "sell", "water", "to", "a", "fish", "sell", "the", "time", "to", "a", "clock" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "Press", "on", "the", "gas", "Take", "your", "foot", "off", "the", "brakes", "Then", "run", "to", "be", "the", "president", "of", "the", "United", "States" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "Big", "smile", "mate", "big", "wave", "that\'s", "great", "Now", "the", "truth", "is", "overrated", "tell", "lies", "out", "the", "gate" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "Polarize", "the", "people", "controversy", "is", "the", "game", "It", "don\'t", "matter", "if", "they", "hate", "you", "if", "they", "all", "say", "your", "name" }.Select(x => x.ToLower()).Distinct().ToList());
      Lyrics.Add(new List<string> { "The", "world", "is", "yours", "Step", "out", "on", "a", "stage", "to", "a", "round", "of", "applause", "You\'re", "a", "liar", "a", "cheat", "a", "devil", "a", "whore", "and", "you", "sell", "sea", "shells", "on", "the", "sea", "shore" }.Select(x => x.ToLower()).Distinct().ToList());
      GenerateStage();
      StartCoroutine(NewStageAnimation(false));
   }

   void StageIncrease () {
      LEDs[Stage].GetComponent<MeshRenderer>().material = Lit;
      Stage++;
      if (Stage == 3) {
         DisplayText.text = " ";
         GetComponent<KMBombModule>().HandlePass();
         ModuleSolved = true;
      }
      else {
         GenerateStage();
         StartCoroutine(NewStageAnimation(true));
      }
   }

   void GenerateStage () {
      PhraseForDisplay = "";
      Iteration &= 0;
      Answer[0] = Rnd.Range(0, 4); //Generates row
      Answer[1] = Rnd.Range(0, 4); //Generates column
      Answer[2] = Rnd.Range(0, 11); //Generates phrase
      for (int i = 0; i < 5; i++) {
         if (i != 0) {
            ButtonOrder[i] = Rnd.Range(ButtonOrder[i - 1] + 1, Lyrics[Answer[2]].Count() - (5 - i)); //Chooses five spots in the phrase, with the interval being n-1 + 1, to last - n. Ensures order and no index out of range errors.
         }
         else {
            ButtonOrder[i] = Rnd.Range(0, Lyrics[Answer[2]].Count() - 5);
         }
      }
      ButtonOrder.Shuffle();
      for (int i = 0; i < 5; i++) {
         int Counter = 1;
         for (int j = 0; j < 5; j++) {
            if (ButtonOrder[i] > ButtonOrder[j]) {
               Counter++;
            }
         }
         OrderToTheButtonOrder[i] = Counter;//Creates an order that the buttons can use to verify answer.
      }
      for (int i = 0; i < 5; i++) {
         Options[i] = Lyrics[Answer[2]][ButtonOrder[i]];
      }
      AnswerOrder = Order[Answer[0], Answer[1]];
      PhraseForDisplay += Row[Answer[0]] + " " + Column[Answer[1]] + " on the\n" + LyricIndicator[Answer[2]];
      Debug.LogFormat("[Money Game #{0}] The generated phrase was \"{1}\".", ModuleId, PhraseForDisplay.Replace('\n', ' '));
      Debug.LogFormat("[Money Game #{0}] Options are \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\".", ModuleId, Options[0], Options[1], Options[2], Options[3], Options[4]);
      Debug.LogFormat("[Money Game #{0}] The order is {1}.", ModuleId, AnswerOrder);
      Debug.LogFormat("<Money Game #{0}> Button order is {1} {2} {3} {4} {5}.", ModuleId, OrderToTheButtonOrder[0], OrderToTheButtonOrder[1], OrderToTheButtonOrder[2], OrderToTheButtonOrder[3], OrderToTheButtonOrder[4]);
   }

   protected IEnumerator NewStageAnimation (bool Delay) {
      Animating = true;
      DisplayText.text = " ";
      yield return new WaitForSeconds(0.2f);
      for (int i = 0; i < 5; i++) {
         ButtonText[i].text = "";
         ButtonText[i].fontSize = 180;
         yield return new WaitForSeconds(0.1f);
      }

      if (Delay) {
         yield return new WaitForSeconds(0.6f);
      }

      DisplayText.text = PhraseForDisplay;
      yield return new WaitForSeconds(0.2f);
      for (int i = 0; i < 5; i++) {
         ButtonText[i].text = Options[i];
         if (i != 4 && (new string[] { "controversy", "corporation", "influencers"}.Contains(ButtonText[i].text.ToLower()))) {
            ButtonText[i].fontSize = 144;
         }
         if (i != 4 && (new string[] { "entertainment" }.Contains(ButtonText[i].text.ToLower()))) {
            ButtonText[i].fontSize = 130;
         }
         yield return new WaitForSeconds(0.1f);
      }
      Animating = false;
   }

   /* Realized this would be fucking useless
   class Possibilities {
      public string LastLine { get; set; }
      public string[] Lyrics { get; set; }
      public string Order { get; set; }

      public Possibilities (string LastLine, string[] Lyrics, string Order) {
         this.LastLine = LastLine;
         this.Lyrics = Lyrics;
         this.Order = Order;
      }
   }

   readonly List<Possibilities> AllCombinations = new List<Possibilities> {

   };
   */

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} <Label> to press that label, chain commands using spaces.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      string[] Parameters = Command.Trim().ToUpper().Split(' ');
      yield return null;
      for (int i = 0; i < Parameters.Length; i++) {
         int Counter = 0;
         for (int j = 0; j < 5; j++) {
            if (ButtonText[j].text.ToUpper() != Parameters[i]) {
               Counter++;
            }
         }
         if (Counter == 5) {
            yield return "sendtochaterror I don't understand!";
            yield break;
         }
      }
      for (int i = 0; i < Parameters.Length; i++) {
         for (int j = 0; j < 5; j++) {
            if (ButtonText[j].text.ToUpper() == Parameters[i]) {
               Buttons[j].OnInteract();
               yield return new WaitForSeconds(.1f);
            }
         }
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      while (!ModuleSolved) {
         while (Animating) {
            yield return true;
         }
         int start = Iteration;
         for (int j = start; j < AnswerOrder.Length; j++)
         {
            for (int i = 0; i < 5; i++)
            {
               if (AnswerOrder[j].ToString() == OrderToTheButtonOrder[i].ToString())
               {
                   Buttons[i].OnInteract();
                   yield return new WaitForSeconds(.1f);
                   break;
               }
            }
         }
      }
   }
}
