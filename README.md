# SmartishTable
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/SmartishTable.svg?style=flat-square)](https://www.nuget.org/packages/SmartishTable)
[![Nuget (with prereleases)](https://img.shields.io/nuget/dt/SmartishTable.svg?style=flat-square)](https://www.nuget.org/packages/SmartishTable)

Blazor Table/Grid/ListView/Whatever you want.
Not dependent on any CSS framework.

## Install
- Add [SmartishTable Nuget](https://www.nuget.org/packages/SmartishTable/)
	- dotnet add package SmartishTable
- Add following to your index.html
	- `<link href="_content/SmartishTable/smartish-table.css" rel="stylesheet" />`

## Dependencies
- None!

## Features
- Fully customizable UI
	- No UI framework dependency
- Paging
- Sorting
- Filtering
	- Boolean (Equals, Not Equals, IsTrue, Is False)
	- DateTimes (Equals, Not Equals, >, >=, <, <=)
	- Numbers (Equals, Not Equals, >, >=, <, <=)
	- Strings (Contains, StartsWith, EndsWith, Equals, Not Equals)
	- Custom (Create a component that implements IFilter<TItem> and off you go!)
