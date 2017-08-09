#load @".paket/load/net45/canopy.fsx"
#load @".paket/load/net45/FSharp.Data.fsx"

open FSharp.Data
open canopy
open runner
open OpenQA.Selenium
open System.Text.RegularExpressions

let baseUrl = "http://voices.revealdigital.com"
let indexUrl = "/cgi-bin/independentvoices?a=cl&cl=CL1&sp=IBJBJF&ai=1&e=-------en-20--1--txt-txIN---------------1"
let topLevel = HtmlDocument.Load(baseUrl + indexUrl)
let issues = 
    topLevel.CssSelect "li > a" 
    |> List.map (fun a -> 
        (baseUrl + a.AttributeValue("href"), a.InnerText().Trim()))

let listPages () =
    let pageLinkRegex = Regex("independentvoices")
    let pageUrls =
        elements "td a"
        |> List.map (fun e -> e.GetAttribute "href")
        |> List.distinct
        |> List.filter (fun e -> (pageLinkRegex.IsMatch e))
    pageUrls

// choco install selenium-all-drivers
chromeDir <- "C:\\tools\\selenium"
start chrome

url (fst issues.[1])
elements "td a";;
