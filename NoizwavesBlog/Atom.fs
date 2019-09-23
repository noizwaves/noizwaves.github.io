module NoizwavesBlog.Atom

open System
open NoizwavesBlog.Domain
open NoizwavesBlog.Markdown
open Suave
open Suave.Operators

type AtomFeed = FSharp.Data.XmlProvider<"""<feed xmlns="http://www.w3.org/2005/Atom">
  <title>Adam Neumann's blog</title>
  <link href="https://blog.noizwaves.io/" />
  <link href="https://blog.noizwaves.io/feed.atom" rel="self" />
  <updated>SOME BLOG DATE</updated>
  <author>
    <name>Adam Neumann</name>
  </author>
  <id>https://blog.noizwaves.io/</id>
  <entry>
    <title>Foo Bar</title>
    <link href="https://blog.noizwaves.io/2019/09/23/foo-bar"/>
    <content type="html">Content</content>
    <id>https://blog.noizwaves.io/2019/09/23/foo-bar</id>
    <updated>DATE AS STRING</updated>
    <summary>Summary</summary>
  </entry>
  <entry>
    <title>Foo Bar 2</title>
    <link href="https://blog.noizwaves.io/2019/09/23/foo-bar-2"/>
    <content type="html">Content 2</content>
    <id>https://blog.noizwaves.io/2019/09/23/foo-bar-2</id>
    <updated>DIFFERENT DATE AS STRING</updated>
    <summary>Summary</summary>
  </entry>
</feed>""">

let private derivePostUrl (post : BlogPost) : string =
    sprintf "https://blog.noizwaves.io/%04i/%02i/%02i/%s" post.slug.year post.slug.month post.slug.day post.slug.name

let private formatForAtom (date : DateTimeOffset) : string =
    date.ToString "yyyy-MM-ddTHH:mm:ssZ"

let private postToEntry (post : BlogPost) : AtomFeed.Entry =
    let url = post |> derivePostUrl
    let date = post.createdAt |> formatForAtom
    let content = post.body |> convertToHtml
    let summary = post.body |> convertToText

    AtomFeed.Entry (post.title, AtomFeed.Link2 url, AtomFeed.Content ("html", content), url, date, summary)

let private entriesToFeed (entries : AtomFeed.Entry list) : AtomFeed.Feed =
    let updated = DateTimeOffset (2003, 12, 13, 18, 30, 2, TimeSpan.Zero) |> formatForAtom
    let pageLink = AtomFeed.Link ("https://blog.noizwaves.io/", None)
    let atomLink = AtomFeed.Link ("https://blog.noizwaves.io/feed.atom", Some "self")

    AtomFeed.Feed
        ("Adam Neumann's blog",
         [| pageLink; atomLink |],
         updated,
         AtomFeed.Author "Adam Neumann",
         "https://blog.noizwaves.io/",
         entries |> List.toArray)

let handleAtomFeed (fetch : FetchPosts) : WebPart =
    fetch ()
    |> List.map postToEntry
    |> entriesToFeed
    |> sprintf """<?xml version="1.0" encoding="utf-8"?>
%O
"""
    |> Successful.OK
    >=> Writers.setMimeType "application/xml+atom"