using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace ABSARecon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<VISADATA> accountDetails = new List<VISADATA>();
            List<CardCentre> cardDetails = new List<CardCentre>();
            try
            {
                Console.WriteLine("----------------Start--------------------");

                Console.WriteLine("-----------------------------------------");

                Console.WriteLine("Start Time ------------->   " + DateTime.Now);
                string source = ConfigurationManager.AppSettings["inputPath"];
                string destination = ConfigurationManager.AppSettings["destination"];
                string output = ConfigurationManager.AppSettings["outputPath"];
                string backup = ConfigurationManager.AppSettings["backup"];
                string VisaRecon = ConfigurationManager.AppSettings["visaRecon"];
                string Report = ConfigurationManager.AppSettings["Report"];

                if (!UserFunctions.KillAllExcelInstaces())
                {
                    Console.WriteLine(" ");
                    Console.WriteLine("Unable to kill all excel instance");
                    Task.Factory.StartNew(() => UserFunctions.WriteLog(" ", " ", "Unable to kill all excel instance", ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
                }

                Console.WriteLine(" ");
                Console.WriteLine("Excel instances killed sucessfully");
                Task.Factory.StartNew(() => UserFunctions.WriteLog(" ", " ", "Excel instances killed sucessfully", ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

                UserFunctions.ReadAllFiles(source, out List<FileDetails> fileDetails);


                if (!fileDetails.Any())
                {
                    Console.WriteLine("No data found in location");
                    Task.Factory.StartNew(() => UserFunctions.WriteLog(" ", " ", "No data found in location", ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
                    Thread.Sleep(10000);
                    return;
                }
                Console.WriteLine(" ");
                Task.Factory.StartNew(() => UserFunctions.WriteLog(" ", " ", "Data read from file successfully", ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
                Console.WriteLine("Data read from file successfully");

                foreach (var item in fileDetails)
                {
                    string filePath = item.FilePath;
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string jsonInput = UserFunctions.ReadExcelToJson(filePath, destination, /*Path.GetFileNameWithoutExtension(filePath)*/ fileName);
                    string message = "";

                    if (string.IsNullOrEmpty(jsonInput))
                    {
                        Console.WriteLine("Unable to read data from " + filePath);
                        Task.Factory.StartNew(() => UserFunctions.WriteLog(item.FileNameWithoutExtension, " ", "Unable to read data from " + filePath, ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
                        Thread.Sleep(10000);
                        return;
                    }

                    Console.WriteLine(" ");
                    Console.WriteLine("Data read from json succesfully successfully");
                    Task.Factory.StartNew(() => UserFunctions.WriteLog(item.FileNameWithoutExtension, " ", "Data read from json succesfully successfully", ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

                    if (fileName == StaticVariables.RawConversionVisa)
                    {
                        UserFunctions.ReadJson(jsonInput, out accountDetails);

                        UserFunctions.CleanUpData(accountDetails, out List<CleanedData> cleanData, out message);

                        UserFunctions.RemoveDuplicates(cleanData, out List<CleanedDataTwo> removedDuplicate, out string messages);
                        UserFunctions.AmtEachDay(removedDuplicate, out List<CleanedDataTwo> amtEachDay, out message);
                        UserFunctions.getDuplicates(cleanData, out List<CleanedData> getDuplicates, out message);

                        if (string.IsNullOrEmpty(jsonInput))
                        {
                            Console.WriteLine(message + " " + filePath);
                            Task.Factory.StartNew(() => UserFunctions.WriteLog(item.FileNameWithoutExtension, " ", message + " " + filePath, ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
                            Thread.Sleep(10000);
                            return;
                        }

                        Console.WriteLine(" ");
                        Console.WriteLine(message);
                        Task.Factory.StartNew(() => UserFunctions.WriteLog(item.FileNameWithoutExtension, " ", message, ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));


                        string generatedFile = "Clean Data " + DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss");

                        UserFunctions.CreateExcel(generatedFile, new List<string> { JsonConvert.SerializeObject(amtEachDay) }, out string outputFile, output);

                        UserFunctions.MoveFile(item.FilePath, backup + Path.GetFileName(item.FilePath));
                    }

                    else
                    {
                        
                        UserFunctions.ReadJsonTwo(jsonInput, out cardDetails);
                        UserFunctions.SortedData(cardDetails, out List<cleanCardCentre> sortedData, out message);
                        UserFunctions.writeToReconSheet(sortedData, VisaRecon, "A009", Report, out message);

                        UserFunctions.MoveFile(item.FilePath, backup + Path.GetFileName(item.FilePath));
                    }
                    
                }

            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => UserFunctions.WriteLog(" ", " ", ex.Message + "  || " + ex.StackTrace, ConfigurationManager.AppSettings["ApplicationName"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
                Console.WriteLine(" ");
                Console.WriteLine("Exception -------------------->    " + ex.Message + "  || " + ex.StackTrace);
            }
            Console.WriteLine("");

            Console.WriteLine(accountDetails.Count + " files  process and completed @ " + DateTime.Now);
            Console.WriteLine("");
            Console.WriteLine("Process completed");
            Thread.Sleep(15000);
        }
    }
}

