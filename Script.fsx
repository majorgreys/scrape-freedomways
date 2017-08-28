#load @".paket/load/net45/canopy.fsx"
#load @".paket/load/net45/FSharp.Data.fsx"

open FSharp.Data
open canopy
open OpenQA.Selenium
open System.Text.RegularExpressions
open FSharp.Data.HttpContentTypes

// `choco install selenium-all-drivers`
chromeDir <- "C:\\tools\\selenium"

let baseUrl = "http://voices.revealdigital.com"
let indexUrl = "/cgi-bin/independentvoices?a=cl&cl=CL1&sp=IBJBJF&ai=1&e=-------en-20--1--txt-txIN---------------1"
let topLevel = HtmlDocument.Load(baseUrl + indexUrl)
let issues = 
    topLevel.CssSelect "li > a" 
    |> List.map (fun a -> 
        (baseUrl + a.AttributeValue("href"), a.InnerText().Trim()))

let (|FirstRegexGroup|_|) pattern input =
   let m = Regex.Match(input,pattern) 
   if (m.Success) then Some m.Groups.[1].Value else None 
let listPagePdfUrls issueUrl =
    let parsePageUrlForPageId pageUrl =
        match pageUrl with
        | FirstRegexGroup ".*&d=(.*)&" pageId ->
            Some pageId
        | _ -> None
    let issueId = parsePageUrlForPageId issueUrl
    // browse to url
    url issueUrl
    // give some time for page to load
    sleep 5
    // list pdf urls by examining each issue page
    elements "td a"
        |> List.map (fun e -> e.GetAttribute "href")
        |> List.distinct
        |> List.map parsePageUrlForPageId
        |> List.choose id // remove None
        |> List.filter (fun e -> e.Equals(issueId.Value) |> not) // remove link to issue index page
        // urls to static pdfs are easy to construct once we have the page ids
        |> List.map (fun e -> 
            (e, sprintf "http://voices.revealdigital.com/cgi-bin/independentvoices?a=is&oid=%s&type=staticpdf" e))

let listPdfUrls () =
    let pdfUrls = 
        issues 
        |> List.map (fun e -> listPagePdfUrls (fst e))
        |> List.collect id
    pdfUrls

let pdfUrls = listPdfUrls()
System.IO.File.WriteAllLines("pdfurls.txt", pdfUrls |> List.map snd)
let R = System.Random()
start chrome
System.IO.File.ReadAllLines("pdfurls.txt")
    |> Array.toList
    |> List.filter (fun e -> e.Contains("1963"))
    |> List.iter (fun e ->
        printfn "Downloading %s" e 
        url e 
        // Wait before proceeding to next file so as not to overwhelm the server with repeated requests
        sleep (R.Next(5, 15))
        )
quit ()