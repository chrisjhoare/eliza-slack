#if INTERACTIVE
#I "../packages"
#r "Suave/lib/net40/Suave.dll"
#r "FSharp.Data/lib/net40/FSharp.Data.dll"
#r "Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#else
module Eliza
#endif

open Suave
open System
open Suave.Filters
open Suave.Operators
open Newtonsoft.Json
open FSharp.Data

let asm, debug = 
  if System.Reflection.Assembly.GetExecutingAssembly().IsDynamic then __SOURCE_DIRECTORY__, true
  else IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), false
let root = IO.Path.GetFullPath(IO.Path.Combine(asm, "..", "web"))

// --------------------------------------------------------------------------------------
// Loading content
// --------------------------------------------------------------------------------------

type SlackEvent = JsonProvider<"""{
    "type": "url_verification" }""">

type SlackHandshake = JsonProvider<"""{
    "token": "Jhj5dZrVaK7ZwHHjRyZWjbDl",
    "challenge": "3eZbrw1aBm2rZgRNFdxV2595E9CY3gmdALWMmHkvFXO7tYXAYM8P",
    "type": "url_verification" }""">

let handleSlackEvent json =
  printfn "SLACK MESSAGE!\n%s" json
  match SlackEvent.Parse(json).Type with
  | "url_verification" ->
      Writers.addHeader "Content-type" "application/x-www-form-urlencoded"
      >=> Successful.OK(SlackHandshake.Parse(json).Challenge)
  | _ -> 
      RequestErrors.BAD_REQUEST "bad request"

let app = request (fun r ->
  printfn "-----------------------\n%A" r
  choose [
    POST >=> request (fun r ->
      let body = Text.UTF32Encoding.UTF8.GetString(r.rawForm)
      handleSlackEvent body)
    path "/" >=> Successful.OK "running"
    Files.browse root ] )
