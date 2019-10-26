using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestTaskService.Model;
using System.Net;
using System.Xml.Linq;
using System.Threading;

namespace TestTaskService
{
    public class TestTask
    {
        bool flag = true;
        TimeSpan interval = new TimeSpan(24, 0, 0);
        //TimeSpan testInterval = new TimeSpan(0, 0, 5); Интервал для теста


        /// <summary>
        /// Запуск опроса курсов валют с сохранение в бд
        /// </summary>
        public void Start()
        {
            //Добавление нескольких тестовых записей в таблицу Curencies если таблица пустая
            using (TestTaskContext db = new TestTaskContext())
            {
                if (!db.Currencies.Any())
                {
                    var currencies = new List<Currency>
                    {
                        new Currency{ CharCode ="USD" ,Id="R01235"},
                        new Currency{ CharCode ="EUR" ,Id="R01239"},
                        new Currency{ CharCode ="JPY" ,Id="R01820"}
                    };
                    db.Currencies.AddRange(currencies);
                    db.SaveChanges();
                }
            }

            while (flag)
            {
                var dateString = DateTime.Now.ToString("dd.MM.yyyy");

                WebClient wc = new WebClient() { Encoding = Encoding.GetEncoding(1251) };
                var response = wc.DownloadString($"http://www.cbr.ru/scripts/XML_daily.asp?date_req={dateString}");

                var col = XDocument.Parse(response).Root;
                var elements = col.Elements();

                using (TestTaskContext db = new TestTaskContext())
                {
                    //Удаляем курсы за текущую дату чтобы не было дублей за один день
                    var todayCurrencyRates = db.CurrencyRates
                        .Where(x => x.Date.Year == DateTime.Now.Year
                                 && x.Date.Month == DateTime.Now.Month
                                 && x.Date.Day == DateTime.Now.Day);
                    db.CurrencyRates.RemoveRange(todayCurrencyRates);

                    //Выбираем курсы только которые есть в таблице Currencies и сохраняем их в базу с текущей датой
                    var charCodes = db.Currencies.Select(e => e.CharCode).ToList();
                    foreach (var item in elements)
                    {
                        var element = item.Element("CharCode").Value.ToString();
                        if (charCodes.Contains(element))
                        {
                            CurrencyRate cr = new CurrencyRate();
                            cr.Date = DateTime.Now.Date;
                            cr.Currency = element;
                            cr.Сourse = decimal.Parse(item.Element("Value").Value.ToString());
                            db.CurrencyRates.Add(cr);
                        }
                    }
                    db.SaveChanges();
                }
                Thread.Sleep(interval);
            }
        }

        /// <summary>
        /// Остановка опроса курсов валют
        /// </summary>
        public void Stop()
        {
            flag = false;
        }

        /// <summary>
        /// Возвращает курс валюты по заданному коду за указанную дату
        /// </summary>
        /// <param name="charCode">Код валюты</param>
        /// <param name="date">Дата</param>
        /// <returns></returns>
        public decimal GetRate(string charCode, DateTime date)
        {
            using (TestTaskContext db = new TestTaskContext())
            {
                return db.CurrencyRates
                    .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day && x.Currency == charCode)
                    .Select(x => x.Сourse)
                    .FirstOrDefault();
            }
        }
    }
}
