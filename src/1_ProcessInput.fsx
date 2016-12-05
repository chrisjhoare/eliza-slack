module Part1
//==================================================
// Eliza, the psychotherapist bot
// Part 1: Parsing input text
//==================================================
//
// Eliza works by mathing user input with predefined patterns.
// In this part, we will parse both the inputs written by
// the user, and the patterns.
// Each pattern contains words and wildcards, for example:
// 
//   Hello *
//
// The wildcards serve as placeholders for any other text. 
// In this part we will write a function to parse text
// into a list of words and wildcards.

// Pattern contains words and wildcards
type Pattern =
    | Word of string
    | Wildcard

// Each sentence contains the pattern, and 
// additional information to distinguish questions from
// other sentences
type Sentence = {
    Contents : Pattern list
    IsQuestion : bool
}

// helper function to standardize user input and remove punctuation
let cleanText (input : string) =
    input.ToLower()
        .Replace("."," ").Replace("?"," ").Replace("!"," ").Replace(","," ")

// -------------------------------------------------
// TODO: write a function that splits text into individual words (or wildcards)
// and returns Pattern list 
// -------------------------------------------------

let mapString s = 
    match s with
        | "*" -> Wildcard
        | s -> Word s


let parseText (input : string) : Pattern list = 
    input.Split(' ')
        |> Seq.filter(fun s -> s <> "")
        |> Seq.map mapString
        |> Seq.toList
    
    



// -------------------------------------------------
// TODO: write a function that checks if the input represents a question
// -------------------------------------------------
let isQuestion (input : string) = 
    input.EndsWith("?")



// Test your implementation:
let text = "Hello, is * there?"
let sentence = 
    { Contents = text |> cleanText |> parseText 
      IsQuestion = isQuestion text }
