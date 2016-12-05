module Part4

#if INTERACTIVE
#load "3_Unification.fsx"
#endif

open Part1
open Part2
open Part3

//==================================================
// Eliza, the psychotherapist bot
// Part 4: Putting all the parts together
//==================================================

// Helper function to choose a random answer from an array
// of possible answers
let rnd = System.Random()
let chooseAnswer (xs: Answer []) = xs.[rnd.Next(xs.Length)]


// Helper function that takes an Answer (Pattern list),
// and extracts just the text in the answer. Returns a list
// of strings.
let extractText (pattern : Answer) = 
    pattern 
    |> List.map (fun p ->
        match p with
        | Word w -> w
        | _ -> "")
    |> List.filter (fun x -> x <> "")

// Function fillAnswer takes an Answer (Pattern list) that contains 
// one Wildcard, and a text in the form of a string.
// It replaces the wildcard with the string and merges
// all the parts into a single string, where the words 
// are separated by whitespace. 
let fillAnswer (text : string) (pattern : Answer) =
    pattern 
    |> List.fold (fun result p ->
        match p with
        | Wildcard -> result + " " + text
        | Word w -> result + " " + w ) ""

// -------------------------------------------------
// TODO: Function that takes an input as a Sentence, 
// and possible phrases, and returns Eliza's answer. 
// 1. It goes through the the phrases and
//    finds the first one that matches the input.
//    [ use the matchPattern function ]
// 2. If a match is found, it extracts the identified substitution,
//    and reflects its text.
//    [ use extractText and reflect functions, then concatenate all
//     results into a single string ]
// 3. Choose an answer randomly from the possible answers for the
//    matching pattern. Fill its wildcard with the identified user
//    text from step 2.
//    [ use the chooseAnswer and fillAnswer functions ]
// 4. Return the answer!
// -------------------------------------------------

let rec getAnswer (phrases : Phrases list) (input: Sentence) =
    let patterns = 
        phrases |> Seq.map(fun p -> (p, matchPattern p.InputPattern input))
                |> Seq.choose(fun (p, r) -> match r with Some r' -> Some (p,r') | _ -> None)
                |> Seq.take 1
                |> Seq.tryHead
    match patterns with
            | Some (x:Phrases, p) -> 
                        let reflected = p |> (extractText >> reflect) 
                        let replacement = System.String.Join(" ", reflected)
                        x.AnswerPatterns
                            |> chooseAnswer
                            |> fillAnswer replacement

            | None -> ""







// -------------------------------------------------
// TODO: Talk to Eliza! 
// -------------------------------------------------

let getInput text = { Contents = text |> cleanText |> parseText; IsQuestion = false }

"I'm an F# fan" |> getInput |> getAnswer phrases 
