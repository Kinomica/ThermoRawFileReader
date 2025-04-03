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

public class TransitionTrace
{
    public double Q1 { get; set; }
    public double Q3 { get; set; }
    public List<double> Times { get; set; } = new List<double>();
    public List<double> Intensities { get; set; } = new List<double>();
}


class Program
{
    // need to reconstruct the chromatogram by aggregating scan-by-scan info
    // 1. extract the scan’s Q1 & Q3
    // 2. get the scan’s retention time
    // 3. get the scan’s intensity
    // 4. group scans with the same Q1 to Q3 transition and aggregate them into a time series
    public static Dictionary<string, TransitionTrace> ReconstructSRMChromatograms(IRawDataPlus rawFile)
    {
        var traces = new Dictionary<string, TransitionTrace>();

        int firstScan = rawFile.RunHeaderEx.FirstSpectrum;
        int lastScan = rawFile.RunHeaderEx.LastSpectrum;

        for (int scanNumber = firstScan; scanNumber <= lastScan; scanNumber++)
        {
            var filter = rawFile.GetFilterForScanNumber(scanNumber);
            if (filter.MSOrder != MSOrderType.Ms2)
                continue;

            double q1 = filter.GetMass(0);
            double rt = rawFile.RetentionTimeFromScanNumber(scanNumber);
            var stats = rawFile.GetScanStatsForScanNumber(scanNumber);
            var cs = rawFile.GetSegmentedScanFromScanNumber(scanNumber, stats);

            if (cs == null || cs.Positions.Length == 0 || cs.Intensities.Length == 0)
            {
                Console.WriteLine($"Skipping scan {scanNumber} - no centroid data.");
                continue;
            }

            for (int i = 0; i < cs.Positions.Length; i++)
            {
                double q3 = cs.Positions[i];          // Fragment ion m/z
                double intensity = cs.Intensities[i]; // Corresponding intensity
                string key = $"{q1:0.0000}_{q3:0.0000}";

                if (!traces.ContainsKey(key))
                    traces[key] = new TransitionTrace { Q1 = q1, Q3 = q3 };

                traces[key].Times.Add(rt);
                traces[key].Intensities.Add(intensity);
            }
        }

        return traces;
    }



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


            // This approach did not work - it turns out QqQ SRM chromatograms are not directly available via GetChromatogramData(),
            // because the RAW file doesn't store them as precomputed chromatograms

            //var transitions = new HashSet<(double Q1, double Q3)>();

            //for (int scanNumber = runHeader.FirstSpectrum; scanNumber <= runHeader.LastSpectrum; scanNumber++)
            //{
            //    var filter = rawFile.GetFilterForScanNumber(scanNumber);
            //    if (filter.MSOrder != MSOrderType.Ms2) continue;

            //    var scanEvent = rawFile.GetScanEventForScanNumber(scanNumber);
            //    if (scanEvent.MassCount >= 2)
            //    {
            //        double q1 = scanEvent.GetMass(0);
            //        double q3 = scanEvent.GetMass(1);
            //        transitions.Add((q1, q3));
            //    }
            //}

            //Console.WriteLine($"\n✅ Found {transitions.Count} transitions");

            //Directory.CreateDirectory("Chromatograms");

            //foreach (var (q1, q3) in transitions)
            //{
            //    var traceSettings = new ChromatogramTraceSettings(TraceType.MassRange)
            //    {
            //        Filter = "ms2",
            //        MassRanges = new[] { new Range(q3, q3) }  // Target Q3 ion
            //    };

            //    var tolerance = new MassOptions()
            //    {
            //        Tolerance = 0.5,
            //        ToleranceUnits = ToleranceUnits.amu
            //    };

            //    var chromDataRaw = rawFile.GetChromatogramData(new[] { traceSettings }, -1, -1);


            //    foreach (var (q1, q3) in transitions)
            //    {
            //        var traceSettings = new ChromatogramTraceSettings(TraceType.MassRange)
            //        {
            //            Filter = "ms2",
            //            MassRanges = new[] { new Range(q3, q3) }
            //        };

            //        var chromData = rawFile.GetChromatogramData(new[] { traceSettings }, 0, runHeader.SpectraCount);

            //        // chromData.Intensities and .Times are 2D arrays: [trace][point]
            //        if (chromData != null && chromData.Intensities.Length > 0 && chromData.Intensities[0].Length > 0)
            //        {
            //            double[] times = chromData.Times[0];
            //            double[] intensities = chromData.Intensities[0];

            //            var lines = times.Zip(intensities, (t, i) => $"{t:F3},{i:F0}").ToList();
            //            File.WriteAllLines($"Chromatograms/Q1_{q1:F1}_Q3_{q3:F1}.csv", lines);

            //            Console.WriteLine($"Exported Q1 {q1:F1} -> Q3 {q3:F1}, {lines.Count} points");
            //        }
            //    }
            //}

            var allTraces = ReconstructSRMChromatograms(rawFile);
            Directory.CreateDirectory("Chromatograms");

            // Write to CSV
            foreach (var kvp in allTraces)
            {
                var trace = kvp.Value;
                var lines = trace.Times.Zip(trace.Intensities, (t, i) => $"{t:F3},{i:F0}").ToList();
                File.WriteAllLines($"Chromatograms/Q1_{trace.Q1:F1}_Q3_{trace.Q3:F1}.csv", lines);
                Console.WriteLine($"Exported: Q1 {trace.Q1} -> Q3 {trace.Q3} with {lines.Count} points");
            }
        }
    }
}