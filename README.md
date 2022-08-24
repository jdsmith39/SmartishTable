# SmartishTable
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/SmartishTable.svg?style=flat-square)](https://www.nuget.org/packages/SmartishTable)

Blazor Table/Grid/ListView/Whatever you want.
Not dependent on any CSS framework.

## Install
- Add [SmartishTable Nuget](https://www.nuget.org/packages/SmartishTable/)
	- dotnet add package SmartishTable
- Add the following to your index.html
	- `<link href="_content/SmartishTable/smartish-table.css" rel="stylesheet" />`

## Dependencies
- None!

## Features
- Fully customizable UI
	- No UI framework dependency
- Paging
- Sorting
	- Support for IComparer (example included)
- Filtering
	- Boolean (Equals, Not Equals, Is True, Is False)
	- DateTimes (Equals, Not Equals, >, >=, <, <=)
	- Numbers (Equals, Not Equals, >, >=, <, <=)
	- Strings (Contains, StartsWith, EndsWith, Equals, Not Equals)
	- Custom (Create a component that implements IFilter<TItem> and off you go!)
		- example included!
- Included examples:
	- Simple example of cards with sorting.
	- Regular Table with lots of data and sorting/filtering and max number of sorts being 2
	- Parameter based auto refreshing
	- Custom Filter
	- Dynamic Columns using a dictionary.
		- Custom Comparer
	- Nested objects with sorting and filtering
	- Get/Set configuration settings (for now, this is page #, page size, and sorting info), so the settings can be stored in localstorage (or somewhere else) and reloaded on next visit.
		- also shows how to use the OnDataUpdated eventCallback which also sends back the current configuration settings.
		- Binding Max Number of Sorts so it can be changed on the fly