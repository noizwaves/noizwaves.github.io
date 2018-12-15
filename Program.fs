﻿module Program

open Suave
open Suave.DotLiquid
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Json
open Suave.Utils
open System
open System
open System.Collections.Generic
open System.IO
open System.Text
open FSharp.Markdown

// Domain
let parseFromLongString (value : string) : DateTime option = Some <| System.DateTime.Parse value

type Slug =
    { year : int
      month : int
      day : int
      name : string }

let slugFromUrlParts (year : string) (month : string) (day : string) (name : string) : Slug option =
    let yearC = String.length year = 4
    let monthC = String.length month = 2
    let dayC = String.length day = 2
    match (yearC && monthC && dayC) with
    | true -> 
        Some { year = int year
               month = int month
               day = int day
               name = name }
    | false -> None

type BlogPost =
    { slug : Slug
      title : string
      createdAt : DateTime
      body : string }

type FetchPosts = unit -> BlogPost list

type FetchPost = Slug -> BlogPost option

// Disk serialisation
let private shallowYamlDecode (yml : string) : Map<String, String> =
    yml
    |> String.split '\n'
    |> List.map (fun s -> 
           let index = s.IndexOf(":")
           let key = s.Substring(0, index).Trim()
           let value = s.Substring(index + 1).Trim().Trim('\"')
           (key, value))
    |> Map.ofList

let private parsePostFile (raw : string) =
    let split = raw.Split("---")
    if (split.Length >= 2) then 
        let document = Array.get split (split.Length - 1) |> String.trim
        
        let frontmatter =
            Array.get split (split.Length - 2)
            |> String.trim
            |> shallowYamlDecode
        (frontmatter, document)
    else (Map.empty, raw)

let private fromRawString (filename : string) (raw : string) : BlogPost =
    let (frontMatter, body) = parsePostFile raw
    let title = Map.find "title" frontMatter
    let name = String.substring 11 filename
    
    let filenameCreatedAt =
        match String.split '-' filename with
        | year :: month :: day :: _ -> new DateTime(int year, int month, int day)
        | _ -> failwith "Unable to parse date"
    
    let createdAt : DateTime =
        Map.tryFind "date" frontMatter
        |> Option.bind parseFromLongString
        |> Option.defaultValue filenameCreatedAt
    
    let slug =
        { year = createdAt.Year
          month = createdAt.Month
          day = createdAt.Day
          name = name }
    
    { slug = slug
      title = title
      createdAt = createdAt
      body = body }

let private loadPostsFromFolder (folder : string) : BlogPost list =
    folder
    |> System.IO.Directory.GetFiles
    |> Array.toList
    |> List.map (fun path -> 
           let filename = System.IO.Path.GetFileNameWithoutExtension path
           path
           |> System.IO.File.ReadAllText
           |> fromRawString filename)

let private safeFind predicate list =
    try 
        list
        |> List.find predicate
        |> Some
    with :? System.Collections.Generic.KeyNotFoundException -> None

let private findPostInList (posts : BlogPost list) (slug : Slug) : BlogPost option =
    posts |> safeFind (fun p -> p.slug.Equals(slug))

// HTML, Markdown formatting
let rec private viewSpan (s : MarkdownSpan) =
    match s with
    | Literal(text) -> text
    | InlineCode(code) -> sprintf "<code class=\"highlighter-rouge\">%s</code>" code
    | Strong(span) -> sprintf "<strong>%s</strong>" (viewSpans span)
    | Emphasis _ -> failwith "Emphasis not translated yet"
    | AnchorLink _ -> failwith "AnchorLink not translated yet"
    | DirectLink(body, (link, _)) -> sprintf """<a href="%s">%s</a>""" link (viewSpans body)
    | IndirectLink _ -> failwith "IndirectLink not translated yet"
    | DirectImage(body, (link, _)) -> 
        let src = link |> String.replace "{{ site.url }}" ""
        sprintf "<img src=\"%s\" alt=\"%s\" />" src body
    | IndirectImage _ -> failwith "IndirectImage not translated yet"
    | HardLineBreak _ -> "<br>"
    | LatexInlineMath _ -> failwith "LatexInlineMath not translated yet"
    | LatexDisplayMath _ -> failwith "LatexDisplayMath not translated yet"
    | EmbedSpans _ -> failwith "EmbedSpans not translated yet"

and private viewSpans (spans : MarkdownSpans) =
    spans
    |> List.map viewSpan
    |> List.reduce (fun s1 s2 -> s1 + s2)

let private viewParagraph p =
    match p with
    | Heading(size, spans) -> sprintf "<h%i>%s</h%i>" size (viewSpans spans) size
    | Paragraph(spans) -> sprintf "<p>%s</p>" (viewSpans spans)
    | CodeBlock(code, _, _) -> 
        sprintf 
            """<div class="highlighter-rouge"><div class="highlight"><pre class="highlight"><code>%s</code></pre></div></div>""" 
            code
    | InlineBlock _ -> failwith "InlineBlock not translated yet"
    | ListBlock _ -> failwith "ListBlock not translated yet"
    | QuotedBlock _ -> failwith "QuotedBlock not translated yet"
    | Span _ -> failwith "Span not translated yet"
    | LatexBlock _ -> failwith "LatexBlock not translated yet"
    | HorizontalRule _ -> failwith "HorizontalRule not translated yet"
    | TableBlock _ -> failwith "TableBlock not translated yet"
    | EmbedParagraphs _ -> failwith "EmbedParagraphs not translated yet"

let private toHtmlString (document : MarkdownDocument) : string =
    document.Paragraphs
    |> List.map viewParagraph
    |> List.reduce (fun s1 s2 -> s1 + s2)

// HTML
type PostHtmlDto =
    { title : string
      createdAt : string
      bodyHtml : string }

type PostItemHtmlDto =
    { title : string
      createdAt : string
      link : string }

type PostsHtmlDto =
    { posts : PostItemHtmlDto list }

let private formatCreateDate (value : DateTime) : string = value.ToString("MMM d, yyyy")
let private derivePostUrl (post : BlogPost) : string =
    sprintf "/%04i/%02i/%02i/%s" post.slug.year post.slug.month post.slug.day post.slug.name

let private toPostHtmlDto (post : BlogPost) : PostHtmlDto =
    { title = post.title
      createdAt = post.createdAt |> formatCreateDate
      bodyHtml =
          post.body
          |> Markdown.Parse
          |> toHtmlString }

// Flows
let private handleBlogPost (fetch : FetchPost) (year, month, day, titleSlug) =
    request (fun r -> 
        let post =
            slugFromUrlParts year month day titleSlug
            |> Option.bind fetch
            |> Option.map toPostHtmlDto
        match post with
        | Some dto -> page "post.html.liquid" dto
        | None -> NOT_FOUND "404")

let private handleBlogPosts (fetch : FetchPosts) request =
    let posts =
        fetch()
        |> List.sortByDescending (fun p -> p.createdAt)
        |> List.map (fun post -> 
               { title = post.title
                 createdAt = post.createdAt |> formatCreateDate
                 link = post |> derivePostUrl })
    
    let model = { posts = posts }
    page "posts.html.liquid" model

[<EntryPoint>]
let main _ =
    let port =
        Environment.GetEnvironmentVariable "PORT"
        |> Parse.int32
        |> Choice.fold id (fun _ -> 8080)
    
    let local = Suave.Http.HttpBinding.createSimple HTTP "0.0.0.0" port
    
    let config =
        { defaultConfig with bindings = [ local ]
                             homeFolder = Some(Path.GetFullPath "./public") }
    setTemplatesDir "./templates"
    setCSharpNamingConvention()
    let posts = loadPostsFromFolder "_posts"
    let fetchPosts = fun () -> posts
    let fetchPost = findPostInList posts
    
    let app : WebPart =
        choose [ GET >=> path "/" >=> request (handleBlogPosts fetchPosts)
                 GET >=> pathScan "/%s/%s/%s/%s" (handleBlogPost fetchPost)
                 GET >=> Files.browseHome
                 RequestErrors.NOT_FOUND "404" ]
    startWebServer config app
    0
