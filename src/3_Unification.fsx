module Part3

#if INTERACTIVE
#load "1_ProcessInput.fsx" "2_LoadData.fsx"
#endif

open Part1

//==================================================
// Eliza, the psychotherapist bot
// Part 3: Unification
//==================================================
//
// In this part, we will implement how Eliza matches
// user input with the predefined patterns that we loaded
// in Part 2.
// In computer science terms, we will write simplified 
// unification that tries to match two expression and 
// find suitable substitutions. In our case:
//    "* enjoyed *"
// will unify with
//    "I really enjoyed the F# tutorial."
// where the substitution matches the first wildcard
// with "I really", and the second with "the F# tutorial."

// -------------------------------------------------
// DEMO: comparing two lists with recursive functions
// The following function compares two lists and returns
// the elements where the second list differs from the first.
// The accumulator variable "acc" gathers the result
// -------------------------------------------------
let rec demoCompare l1 l2 acc = 
    match l1, l2 with
    | x::xs, y::ys when x = y -> 
        // x and y match, continue
        demoCompare xs ys acc
    | x::xs, y::ys -> 
        // x and y don't match, add y to the accumulator and continue
        demoCompare xs ys (y::acc)
    | [],[] -> 
        // return the accumulated results
        acc |> List.rev  |> Some
    | _ -> // fail if the lists don't have the same length
        None
let list1 = [1;2;3;4;5]
let list2 = [1;2;5;4;7]
// 5 and 7 in the second list don't match the first list
demoCompare list1 list2 [] 


// -------------------------------------------------
// TODO: Write unification function that returns either
// None, if no match is found, or
// Some(substitution) when a match is found
// Hints: 
//   - Both input and pattern are lists of elements, go
//     through them recursively. 
//   - If the first word in the pattern matches the first 
//     word in the input, continue with the rest of the elements
//     in the pattern and the input
//   - If there's a wildcard in the pattern, it can be substituted
//     either with nothing, with a word, or with multiple words. 
// -------------------------------------------------



let rec unification pattern input acc =
    System.Console.WriteLine(sprintf "%A %A %A" pattern input acc)
    match pattern, input with
    | [], [] -> Some acc
    | Wildcard :: xs, [] -> Some acc
    | [Wildcard], [] -> Some acc 
    | Wildcard :: xs, y :: ys -> 
        let result = unification xs input acc
        match result with 
            | Some r -> result 
            | None -> unification (Wildcard :: xs) ys (List.append acc [y])
    | [x], [y] when x = y -> Some acc
    | x :: xs, y :: ys when x = y -> 
                    unification xs ys acc  
    | _, _ -> None


let x = unification [Wildcard] [] [Word "b"] 
  // Pattern match on pattern and input:
  //  - if both are empty, we succeeded!
  //  - if pattern starts with wildcard return the result of one of the
  //    following two recursive calls (first one, if it succeeds; second otherwise)
  //     * recursively match rest of the pattern (without wildcard)
  //       with the original input (and don't assing more words to wildcard)
  //     * match all of the pattern (with wildcard)
  //       with the rest of the input (assign the first word to the wildcard)
  //  - if the pattern & input start with the same word, skip them
  //  - otherwise we fail (because the pattern doesn't match input)
   // CHANGE ME

// Test the unification function - all expressions should return true
(unification (parseText "a *") (parseText "a b") []) = Some [Word "b"]
(unification (parseText "a *") (parseText "b b") []) = None
(unification (parseText "a *") (parseText "a b c") []) = Some [Word "b"; Word "c"]
(unification (parseText "* a *") (parseText "a a b c") []) = Some [Word "a"; Word "b"; Word "c"]
(unification (parseText "* a") (parseText "a") []) = Some []

// -------------------------------------------------
// TODO: Write the function that will match user input with pattern.
// Because our unification function ignores punctuation, we need to 
// check for it manually here, before calling the unfication function.
// More specifically, if the pattern is a question, then the user 
// input must also be a question. If the pattern is a question and the
// user input is not a question, the two don't match.
// If the user input is a question, the pattern doesn't have to be a 
// question as well.
// After checking if the question condition is satisfied, call the 
// unification function and return the result. 
//
// The function should return None if the question condition is not
// satisfied, or if no unification could be found. Otherwise return
// Some with the identified substitution.
// -------------------------------------------------
let matchPattern (pattern : Sentence) (input : Sentence) = 
    match pattern, input with
    | p, i when p.IsQuestion && i.IsQuestion -> unification pattern.Contents input.Contents []
    | p, _ when p.IsQuestion -> None
    | p, i -> unification p.Contents i.Contents []



