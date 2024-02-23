# Revit Plugins
Contains code for Revit plugins made to help with documentation and other purposes. 

![In software ribbon panel for the plugins](https://github.com/jonathanatger/RevitPlugins/assets/50679537/2217adbd-628f-45d0-b00a-3e3a43571929)

## About Revit 

Revit is an architecture and civil engineering software designed to model buildings and their data (contributing to the Building Information Modelling paradigm). It is particularly good at editing graphical documentation as well as spreadsheets and quantity takeoffs from a single model.

For more info : [Revit overview](https://www.autodesk.fr/products/revit/overview?term=1-YEAR&tab=subscription)

## Technologies used

The language used for the plugins is C# - it is the most well documented and supported available amongst the compatible languages. The plugins almost exclusively use system libraries and the Revit API - one exception is the WPF (Windows presentation format) used for the UI. It is flexible and has similarities with web development. All plugins are modeless (run in parallel to Revit instead of halting its execution while on) and therefore suscribe to `ExternalArgsHandler` when they need to interact with Revit in a transformative way. All `Transactions` are wrapped into such a handler. The UI are dynamically populated with the Revit model data. A new Ribbon Panel with all the graphical elements is created using `RibbonPanel`. Every plugin is compiled to a separate namespace and therefore .dll, for flexibility in the choice of the plugins.

## List of Plugins
### Rename and duplicate Views

### Create sheets from Views
![Revit plugin UI](https://github.com/jonathanatger/RevitPlugins/assets/50679537/da65fef6-ceec-404a-a538-b2356246cf8e)

### Pin elements
![Revit plugin UI](https://github.com/jonathanatger/RevitPlugins/assets/50679537/6cc7a93a-a647-43cb-90dc-0ba99ca9751e)

### Copy presentation placement
![Revit plugin UI](https://github.com/jonathanatger/RevitPlugins/assets/50679537/a1013d3e-020e-439f-b89f-bedba321adc4)

