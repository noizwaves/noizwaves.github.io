module MarkdownTests

open Xunit
open NoizwavesBlog.OwnMarkdown


let private debug =
    if not (System.Diagnostics.Debugger.IsAttached) then
      printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
    while not (System.Diagnostics.Debugger.IsAttached) do
      System.Threading.Thread.Sleep(100)

[<Fact>]
let ``Single line paragraph``() =
    let expected: Markdown = [ Paragraph [ Span "Foo" ] ]
    let actual: Markdown = ParseOwn """Foo""" |> Option.get
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Single line paragraph with newline``() =
    let expected: Markdown = [ Paragraph [ Span "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """Foo
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Multiple single line paragraphs``() =
    let expected: Markdown = [ Paragraph [ Span "Foo" ]; Paragraph [ Span "Bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """Foo

Bar"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Multiple line paragraph``() =
    let expected: Markdown =
        [ Paragraph [ Span "Foo"; Span "Bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """Foo
Bar"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Multiple paragraphs``() =
    let expected: Markdown =
        [ Paragraph [ Span "Foo" ]
        ; Paragraph [ Span "Bar"; Span "Baz" ]
        ]
    let actual: Markdown = Option.get <| ParseOwn """Foo

Bar
Baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Emphasized text in a paragraph``() =
    let expected: Markdown =
        [ Paragraph [ Emphasized "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """_Foo_"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Bold text in a paragraph``() =
    let expected: Markdown =
        [ Paragraph [ Bolded "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """**Foo**"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Alternatively bolded text in a paragraph``() =
    let expected: Markdown =
        [ Paragraph [ Bolded "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """__Foo__"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Mix of regular, bolded, and emphasized text in a paragraph``() =
    let expected: Markdown =
        [ Paragraph
            [ Emphasized "Foo"
            ; Span " Bar"
            ; Span "Baz "
            ; Emphasized "Qux"
            ; Bolded "Quux"
            ; Span " Quuz "
            ; Emphasized "Corge"
            ; Span " "
            ; Bolded "Grault"
            ]
        ]
    let actual: Markdown = Option.get <| ParseOwn """_Foo_ Bar
Baz _Qux_
**Quux** Quuz _Corge_ __Grault__"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline link in a paragraph``() =
    let expected: Markdown = [ Paragraph [ InlineLink("www.example.com", "foobar") ] ]
    let actual: Markdown = Option.get <| ParseOwn """[foobar](www.example.com)"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span using single backticks``() =
    let expected: Markdown = [ Paragraph [ Code "foobar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """`foobar`"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span using double backticks``() =
    let expected: Markdown = [ Paragraph [ Code "baz" ] ]
    let actual: Markdown = Option.get <| ParseOwn """``baz``"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span containing a literal backtick``() =
    let expected: Markdown = [ Paragraph [ Code "b`az" ] ]
    let actual: Markdown = Option.get <| ParseOwn """``b`az``"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span containing two literal backticks``() =
    let expected: Markdown = [ Paragraph [ Code "b`a`z" ] ]
    let actual: Markdown = Option.get <| ParseOwn """``b`a`z``"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span containing multiple literal backticks``() =
    let expected: Markdown = [ Paragraph [ Code "b`a`z` `q`u`x" ] ]
    let actual: Markdown = Option.get <| ParseOwn """``b`a`z` `q`u`x``"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span containing multiple token characters``() =
    let expected: Markdown = [ Paragraph [ Code "([_*#])# > " ] ]
    let actual: Markdown = Option.get <| ParseOwn """`([_*#])# > `"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Inline code span containing a syntactically valid link``() =
    let expected: Markdown = [ Paragraph [ Code "[not a link](www.example.com)" ] ]
    let actual: Markdown = Option.get <| ParseOwn """`[not a link](www.example.com)`"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Double backticked inline code span containing multiple token characters``() =
    let expected: Markdown = [ Paragraph [ Code "([_*#])# > " ] ]
    let actual: Markdown = Option.get <| ParseOwn """``([_*#])# > ``"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Double backticked inline code span containing multiple token characters and escaped backticks``() =
    let expected: Markdown = [ Paragraph [ Code "([_`*`#])# > " ] ]
    let actual: Markdown = Option.get <| ParseOwn """``([_`*`#])# > ``"""
    Assert.Equal<Markdown>(expected, actual)

// Heading 1

[<Fact>]
let ``Heading 1``() =
    let expected: Markdown = [ Heading1 [ Span "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """# Foo"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 1 immediately followed by paragraph``() =
    let expected: Markdown = [ Heading1 [ Span "foo" ]; Paragraph [ Span "bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """# foo
bar
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 1 followed by paragraph``() =
    let expected: Markdown = [ Heading1 [ Span "foo" ]; Paragraph [ Span "bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """# foo

bar
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Regular paragraph containing heading 1-like characters``() =
    let expected: Markdown = [ Paragraph [ Span "foo # Bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """foo # Bar"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Regular paragraph immediately followed by heading 1``() =
    let expected: Markdown = [ Paragraph [ Span "foo" ]; Heading1 [ Span "baz" ] ]
    let actual: Markdown = Option.get <| ParseOwn """foo
# baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Regular paragraph with subsequent sentence containing heading 1-like characters``() =
    let expected: Markdown = [ Paragraph [ Span "foo"; Span "bar # baz" ] ]
    let actual: Markdown = Option.get <| ParseOwn """foo
bar # baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 1 that includes the hash space string``() =
    let expected: Markdown = [ Heading1 [ Span "foo # bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn "# foo # bar"
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 1 that includes sentence parts``() =
    let expected: Markdown = [ Heading1 [ Emphasized "foo"; Span " "; Bolded "bar"; Span " baz # h1 "; Code "Qux"; InlineLink("quuz", "quux") ] ]
    let actual: Markdown = Option.get <| ParseOwn "# _foo_ **bar** baz # h1 `Qux`[quux](quuz)"
    Assert.Equal<Markdown>(expected, actual)

// Heading2

[<Fact>]
let ``Heading 2``() =
    let expected: Markdown = [ Heading2 [ Span "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """## Foo"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 2 immediately followed by paragraph``() =
    let expected: Markdown = [ Heading2 [ Span "foo" ]; Paragraph [ Span "bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """## foo
bar
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 2 followed by paragraph``() =
    let expected: Markdown = [ Heading2 [ Span "foo" ]; Paragraph [ Span "bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """## foo

bar
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Regular paragraph with subsequent sentence containing heading 2-like characters``() =
    let expected: Markdown = [ Paragraph [ Span "foo"; Span "bar ## baz" ] ]
    let actual: Markdown = Option.get <| ParseOwn """foo
bar ## baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 2 that includes the hash space string``() =
    let expected: Markdown = [ Heading2 [ Span "foo ## bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn "## foo ## bar"
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 2 that includes sentence parts``() =
    let expected: Markdown = [ Heading2 [ Emphasized "foo"; Span " "; Bolded "bar"; Span " baz ## h2 "; Code "Qux"; InlineLink("quuz", "quux") ] ]
    let actual: Markdown = Option.get <| ParseOwn "## _foo_ **bar** baz ## h2 `Qux`[quux](quuz)"
    Assert.Equal<Markdown>(expected, actual)

// Heading3

[<Fact>]
let ``Heading 3``() =
    let expected: Markdown = [ Heading3 [ Span "Foo" ] ]
    let actual: Markdown = Option.get <| ParseOwn """### Foo"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 3 immediately followed by paragraph``() =
    let expected: Markdown = [ Heading3 [ Span "foo" ]; Paragraph [ Span "bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """### foo
bar
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 3 followed by paragraph``() =
    let expected: Markdown = [ Heading3 [ Span "foo" ]; Paragraph [ Span "bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn """### foo

bar
"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Regular paragraph with subsequent sentence containing heading 3-like characters``() =
    let expected: Markdown = [ Paragraph [ Span "foo"; Span "bar ### baz" ] ]
    let actual: Markdown = Option.get <| ParseOwn """foo
bar ### baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 3 that includes the hash space string``() =
    let expected: Markdown = [ Heading3 [ Span "foo ### bar" ] ]
    let actual: Markdown = Option.get <| ParseOwn "### foo ### bar"
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Heading 3 that includes sentence parts``() =
    let expected: Markdown = [ Heading3 [ Emphasized "foo"; Span " "; Bolded "bar"; Span " baz ### h3 "; Code "Qux"; InlineLink("quuz", "quux") ] ]
    let actual: Markdown = Option.get <| ParseOwn "### _foo_ **bar** baz ### h3 `Qux`[quux](quuz)"
    Assert.Equal<Markdown>(expected, actual)

// CodeBlock

[<Fact>]
let ``Single line block of code``() =
    let expected: Markdown = [ CodeBlock ("foo", None) ]
    let actual: Markdown = Option.get <| ParseOwn """```
foo
```"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Multiline block of code``() =
    let expected: Markdown = [ CodeBlock ("foo\nbar\nbaz", None) ]
    let actual: Markdown = Option.get <| ParseOwn """```
foo
bar
baz
```"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Block of code with trailing new lines``() =
    let expected: Markdown = [ CodeBlock ("foo", None) ]
    let actual: Markdown = Option.get <| ParseOwn """```
foo
```

"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Block of code containing token characters``() =
    let expected: Markdown = [ CodeBlock ("([_*])# > #", None) ]
    let actual: Markdown = Option.get <| ParseOwn """```
([_*])# > #
```"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Block of code with language hint``() =
    let expected: Markdown = [ CodeBlock ("foo", Some "javascript") ]
    let actual: Markdown = Option.get <| ParseOwn """```javascript
foo
```"""
    Assert.Equal<Markdown>(expected, actual)

// QuoteBlock

[<Fact>]
let ``Single line quote block``() =
    let expected: Markdown = [ QuoteBlock "Foo" ]
    let actual: Markdown = Option.get <| ParseOwn """> Foo"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Multiline quote block``() =
    let expected: Markdown = [ QuoteBlock "Foo Bar Baz" ]
    let actual: Markdown = Option.get <| ParseOwn """> Foo
> Bar
> Baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Quote block with trailing new lines``() =
    let expected: Markdown = [ QuoteBlock "Foo Bar" ]
    let actual: Markdown = Option.get <| ParseOwn """> Foo
> Bar

"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Quote block containing token characters``() =
    let expected: Markdown = [ QuoteBlock "([_*])`# > #" ]
    let actual: Markdown = Option.get <| ParseOwn "> ([_*])`# > #"
    Assert.Equal<Markdown>(expected, actual)

// Ordered list

[<Fact>]
let ``Ordered list with one item``() =
    let expected: Markdown = [ OrderedList [ ListItem [ Span "foo" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn "1.  foo"
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Ordered list with many items``() =
    let expected: Markdown = [ OrderedList [ ListItem [ Span "foo" ]; ListItem [ Span "bar" ]; ListItem [ Span "baz" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn """1.  foo
1.  bar
1.  baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Ordered list with trailing new lines``() =
    let expected: Markdown = [ OrderedList [ ListItem [ Span "foo" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn """1.  foo

"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Ordered list with formatted text``() =
    let expected: Markdown = [ OrderedList [ ListItem [ Span "foo "; Emphasized "bar"; Bolded "baz"; Code "quz" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn "1.  foo _bar_**baz**`quz`"
    Assert.Equal<Markdown>(expected, actual)

// Unordered list

[<Fact>]
let ``Unordered list with one item``() =
    let expected: Markdown = [ UnorderedList [ ListItem [ Span "foo" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn "-   foo"
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Unordered list with many items``() =
    let expected: Markdown = [ UnorderedList [ ListItem [ Span "foo" ]; ListItem [ Span "bar" ]; ListItem [ Span "baz" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn """-   foo
-   bar
-   baz"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Unordered list with trailing new lines``() =
    let expected: Markdown = [ UnorderedList [ ListItem [ Span "foo" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn """-   foo

"""
    Assert.Equal<Markdown>(expected, actual)

[<Fact>]
let ``Unordered list with formatted text``() =
    let expected: Markdown = [ UnorderedList [ ListItem [ Span "foo "; Emphasized "bar"; Bolded "baz"; Code "quz" ] ] ]
    let actual: Markdown = Option.get <| ParseOwn "-   foo _bar_**baz**`quz`"
    Assert.Equal<Markdown>(expected, actual)

//[<Fact>]
//let ``A heading 1 whose contents starts with a hash``() =
//    let expected: Markdown = [ Heading1 [ Span "#foo" ] ]
//    let actual: Markdown = Option.get <| ParseOwn "# #foo"
//    Assert.Equal<Markdown>(expected, actual)
//
//[<Fact>]
//let ``A heading 2 whose contents starts with a hash``() =
//    let expected: Markdown = [ Heading1 [ Span "#foo" ] ]
//    let actual: Markdown = Option.get <| ParseOwn "## #foo"
//    Assert.Equal<Markdown>(expected, actual)
//
//[<Fact>]
//let ``An element whose inner text that starts with a Heading 1``() =
//    let expected: Markdown = [ Paragraph [ Bolded " # foo" ] ]
//    let actual: Markdown = Option.get <| ParseOwn "** # foo**"
//    Assert.Equal<Markdown>(expected, actual)