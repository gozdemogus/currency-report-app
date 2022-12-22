using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using currencyApp.Models;
using RestSharp;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using OfficeOpenXml;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;

namespace currencyApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
     
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    public IActionResult Currency(int id)
    {

        var currentDate = DateTime.Now;
        var oneMonthEarlier = currentDate.AddMonths(-1);

        var dateCurrentString = currentDate.ToString("yyyy-MM-dd");
        var dateEarlierString = oneMonthEarlier.ToString("yyyy-MM-dd");


        var client = new WebClient();
        client.Headers.Add("apikey", "");
        var apiKey = "";

        var url = $@"https://api.apilayer.com/exchangerates_data/timeseries?start_date=2022-11-21&end_date=2022-12-21&base=TRY&symbols=EUR,USD&apikey={apiKey}";
        var response = client.DownloadString(url);

        var data = JsonConvert.DeserializeObject<dynamic>(response);


        List<Currency> currencies = new List<Currency>();
        // process the data


        foreach (var date in data.rates)
        {
            var usdRate = date.Value.USD;
            var euroRate = date.Value.EUR;
            string dateString = date.Name;

            Currency currency = new Currency();
            currency.DolarRate = 1 / double.Parse(usdRate.ToString());
            currency.EuroRate = 1 / double.Parse(euroRate.ToString());
            currency.Date = dateString;
            currencies.Add(currency);
        }

        var orderCurrenciesUSD = currencies.OrderByDescending(x => x.DolarRate).Take(5).ToList();
        var orderCurrenciesEUR = currencies.OrderByDescending(x => x.EuroRate).Take(5).ToList();


        using (var workBook = new XLWorkbook())
        {

            if (id == 1)
            {
                var workSheet = workBook.Worksheets.Add("Currencies");
            workSheet.Cell(1, 1).Value = "Date";
            workSheet.Cell(1, 2).Value = "EUR";

                int rowCount = 2;
                foreach (var item in currencies)
                {
                    workSheet.Cell(rowCount, 1).Value = item.Date;
                    workSheet.Cell(rowCount, 2).Value = item.EuroRate;
                    rowCount++;
                }

            }

            if (id == 2)
            {
                var workSheet = workBook.Worksheets.Add("Currencies");
                workSheet.Cell(1, 1).Value = "Date";
                workSheet.Cell(1, 3).Value = "USD";

                int rowCount = 2;
                foreach (var item in currencies)
                {
                    workSheet.Cell(rowCount, 1).Value = item.Date;
                    workSheet.Cell(rowCount, 3).Value = item.DolarRate;
                    rowCount++;
                }

            }


            using (var stream = new MemoryStream())
            {
                workBook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "currency.xlsx");
            }
        }

        return View();
    }

 

    [HttpGet]
    public IActionResult CurrencyChart(int id)
    {

        var currentDate = DateTime.Now;
        var oneMonthEarlier = currentDate.AddMonths(-1);

        var dateCurrentString = currentDate.ToString("yyyy-MM-dd");
        var dateEarlierString = oneMonthEarlier.ToString("yyyy-MM-dd");


        var client = new WebClient();
        client.Headers.Add("apikey", "");
        var apiKey = "";

        var url = $@"https://api.apilayer.com/exchangerates_data/timeseries?start_date=2022-11-21&end_date=2022-12-21&base=TRY&symbols=EUR,USD&apikey={apiKey}";
        var response = client.DownloadString(url);

        var data = JsonConvert.DeserializeObject<dynamic>(response);


        List<Currency> currencies = new List<Currency>();
        // process the data

        foreach (var date in data.rates)
        {
            var usdRate = date.Value.USD;
            var euroRate = date.Value.EUR;
            string dateString = date.Name;

            Currency currency = new Currency();
            currency.DolarRate = 1 / double.Parse(usdRate.ToString());
            currency.EuroRate = 1 / double.Parse(euroRate.ToString());
            currency.Date = dateString;
            currencies.Add(currency);
        }


        var orderCurrenciesUSD = currencies.OrderByDescending(x => x.DolarRate).Take(5).ToList();
        var orderCurrenciesEUR = currencies.OrderByDescending(x => x.EuroRate).Take(5).ToList();

        if (id == 1)
        {
            return Json(new { jsonList = orderCurrenciesEUR });
        }
        if (id == 2)
        {
            return Json(new { jsonList = orderCurrenciesUSD });
        }
        else
        {
            return View();
        }

    }

}
