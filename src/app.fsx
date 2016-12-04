#if INTERACTIVE
#r "../packages/Suave/lib/net40/Suave.dll"
#else
module Eliza
#endif

open Suave
open System
open Suave.Filters
open Suave.Operators
open FSharp.Data

let asm, debug = 
  if System.Reflection.Assembly.GetExecutingAssembly().IsDynamic then __SOURCE_DIRECTORY__, true
  else IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), false
let root = IO.Path.GetFullPath(IO.Path.Combine(asm, "..", "web"))

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

let makeResponse =
  sprintf "{ \"response_type\": \"in_channel\", \"text\": \"%s\" }" 

let elizaHandler = request (fun req -> 
  let question = parseRequest req
  let answer = "Did you really say " + question.Text + "?"
  Successful.OK(makeResponse answer) )

let app = 
  choose [
    POST 
      >=> path "/eliza" 
      >=> Writers.addHeader "Content-type" "application/json" 
      >=> elizaHandler
    path "/" >=> Files.browseFile root "index.html"
    Files.browse root ]
