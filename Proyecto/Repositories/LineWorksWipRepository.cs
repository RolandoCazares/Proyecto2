using System;
using System.Data;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using proyecto.Contracts;
using proyecto.Data;
using HtmlAgilityPack;


namespace proyecto.Repositories
{
    public class LineWorksWipRepository : IDbContextWip
    {

        private static readonly Lazy<LineWorksWipRepository> lazy = new Lazy<LineWorksWipRepository>(() => new LineWorksWipRepository());
        public static LineWorksWipRepository Instance { get => lazy.Value; }
        public async Task<DataTable> RunQueryWip(string Process, string Stations, DateTime FromDate, DateTime ToDate)
        {
            DataTable dataTable = new DataTable();

            string[] resultEstaciones = Stations.Split(',');

            try
            {
                foreach (string Estacion in resultEstaciones)
                {
                    using (var driver = new ChromeDriver())
                    {
                        driver.Navigate().GoToUrl("http://famesasv.cw01.contiwan.com:8778/iGate/wip");
                        driver.SwitchTo().Frame("idLoginFrame");
                        IWebElement botonEvaprod = driver.FindElement(By.Id("mi_33"));
                        botonEvaprod.Click();
                        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
                        driver.SwitchTo().Frame("_sub"); //papa
                        driver.SwitchTo().Frame("confr");//hijo
                        IWebElement divElement = driver.FindElement(By.CssSelector("div[fltr='testrun query_4'][onclick*='openFilterWindow']"));
                        divElement.Click();
                        IWebElement inputElement = driver.FindElement(By.Id("fltr1"));
                        inputElement.SendKeys(Estacion);
                        inputElement.SendKeys(Keys.Enter);
                        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        driver.SwitchTo().ParentFrame();
                        driver.SwitchTo().Frame("topfr");
                        string ventanaPadre = driver.CurrentWindowHandle;
                        IWebElement inputElementFromDate = driver.FindElement(By.CssSelector("input[name='tsf_ti_start']"));
                        string formattedDateTimeFromDate = FromDate.ToString("MM.dd.yyyy HH:mm:ss");
                        inputElementFromDate.Clear();
                        inputElementFromDate.SendKeys(formattedDateTimeFromDate);
                        IWebElement imgElement = driver.FindElement(By.CssSelector("img[src='staticFile/standard/images/controls/v3_bt/to.png']"));
                        imgElement.Click();
                        WebDriverWait wait3 = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                        wait3.Until(d => d.WindowHandles.Count > 1);
                        foreach (string ventanaHandle in driver.WindowHandles)
                        {
                            if (ventanaHandle != ventanaPadre)
                            {
                                driver.SwitchTo().Window(ventanaHandle);
                                IWebElement checkboxElement = driver.FindElement(By.XPath("//label[text()='Use current system time']/preceding-sibling::input[@type='checkbox']"));

                                checkboxElement.Click();
                                IWebElement okButton = driver.FindElement(By.XPath("//button[text()='OK']"));

                                okButton.Click();
                                driver.SwitchTo().Window(ventanaPadre);

                                break;
                            }
                        }
                        driver.SwitchTo().Window(ventanaPadre);
                        driver.SwitchTo().Frame("idLoginFrame");
                        driver.SwitchTo().Frame("_sub"); //papa
                        driver.SwitchTo().Frame("topfr");
                        IWebElement inputElementToDate = driver.FindElement(By.CssSelector("input[name='tsf_ti']"));
                        string formattedDateTimeToDate = ToDate.ToString("dd.MM.yyyy HH:mm:ss");
                        inputElementToDate.Clear();
                        inputElementToDate.SendKeys(formattedDateTimeToDate);
                        IWebElement showButton = driver.FindElement(By.CssSelector("button[name='show'][onclick*='submitButtonControlForm']"));
                        showButton.Click();
                        driver.SwitchTo().ParentFrame();
                        driver.SwitchTo().Frame("topfr");
                        IWebElement botonExportar = driver.FindElement(By.CssSelector("img[title='Export displayed report rows to XML']"));

                        botonExportar.Click();
                        WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(20000));
                        wait2.Until(d => d.WindowHandles.Count > 1);

                        foreach (string ventanaHandle in driver.WindowHandles)
                        {
                            if (ventanaHandle != ventanaPadre)
                            {
                                driver.SwitchTo().Window(ventanaHandle);
                                break;
                            }
                        }
                        await Task.Delay(30000);

                        IWebElement bodyElement = driver.FindElement(By.ClassName("pretty-print"));

                        WebDriverWait wait5 = new WebDriverWait(driver, TimeSpan.FromSeconds(3000));
                        await Task.Delay(2000); // 20,000 milisegundos = 20 segundos

                        // Crear un documento HTML utilizando HtmlAgilityPack
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(bodyElement.GetAttribute("innerText"));

                        // Obtener todas las filas y columnas
                        var rows = htmlDoc.DocumentNode.SelectNodes("//row");

                        if (rows != null && rows.Count > 0)
                        {
                            // Agregar columnas
                            var columnNodes = rows[0].SelectNodes("column");
                            if (columnNodes != null)
                            {
                                foreach (var columnNode in columnNodes)
                                {
                                    string columnName = columnNode.GetAttributeValue("name", "").Trim();

                                    // Verificar si la columna ya existe antes de agregarla
                                    if (!dataTable.Columns.Contains(columnName))
                                    {
                                        dataTable.Columns.Add(columnName);
                                    }
                                        
                                    
                                }
                                if (!dataTable.Columns.Contains("Process"))
                                {
                                    dataTable.Columns.Add("Process");
                                }
                            }

                            // Agregar filas
                            foreach (var row in rows)
                            {
                                var dataNodes = row.SelectNodes("column");
                                if (dataNodes != null)
                                {
                                    DataRow dataRow = dataTable.NewRow();
                                    for (int j = 0; j < dataNodes.Count; j++)
                                    {
                                        dataRow[j] = dataNodes[j].InnerText.Trim();
                                    }

                                    // Agregar valor para la columna "Process"
                                    dataRow["Process"] = Process; // Reemplaza "ValorProceso" con el valor que desees

                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }

                        // Ahora, tienes tu DataTable con los datos de la tabla HTML

                        // Cerrar el navegador al finalizar
                        driver.Quit();


                    }
                }

            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dataTable;

        }
    }
}
