# Thermo RAW File Reader – QQQ Chromatogram Extractor

This project extends Thermo Fisher's official [RawFileReader](https://github.com/thermofisherlsms/RawFileReader) to support **extraction of chromatograms from triple quadrupole (QQQ/SRM/MRM) data** stored in `.RAW` files

## Background

The original `GetChromatogramData()` API provided by Thermo's DLLs does **not support SRM/QQQ data**, as QQQ chromatograms are **not stored** directly in the `.RAW` file — instead, they must be **reconstructed** by aggregating centroided scan-level data

This project provides a practical example of how to go through all scans, extract the Q1 and Q3 values, fetch intensities and assemble SRM chromatograms programmatically.

This is (poorly) written in C#

## Project Structure

- `Program.cs`:
  - read a basic RAW file metadata
  - extract scan-by-scan MS2 (SRM) data
  - grouping data by transition (Q1 → Q3)
  - write reconstructed chromatograms inefficiently to `.csv` files
- `Chromatograms/`: output folder for export 
- `data/`: folder to add `.raw` files

## Requirements

- Windows OS (64-bit only)
- Visual Studio or .NET build tools
- Thermo Fisher’s `CommonCore` libraries (bundled in `Dependencies/`)
- Compatible with `.RAW` files from Thermo Altis instruments

## Run

1. clone this repo and open the solution in **Visual Studio (64-bit)**
1.5 you might have to add a reference in visual studio to the DLLs in the project
2. place a `.raw` file inside the `data/` folder
3. update `rawFilePath` in `Program.cs` until I figure out how to parse arguments
4. build and run the application
5. reconstructed SRM traces are saved to the `Chromatograms/` folder as CSVs

## Example Output
CVS file for now but to be parsed and reshaped into more meaningful format

