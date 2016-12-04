#if INTERACTIVE
#r "../packages/Suave/lib/net40/Suave.dll"
#load "4_Eliza.fsx"
#else
module Eliza
#endif

open Suave
open System
open Suave.Filters
open Suave.Operators
open FSharp.Data

// The current directory - this is __SOURCE_DIRECTORY__ when running locally
// using F# interactive and the directory of the current assembly when
// running as compiled application in Azure (yeah, this is ugly, ignore it :-))
let asm, debug = 
  if System.Reflection.Assembly.GetExecutingAssembly().IsDynamic then __SOURCE_DIRECTORY__, true
  else IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), false
let root = IO.Path.GetFullPath(IO.Path.Combine(asm, "..", "web"))


// Parse Slack requests - when Slack calls us, it gives us various info as form data
// (from https://dusted.codes/creating-a-slack-bot-with-fsharp-and-suave-in-less-than-5-minutes)
type SlackRequest =
  { Token : string
    TeamId : string
    TeamDomain : string
    ChannelId : string
    ChannelName : string
    UserId : string
    UserName : string
    Command : string
    Text : string
    ResponseUrl : string }

let parseRequest (req:HttpRequest) =
  let get key =
    match req.formData key with
    | Choice1Of2 x -> x
    | _ -> ""
  { Token = get "token"
    TeamId = get "team_id"
    TeamDomain = get "team_domain"
    ChannelId = get "channel_id"
    ChannelName = get "channel_name"
    UserId = get "user_id"
    UserName = get "user_name"
    Command = get "command"
    Text = get "text"
    ResponseUrl = get "response_url" }

// We respond with JSON document that contains the answer text. The 'in_channel' type
// specifies that the message will be visible as ordinary message in the channel
let makeResponse =
  sprintf "{ \"response_type\": \"in_channel\", \"text\": \"%s\" }" 

open Part1
open Part2
open Part3
open Part4

let elizaHandler = request (fun req -> 
  let question = parseRequest req
  let answer = "You said '" + question.Text + "'. Are you sure?"
  // TODO: Let Eliza answer the query!
  // (use 'getAnswer' function as you did in '4_Eliza.fsx')
  Successful.OK(makeResponse answer) )

let app = 
  choose [
    POST 
      >=> path "/eliza" 
      >=> Writers.addHeader "Content-type" "application/json" 
      >=> elizaHandler
    path "/" >=> Files.browseFile root "index.html"
    Files.browse root ]
