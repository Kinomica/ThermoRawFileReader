using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

class Program
{
    static void Main(string[] args)
    {
        string rawFilePath = @"C:\\Users\\naz\\Documents\\GitHub\\ThermoRawFileReader\\data\\test.raw";
        Console.WriteLine("Running from: " + Environment.CurrentDirectory);

        using (IRawDataPlus rawFile = RawFileReaderAdapter.FileFactory(rawFilePath))
        {
            if (!rawFile.IsOpen)
            {
                Console.WriteLine("❌ Could not open RAW file.");
                return;
            }

            rawFile.SelectInstrument(Device.MS, 1);

            Console.WriteLine("RAW file opened successfully!");
            Console.WriteLine($"File Name: {rawFile.FileName}");

            var instrumentModel = rawFile.GetInstrumentData().Model;
            Console.WriteLine($"Instrument Model: {instrumentModel}");

            var runHeader = rawFile.RunHeaderEx;
            Console.WriteLine($"Start Time: {runHeader.StartTime}");
            Console.WriteLine($"End Time: {runHeader.EndTime}");
            Console.WriteLine($"Spectra Count: {runHeader.SpectraCount}");
        }
    }
}