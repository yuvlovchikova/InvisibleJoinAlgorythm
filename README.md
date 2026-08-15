<p align="right">
  <b>English</b> · <a href="./README_RU.md">Русский</a>
</p>

# Invisible Join: Bitmap-Based Query Execution for a Star Schema

Academic database-systems project implementing a bitmap-based query execution workflow for a star schema without materializing conventional joins between dimension tables and the fact table.

The implementation parses filters, evaluates them on the relevant tables, propagates matching keys to the fact table through bitmap structures, intersects the resulting bitmaps, and returns only the requested fact-table columns.

## Tech

C# · .NET · bitmap indexes · custom bitmap/RoaringBitmap structures · star schema · OLAP/query processing

## How it works

The execution pipeline is organized around the following stages:

1. parse requested output columns and filter predicates;
2. evaluate each predicate on its source table and encode matching keys in a bitmap;
3. map dimension-table matches to positions in `FactResellerSales` through the corresponding foreign-key column;
4. combine all fact-table bitmaps with logical `AND`;
5. project the requested columns for rows that remain in the final bitmap.

The core filtering logic is implemented explicitly in C#, including custom bitmap data structures, rather than delegated to a database engine.

## Schema

The implementation works with an AdventureWorks-style star schema centered on `FactResellerSales` and dimensions including products, resellers, currencies, promotions, sales territories, employees, and dates.

## Repository

- `InvisibleJoinAlgorythm/Program.cs` — query parsing, bitmap construction, filter propagation, bitmap intersection, and output projection;
- `InvisibleJoinAlgorythm.sln` — Visual Studio solution.

## Project context

This is an archived academic implementation focused on data structures and query-execution mechanics. The schema and input layout are intentionally explicit in the source code, so the repository should be read as an algorithms/database-internals project rather than a general-purpose query engine.
