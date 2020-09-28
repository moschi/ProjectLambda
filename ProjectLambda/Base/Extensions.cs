using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static ProjectLambda.Base.LambdaFile;
using Excel = Microsoft.Office.Interop.Excel;

namespace ProjectLambda.Base
{
    public static class Extensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        }
    }

    public static class VariaHelper
    {
        public static double GetFileSize(string filePath)
        {
            var info = new FileInfo(filePath);
            return (info.Length / 1024) / 1024; // in mb
        }

        public static ObservableCollection<LambdaFile> ReadExcelFile(string path, AddLogEntryDelegate addLogEntryDelegate)
        {
            addLogEntryDelegate(LogLevel.INFO, "ExcelFileReader", "Beginning reading of file");
            // objects which need to be released are declared outside of the safe try-catch-finally
            Excel.Application xlApp = null;
            Excel.Workbook xlWorkbook = null;
            Excel._Worksheet xlWorksheet1 = null; // filepathcollection 1
            Excel.Range xlRange1 = null; // cellrange of worksheet 1
            try
            {
                // initialize the interop with Excel
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open(path);

                xlWorksheet1 = xlWorkbook.Sheets[1];
                xlRange1 = xlWorksheet1.UsedRange;

                // create temporary directory
                var importDict = new Dictionary<string, string>();

                addLogEntryDelegate(LogLevel.INFO, "ExcelFileReader", "Beginning reading of worksheet");
                int rowCountS1 = xlRange1.Rows.Count;
                // iterate over first and second column of the worksheet
                // excel is not zero based, hence starting at index 1
                for (int rowIndex = 1; rowIndex <= rowCountS1; rowIndex++)
                {
                    // read first cell value
                    if (xlRange1.Cells[rowIndex, 1] != null && xlRange1.Cells[rowIndex, 1].Value2 != null)
                    {
                        if(xlRange1.Cells[rowIndex, 2] != null && xlRange1.Cells[rowIndex, 2].Value2 != null)
                        {
                            string sourcePath = xlRange1.Cells[rowIndex, 1].Value2.ToString();
                            string targetPath = xlRange1.Cells[rowIndex, 2].Value2.ToString();

                            importDict.SaveAdd(sourcePath, targetPath, addLogEntryDelegate);
                        }
                        else
                        {
                            string sourcePath = xlRange1.Cells[rowIndex, 1].Value2.ToString();
                            addLogEntryDelegate(LogLevel.WARNING, "ExcelFileReader", $"No target path was specified for source path {sourcePath}.");
                        }
                    }
                }

                var lamdaFiles = new List<LambdaFile>();

                // create LambdaFiles from Dictionary
                foreach (KeyValuePair<string, string> pair in importDict)
                {
                    lamdaFiles.Add(new LambdaFile(pair.Key, pair.Value));
                }

                addLogEntryDelegate(LogLevel.INFO, "ExcelFileReader", $"Found {importDict.Count} files in total");
                return new ObservableCollection<LambdaFile>(lamdaFiles);
            }
            catch (Exception ex)
            {
                addLogEntryDelegate(LogLevel.EXCEPTION, "ExcelFileReader", $"Reading failed: {ex.Message}");
                throw new Exception("Something went wrong while reading Excel.", ex);
            }
            finally
            {
                addLogEntryDelegate(LogLevel.INFO, "ExcelFileReader", "Finalizing reading of excel file");

                // collect garbage
                // normally wouldn't call the GC from code, but in the case of the messy office interop its fine
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // the following unmanaged objects are closed / quit / released under consideration if they are null which might happen 
                // if something went wrong while reading the file

                // release com objects to fully kill excel process
                xlRange1?.ReleaseComObject();
                xlWorksheet1?.ReleaseComObject();

                // close and release
                xlWorkbook?.Close();
                xlWorkbook?.ReleaseComObject();

                // quit and release
                xlApp?.Quit();
                xlApp?.ReleaseComObject();
            }
        }

        // extension is used so the calls can be one-liners and are not called if the objects are null
        public static void ReleaseComObject(this object obj)
        {
            Marshal.ReleaseComObject(obj);
        }

        public static void SaveAdd(this Dictionary<string, string> dict, string key, string value, AddLogEntryDelegate logEntryDelegate)
        {
            if (dict.ContainsValue(value))
            {
                logEntryDelegate(LogLevel.CRITICAL, "ExcelFileReader",
                    $"The excel file contains at least two entries with the same targetpath: {value}. All but the first registered will be skipped!");

                return;
            }

            if (dict.ContainsKey(key))
            {
                logEntryDelegate(LogLevel.WARNING, "ExcelFileReader",
                    $"The excel file contains two entries for the following path: {key}. First value: {dict[key]}, Second value: {value}. All but the first registered will be skipped!");

                return;
            }
            dict.Add(key, value);
        }
    }
}
