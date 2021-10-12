# Cyclops

The Cyclops library can be used to apply a workflow of data analysis
steps to data stored in a SQLite database. Workflows are defined in
the master Cyclops Operations SQLite database (`Cyclops_Operations.db3`), 
with a separate table for each workflow.  Rows in each Workflow table 
track the analysis steps that the workflow will apply.

## Workflow Table

Columns in each workflow table are Step, Module, ModuleType, Parameter, 
and Value. Each workflow step will typically have multiple rows, with 
one row for each parameter related to the workflow step.  The following 
is an excerpt from the LabelFreeMainOperation workflow
(tracked via table `T_LabelFreeLog2PipelineOperation`)

| Step        | Module      | ModuleType  | Parameter | Value |
| ------------|-------------|-------------|-----------|-------|
| 1 | 1 | LoadRSourceFiles | 1 | removeFirstCharacters | true |
| 2 | 2 | Import | 1 | source | sqlite |
| 3 | 2 | Import | 1 | target | R |
| 4 | 2 | Import | 1 | importDatasetType | data.frame |
| 5 | 2 | Import | 1 | tableType | columnMetaDataTable |
| 6 | 2 | Import | 1 | inputTableName | `t_factors` |
| 7 | 2 | Import | 1 | newTableName | `T_Column_Metadata` |
| 8 | 3 | ExportTable | 2 | source | R |
| 9 | 3 | ExportTable | 2 | target | csv |
| 10 | 3 | ExportTable | 2 | tableName | `T_Column_Metadata` |
| 11 | 3 | ExportTable | 2 | fileName | `T_Column_Metadata.csv` |
| 12 | 3 | ExportTable | 2 | separatingCharacter | , |
| 13 | 3 | ExportTable | 2 | includeRowNames | false |
| 36 | 7 | Transform | 1 | inputTableName | `T_Data` |
| 37 | 7 | Transform | 1 | newTableName | `Log_T_Data` |
| 38 | 7 | Transform | 1 | scale | 1 |
| 39 | 7 | Transform | 1 | add | 0 |
| 40 | 7 | Transform | 1 | logBase | 2 |
| 41 | 7 | Transform | 1 | set02na | true |
| 42 | 8 | SummarizeData | 1 | inputTableName | `T_Data` |
| 43 | 8 | SummarizeData | 1 | newTableName | `Summary_T_Data` |
| 44 | 9 | SummarizeData | 1 | inputTableName | `Log_T_Data` |
| 45 | 9 | SummarizeData | 1 | newTableName | `Summary_Log_T_Data` |
| 46 | 10 | Aggregate | 1 | inputTableName | `Log_T_Data` |
| 47 | 10 | Aggregate | 1 | newTableName | `Agg_Log_T_Data` |
| 48 | 10 | Aggregate | 1 | factorTable | `T_Column_Metadata` |
| 49 | 10 | Aggregate | 1 | margin | 2 |
| 50 | 10 | Aggregate | 1 | function | mean |
| 73 | 16 | BarPlot | 3 | tableName | `T_MAC_MassTagID_Summary` |
| 74 | 16 | BarPlot | 3 | dataColumns | `Mass_Tag_ID` |
| 75 | 16 | BarPlot | 3 | plotFileName | `LBF_Analysis_Summary.png` |
| 76 | 16 | BarPlot | 3 | backgroundColor | white |
| 77 | 16 | BarPlot | 3 | log | TRUE |
| 205 | 40 | Save | 2 |  |  |

## Initializing a Workflow

The CyclopsTest projects demonstrates how to initialize a workflow.
It loads the workflow name from file `ITQ_ExportOperation.xml`.
That file specifies workflow `iTRAQMainOperation`, which is
defined in table `T_iTRAQ_PipelineOperation`.  This workflow
exports several tables from a SQLite database, saving them as
tab-delimited text files.

## Contacts

Written by Joseph Brown for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov \
Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics

## License

The Cyclops library is licensed under the 2-Clause BSD License; 
you may not use this file except in compliance with the License.  You may obtain 
a copy of the License at https://opensource.org/licenses/BSD-2-Clause

Copyright 2018 Battelle Memorial Institute
