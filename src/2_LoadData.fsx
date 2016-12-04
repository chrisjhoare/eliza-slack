module Part2
#if INTERACTIVE
#r "/Users/evelina/Projects/ElizaTutorial/packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#load "1_ProcessInput.fsx"
#endif
open FSharp.Data
open Part1

//==================================================
// Eliza, the psychotherapist bot
// Part 2: Loading data
//==================================================
// 
// In this part, we will use F# data with Json type provider
// to load all the data that Eliza needs to function.

// -------------------------------------------------
// DEMO: F# type providers
// -------------------------------------------------

// you can create the json type provider either using an example document
type Demo1 = JsonProvider<"../data/demo.json">
let demoPhrases1 = Demo1.GetSamples()

// or giving it an example of the json document directly
type Demo2 = JsonProvider<"[{\"question\":\"Hello?\",\"answers\":[\"hello\",\"bye\"]}]">
// and then load the actual document
let demoPhrases2 = Demo2.Load("../data/demo.json")

// Explore the contents of the file
demoPhrases2
|> Array.iter (fun x -> 
    printfn "%s" x.Question
    x.Answers |> Array.iter (fun a -> printfn " >> %s" a)) 

// -------------------------------------------------
// TODO: First, we will load the phrases and patterns that Eliza
// uses to respond to user input. These are located in the 
// data folder in the "phrases.json" file. The file contains
// a list of patterns that match user input, and for each pattern,
// it contains a list of possible answer patterns.

// Eliza's answer is a list of words and wildcards
type Answer = Pattern list

// Each phrase contains a pattern that matches user input,
// and an array of potential answers
type Phrases =
    { InputPattern : Sentence
      AnswerPatterns : Answer [] }

// This is how it looks:
let p = { 
    InputPattern = 
      { Contents = [ Word "hello"; Wildcard ]; 
        IsQuestion = false }; 
    AnswerPatterns = [|
        [Word "Hi"; Word "there,";  Wildcard]
        |]
    }

// Create the type provider
type PsychoPhrases = JsonProvider<"[{\"pattern\":\"Something\",\"answers\":[\"Something else.\"]}]">

// -------------------------------------------------
// TODO: Load the phrases from the file and parse their contents.
// When finished, the phrases variable should contain a list
// of Phrases. 
// In each phrase, the InputPattern should contain the clean and parsed
// text of the phrase, without punctuation and in lower case (see Part 1).
// The Answer patterns can keep the punctuation etc.
// -------------------------------------------------
let phrases =
    // ---
    PsychoPhrases.Load("../data/phrases.json")
    |> Array.map (fun phrase ->
        let pattern:Sentence = 
            { Contents = phrase.Pattern |> cleanText |> parseText
              IsQuestion = isQuestion phrase.Pattern }
        let answers = phrase.Answers |> Array.map parseText
        { InputPattern = pattern
          AnswerPatterns = answers }
    )
    |> List.ofArray
    // ---

//==================================================
// Eliza often repeats part of user input, for example:
// 
//    User: I want my cake.
//    Eliza: Why do you want your cake?
// 
// To perform this, Eliza needs to modify user input and
// change all occurrences of "my" to "your", "me" to "you"
// and other similar phrases. 

// -------------------------------------------------
// DEMO: Dictionary may be useful for translating between 
// the two versions of words
// -------------------------------------------------
let demoDict = 
    [| ("white","black");
       ("day","night") |]
    |> dict
if demoDict.ContainsKey "white" then printfn "%s" demoDict.["white"]

// -------------------------------------------------
// TODO: Load the pronouns from the "reflections.json" file 
// and write a function that uses a predefined
// set of pronouns and transforms them into their 
// reflected form.
// -------------------------------------------------

// ---
type Reflections = JsonProvider<"../data/reflections.json">
let reflections = 
    Reflections.GetSamples()
    |> Array.map (fun x -> x.Input, x.Output)
    |> dict
// ---

let reflect (text : string list) = 
   // ---
   text 
   |> List.map (fun t -> 
        if reflections.ContainsKey(t) 
        then reflections.[t] 
        else t)
   // ---        