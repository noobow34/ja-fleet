using System;
using System.IO;
using System.Collections.Generic;
using ClosedXML.Excel;
using jafleet.Models;

namespace jafleet.Excel
{
    public class ExcelReader
    {

        private static String ExcelFilePath = "../ja-fleet_db/JA-Fleet.xlsx";

        public List<Aircraft> GetAircraftInfo(String sheetName){
            List<Aircraft> aircraftList = new List<Aircraft>();

            using (FileStream fs = new FileStream(ExcelFilePath, FileMode.Open, FileAccess.Read))
            {
                var excel = new XLWorkbook(fs);
                var sheet = excel.Worksheet(sheetName);
                for (int i = 2; i <= sheet.LastRowUsed().RowNumber(); i++)
                {
                    Aircraft aircraft = new Aircraft();

                    aircraft.Airline = sheet.Cell(i, 1).Value.ToString();
                    aircraft.Type = sheet.Cell(i, 2).Value.ToString();
                    aircraft.RegistrationNumber = sheet.Cell(i, 3).Value.ToString();
                    aircraft.SerialNumber = sheet.Cell(i, 4).Value.ToString();
                    aircraft.RegistrationDate = sheet.Cell(i, 5).Value.ToString();
                    aircraft.Wifi = sheet.Cell(i, 6).Value.ToString();
                    aircraft.Status = sheet.Cell(i, 7).Value.ToString();
                    aircraft.Remarks = sheet.Cell(i, 8).Value.ToString();

                    aircraftList.Add(aircraft);
                }

            }

            return aircraftList;
        }
    }
}
