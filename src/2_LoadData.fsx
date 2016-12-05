module Part2

#if INTERACTIVE
#r "../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
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
for x in demoPhrases2 do
    printfn "%s" x.Question
    for a in x.Answers do printfn " >> %s" a 

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
let phrasesJson = PsychoPhrases.Load("../data/phrases.json")
// -------------------------------------------------
// TODO: Load the phrases from the file and parse their contents.
// When finished, the phrases variable should contain a list
// of Phrases. 
// In each phrase, the InputPattern should contain the clean and parsed
// text of the phrase, without punctuation and in lower case (see Part 1).
// The Answer patterns can keep the punctuation etc.
// -------------------------------------------------

let clean text = 
    { Contents = text |> cleanText |> parseText 
      IsQuestion = isQuestion text }

  
    

let phrases : Phrases list =
    let input = PsychoPhrases.Load("../data/phrases.json")
    input
        |> Seq.map(fun item -> { InputPattern = clean item.Pattern; AnswerPatterns = item.Answers |> Array.map parseText })
        |> Seq.toList

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

// 1. Create Json type provider to extract information from the 
// "reflections.json" file
// and use them to create a dictionary of translations

type Reflections = JsonProvider<"[{\"input\":\"Something\",\"output\":\"Something else.\"}]">


let getReflections = 
    Reflections.Load("../data/reflections.json")
        |> Array.map(fun i -> (i.Input, i.Output))
        |> Map.ofArray 

// 2. Reflect all the words in the text that can be reflected
// based on the dictionary
let reflect (text : string list) =

    let reflectWord word = 
        match getReflections.TryFind word with
        | Some reflection -> reflection
        | _ -> word

    text |> List.map reflectWord
