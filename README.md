# TemplateSharp
A simple offline template renderer written in C# using AngleSharp library.

## 1. Include a page
```html
<include>path_to_file.html</include> <!-- attributes are optional and passed to the included file -->
```
## 2. Use another page as a parent
child.html:
```html
<parent>parent.html</parent> <!-- attributes are optional and passed to the parent file -->
```
parent.html:
```html
<content><!-- here will be the child page's content --></content>
```
## 3. Pass attributes
child.html:
```html
<parent var="12" var2="20">parent.html</parent>
```
parent.html:
```html
<content></content>
<span><attr>var</attr></span> <!-- will be rendered as <span>12</span> -->
<span><attr>var2</attr></span> <!-- will be rendered as <span>20</span> -->
```
*Also, inline style may be used:*
```html
<span attr>@@var</span>
<span attr>@@var2</span>
```

## 4. Require tag
```html
<!-- The following tag and its content will be removed if no "var" attribute is passed -->
<require name="var">
    <p>Lorem ipsum</p>
</require>
<!-- Also, value checking is possible -->
<require name="var" value="empty">
    <p attr>var equals @@var</p>
</require>
```
Note: don't place require tags in the head section. Use **headdata** tag instead.

## 5. Headdata tag
**```<headdata>``` is placed inside HTML body. Its content is always moved to the head section of the document.**

## Parameters

| Name  | Description |
| ------------- | ------------- |
| -path [string] | Working directory  |
| -output [string] | Output directory  |
| -minify  | Removes new lines and tabulation from the output files  |