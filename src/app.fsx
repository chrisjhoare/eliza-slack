#if INTERACTIVE
#I "../packages"
#r "Suave/lib/net40/Suave.dll"
#r "Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#else
module Eliza
#endif

open Suave
open System
open Suave.Filters
open Suave.Operators
open Newtonsoft.Json

let (</>) a b = IO.Path.Combine(a, b)

let asm, debug = 
  if System.Reflection.Assembly.GetExecutingAssembly().IsDynamic then __SOURCE_DIRECTORY__, true
  else IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), false
let root = IO.Path.GetFullPath(asm </> ".." </> "web")

// --------------------------------------------------------------------------------------
// Loading content
// --------------------------------------------------------------------------------------

let app = 
  choose [
    path "/" >=> Successful.OK "running"
    Files.browse root ]
