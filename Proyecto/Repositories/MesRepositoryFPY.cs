using proyecto.Contracts;
using proyecto.Data;
using proyecto.Helpers;
using proyecto.Services;
using proyecto.Models.ExampleModels;
using System.Data;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;
using proyecto.Models.Top10;
using System.Collections.Generic;
using System.Diagnostics;
using proyecto.Models.FPY.Historial;
using static System.Net.Mime.MediaTypeNames;
using proyecto.Models.FPY;
using proyecto.Models.FPY.TopOffender;
using NuGet.Packaging;
using System.Diagnostics;
using System.Threading.Tasks;
using SshNet.Security.Cryptography;
using System;
using System.Globalization;
using proyecto.Models;
using System.Threading; 


namespace proyecto.Repositories
{
    public class MesRepositoryFPY : IMesRepositoryFPY
    {
        private AnalysisDbContext _context;
        public MesRepositoryFPY(AnalysisDbContext context)
        {
            _context = context;
        }

        private static IDbContext dbContext = OracleDbContext.Instance;
        private static IDbContextWip dbcontextWip = LineWorksWipRepository.Instance;
        static (DateTime FromDate, DateTime ToDate) GetWeekDates(string weekString)
        {
            // Parse la cadena de la semana en un objeto CultureInfo
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            // Obtenga el año y la semana del formato "yyyy-Www"
            int year = int.Parse(weekString.Substring(0, 4));
            int week = int.Parse(weekString.Substring(6));

            // Obtenga la fecha del primer día de la semana
            DateTime jan1 = new DateTime(year, 1, 1);
            DateTime firstDayOfWeek = jan1.AddDays((week - 1) * 7 - (int)jan1.DayOfWeek + (int)DayOfWeek.Monday);

            // Calcule la fecha del último día de la semana (domingo)
            DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);

            // Ajuste las horas, minutos y segundos
            DateTime fromDateTime = new DateTime(firstDayOfWeek.Year, firstDayOfWeek.Month, firstDayOfWeek.Day, 0, 0, 0);
            DateTime toDateTime = new DateTime(lastDayOfWeek.Year, lastDayOfWeek.Month, lastDayOfWeek.Day, 23, 59, 59);

            return (fromDateTime, toDateTime);
        }

        static DateTime[] ObtenerFechasEnRango(DateTime fechaInicial, DateTime fechaFinal)
        {
            // Calcular la cantidad de días en el rango
            int cantidadDias = (int)(fechaFinal - fechaInicial).TotalDays + 1;

            // Crear un arreglo para almacenar las fechas
            DateTime[] fechasEnRango = new DateTime[cantidadDias];

            // Llenar el arreglo con las fechas en el rango
            for (int i = 0; i < cantidadDias; i++)
            {
                fechasEnRango[i] = fechaInicial.AddDays(i);
            }

            return fechasEnRango;
        }

        static string[][] FiltrarPorClave(string clave, string[][] arregloDeArreglos)
        {
            // Filtrar los arreglos que contienen la clave especificada
            var resultado = arregloDeArreglos.Where(arr => arr.Length > 0 && arr[0].Equals(clave)).ToArray();

            // Devolver el resultado filtrado
            return resultado;
        }

        static (string[][], double Goal, double GoalRolado) ObtenerEstacionesFPY(string Product, string Process)
        {
            double Goal = 0;
            double GoalRolado = 0;
            string[][] arregloDeArreglosDeCOMCCELLSoProcesos = new string[0][];

            if (Product == "FGEN1M")
            {
                Goal = 98.47;
                GoalRolado = 91.12;
                string[] ICTFGEN1M = { "ICT", "98", "CCN_SEMI", "CCN_ICT_PCB0103" };
                string[] FLASHFGEN1M = { "FLASH", "99.3", "CCN_SEMI", "CCN_FLASH_PCB0203" };
                string[] EOLFGEN1M = { "EOL", "97", "CCN_BE2_FIN", "CCN_PRU-FIN1_3140911,CCN_PRU-FIN2_3140911" };
                string[][] ArregloFORDGEN1 = { ICTFGEN1M, FLASHFGEN1M, EOLFGEN1M };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFORDGEN1).ToArray();
            }
            else if (Product == "FGEN3")
            {
                Goal = 98.33;
                GoalRolado = 93.45;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "98", "CCN_SEMI", "CCN_ICT_L3_PCB0103,CCN_ICT_L2_PCB0103,CCN_ICT_PCB0103,AN_ICT_L4_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "CCN_FLASH_PCB0203,CCN_FLASH1_FH_L2_PCB0203,AN_FLASH1_FH_L3_PCB0203" };
                string[] EOLFGEN3 = { "EOL", "97", "CCN_BE1_FIN", "CCN_PRU-FIN1_L2_3140911,CCN_PRU-FIN1_L3_3140911,CCN_PRU-FIN2_L2_3140911,CCN_PRU-FIN2_L3_3140911,CCN_PRU-FIN3_L2_3140911,CCN_PRU-FIN3_L3_3140911,CCN_PRU-FIN4_3140911,CCN_PRU-FIN4_L2_3140911,CCN_PRU-FIN4_L3_3140911,CCN_PRU-FIN5_3140911,CCN_PRU-FIN6_3140911,CCN_PRU-FIN3_3140911" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3, EOLFGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_PCBA")
            {
                Goal = 98.65;
                GoalRolado = 97.31;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "98", "CCN_SEMI", "CCN_ICT_L3_PCB0103,CCN_ICT_L2_PCB0103,CCN_ICT_PCB0103,AN_ICT_L4_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "CCN_FLASH_PCB0203,CCN_FLASH1_FH_L2_PCB0203,AN_FLASH1_FH_L3_PCB0203" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_BE")
            {
                Goal = 98;
                GoalRolado = 96.03;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] EOLFGEN3 = { "EOL", "97", "CCN_BE1_FIN", "CCN_PRU-FIN1_L2_3140911,CCN_PRU-FIN1_L3_3140911,CCN_PRU-FIN2_L2_3140911,CCN_PRU-FIN2_L3_3140911,CCN_PRU-FIN3_L2_3140911,CCN_PRU-FIN3_L3_3140911,CCN_PRU-FIN4_3140911,CCN_PRU-FIN4_L2_3140911,CCN_PRU-FIN4_L3_3140911,CCN_PRU-FIN5_3140911,CCN_PRU-FIN6_3140911,CCN_PRU-FIN3_3140911" };
                string[] PINCHKGEN3 = { "PINCHK", "99", "CCN_BE1_FIN", "CCN_PINCHK1_L2_3171311,CCN_PINCHK_3171311,CCN_PINCHK1_L3_3171311,AN_PINCHEK_L5_317311" };
                string[][] ArregloFGEN3 = { EOLFGEN3, PINCHKGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_PINCHK")
            {
                Goal = 99;
                GoalRolado = 99;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] PINCHKGEN3 = { "PINCHK", "99", "CCN_BE1_FIN", "CCN_PINCHK1_L2_3171311,CCN_PINCHK_3171311,CCN_PINCHK1_L3_3171311,AN_PINCHEK_L5_317311" };
                string[][] ArregloFGEN3 = { PINCHKGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_L1")
            {
                Goal = 98.43;
                GoalRolado = 95.35;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "99", "CCN_SEMI", "CCN_ICT_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "CCN_FLASH_PCB0203" };
                string[] EOLFGEN3 = { "EOL", "97", "CCN_BE1_FIN", "CCN_PRU-FIN4_3140911,CCN_PRU-FIN5_3140911,CCN_PRU-FIN6_3140911,CCN_PRU-FIN3_3140911" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3, EOLFGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_L2")
            {
                Goal = 98.43;
                GoalRolado = 95.35;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "98", "CCN_SEMI", "CCN_ICT_L2_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "CCN_FLASH1_FH_L2_PCB0203" };
                string[] EOLFGEN3 = { "EOL", "97", "CCN_BE1_FIN", "CCN_PRU-FIN1_L2_3140911,CCN_PRU-FIN2_L2_3140911,CCN_PRU-FIN3_L2_3140911,CCN_PRU-FIN4_L2_3140911" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3, EOLFGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_L3")
            {
                Goal = 98.43;
                GoalRolado = 95.35;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "98", "CCN_SEMI", "CCN_ICT_L3_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "AN_FLASH1_FH_L3_PCB0203" };
                string[] EOLFGEN3 = { "EOL", "97", "CCN_BE1_FIN", "CCN_PRU-FIN1_L3_3140911,CCN_PRU-FIN2_L3_3140911,CCN_PRU-FIN3_L3_3140911,CCN_PRU-FIN4_L3_3140911" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3, EOLFGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN3_L4")
            {
                Goal = 98.65;
                GoalRolado = 97.31;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "98", "CCN_SEMI", "AN_ICT_L4_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "AN_FLASH4_FH_L4_PCB0203" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN1MR_Line_PCBA_3")
            {
                Goal = 98.65;
                GoalRolado = 97.31;
                string[] ICTGEN1MR = { "ICT", "98", "CCN_SEMI", "AN_ICT3_PCB0103_MR" };
                string[] FLASHGEN1MR = { "FLASH", "99.3", "CCN_SEMI", "AN_FLASH1_PCB0203_MR" };
                string[][] ArregloMSM = { ICTGEN1MR, FLASHGEN1MR };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloMSM).ToArray();
            }
            else if (Product == "FGEN1MR_Line_PCBA_4")
            {
                Goal = 98.65;
                GoalRolado = 97.31;
                string[] ICTGEN1MR = { "ICT", "98", "CCN_SEMI", "AN_ICT4_PCB0103_MR" };
                string[] FLASHGEN1MR = { "FLASH", "99.3", "CCN_SEMI", "AN_FLASH4_PCB0203_MR" };
                string[][] ArregloMSM = { ICTGEN1MR, FLASHGEN1MR };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloMSM).ToArray();
            }
            else if (Product == "FGEN1MR_Line_BE_4")
            {
                Goal = 97.75;
                GoalRolado = 91.24;
                string[] MVTFGEN1M = { "MVT", "99", "CCN_BE2_FIN", "CCN_AUTINSP_L4_3143311" };
                string[] RELAYTESTFGEN1M = { "RELAY TEST", "99", "CCN_SEMI", "CCN_TEST_PCB0404" };
                string[] EOLFGEN1M = { "EOL", "95", "CCN_BE2_FIN", "CCN_PRU-FIN1_L4_3140911,CCN_PRU-FIN2_L4_3140911,CCN_PRU-FIN3_L4_3140911" };
                string[] PINCHECKFGEN1M = { "PINCHECK", "98", "CCN_BE3_FIN", "AN_PINCHECK_L5_3171311" };

                string[][] ArregloFORDGEN1 = { MVTFGEN1M, RELAYTESTFGEN1M, EOLFGEN1M, PINCHECKFGEN1M };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFORDGEN1).ToArray();
            }
            else if (Product == "FGEN1MR_Line_BE_5")
            {
                Goal = 97;
                GoalRolado = 94.05;
                string[] MVTFGEN1M = { "MVT", "99", "CCN_BE2_FIN", "AN_AUTINSP_L5_3143311" };
                string[] EOLFGEN1M = { "EOL", "95", "CCN_BE2_FIN", "AN_PRU-FIN1_L5_3140911,AN_PRU-FIN2_L5_3140911,AN_PRU-FIN3_L5_3140911" };
                string[][] ArregloFORDGEN1 = { MVTFGEN1M, EOLFGEN1M };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFORDGEN1).ToArray();
            }
            else if (Product == "FGEN1MR")
            {
                Goal = 98.05;
                GoalRolado = 90.6;
                string[] ICTGEN1MR = { "ICT", "98", "CCN_SEMI", "AN_ICT3_PCB0103_MR,AN_ICT4_PCB0103_MR" };
                string[] FLASHGEN1MR = { "FLASH", "99.3", "CCN_SEMI", "AN_FLASH1_PCB0203_MR,AN_FLASH4_PCB0203_MR" };
                string[] MVTFGEN1M = { "MVT", "99", "CCN_BE2_FIN", "CCN_AUTINSP_L4_3143311,AN_AUTINSP_L5_3143311" };
                string[] EOLFGEN1M = { "EOL", "97", "CCN_BE2_FIN", "CCN_PRU-FIN1_L4_3140911,CCN_PRU-FIN2_L4_3140911,CCN_PRU-FIN3_L4_3140911,AN_PRU-FIN1_L5_3140911,AN_PRU-FIN2_L5_3140911,AN_PRU-FIN3_L5_3140911" };
                string[] PINCHECKFGEN1M = { "PINCHECK", "98", "CCN_BE3_FIN", "AN_PINCHECK_L5_3171311" };
                string[] RELAYTESTFGEN1M = { "RELAY TEST", "99", "CCN_SEMI", "CCN_TEST_PCB0404" };

                string[][] ArregloFORDGEN1 = { ICTGEN1MR, FLASHGEN1MR, MVTFGEN1M, EOLFGEN1M, PINCHECKFGEN1M, RELAYTESTFGEN1M };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFORDGEN1).ToArray();
            }
            else if (Product == "MSM")
            {
                Goal = 98.45;
                GoalRolado = 93.92;
                string[] ICTMSM = { "ICT", "99", "CCN_SEMI", "CCN_ICT1_3180111,CCN_ICT2_3180111" };
                string[] FLASHMSM = { "FLASH", "99.3", "CCN_FLASH", "CCN_FLASH_3180211" };
                string[] EOLMSM = { "EOL", "96.5", "CCN_SEMI", "CCN_PRU-FIN6_3180911,CCN_PRU-FIN2_3180911,CCN_PRU-FIN3_3180911,CCN_PRU-FIN5_3180911,CCN_PRU-FIN1_3180911,CCN_PRU-FIN4_3180911,CCN_PRU-FIN7_3180911,CCN_PRU-FIN8_3180911" };
                string[] PINCHEKMSM = { "PINCHECK", "99", "CCN_SEMI", "CCN_PINCHK_3181311,CCN_PINCHK_CSWM_3181311" };
                string[][] ArregloMSM = { ICTMSM, FLASHMSM, EOLMSM, PINCHEKMSM };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloMSM).ToArray();
            }
            else if (Product == "HONDA")
            {
                Goal = 98;
                GoalRolado = 97;
                string[] FLASHHONDA = { "FLASH", "98.5", "CCN_FLASH", "CCN_TEST_PCB1002,CCN_TEST-UNIT_PCB1002" };
                string[] EOLHONDA = { "EOL", "98.5", "CCN_BE3_FIN", "CCN_PRU-FIN_3130911" };
                string[] PINCHEKHONDA = { "PINCHECK", "98.5", "CCN_BE3_FIN", "CCN_CHKPIN_3131311" };
                string[][] ArregloHONDA = { FLASHHONDA, EOLHONDA, PINCHEKHONDA };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloHONDA).ToArray();
            }
            else if (Product == "SUBARU_L1")
            {
                Goal = 98.75;
                GoalRolado = 90.4;
                string[] ICTSUBARU = { "ICT", "99", "CCN_BE2_FIN", "CCN_ICT_3170111" };
                string[] FLASHSUBARU = { "FLASH", "99.3", "CCN_BE2_FIN", "CCN_FLASH2_3170211" };
                string[] SWLSUBARU = { "SWL", "97.3", "CCN_BE2_FIN", "CCN_SWL_3170811" };
                string[] EOLSUBARU = { "EOL", "99", "CCN_BE2_FIN", "CCN_PRU-FIN1_3170911,CCN_PRU-FIN2_3170911,CCN_PRU-FIN3_3170911,CCN_PRU-FIN4_3170911,CCN_PRU-FIN5_3170911" };
                string[] BATTESTSUBARU = { "BATTERY TEST", "99.2", "CCN_BE2_FIN", "CCN_BATTEST_3170411" };
                string[] RFTSUBARU = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT1_3171011,CCN_RFT2_3171011,CCN_RFT3_3171011,CCN_RFT4_3171011,CCN_RFT5_3171011,CCN_RFT6_3171011" };
                string[] TLCSUBARU = { "TLC", "99", "CCN_BE2_FIN", "CCN_TLC1_3171211,CCN_TLC2_3171211,CCN_TLC3_3171211,CCN_TLC4_3171211" };
                string[] PINCHECKSUBARU = { "PINCHECK", "99.5", "CCN_BE2_FIN", "CCN_CHKPIN_3171301" };

                string[][] ArregloSUBARU = { BATTESTSUBARU, ICTSUBARU, FLASHSUBARU, SWLSUBARU, EOLSUBARU, RFTSUBARU, TLCSUBARU, PINCHECKSUBARU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloSUBARU).ToArray();
            }
            else if (Product == "SUBARU_L2")
            {
                Goal = 98.75;
                GoalRolado = 90.4;
                string[] ICTSUBARU = { "ICT", "99", "CCN_BE2_FIN", "CCN_ICT2_3170111" };
                string[] FLASHSUBARU = { "FLASH", "99.3", "CCN_BE2_FIN", "CCN_FLASH3_3170211" };
                string[] SWLSUBARU = { "SWL", "97.3", "CCN_BE2_FIN", "CCN_SWL2_3170811" };
                string[] EOLSUBARU = { "EOL", "99", "CCN_BE2_FIN", "CCN_PRU-FIN6_3170911,CCN_PRU-FIN7_3170911,CCN_PRU-FIN8_3170911" };
                string[] BATTESTSUBARU = { "BATTERY TEST", "99.2", "CCN_BE2_FIN", "CCN_BATTEST2_3170411" };
                string[] RFTSUBARU = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT7_3171011,CCN_RFT8_3171011,CCN_RFT9_3171011,CCN_RFT10_3171011" };
                string[] TLCSUBARU = { "TLC", "99", "CCN_BE2_FIN", "CCN_TLC5_3171211,CCN_TLC6_3171211,CCN_TLC7_3171211,CCN_TLC8_3171211" };
                string[] PINCHECKSUBARU = { "PINCHECK", "99.5", "CCN_BE2_FIN", "CCN_CHKPIN2_3171301" };

                string[][] ArregloSUBARU = { BATTESTSUBARU, ICTSUBARU, FLASHSUBARU, SWLSUBARU, EOLSUBARU, RFTSUBARU, TLCSUBARU, PINCHECKSUBARU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloSUBARU).ToArray();
            }
            else if (Product == "SUBARU")
            {
                Goal = 98.75;
                GoalRolado = 90.4;
                string[] ICTSUBARU = { "ICT", "98", "CCN_BE2_FIN", "CCN_ICT_3170111,CCN_ICT2_3170111" };
                string[] FLASHSUBARU = { "FLASH", "98", "CCN_BE2_FIN", "CCN_FLASH2_3170211,CCN_FLASH3_3170211" };
                string[] SWLSUBARU = { "SWL", "98", "CCN_BE2_FIN", "CCN_SWL_3170811,CCN_SWL2_3170811" };
                string[] EOLSUBARU = { "EOL", "98", "CCN_BE2_FIN", "CCN_PRU-FIN1_3170911,CCN_PRU-FIN2_3170911,CCN_PRU-FIN3_3170911,CCN_PRU-FIN4_3170911,CCN_PRU-FIN5_3170911,CCN_PRU-FIN6_3170911,CCN_PRU-FIN7_3170911,CCN_PRU-FIN8_3170911" };
                string[] BATTESTSUBARU = { "BATTERY TEST", "98", "CCN_BE2_FIN", "CCN_BATTEST_3170411,CCN_BATTEST2_3170411" };
                string[] RFTSUBARU = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT1_3171011,CCN_RFT2_3171011,CCN_RFT3_3171011,CCN_RFT4_3171011,CCN_RFT5_3171011,CCN_RFT6_3171011,CCN_RFT7_3171011,CCN_RFT8_3171011,CCN_RFT9_3171011,CCN_RFT10_3171011" };
                string[] TLCSUBARU = { "TLC", "98", "CCN_BE2_FIN", "CCN_TLC1_3171211,CCN_TLC2_3171211,CCN_TLC3_3171211,CCN_TLC4_3171211,CCN_TLC5_3171211,CCN_TLC6_3171211,CCN_TLC7_3171211,CCN_TLC8_3171211" };
                string[] PINCHECKSUBARU = { "PINCHECK", "98", "CCN_BE2_FIN", "CCN_CHKPIN_3171301,CCN_CHKPIN2_3171301" };

                string[][] ArregloSUBARU = { BATTESTSUBARU, ICTSUBARU, FLASHSUBARU, SWLSUBARU, EOLSUBARU, RFTSUBARU, TLCSUBARU, PINCHECKSUBARU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloSUBARU).ToArray();
            }
            else if (Product == "ONSTAR")
            {
                Goal = 98; //Dato de Oscar // 97 por estacion
                GoalRolado = 86.81;
                string[] BATTTESTONSTAR = { "BATTTEST", "98", "CCN_SEMI", "CCN_BATTTEST_3160412,CCN_BATTTEST_3160411" };
                string[] ICTONSTAR = { "ICT", "98", "CCN_SEMI", "CCN_ICT_3160111,CCN_ICT_3160112" };
                string[] SWLONSTAR = { "SWL", "98", "CCN_BE2_FIN", "CCN_SWL_3160811,CCN_SWL2_3160811,CCN_SWL_3160812,CCN_SWL2_3160812,CCN_SWL3_3160812,CCN_SWL3_3160811" };
                string[] EOLONSTAR = { "EOL", "98", "CCN_BE2_FIN", "CCN_PRUFIN_3160911,CCN_PRUFIN2_3160911,CCN_PRUFIN3_3160911,CCN_PRUFIN_3160912,CCN_PRUFIN2_3160912,CCN_PRUFIN3_3160912" };
                string[] RFTONSTAR = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT_3161011,CCN_RFT_3161012" };
                string[] TLCONSTAR = { "TLC", "98", "CCN_BE2_FIN", "CCN_TLC1_1_3161211,CCN_TLC1_2_3161211,CCN_TLC1_3_3161211,CCN_TLC1_4_3161211,CCN_TLC2_1_3161211,CCN_TLC2_2_3161211,CCN_TLC2_3_3161211,CCN_TLC2_4_3161211,CCN_TLC3_1_3161211,CCN_TLC3_2_3161211,CCN_TLC3_3_3161211,CCN_TLC3_4_3161211,CCN_TLC4_1_3161211,CCN_TLC4_2_3161211,CCN_TLC4_3_3161211,CCN_TLC4_4_3161211,CCN_TLC5_1_3161211,CCN_TLC5_2_3161211,CCN_TLC5_3_3161211,CCN_TLC5_4_3161211,CCN_TLC6_1_3161211,CCN_TLC6_2_3161211,CCN_TLC6_3_3161211,CCN_TLC6_4_3161211,CCN_TLC1_1_3161212,CCN_TLC1_2_3161212,CCN_TLC1_3_3161212,CCN_TLC1_4_3161212,CCN_TLC2_1_3161212,CCN_TLC2_2_3161212,CCN_TLC2_3_3161212,CCN_TLC2_4_3161212,CCN_TLC3_1_3161212,CCN_TLC3_2_3161212,CCN_TLC3_3_3161212,CCN_TLC3_4_3161212,CCN_TLC4_1_3161212,CCN_TLC4_2_3161212,CCN_TLC4_3_3161212,CCN_TLC4_4_3161212,CCN_TLC5_1_3161212,CCN_TLC5_2_3161212,CCN_TLC5_3_3161212,CCN_TLC5_4_3161212,CCN_TLC6_1_3161212,CCN_TLC6_2_3161212,CCN_TLC6_3_3161212,CCN_TLC6_4_3161212" };
                string[] PINCHECKONSTAR = { "PINCHECK", "98", "CCN_BE2_FIN", "CCN_CHKPIN_LA_3161321,CCN_CHKPIN_LB_3161321" };

                string[][] ArregloONSTAR = { BATTTESTONSTAR, ICTONSTAR, SWLONSTAR, EOLONSTAR, RFTONSTAR, TLCONSTAR, PINCHECKONSTAR };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "ONSTAR_L1")
            {
                Goal = 98; //Dato de Oscar // 97 por estacion
                GoalRolado = 86.81;
                string[] BATTTESTONSTAR = { "BATTTEST", "98", "CCN_SEMI", "CCN_BATTTEST_3160411" };
                string[] ICTONSTAR = { "ICT", "98", "CCN_SEMI", "CCN_ICT_3160111" };
                string[] SWLONSTAR = { "SWL", "98", "CCN_BE2_FIN", "CCN_SWL_3160811,CCN_SWL2_3160811,CCN_SWL3_3160811" };
                string[] EOLONSTAR = { "EOL", "98", "CCN_BE2_FIN", "CCN_PRUFIN_3160911,CCN_PRUFIN2_3160911,CCN_PRUFIN3_3160911" };
                string[] RFTONSTAR = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT_3161011" };
                string[] TLCONSTAR = { "TLC", "98", "CCN_BE2_FIN", "CCN_TLC1_1_3161211,CCN_TLC1_2_3161211,CCN_TLC1_3_3161211,CCN_TLC1_4_3161211,CCN_TLC2_1_3161211,CCN_TLC2_2_3161211,CCN_TLC2_3_3161211,CCN_TLC2_4_3161211,CCN_TLC3_1_3161211,CCN_TLC3_2_3161211,CCN_TLC3_3_3161211,CCN_TLC3_4_3161211,CCN_TLC4_1_3161211,CCN_TLC4_2_3161211,CCN_TLC4_3_3161211,CCN_TLC4_4_3161211,CCN_TLC5_1_3161211,CCN_TLC5_2_3161211,CCN_TLC5_3_3161211,CCN_TLC5_4_3161211,CCN_TLC6_1_3161211,CCN_TLC6_2_3161211,CCN_TLC6_3_3161211,CCN_TLC6_4_3161211" };
                string[] PINCHECKONSTAR = { "PINCHECK", "98", "CCN_BE2_FIN", "CCN_CHKPIN_LB_3161321" };

                string[][] ArregloONSTAR = { BATTTESTONSTAR, ICTONSTAR, SWLONSTAR, EOLONSTAR, RFTONSTAR, TLCONSTAR, PINCHECKONSTAR };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "ONSTAR_L2")
            {
                Goal = 98; //Dato de Oscar // 97 por estacion
                GoalRolado = 86.81;
                string[] BATTTESTONSTAR = { "BATTTEST", "98", "CCN_SEMI", "CCN_BATTTEST_3160412" };
                string[] ICTONSTAR = { "ICT", "98", "CCN_SEMI", "CCN_ICT_3160112" };
                string[] SWLONSTAR = { "SWL", "98", "CCN_BE2_FIN", "CCN_SWL_3160812,CCN_SWL2_3160812,CCN_SWL3_3160812" };
                string[] EOLONSTAR = { "EOL", "98", "CCN_BE2_FIN", "CCN_PRUFIN_3160912,CCN_PRUFIN2_3160912,CCN_PRUFIN3_3160912" };
                string[] RFTONSTAR = { "RFT", "98", "CCN_BE2_FIN", ",CCN_RFT_3161012" };
                string[] TLCONSTAR = { "TLC", "98", "CCN_BE2_FIN", "CCN_TLC1_1_3161212,CCN_TLC1_2_3161212,CCN_TLC1_3_3161212,CCN_TLC1_4_3161212,CCN_TLC2_1_3161212,CCN_TLC2_2_3161212,CCN_TLC2_3_3161212,CCN_TLC2_4_3161212,CCN_TLC3_1_3161212,CCN_TLC3_2_3161212,CCN_TLC3_3_3161212,CCN_TLC3_4_3161212,CCN_TLC4_1_3161212,CCN_TLC4_2_3161212,CCN_TLC4_3_3161212,CCN_TLC4_4_3161212,CCN_TLC5_1_3161212,CCN_TLC5_2_3161212,CCN_TLC5_3_3161212,CCN_TLC5_4_3161212,CCN_TLC6_1_3161212,CCN_TLC6_2_3161212,CCN_TLC6_3_3161212,CCN_TLC6_4_3161212" };
                string[] PINCHECKONSTAR = { "PINCHECK", "98", "CCN_BE2_FIN", "CCN_CHKPIN_LA_3161321" };

                string[][] ArregloONSTAR = { BATTTESTONSTAR, ICTONSTAR, SWLONSTAR, EOLONSTAR, RFTONSTAR, TLCONSTAR, PINCHECKONSTAR };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "DCM_LTA")
            {
                Goal = 97.75; //Dato de Oscar // 97 por estacion
                GoalRolado = 91.3;
                string[] ICTLTA = { "ICT", "96", "CCN_SEMI", "AN_ICT_3020112" };
                string[] FLASHLTA = { "FLASH", "96", "SMD_MOPS", "AN_FLASH_3020212" };
                string[] EOLLTA = { "EOL", "95", "CCN_SEMI", "AN_EOL1_3010911,AN_EOL2_3010911" };
                string[] PINCKLTA = { "PINCK", "95", "CCN_SEMI", ",AN_PINCHK_3011311" };

                string[][] ArregloONSTAR = { ICTLTA, FLASHLTA, EOLLTA, PINCKLTA };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "DCM_LTA_FUN_MEC")
            {
                Goal = 97.75; //Dato de Oscar // 97 por estacion
                GoalRolado = 91.3;
                string[] ICTLTA = { "ICT", "96", "CCN_SEMI", "AN_ICT_3020112" };
                string[] FLASHLTA = { "FLASH", "96", "SMD_MOPS", "AN_FLASH_3020212" };
                string[] PININSLTA = { "PININSERTION", "95", "CCN_SEMI", "AN_PIN_3022721" };
                string[] COAT1LTA = { "COAT1", "95", "CCN_SEMI", "AN_COAT_3022512" };
                string[] COAT2LTA = { "COAT2", "95", "CCN_SEMI", "AN_COAT_3022522" };
                string[] SNAPLTA = { "SNAPCHECK", "95", "CCN_SEMI", "AN_SNAP_3012411" };
                string[] EOLLTA = { "EOL", "95", "CCN_SEMI", "AN_EOL1_3010911,AN_EOL2_3010911" };
                string[] PINCKLTA = { "PINCK", "95", "CCN_SEMI", ",AN_PINCHK_3011311" };

                string[][] ArregloONSTAR = { ICTLTA, FLASHLTA, PININSLTA, COAT1LTA, COAT2LTA, SNAPLTA, EOLLTA, PINCKLTA };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "HYUNDAI_DCU")
            {
                Goal = 97;
                GoalRolado = 85;
                string[] ICTHYUNDAI_DCU = { "ICT", "97", "CCN_SEMI", "AN_ICT_3250111" };
                string[] FLASHHYUNDAI_DCU = { "FLASH", "97", "SMD_MOPS", "AN_FLASH_3250211" };
                string[] SWLHYUNDAI_DCU = { "SWL", "97", "CCN_BE1_FIN", "AN_SWL1_3250811,AN_SWL2_3250811" };
                string[] EOLHYUNDAI_DCU = { "EOL", "97", "CCN_BE1_FIN", "AN_RFT1_3250911,AN_RFT2_3250911" };
                string[] PINCKHYUNDAI_DCU = { "TLC", "97", "CCN_BE1_FIN", "AN_TLC1_3251211,AN_TLC2_3251211,AN_TLC3_3251211" };

                string[][] ArregloHYUNDAI_DCU = { ICTHYUNDAI_DCU, FLASHHYUNDAI_DCU, SWLHYUNDAI_DCU, EOLHYUNDAI_DCU, PINCKHYUNDAI_DCU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloHYUNDAI_DCU).ToArray();
            }
            else if (Product == "HYUNDAI_DCU_FUN_MEC")
            {
                Goal = 97;
                GoalRolado = 85;
                string[] ICTHYUNDAI_DCU = { "ICT", "97", "CCN_SEMI", "AN_ICT_3250111" };
                string[] FLASHHYUNDAI_DCU = { "FLASH", "97", "SMD_MOPS", "AN_FLASH_3250211" };
                string[] INSPHYUNDAI_DCU = { "INSP", "97", "CCN_BE1_FIN", "AN_INSP_3250521" };
                string[] SCREWHYUNDAI_DCU = { "SCREW", "97", "CCN_BE1_FIN", "AN_SCREW1_3250611" };
                string[] SWLHYUNDAI_DCU = { "SWL", "97", "CCN_BE1_FIN", "AN_SWL1_3250811,AN_SWL2_3250811" };
                string[] LABELHYUNDAI_DCU = { "LABEL", "97", "CCN_BE1_FIN", "AN_LABEL_3250731" };
                string[] EOLHYUNDAI_DCU = { "EOL", "97", "CCN_BE1_FIN", "AN_RFT1_3250911,AN_RFT2_3250911" };
                string[] TLCHYUNDAI_DCU = { "TLC", "97", "CCN_BE1_FIN", "AN_TLC1_3251211,AN_TLC2_3251211,AN_TLC3_3251211" };
                string[] PINCKHYUNDAI_DCU = { "PINCK", "97", "CCN_BE1_FIN", "AN_PINCHECK_3251311" };
                string[] BUBHYUNDAI_DCU = { "BUB", "97", "CCN_BE1_FIN", "AN_BUB_3251111" };

                string[][] ArregloHYUNDAI_DCU = { ICTHYUNDAI_DCU, FLASHHYUNDAI_DCU, INSPHYUNDAI_DCU, SCREWHYUNDAI_DCU, SWLHYUNDAI_DCU, LABELHYUNDAI_DCU, EOLHYUNDAI_DCU, TLCHYUNDAI_DCU, PINCKHYUNDAI_DCU, BUBHYUNDAI_DCU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloHYUNDAI_DCU).ToArray();
            }



            if (Process != "")
            {

                arregloDeArreglosDeCOMCCELLSoProcesos = FiltrarPorClave(Process, arregloDeArreglosDeCOMCCELLSoProcesos);
            }


            return (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado);
        }

        public Top10Final GetDataSpecifict(string Seriales, DateTime FromDate, DateTime ToDate)
        {
            string[] arregloSeriales = new string[0];
            arregloSeriales = Seriales.Split(",");

            int blockSize = 3;

            // Procesa el arreglo en bloques
            for (int i = 0; i < arregloSeriales.Length; i += blockSize)
            {
                // Obtiene el bloque actual
                int currentBlockSize = Math.Min(blockSize, arregloSeriales.Length - i);
                string[] currentBlock = new string[currentBlockSize];
                Array.Copy(arregloSeriales, i, currentBlock, 0, currentBlockSize);

                string stringSeriesCiclo = String.Join(",", Array.ConvertAll(currentBlock, item => $"{item}"));
                string query = MesQueryFabric.QueryForLastUbicationBySerialID(stringSeriesCiclo);
                DataTable queryResult = dbContext.RunQuery(query);

            }








            Top10Final resultadovacio = new Top10Final();

            List<SpecifictTestModel> resultadoFIN = new List<SpecifictTestModel>();
            List<SpecifictTestModel> resultadoTemp = new List<SpecifictTestModel>();
            DateTime[] ArregloDeDiasBuscados = ObtenerFechasEnRango(FromDate, ToDate);
            foreach (DateTime dateTime in ArregloDeDiasBuscados)
            {
                string query = MesQueryFabric.QueryForSpecifictTest(dateTime, dateTime.AddDays(1));
                DataTable queryResult = dbContext.RunQuery(query);
                resultadoTemp = DataTableHelper.DataTableSpecifictdata(queryResult);
                resultadoFIN.AddRange(resultadoTemp);
                Console.WriteLine(dateTime);

            }
            return resultadovacio;
        }
        public TopOffenderFinal GetDataTopOffenders(string Product, string Fecha, string Process, string Estacion, string Day, int TypeSearch, string fromDateStr, string toDateStr)
        {
          
            TopOffenderFinal resultadoFinal = new TopOffenderFinal();
            DateTime FromDate = new DateTime();
            DateTime ToDate = new DateTime();
            if (TypeSearch == 1)
            {
                var Fechas = GetWeekDates(Fecha.ToUpper());
                FromDate = Fechas.FromDate;
                ToDate = Fechas.ToDate;
            }if(TypeSearch == 2)
            {
                FromDate = DateTime.ParseExact(fromDateStr, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                ToDate = DateTime.ParseExact(toDateStr, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            string stringFromDate = FromDate.ToString("dd-MM-yyyy");
            string stringToDate = ToDate.ToString("dd-MM-yyyy");
            int contadorRenglones = 0;
            var (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado) = ObtenerEstacionesFPY(Product, Process);
            try
            {
                List<string> arregloDeGoalsPorProcesos = new List<string>();
                foreach (var RenglonArreglo in arregloDeArreglosDeCOMCCELLSoProcesos)
                {
                    arregloDeGoalsPorProcesos.Add(RenglonArreglo[0] + ": " + RenglonArreglo[1] + "%");
                    List<HistoryRow> resultado = new List<HistoryRow>();
                    string estaciones = RenglonArreglo[3];
                    string[] arregloComcells = new string[0];

                    if(Estacion == "0")
                    {
                        if (estaciones.Contains(","))
                        {

                            arregloComcells = estaciones.Split(",");
                        }
                        else
                        {
                            arregloComcells = new string[] { RenglonArreglo[3] };
                        }
                    }
                    else
                    {
                        arregloComcells = new string[] { Estacion }; 
                    }

                    DateTime[] ArregloDeDiasBuscados = new DateTime[0];
                    if (Day != "0")
                    {
                        
                        DateTime DAY = DateTime.ParseExact(Day, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        ArregloDeDiasBuscados = new DateTime[] { DAY };
                    }
                    else
                    {
                        ArregloDeDiasBuscados = ObtenerFechasEnRango(FromDate, ToDate);
                    }

                    foreach (DateTime dateTime in ArregloDeDiasBuscados)
                    {
                        foreach (string comccell in arregloComcells)
                        {
                            List<HistoryRow> resultadoTemp = new List<HistoryRow>();
                            string query = MesQueryFabric.QueryForFailsFPYHistory(RenglonArreglo[0], comccell, dateTime, dateTime.AddDays(1));
                            DataTable queryResult = dbContext.RunQuery(query);
                            resultadoTemp = DataTableHelper.DataTableToFPYdataTopOffender(queryResult);
                            resultado.AddRange(resultadoTemp);
                        }
                    }

                    List<TopOffendersFPY> TopOffenderByDescription = new List<TopOffendersFPY>();

                    resultado.RemoveAll(row => row.job.IndexOf("Q_", StringComparison.OrdinalIgnoreCase) != -1 || row.job.IndexOf("GOLDEN", StringComparison.OrdinalIgnoreCase) != -1);

                    contadorRenglones = resultado.Count();

                    var counts = resultado.GroupBy(row => new { row.testid, row.description })
                                         .Select(group => new TopOffendersFPY
                                         {
                                             testid = group.Key.testid,
                                             description = group.Key.description,
                                             Count = group.Count().ToString(),
                                             porcentaje = Math.Round(((double)group.Count() / contadorRenglones) * 100, 2),
                                             TotalCount = contadorRenglones
                                         })
                                         .OrderByDescending(item => int.Parse(item.Count));

                    TopOffenderByDescription.AddRange(counts);

                    resultadoFinal = new TopOffenderFinal()
                    {
                        HistoryRows = resultado,
                        TopOffenderByDescription = TopOffenderByDescription,
                    };
                }
            }
            catch
            {

            }
                return resultadoFinal;
        }
        
        public async Task<(Response, bool)> GetFPYData(string Product, DateTime FromDate, DateTime ToDate, string Week, int TypeSearch)
        {
            string DayWeek = "";
            bool RespuestaEsperada = true;
            Response resultadoFinal = new Response();
            DateTime[] arregloDeFechas = ObtenerFechasEnRango(FromDate, ToDate);

            //ReportesSemanaCompleta
            List<HistoryModel> resultadoFinalHistorial = new List<HistoryModel>();
            List<HistoryModel> resultadoFinalHistorialRunAndRate = new List<HistoryModel>();
            List<ReportFPYByProcessFinal> resultadoReporteFPYbyProcess = new List<ReportFPYByProcessFinal>();
            List<ReportFPYByProcessAndStationFinal> resultadoReporteFPYbyProcessAndStation = new List<ReportFPYByProcessAndStationFinal>();
            List<ReportFPYByProcessAndModelFinal> resultadoReporteFPYbyProcessAndModel = new List<ReportFPYByProcessAndModelFinal>();

            //ReportesPorDia
            List<ReportFPYByProcessAndDayFinal> resultadoReporteFPYbyProcessAndDay = new List<ReportFPYByProcessAndDayFinal>();
            List<ReportFPYByProcessAndStationAndDayFinal> resultadoReporteFPYbyProcessAndStationAndDay = new List<ReportFPYByProcessAndStationAndDayFinal>();

            //ReportesPorDiaHora
            List<ReportFPYByProcessAndDayHourFinal> resultadoReporteFPYbyProcessAndDayHour = new List<ReportFPYByProcessAndDayHourFinal>();
            List<ReportFPYByProcessAndStationDayHourFinal> resultadoReporteFPYbyProcessAndStationAndDayHour = new List<ReportFPYByProcessAndStationDayHourFinal>();

      
            string fromDateAsString = FromDate.ToString("dd-MM-yyyy HH:mm:ss"); ; // Convierte FromDate a cadena
            string toDateAsString = ToDate.ToString("dd-MM-yyyy HH:mm:ss"); ;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Iniciar el cronómetro
            var (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado) = ObtenerEstacionesFPY(Product, "");
            bool breakAll = false;  // Variable para controlar el break en el bucle externo
            List<string> arregloDeGoalsPorProcesos = new List<string>();
                foreach (var RenglonArreglo in arregloDeArreglosDeCOMCCELLSoProcesos)
                {
                    bool breakOuterLoopFin = false;  // Variable para controlar el break en el bucle externo
                    arregloDeGoalsPorProcesos.Add(RenglonArreglo[0] + ": " + RenglonArreglo[1] + "%");
                    List<HistoryModel> resultado = new List<HistoryModel>();
                    string estaciones = RenglonArreglo[3];
                    string[] arregloComcells = new string[0];
                    if (estaciones.Contains(","))
                    {
                        
                        arregloComcells = estaciones.Split(",");
                    }
                    else
                    {
                        arregloComcells = new string[] { RenglonArreglo[3] };
                    }

                    DateTime[] ArregloDeDiasBuscados = ObtenerFechasEnRango(FromDate, ToDate);
                foreach (DateTime dateTime in ArregloDeDiasBuscados)
                    {
                        bool breakOuterLoop = false;  // Variable para controlar el break en el bucle externo

                        foreach (string comccell in arregloComcells)
                        {
                            List<HistoryModel> resultadoTemp = new List<HistoryModel>();
                            string query = MesQueryFabric.QueryForPassAndFailsFPY(RenglonArreglo[0], comccell, dateTime, dateTime.AddDays(1));
                            (DataTable queryResult, int success) = dbContext.RunQueryFPY(query, Product + "- byweek - " + Week +  " - " + fromDateAsString + " - " + toDateAsString);

                            if (success == 1)
                            {
                                resultadoTemp = DataTableHelper.DataTableToFPYdata(queryResult);
                                resultado.AddRange(resultadoTemp);
                            }
                            else
                            {
                                breakOuterLoop = true;  // Establece la bandera para romper el bucle externo
                                break;  // Rompe el bucle interno
                            }
                        }

                        if (breakOuterLoop)
                        {
                            breakOuterLoopFin = true;
                            break;
                        }
                    }
                    if (breakOuterLoopFin)
                    {
                    breakAll = true;
                         RespuestaEsperada = false;
                        break;
                    }
                    else
                    {
                    
                    var (historialFiltrado, historialRegistrosGolden) = FiltrarPorOrden(resultado, Product, RenglonArreglo[2]);
                        List<HistoryModel> resultadoFiltradoGolden = historialFiltrado;
                        List<HistoryModel> resultadoOrdenado = OrdenarPorFechaYHoraAscendente(resultadoFiltradoGolden);
                        List<HistoryModel> resultadoSinDuplicados = EliminarDuplicadosPorSerial(resultadoOrdenado);
                        List<HistoryModel> resultadoSinGoldensProductivas = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoSinDuplicados, historialRegistrosGolden);
                        List<HistoryModel> resultadoSinGoldensProductivasRunAndRate = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoOrdenado, historialRegistrosGolden);
                        resultadoFinalHistorial.AddRange(resultadoSinGoldensProductivas);
                        resultadoFinalHistorialRunAndRate.AddRange(resultadoSinGoldensProductivasRunAndRate);

                        //Creacion de reportes porsemana
                        List<ReportFPYByProcessAndStationFinal> reportFPYByProcessAndStations = CrearReportesFPYbyProcessAndStation(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0], RenglonArreglo[1]);
                        resultadoReporteFPYbyProcessAndStation.AddRange(reportFPYByProcessAndStations);

                        double roladoDelProceso = CalcularRoladoPorProceso(reportFPYByProcessAndStations);
                        ReportFPYByProcessFinal reportFPYByProcess = CrearReporteFPYbyProcess(resultadoSinGoldensProductivas, RenglonArreglo[0], resultadoSinGoldensProductivasRunAndRate, roladoDelProceso);
                        resultadoReporteFPYbyProcess.Add(reportFPYByProcess);
                        List<ReportFPYByProcessAndModelFinal> reportFPYByProcessAndModels = CrearReportesFPYbyProcessAndModel(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0]);
                        resultadoReporteFPYbyProcessAndModel.AddRange(reportFPYByProcessAndModels);

                        //Creacion de reportes por dia
                        List<ReportFPYByProcessAndStationAndDayFinal> reportFPYBYProcessAndStationsAndDay = CrearReportesFPYbyProcessAndStationAndDay(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0]);
                        resultadoReporteFPYbyProcessAndStationAndDay.AddRange(reportFPYBYProcessAndStationsAndDay);
                        List<ReportFPYByProcessAndDayFinal> reportFPYBYProcessAndDay = CrearReporteFPYbyProcessAndDay(resultadoSinGoldensProductivas, RenglonArreglo[0], resultadoSinGoldensProductivasRunAndRate, arregloDeFechas);
                        resultadoReporteFPYbyProcessAndDay.AddRange(reportFPYBYProcessAndDay);

                        //Creacion de reportes por hora
                        List<ReportFPYByProcessAndStationDayHourFinal> reportFPYBYProcessAndStationsAndDayHour = CrearReportesFPYbyProcessAndStationAndDayHour(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0], arregloDeFechas);
                        resultadoReporteFPYbyProcessAndStationAndDayHour.AddRange(reportFPYBYProcessAndStationsAndDayHour);
                        List<ReportFPYByProcessAndDayHourFinal> reportFPYBYProcessAndDayAndHour = CrearReporteFPYbyProcessAndDayHour(resultadoSinGoldensProductivas, RenglonArreglo[0], resultadoSinGoldensProductivasRunAndRate, arregloDeFechas);
                        resultadoReporteFPYbyProcessAndDayHour.AddRange(reportFPYBYProcessAndDayAndHour);
                    }
                }

                if (!breakAll)
                {
                
                var (rolado, roladoRunAndRate) = CalcularRolado(resultadoReporteFPYbyProcess);
                ReportFPY reporeteFPY = CrearReporteFPY(resultadoFinalHistorial, rolado, resultadoFinalHistorialRunAndRate, roladoRunAndRate);
                stopwatch.Stop();
                var elapsed = stopwatch.Elapsed;
                string timeString = ($"{elapsed.Minutes}:{elapsed.Seconds}");

                resultadoFinal = new Response()
                {
                    ReportFPY = reporeteFPY,
                    ReportFPYByProcess = resultadoReporteFPYbyProcess.OrderBy(f => f.FPY).ToList(),
                    ReportFPYByProcessAndStation = resultadoReporteFPYbyProcessAndStation,
                    ReportFPYByProcessAndModel = resultadoReporteFPYbyProcessAndModel,
                    ReportFPYBYProcessAndDay = resultadoReporteFPYbyProcessAndDay,
                    ReportFPYBYProcessAndStationsAndDay = resultadoReporteFPYbyProcessAndStationAndDay,
                    ReportFPYBYProcessAndDayAndHour = resultadoReporteFPYbyProcessAndDayHour,
                    ReportFPYBYProcessAndStationsAndDayHour = resultadoReporteFPYbyProcessAndStationAndDayHour,
                    Data = resultadoFinalHistorial,
                    DataRunAndRate = resultadoFinalHistorialRunAndRate,
                    Goal = Goal,
                    GoalRolado = GoalRolado,
                    ArregloDeGoalsPorProceso = arregloDeGoalsPorProcesos,
                    timeElapsed = timeString,
                    Product = Product,
                    Week = Week,
                    PeriodoBuscado = "From date: " + fromDateAsString + " to date " + toDateAsString,
                    TypeSearch = TypeSearch,
                    FromDate = fromDateAsString,
                    ToDate = toDateAsString,
                };
            }
                

            
            return (resultadoFinal, RespuestaEsperada);
        }

        public async Task<(Response, bool)> GetFPYDatabyDay(string Product, DateTime FromDate, DateTime ToDate, int TypeSearch)
        {
            bool RespuestaEsperada = true;
            Response resultadoFinal = new Response();
            DateTime[] arregloDeFechas = ObtenerFechasEnRango(FromDate, ToDate);

            //ReportesSemanaCompleta
            List<HistoryModel> resultadoFinalHistorial = new List<HistoryModel>();
            List<HistoryModel> resultadoFinalHistorialRunAndRate = new List<HistoryModel>();
            List<ReportFPYByProcessFinal> resultadoReporteFPYbyProcess = new List<ReportFPYByProcessFinal>();
            List<ReportFPYByProcessAndStationFinal> resultadoReporteFPYbyProcessAndStation = new List<ReportFPYByProcessAndStationFinal>();
            List<ReportFPYByProcessAndModelFinal> resultadoReporteFPYbyProcessAndModel = new List<ReportFPYByProcessAndModelFinal>();

            //ReportesPorDia
            List<ReportFPYByProcessAndDayFinal> resultadoReporteFPYbyProcessAndDay = new List<ReportFPYByProcessAndDayFinal>();
            List<ReportFPYByProcessAndStationAndDayFinal> resultadoReporteFPYbyProcessAndStationAndDay = new List<ReportFPYByProcessAndStationAndDayFinal>();

            //ReportesPorDiaHora
            List<ReportFPYByProcessAndDayHourFinal> resultadoReporteFPYbyProcessAndDayHour = new List<ReportFPYByProcessAndDayHourFinal>();
            List<ReportFPYByProcessAndStationDayHourFinal> resultadoReporteFPYbyProcessAndStationAndDayHour = new List<ReportFPYByProcessAndStationDayHourFinal>();


            string fromDateAsString = FromDate.ToString("dd-MM-yyyy HH:mm:ss"); ; // Convierte FromDate a cadena
            DateTime ToDateConHoras = ToDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            string toDateAsStringConHoras = ToDateConHoras.ToString("dd-MM-yyyy HH:mm:ss");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Iniciar el cronómetro
            var (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado) = ObtenerEstacionesFPY(Product, "");
            bool breakAll = false;  // Variable para controlar el break en el bucle externo
            List<string> arregloDeGoalsPorProcesos = new List<string>();
            foreach (var RenglonArreglo in arregloDeArreglosDeCOMCCELLSoProcesos)
            {
                bool breakOuterLoopFin = false;  // Variable para controlar el break en el bucle externo
                arregloDeGoalsPorProcesos.Add(RenglonArreglo[0] + ": " + RenglonArreglo[1] + "%");
                List<HistoryModel> resultado = new List<HistoryModel>();
                string estaciones = RenglonArreglo[3];
                string[] arregloComcells = new string[0];
                if (estaciones.Contains(","))
                {

                    arregloComcells = estaciones.Split(",");
                }
                else
                {
                    arregloComcells = new string[] { RenglonArreglo[3] };
                }

                DateTime[] ArregloDeDiasBuscados = ObtenerFechasEnRango(FromDate, ToDate);
                foreach (DateTime dateTime in ArregloDeDiasBuscados)
                {
                    bool breakOuterLoop = false;  // Variable para controlar el break en el bucle externo

                    foreach (string comccell in arregloComcells)
                    {
                        List<HistoryModel> resultadoTemp = new List<HistoryModel>();
                        string query = MesQueryFabric.QueryForPassAndFailsFPY(RenglonArreglo[0], comccell, dateTime, dateTime.AddDays(1));
                        (DataTable queryResult, int success) = dbContext.RunQueryFPY(query, Product + "- byDay - " + fromDateAsString + " - " + toDateAsStringConHoras);

                        if (success == 1)
                        {
                            resultadoTemp = DataTableHelper.DataTableToFPYdata(queryResult);
                            resultado.AddRange(resultadoTemp);
                        }
                        else
                        {
                            breakOuterLoop = true;  // Establece la bandera para romper el bucle externo
                            break;  // Rompe el bucle interno
                        }
                    }

                    if (breakOuterLoop)
                    {
                        
                        breakOuterLoopFin = true;
                        break;
                    }
                }
                if (breakOuterLoopFin)
                {
                    breakAll = true;
                    RespuestaEsperada = false;
                    break;
                }
                else
                {
                    var (historialFiltrado, historialRegistrosGolden) = FiltrarPorOrden(resultado, Product, RenglonArreglo[2]);
                    List<HistoryModel> resultadoFiltradoGolden = historialFiltrado;
                    List<HistoryModel> resultadoOrdenado = OrdenarPorFechaYHoraAscendente(resultadoFiltradoGolden);
                    List<HistoryModel> resultadoSinDuplicados = EliminarDuplicadosPorSerial(resultadoOrdenado);
                    List<HistoryModel> resultadoSinGoldensProductivas = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoSinDuplicados, historialRegistrosGolden);
                    List<HistoryModel> resultadoSinGoldensProductivasRunAndRate = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoOrdenado, historialRegistrosGolden);
                    resultadoFinalHistorial.AddRange(resultadoSinGoldensProductivas);
                    resultadoFinalHistorialRunAndRate.AddRange(resultadoSinGoldensProductivasRunAndRate);

                    //Creacion de reportes porsemana
                    List<ReportFPYByProcessAndStationFinal> reportFPYByProcessAndStations = CrearReportesFPYbyProcessAndStation(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0], RenglonArreglo[1]);
                    resultadoReporteFPYbyProcessAndStation.AddRange(reportFPYByProcessAndStations);

                    double roladoDelProceso = CalcularRoladoPorProceso(reportFPYByProcessAndStations);
                    ReportFPYByProcessFinal reportFPYByProcess = CrearReporteFPYbyProcess(resultadoSinGoldensProductivas, RenglonArreglo[0], resultadoSinGoldensProductivasRunAndRate, roladoDelProceso);
                    resultadoReporteFPYbyProcess.Add(reportFPYByProcess);
                    List<ReportFPYByProcessAndModelFinal> reportFPYByProcessAndModels = CrearReportesFPYbyProcessAndModel(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0]);
                    resultadoReporteFPYbyProcessAndModel.AddRange(reportFPYByProcessAndModels);

                    //Creacion de reportes por dia
                    List<ReportFPYByProcessAndStationAndDayFinal> reportFPYBYProcessAndStationsAndDay = CrearReportesFPYbyProcessAndStationAndDay(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0]);
                    resultadoReporteFPYbyProcessAndStationAndDay.AddRange(reportFPYBYProcessAndStationsAndDay);
                    List<ReportFPYByProcessAndDayFinal> reportFPYBYProcessAndDay = CrearReporteFPYbyProcessAndDay(resultadoSinGoldensProductivas, RenglonArreglo[0], resultadoSinGoldensProductivasRunAndRate, arregloDeFechas);
                    resultadoReporteFPYbyProcessAndDay.AddRange(reportFPYBYProcessAndDay);

                    //Creacion de reportes por hora
                    List<ReportFPYByProcessAndStationDayHourFinal> reportFPYBYProcessAndStationsAndDayHour = CrearReportesFPYbyProcessAndStationAndDayHour(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, RenglonArreglo[0], arregloDeFechas);
                    resultadoReporteFPYbyProcessAndStationAndDayHour.AddRange(reportFPYBYProcessAndStationsAndDayHour);
                    List<ReportFPYByProcessAndDayHourFinal> reportFPYBYProcessAndDayAndHour = CrearReporteFPYbyProcessAndDayHour(resultadoSinGoldensProductivas, RenglonArreglo[0], resultadoSinGoldensProductivasRunAndRate, arregloDeFechas);
                    resultadoReporteFPYbyProcessAndDayHour.AddRange(reportFPYBYProcessAndDayAndHour);
                }
            }

            if (!breakAll)
            {
                var (rolado, roladoRunAndRate) = CalcularRolado(resultadoReporteFPYbyProcess);
                ReportFPY reporeteFPY = CrearReporteFPY(resultadoFinalHistorial, rolado, resultadoFinalHistorialRunAndRate, roladoRunAndRate);
                stopwatch.Stop();
                var elapsed = stopwatch.Elapsed;
                string timeString = ($"{elapsed.Minutes}:{elapsed.Seconds}");

                resultadoFinal = new Response()
                {
                    ReportFPY = reporeteFPY,
                    ReportFPYByProcess = resultadoReporteFPYbyProcess.OrderBy(f => f.FPY).ToList(),
                    ReportFPYByProcessAndStation = resultadoReporteFPYbyProcessAndStation,
                    ReportFPYByProcessAndModel = resultadoReporteFPYbyProcessAndModel,
                    ReportFPYBYProcessAndDay = resultadoReporteFPYbyProcessAndDay,
                    ReportFPYBYProcessAndStationsAndDay = resultadoReporteFPYbyProcessAndStationAndDay,
                    ReportFPYBYProcessAndDayAndHour = resultadoReporteFPYbyProcessAndDayHour,
                    ReportFPYBYProcessAndStationsAndDayHour = resultadoReporteFPYbyProcessAndStationAndDayHour,
                    Data = resultadoFinalHistorial,
                    DataRunAndRate = resultadoFinalHistorialRunAndRate,
                    Goal = Goal,
                    GoalRolado = GoalRolado,
                    ArregloDeGoalsPorProceso = arregloDeGoalsPorProcesos,
                    timeElapsed = timeString,
                    Product = Product,
                    PeriodoBuscado = "From date: " + fromDateAsString + " to date " + toDateAsStringConHoras,
                    TypeSearch = TypeSearch,
                    FromDate = fromDateAsString,
                    ToDate = toDateAsStringConHoras,
                };
            }



            return (resultadoFinal, RespuestaEsperada);
        }


        public async Task<(Response, bool)> GetDataByStation(string Familia, string Proceso, string Estacion, string IdType, DateTime FromDate, DateTime ToDate)
        {
            List<string> arregloDeGoalsPorProcesos = new List<string>();
            var (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado) = ObtenerEstacionesFPY(Familia, Proceso);
            DateTime[] arregloDeFechas = ObtenerFechasEnRango(FromDate, ToDate);
            string[] RenglonArreglo = arregloDeArreglosDeCOMCCELLSoProcesos[0];
            arregloDeGoalsPorProcesos.Add(RenglonArreglo[0] + ": " + RenglonArreglo[1] + "%");
            bool RespuestaEsperada = true;
            Response resultadoFinal = new Response();

            //ReportesSemanaCompleta
            List<HistoryModel> resultadoFinalHistorial = new List<HistoryModel>();
            List<HistoryModel> resultadoFinalHistorialRunAndRate = new List<HistoryModel>();
            List<ReportFPYByProcessFinal> resultadoReporteFPYbyProcess = new List<ReportFPYByProcessFinal>();
            List<ReportFPYByProcessAndStationFinal> resultadoReporteFPYbyProcessAndStation = new List<ReportFPYByProcessAndStationFinal>();
            List<ReportFPYByProcessAndModelFinal> resultadoReporteFPYbyProcessAndModel = new List<ReportFPYByProcessAndModelFinal>();

            //ReportesPorDia
            List<ReportFPYByProcessAndDayFinal> resultadoReporteFPYbyProcessAndDay = new List<ReportFPYByProcessAndDayFinal>();
            List<ReportFPYByProcessAndStationAndDayFinal> resultadoReporteFPYbyProcessAndStationAndDay = new List<ReportFPYByProcessAndStationAndDayFinal>();

            //ReportesPorDiaHora
            List<ReportFPYByProcessAndDayHourFinal> resultadoReporteFPYbyProcessAndDayHour = new List<ReportFPYByProcessAndDayHourFinal>();
            List<ReportFPYByProcessAndStationDayHourFinal> resultadoReporteFPYbyProcessAndStationAndDayHour = new List<ReportFPYByProcessAndStationDayHourFinal>();

            string fromDateAsString = FromDate.ToString("dd-MM-yyyy HH:mm:ss"); // Convierte FromDate a cadena
            DateTime ToDateConHoras = ToDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            string toDateAsStringConHoras = ToDateConHoras.ToString("dd-MM-yyyy HH:mm:ss");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Iniciar el cronómetro
            bool breakAll = false;  // Variable para controlar el break en el bucle externo
            List<HistoryModel> resultado = new List<HistoryModel>();
            bool breakOuterLoop = false;

            DateTime[] ArregloDeDiasBuscados = ObtenerFechasEnRango(FromDate, ToDate);
                foreach (DateTime dateTime in ArregloDeDiasBuscados)
                {
                        List<HistoryModel> resultadoTemp = new List<HistoryModel>();
                        string query = MesQueryFabric.QueryForPassAndFailsFPY(Proceso, Estacion, dateTime, dateTime.AddDays(1));
                        
                        (DataTable queryResult, int success) = dbContext.RunQueryFPY(query, Familia + "byStation FromDate:" + fromDateAsString + " ToDate: " + toDateAsStringConHoras);

                        if (success == 1)
                        {
                            resultadoTemp = DataTableHelper.DataTableToFPYdata(queryResult);
                            resultado.AddRange(resultadoTemp);
                        }
                        else
                        {
                            breakOuterLoop = true;
                            break;
                        }
                    

                    
                }
                if (!breakOuterLoop)
                {
                var (historialFiltrado, historialRegistrosGolden) = FiltrarPorOrden(resultado, Familia, IdType);
                    List<HistoryModel> resultadoFiltradoGolden = historialFiltrado;
                    List<HistoryModel> resultadoOrdenado = OrdenarPorFechaYHoraAscendente(resultadoFiltradoGolden);
                    List<HistoryModel> resultadoSinDuplicados = EliminarDuplicadosPorSerial(resultadoOrdenado);
                    List<HistoryModel> resultadoSinGoldensProductivas = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoSinDuplicados, historialRegistrosGolden);
                    List<HistoryModel> resultadoSinGoldensProductivasRunAndRate = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoOrdenado, historialRegistrosGolden);
                    resultadoFinalHistorial.AddRange(resultadoSinGoldensProductivas);
                    resultadoFinalHistorialRunAndRate.AddRange(resultadoSinGoldensProductivasRunAndRate);

                    //Creacion de reportes porsemana
                    List<ReportFPYByProcessAndStationFinal> reportFPYByProcessAndStations = CrearReportesFPYbyProcessAndStation(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, Proceso, RenglonArreglo[1]);
                    resultadoReporteFPYbyProcessAndStation.AddRange(reportFPYByProcessAndStations);

                    double roladoDelProceso = CalcularRoladoPorProceso(reportFPYByProcessAndStations);
                    ReportFPYByProcessFinal reportFPYByProcess = CrearReporteFPYbyProcess(resultadoSinGoldensProductivas, Proceso, resultadoSinGoldensProductivasRunAndRate, roladoDelProceso);
                    resultadoReporteFPYbyProcess.Add(reportFPYByProcess);
                    List<ReportFPYByProcessAndModelFinal> reportFPYByProcessAndModels = CrearReportesFPYbyProcessAndModel(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, Proceso);
                    resultadoReporteFPYbyProcessAndModel.AddRange(reportFPYByProcessAndModels);

                    //Creacion de reportes por dia
                    List<ReportFPYByProcessAndStationAndDayFinal> reportFPYBYProcessAndStationsAndDay = CrearReportesFPYbyProcessAndStationAndDay(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, Proceso);
                    resultadoReporteFPYbyProcessAndStationAndDay.AddRange(reportFPYBYProcessAndStationsAndDay);
                    List<ReportFPYByProcessAndDayFinal> reportFPYBYProcessAndDay = CrearReporteFPYbyProcessAndDay(resultadoSinGoldensProductivas, Proceso, resultadoSinGoldensProductivasRunAndRate, arregloDeFechas);
                    resultadoReporteFPYbyProcessAndDay.AddRange(reportFPYBYProcessAndDay);

                    //Creacion de reportes por hora
                    List<ReportFPYByProcessAndStationDayHourFinal> reportFPYBYProcessAndStationsAndDayHour = CrearReportesFPYbyProcessAndStationAndDayHour(resultadoSinGoldensProductivas, resultadoSinGoldensProductivasRunAndRate, Proceso, arregloDeFechas);
                    resultadoReporteFPYbyProcessAndStationAndDayHour.AddRange(reportFPYBYProcessAndStationsAndDayHour);
                    List<ReportFPYByProcessAndDayHourFinal> reportFPYBYProcessAndDayAndHour = CrearReporteFPYbyProcessAndDayHour(resultadoSinGoldensProductivas, Proceso, resultadoSinGoldensProductivasRunAndRate, arregloDeFechas);
                    resultadoReporteFPYbyProcessAndDayHour.AddRange(reportFPYBYProcessAndDayAndHour);

                    var (rolado, roladoRunAndRate) = CalcularRolado(resultadoReporteFPYbyProcess);
                    ReportFPY reporeteFPY = CrearReporteFPY(resultadoFinalHistorial, rolado, resultadoFinalHistorialRunAndRate, roladoRunAndRate);
                    stopwatch.Stop();
                    var elapsed = stopwatch.Elapsed;
                    string timeString = ($"{elapsed.Minutes}:{elapsed.Seconds}");

                    resultadoFinal = new Response()
                    {
                        ReportFPY = reporeteFPY,
                        ReportFPYByProcess = resultadoReporteFPYbyProcess.OrderBy(f => f.FPY).ToList(),
                        ReportFPYByProcessAndStation = resultadoReporteFPYbyProcessAndStation,
                        ReportFPYByProcessAndModel = resultadoReporteFPYbyProcessAndModel,
                        ReportFPYBYProcessAndDay = resultadoReporteFPYbyProcessAndDay,
                        ReportFPYBYProcessAndStationsAndDay = resultadoReporteFPYbyProcessAndStationAndDay,
                        ReportFPYBYProcessAndDayAndHour = resultadoReporteFPYbyProcessAndDayHour,
                        ReportFPYBYProcessAndStationsAndDayHour = resultadoReporteFPYbyProcessAndStationAndDayHour,
                        Data = resultadoFinalHistorial,
                        DataRunAndRate = resultadoFinalHistorialRunAndRate,
                        Goal = Goal,
                        GoalRolado = GoalRolado,
                        ArregloDeGoalsPorProceso = arregloDeGoalsPorProcesos,
                        timeElapsed = timeString,
                        Product = Familia,
                        PeriodoBuscado = "From date: " + fromDateAsString + " to date " + toDateAsStringConHoras,
                        TypeSearch = 2,
                        FromDate = fromDateAsString,
                        ToDate = toDateAsStringConHoras,
                    };
                }



            return (resultadoFinal, RespuestaEsperada);
        }
        
        private List<HistoryModel> FiltrarPorIDTYPE(List<HistoryModel> historial, string IDTYPE)
        {
            var historialFiltradoSinIDTYPE = historial.Where(h => h.ID_TYPE != IDTYPE).ToList();
            var historialFiltrado = historial.Where(h => h.ID_TYPE == IDTYPE).ToList();

            return historialFiltrado;
        }

        static bool ContieneSoloNumeros(string input)
        {
            // Patrón de expresión regular que coincide con cualquier carácter que no sea un número
            string patron = @"[^0-9]";

            // Si no hay coincidencias con el patrón, el string solo contiene números
            return !Regex.IsMatch(input, patron);
        }

        public List<HistoryModel> FiltrarPorModelo(List<HistoryModel> historial, string Producto)
        {
            List<HistoryModel> historialSeriesOtros = new List<HistoryModel>();
            List < HistoryModel > historialFiltrado = new List < HistoryModel >();
            foreach (HistoryModel renglonSerie in historial)
            {
                string serieMayusculas = renglonSerie.Serial_Number.ToUpper();

                
                if (!serieMayusculas.Contains("K") && Producto.Contains("GEN3"))
                {
                    historialFiltrado.Add(renglonSerie);
                }
                else if (serieMayusculas.Contains("K") && Producto == "FGEN1M")
                {
                    historialFiltrado.Add(renglonSerie);

                }
                else if (Producto == "FGEN1MR" || Producto == "FGEN1MR_Line_BE_4" || Producto == "FGEN1MR_Line_BE_5" || Producto == "FGEN1MR_Line_PCBA_3" || Producto == "FGEN1MR_Line_PCBA_4")
                {
                    bool ContieneNum = ContieneSoloNumeros(serieMayusculas);
                    if (ContieneNum)
                    {
                        historialFiltrado.Add(renglonSerie);
                    }else if(renglonSerie.Proceso == "RELAY TEST")
                    {
                        historialFiltrado.Add(renglonSerie);
                    }
                }
                else
                {
                    historialSeriesOtros.Add(renglonSerie);
                }
            }
            return historialFiltrado;
        }

        private (List<HistoryModel> historialFiltrado, List<HistoryModel> historialRegistrosGolden) FiltrarPorOrden(List<HistoryModel> historial, string Product, string IDTYPE)
        {
            List<HistoryModel> resultado = new List<HistoryModel>();

            if (Product.Contains("FGEN"))
            {
                List<HistoryModel> HistorialFiltradoPorModelo = FiltrarPorModelo(historial, Product);
                List<HistoryModel> HistorialFiltradoPorIDTYPE = FiltrarPorIDTYPE(HistorialFiltradoPorModelo, IDTYPE);
                resultado.AddRange(HistorialFiltradoPorIDTYPE);
            }
            else
            {
                resultado.AddRange(historial);
            }

            // Filtrar la lista por el valor "GOLDEN" en la columna ORDEN
            var historialFiltrado = resultado.Where(h => !h.ORDEN.ToUpper().Contains("GOLDEN")).ToList();
            var historialRegistrosGolden = resultado
                .Where(h => h.ORDEN.ToUpper().Contains("GOLDEN"))
                .GroupBy(h => h.Serial_Number)
                .Select(group => group.First())
                .ToList();

            return (historialFiltrado, historialRegistrosGolden);
        }

        private List<HistoryModel> FiltrarRegistrosGoldenConOrdenesProductivas(List<HistoryModel> resultadoSinDuplicados, List<HistoryModel> historialRegistrosGolden)
        {
            // Obtener los valores únicos de la columna Serial_Number en historialRegistrosGolden
            HashSet<string> serialNumbersGolden = new HashSet<string>(historialRegistrosGolden.Select(h => h.Serial_Number));

            // Filtrar resultadoSinDuplicados excluyendo los registros con Serial_Number que ya existen en historialRegistrosGolden
            List<HistoryModel> resultadoFiltrado = resultadoSinDuplicados
                .Where(h => !serialNumbersGolden.Contains(h.Serial_Number))
                .ToList();

            return resultadoFiltrado;
        }

        private List<HistoryModel> OrdenarPorFechaYHoraAscendente(List<HistoryModel> historialFiltrado)
        {
            // Ordenar la lista por EVENT_DATE y EVENT_HOUR de forma ascendente
            var historialOrdenado = historialFiltrado
    .OrderBy(h => h.EVENT_DATE)
    .ThenBy(h => h.EVENT_HOUR)
    .ToList();
            ;

            return historialOrdenado;
        }

        private List<HistoryModel> EliminarDuplicadosPorSerial(List<HistoryModel> historialOrdenado)
        {
            // Crear una nueva lista para almacenar elementos únicos por Serial_Number
            List<HistoryModel> historialSinDuplicados = new List<HistoryModel>();
            List<HistoryModel> historialDeDuplicados = new List<HistoryModel>();

            try
            {
                // Iterar sobre la lista ordenada y agregar elementos únicos por Serial_Number
                foreach (var item in historialOrdenado)
                {
                    if (!historialSinDuplicados.Any(h => h.Serial_Number == item.Serial_Number))
                    {
                        historialSinDuplicados.Add(item);
                    }
                    else
                    {
                        historialDeDuplicados.Add(item);
                    }


                }

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return historialSinDuplicados;
        }

        private ReportFPY CrearReporteFPY(List<HistoryModel> historialSinDuplicados, double rolado, List<HistoryModel> historialConDuplicados, double roladoRunAndRate)
        {
            ReportFPY reporteFPY = new ReportFPY();
            try
            {
                // Calcular los valores para el reporte FPY
                int totalProduced = historialSinDuplicados.Count(h => h.result == "P");
                int totalProducedRunAndRate = historialConDuplicados.Count(h => h.result == "P");
                int totalFailures = historialSinDuplicados.Count(h => h.result == "F");
                int totalFailuresRunAndRate = historialConDuplicados.Count(h => h.result == "F");
                int total = totalProduced + totalFailures;
                int totalRunAndRate = totalProducedRunAndRate + totalFailuresRunAndRate;
                double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;
                double fpyRunAndRate = total > 0 ? ((double)totalProducedRunAndRate / totalRunAndRate) * 100 : 0;
                double fpyrolado = rolado;
                double fpyroladoRunAndRate = roladoRunAndRate;

                // Crear un objeto ReportFPY con los valores calculados
                reporteFPY = new ReportFPY
                {
                    TotalProduced = totalProduced,
                    TotalProducedRunAndRate = totalProducedRunAndRate,
                    TotalFailures = totalFailures,
                    TotalFailuresRunAndRate = totalFailuresRunAndRate,
                    Total = total,
                    TotalRunAndRate = totalRunAndRate,
                    FPY = fpy,
                    FPYRunAndRate= fpyRunAndRate,
                    FPYRolado = fpyrolado,
                    FPYRolandoRunAndRate = fpyroladoRunAndRate
                };
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return reporteFPY;
        }

        private ReportFPYByProcessFinal CrearReporteFPYbyProcess(List<HistoryModel> historialSinDuplicados, string Process, List<HistoryModel> historialConDuplicados, double roladoDelProceso)
        {
            ReportFPYByProcessFinal reporteFPY = new ReportFPYByProcessFinal();
            try
            {
                // Calcular los valores para el reporte FPY
                int totalProduced = historialSinDuplicados.Count(h => h.result == "P");
                int totalProducedRunAndRate = historialConDuplicados.Count(h => h.result == "P");
                int totalFailures = historialSinDuplicados.Count(h => h.result == "F");
                int totalFailuresRunAndRate = historialConDuplicados.Count(h => h.result == "F");
                int total = totalProduced + totalFailures;
                int totalRunAndRate = totalProducedRunAndRate + totalFailuresRunAndRate;
                double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;
                double fpyRunAndRate = totalRunAndRate > 0 ? ((double)totalProducedRunAndRate / totalRunAndRate) * 100 : 0;

                // Crear un objeto ReportFPY con los valores calculados
                reporteFPY = new ReportFPYByProcessFinal
                {
                    Process = Process,
                    TotalProduced = totalProduced,
                    TotalProducedRunAndRate = totalProducedRunAndRate,
                    TotalFailures = totalFailures,
                    TotalFailuresRunAndRate = totalFailuresRunAndRate,
                    Total = total,
                    TotalRunAndRate = totalRunAndRate,
                    FPY = fpy,
                    FPYRunAndRate = fpyRunAndRate,
                    FPYRoladoProceso = roladoDelProceso
                };
            }catch(Exception ex) { Console.WriteLine(ex.Message); };
            return reporteFPY;
        }

        private List<ReportFPYByProcessAndDayFinal> CrearReporteFPYbyProcessAndDay(List<HistoryModel> historialSinDuplicados, string Process, List<HistoryModel> historialConDuplicados, DateTime[] arregloDeFechas)
        {
            List<ReportFPYByProcessAndDayFinal> resultado = new List<ReportFPYByProcessAndDayFinal>();
            try
            {
                foreach (var fecha in arregloDeFechas)
                {
                    string fechaString = fecha.ToString("yyyy-MM-dd HH:mm:ss");

                    ReportFPYByProcessAndDayFinal reporteFPY = new ReportFPYByProcessAndDayFinal();
                    string dateOnly = fecha.ToString("dd/MM/yyyy");
                    int totalProduced = historialSinDuplicados.Count(h => h.result == "P" && h.EVENT_DATE == dateOnly);
                    int totalProducedRunAndRate = historialConDuplicados.Count(h => h.result == "P" && h.EVENT_DATE == dateOnly);
                    int totalFailures = historialSinDuplicados.Count(h => h.result == "F" && h.EVENT_DATE == dateOnly);
                    int totalFailuresRunAndRate = historialConDuplicados.Count(h => h.result == "F" && h.EVENT_DATE == dateOnly);
                    int total = totalProduced + totalFailures;
                    int totalRunAndRate = totalProducedRunAndRate + totalFailuresRunAndRate;
                    double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;
                    double fpyRunAndRate = totalRunAndRate > 0 ? ((double)totalProducedRunAndRate / totalRunAndRate) * 100 : 0;

                    reporteFPY = new ReportFPYByProcessAndDayFinal
                    {
                        Process = Process,
                        Day = dateOnly,
                        TotalProduced = totalProduced,
                        TotalProducedRunAndRate = totalProducedRunAndRate,
                        TotalFailures = totalFailures,
                        TotalFailuresRunAndRate = totalFailuresRunAndRate,
                        Total = total,
                        TotalRunAndRate = totalRunAndRate,
                        FPY = fpy,
                        FPYRunAndRate = fpyRunAndRate,
                    };
                    resultado.Add(reporteFPY);
                };




                
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); };
            return resultado;
        }

        private List<ReportFPYByProcessAndDayHourFinal> CrearReporteFPYbyProcessAndDayHour(List<HistoryModel> historialSinDuplicados, string Process, List<HistoryModel> historialConDuplicados, DateTime[] arregloDeFechas)
        {
            List<ReportFPYByProcessAndDayHourFinal> resultado = new List<ReportFPYByProcessAndDayHourFinal>();
            try
            {
                foreach (var fecha in arregloDeFechas)
                {
                    for (int hora = 0; hora <= 24; hora++)
                    {
                        string fechaString = fecha.ToString("yyyy-MM-dd HH:mm:ss");

                        ReportFPYByProcessAndDayHourFinal reporteFPY = new ReportFPYByProcessAndDayHourFinal();
                        string dateOnly = fecha.ToString("dd/MM/yyyy");


                        int totalProduced = historialSinDuplicados.Count(h =>
                            h.result == "P" &&
                            h.EVENT_DATE == dateOnly &&
                            int.Parse(h.EVENT_HOUR.Split(':')[0]) == hora);

                        int totalProducedRunAndRate = historialConDuplicados.Count(h =>
                            h.result == "P" &&
                            h.EVENT_DATE == dateOnly &&
                            int.Parse(h.EVENT_HOUR.Split(':')[0]) == hora);

                        int totalFailures = historialSinDuplicados.Count(h =>
                            h.result == "F" &&
                            h.EVENT_DATE == dateOnly &&
                            int.Parse(h.EVENT_HOUR.Split(':')[0]) == hora);

                        int totalFailuresRunAndRate = historialConDuplicados.Count(h =>
                            h.result == "F" &&
                            h.EVENT_DATE == dateOnly &&
                            int.Parse(h.EVENT_HOUR.Split(':')[0]) == hora);

                        int total = totalProduced + totalFailures;
                        int totalRunAndRate = totalProducedRunAndRate + totalFailuresRunAndRate;
                        double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;
                        double fpyRunAndRate = totalRunAndRate > 0 ? ((double)totalProducedRunAndRate / totalRunAndRate) * 100 : 0;

                        reporteFPY = new ReportFPYByProcessAndDayHourFinal
                        {
                            Process = Process,
                            Day = dateOnly,
                            Hour = hora+":00",
                            TotalProduced = totalProduced,
                            TotalProducedRunAndRate = totalProducedRunAndRate,
                            TotalFailures = totalFailures,
                            TotalFailuresRunAndRate = totalFailuresRunAndRate,
                            Total = total,
                            TotalRunAndRate = totalRunAndRate,
                            FPY = fpy,
                            FPYRunAndRate = fpyRunAndRate,
                        };
                        resultado.Add(reporteFPY);
                        
                    }
                        
                };
                resultado.RemoveAll(item => item.Total == 0);





            }
            catch (Exception ex) { Console.WriteLine(ex.Message); };
            return resultado;
        }

        private List<ReportFPYByProcessAndStationFinal> CrearReportesFPYbyProcessAndStation(List<HistoryModel> historialSinDuplicados, List<HistoryModel> historialRunAndRate, string Process, string goalString)
        {
            double goalInt = Convert.ToDouble(goalString);
            List<ReportFPYByProcessAndStationFinal> resultado = new List<ReportFPYByProcessAndStationFinal>();
            List<ReportFPYByProcessAndStation> ReportFPYByProcessAndStation = new List<ReportFPYByProcessAndStation>();
            List<ReportFPYByProcessAndStation> ReportFPYProcessAndStatioRunAndRate = new List<ReportFPYByProcessAndStation>();
            try
            {
                // Agrupar por COMMCELL y calcular los valores para cada grupo
                ReportFPYByProcessAndStation = historialSinDuplicados
                    .GroupBy(h => h.COMMCELL)
                    .Select(grupo =>
                    {
                        int totalProduced = grupo.Count(h => h.result == "P");
                        int totalFailures = grupo.Count(h => h.result == "F");
                        int total = totalProduced + totalFailures;
                        double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;

                        return new ReportFPYByProcessAndStation
                        {
                            Process = Process,
                            COMMCELL = grupo.Key,
                            TotalProduced = totalProduced,
                            TotalFailures = totalFailures,
                            Total = total,
                            FPY = fpy
                        };
                    })
                    .OrderBy(h => h.FPY)
                    .ToList();
                ReportFPYProcessAndStatioRunAndRate = historialRunAndRate
                    .GroupBy(h => h.COMMCELL)
                    .Select(grupo =>
                    {
                        int totalProduced = grupo.Count(h => h.result == "P");
                        int totalFailures = grupo.Count(h => h.result == "F");
                        int total = totalProduced + totalFailures;
                        double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;

                        return new ReportFPYByProcessAndStation
                        {
                            Process = Process,
                            COMMCELL = grupo.Key,
                            TotalProduced = totalProduced,
                            TotalFailures = totalFailures,
                            Total = total,
                            FPY = fpy
                        };
                    })
                    .OrderBy(h => h.FPY)
                    .ToList();
                // Fusionar las dos listas en una sola lista de ReportFPYByProcessAndStationFinal
                resultado = ReportFPYByProcessAndStation
                    .Join(ReportFPYProcessAndStatioRunAndRate,
                          r1 => r1.COMMCELL,
                          r2 => r2.COMMCELL,
                          (r1, r2) => new ReportFPYByProcessAndStationFinal
                          {
                              Process = r1.Process,
                              COMMCELL = r1.COMMCELL,
                              TotalProduced = r1.TotalProduced,
                              TotalProducedRunAndRate = r2.TotalProduced,
                              TotalFailures = r1.TotalFailures,
                              TotalFailuresRunAndRate = r2.TotalFailures,
                              Total = r1.Total,
                              TotalRunAndRate = r2.Total,
                              FPY = r1.FPY,
                              FPYRunAndRate = r2.FPY,
                              Goal = goalInt,
                          })
                    .ToList();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return resultado;
        }

        private List<ReportFPYByProcessAndStationAndDayFinal> CrearReportesFPYbyProcessAndStationAndDay(List<HistoryModel> historialSinDuplicados, List<HistoryModel> historialRunAndRate, string Process)
        {
            List<ReportFPYByProcessAndStationAndDayFinal> resultado = new List<ReportFPYByProcessAndStationAndDayFinal>();
            List<ReportFPYByProcessAndStationAndDay> ReportFPYByProcessAndStation = new List<ReportFPYByProcessAndStationAndDay>();
            List<ReportFPYByProcessAndStationAndDay> ReportFPYProcessAndStatioRunAndRate = new List<ReportFPYByProcessAndStationAndDay>();
            try
            {
                ReportFPYByProcessAndStation = historialSinDuplicados
                .GroupBy(h => new { h.COMMCELL, h.EVENT_DATE })
                .Select(grupo =>
                {
                    int totalProduced = grupo.Count(h => h.result == "P");
                    int totalFailures = grupo.Count(h => h.result == "F");
                    int total = totalProduced + totalFailures;
                    double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;

                    return new ReportFPYByProcessAndStationAndDay
                    {
                        Process = Process,
                        COMMCELL = grupo.Key.COMMCELL,
                        Day = grupo.Key.EVENT_DATE,
                        TotalProduced = totalProduced,
                        TotalFailures = totalFailures,
                        Total = total,
                        FPY = fpy
                    };
                })
                .OrderBy(h => h.FPY)
                .ToList();


                ReportFPYProcessAndStatioRunAndRate = historialRunAndRate
                    .GroupBy(h => new { h.COMMCELL, h.EVENT_DATE })
                    .Select(grupo =>
                    {
                        int totalProduced = grupo.Count(h => h.result == "P");
                        int totalFailures = grupo.Count(h => h.result == "F");
                        int total = totalProduced + totalFailures;
                        double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;

                        return new ReportFPYByProcessAndStationAndDay
                        {
                            Process = Process,
                            COMMCELL = grupo.Key.COMMCELL,
                            Day = grupo.Key.EVENT_DATE,
                            TotalProduced = totalProduced,
                            TotalFailures = totalFailures,
                            Total = total,
                            FPY = fpy
                        };
                    })
                    .OrderBy(h => h.FPY)
                    .ToList();

                resultado = ReportFPYByProcessAndStation
                    .Join(
                        ReportFPYProcessAndStatioRunAndRate,
                        r1 => new { r1.COMMCELL, r1.Day },
                        r2 => new { r2.COMMCELL, r2.Day },
                        (r1, r2) => new ReportFPYByProcessAndStationAndDayFinal
                        {
                            Process = r1.Process,
                            COMMCELL = r1.COMMCELL,
                            Day = r1.Day,
                            TotalProduced = r1.TotalProduced,
                            TotalProducedRunAndRate = r2.TotalProduced,
                            TotalFailures = r1.TotalFailures,
                            TotalFailuresRunAndRate = r2.TotalFailures,
                            Total = r1.Total,
                            TotalRunAndRate = r2.Total, // Ajusta si es necesario
                            FPY = r1.FPY,
                            FPYRunAndRate = r2.FPY
                        })
                    .ToList();



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return resultado;
        }

        static string ObtenerHora(string eventHour)
        {
            // Obtener solo la hora de la cadena en formato "HH"
            return DateTime.ParseExact(eventHour, "HH:mm:ss", null).ToString("HH");
        }

        private List<ReportFPYByProcessAndStationDayHourFinal> CrearReportesFPYbyProcessAndStationAndDayHour(List<HistoryModel> historialSinDuplicados, List<HistoryModel> historialRunAndRate, string Process, DateTime[] arregloDeFechas)
        {
            List<ReportFPYByProcessAndStationDayHourFinal> resultado = new List<ReportFPYByProcessAndStationDayHourFinal>();
            List<ReportFPYByProcessAndStationDayHour> ReportFPYByProcessAndStation = new List<ReportFPYByProcessAndStationDayHour>();
            List<ReportFPYByProcessAndStationDayHour> ReportFPYProcessAndStatioRunAndRate = new List<ReportFPYByProcessAndStationDayHour>();
            try
            {
                var resultadosAgrupados = historialSinDuplicados
                    .GroupBy(h => new { h.Proceso, h.COMMCELL, h.EVENT_DATE, Hour = ObtenerHora(h.EVENT_HOUR) })
                    .Select(g => new
                    {
                        Proceso = g.Key.Proceso,
                        COMMCELL = g.Key.COMMCELL,
                        EVENT_DATE = g.Key.EVENT_DATE,
                        EVENT_HOUR = g.Key.Hour,
                        ConteoP = g.Count(item => item.result == "P"),
                        ConteoF = g.Count(item => item.result == "F")
                    });

                foreach (var resultadoRenglon in resultadosAgrupados)
                {
                    int total = resultadoRenglon.ConteoP + resultadoRenglon.ConteoF;

                    ReportFPYByProcessAndStation.Add(new ReportFPYByProcessAndStationDayHour
                    {
                        Process = resultadoRenglon.Proceso,
                        COMMCELL = resultadoRenglon.COMMCELL,
                        Day = resultadoRenglon.EVENT_DATE,
                        Hour = resultadoRenglon.EVENT_HOUR,
                        TotalProduced = resultadoRenglon.ConteoP,
                        TotalFailures = resultadoRenglon.ConteoF,
                        Total = total,
                        FPY = total > 0 ? ((double)resultadoRenglon.ConteoP / total) * 100 : 0
                });
                }

                // Tu consulta existente
                var resultadosAgrupadosRAR = historialRunAndRate
                    .GroupBy(h => new { h.Proceso, h.COMMCELL, h.EVENT_DATE, Hour = ObtenerHora(h.EVENT_HOUR) })
                    .Select(g => new
                    {
                        Proceso = g.Key.Proceso,
                        COMMCELL = g.Key.COMMCELL,
                        EVENT_DATE = g.Key.EVENT_DATE,
                        EVENT_HOUR = g.Key.Hour,
                        ConteoP = g.Count(item => item.result == "P"),
                        ConteoF = g.Count(item => item.result == "F")
                    });

                foreach (var resultadoRenglon in resultadosAgrupadosRAR)
                {
                    int total = resultadoRenglon.ConteoP + resultadoRenglon.ConteoF;

                    ReportFPYProcessAndStatioRunAndRate.Add(new ReportFPYByProcessAndStationDayHour
                    {
                        Process = resultadoRenglon.Proceso,
                        COMMCELL = resultadoRenglon.COMMCELL,
                        Day = resultadoRenglon.EVENT_DATE,
                        Hour = resultadoRenglon.EVENT_HOUR,
                        TotalProduced = resultadoRenglon.ConteoP,
                        TotalFailures = resultadoRenglon.ConteoF,
                        Total = total,
                        FPY = total > 0 ? ((double)resultadoRenglon.ConteoP / total) * 100 : 0
                    });
                }






                List<ReportFPYByProcessAndStationDayHourFinal> union = ReportFPYByProcessAndStation
                   .Join(
                       ReportFPYProcessAndStatioRunAndRate,
                       r1 => new { r1.COMMCELL, r1.Day, r1.Hour },
                       r2 => new { r2.COMMCELL, r2.Day, r2.Hour },
                       (r1, r2) => new ReportFPYByProcessAndStationDayHourFinal
                       {
                           Process = r1.Process,
                           COMMCELL = r1.COMMCELL,
                           Day = r1.Day,
                           Hour = r1.Hour,
                           TotalProduced = r1.TotalProduced,
                           TotalProducedRunAndRate = r2.TotalProduced,
                           TotalFailures = r1.TotalFailures,
                           TotalFailuresRunAndRate = r2.TotalFailures,
                           Total = r1.Total,
                           TotalRunAndRate = r2.Total, // Ajusta si es necesario
                           FPY = r1.FPY,
                           FPYRunAndRate = r2.FPY
                       })
                   .OrderBy(r => r.FPY)
                   .ToList();
                resultado.AddRange(union);
                   resultado = resultado.OrderBy(item => item.Process)
                             .ThenBy(item => item.COMMCELL)
                             .ThenBy(item => item.Day)
                             .ThenBy(item => item.Hour)
                             .ToList();



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return resultado;
        }

        private List<ReportFPYByProcessAndModelFinal> CrearReportesFPYbyProcessAndModel(List<HistoryModel> historialSinDuplicados, List<HistoryModel> historialConDuplicados, string Process)
        {
            List<ReportFPYByProcessAndModelFinal> resultado = new List<ReportFPYByProcessAndModelFinal>();
            List<ReportFPYByProcessAndModel> ReportFPYByProcessAndModel = new List<ReportFPYByProcessAndModel>();
            List<ReportFPYByProcessAndModel> ReportFPYByProcessAndModelConDuplicados = new List<ReportFPYByProcessAndModel>();
            try
            {
                ReportFPYByProcessAndModel = historialSinDuplicados
               .GroupBy(h => h.Modelo)
               .Select(grupo =>
               {
                   int totalProduced = grupo.Count(h => h.result == "P");
                   int totalFailures = grupo.Count(h => h.result == "F");
                   int total = totalProduced + totalFailures;
                   double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;

                   return new ReportFPYByProcessAndModel
                   {
                       Process = Process,
                       Model = grupo.Key,
                       TotalProduced = totalProduced,
                       TotalFailures = totalFailures,
                       Total = total,
                       FPY = fpy
                   };
               }).OrderBy(f =>f.FPY)
               .ToList();

                ReportFPYByProcessAndModelConDuplicados = historialConDuplicados
               .GroupBy(h => h.Modelo)
               .Select(grupo =>
               {
                   int totalProduced = grupo.Count(h => h.result == "P");
                   int totalFailures = grupo.Count(h => h.result == "F");
                   int total = totalProduced + totalFailures;
                   double fpy = total > 0 ? ((double)totalProduced / total) * 100 : 0;

                   return new ReportFPYByProcessAndModel
                   {
                       Process = Process,
                       Model = grupo.Key,
                       TotalProduced = totalProduced,
                       TotalFailures = totalFailures,
                       Total = total,
                       FPY = fpy
                   };
               }).OrderBy(f => f.FPY)
               .ToList();

                resultado = ReportFPYByProcessAndModel
                    .Join(
                        ReportFPYByProcessAndModelConDuplicados,
                        r1 => new { r1.Process, r1.Model },  // Especifica el tipo de la clave para el primer conjunto
                        r2 => new { r2.Process, r2.Model },  // Especifica el tipo de la clave para el segundo conjunto
                        (r1, r2) => new ReportFPYByProcessAndModelFinal
                        {
                            Process = r1.Process,
                            Model = r1.Model,
                            TotalProduced = r1.TotalProduced,
                            TotalProducedRunAndRate = r2.TotalProduced,
                            TotalFailures = r1.TotalFailures,
                            TotalFailuresRunAndRate = r2.TotalFailures,
                            Total = r1.Total,
                            TotalRunAndRate = r2.Total,
                            FPY = r1.FPY,
                            FPYRunAndRate = r2.FPY
                        })
                    .ToList();

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return resultado;
        }

        public (double, double) CalcularRolado(List<ReportFPYByProcessFinal> resultadoReporteFPYbyProcess)
        {
            double Rolado = 1;
            double RoladoRunAndRate = 1;
            try
            {
                double promedioTotal = CalcularPromedioTotal(resultadoReporteFPYbyProcess);

                List<ReportFPYByProcessFinal> resultadosFiltrados = resultadoReporteFPYbyProcess
                    .Where(reporte => reporte.Total >= promedioTotal*.30)
                    .ToList();

                

                foreach (var renglon in resultadosFiltrados)
                {
                    double fpysumarizado = renglon.FPY / 100;
                    Rolado = Rolado * fpysumarizado;
                }
                Rolado = Rolado * 100;


                foreach (var renglon in resultadosFiltrados)
                {
                    double fpysumarizado = renglon.FPYRunAndRate / 100;
                    RoladoRunAndRate = RoladoRunAndRate * fpysumarizado;
                }
                RoladoRunAndRate = RoladoRunAndRate * 100;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (Rolado, RoladoRunAndRate);
        }

        public double CalcularRoladoPorProceso(List<ReportFPYByProcessAndStationFinal> reportFPYByProcessAndStations)
        {
            double Rolado = 1;
            try
            {
                double promedioTotal = CalcularPromedioTotalPorEstacion(reportFPYByProcessAndStations);

                List<ReportFPYByProcessAndStationFinal> resultadosFiltrados = reportFPYByProcessAndStations
                    .Where(reporte => reporte.Total >= promedioTotal*0.3)
                    .ToList();



                foreach (var renglon in resultadosFiltrados)
                {
                    double fpysumarizado = renglon.FPY / 100;
                    Rolado = Rolado * fpysumarizado;
                }
                Rolado = Rolado * 100;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Rolado;
        }

        public static double CalcularPromedioTotal(List<ReportFPYByProcessFinal> resultadoReporteFPYbyProcess)
        {

            double sumaTotal = 0.0;
            try
            {
                if (resultadoReporteFPYbyProcess == null || resultadoReporteFPYbyProcess.Count == 0)
                {
                    // Manejo de lista vacía o nula según tus requerimientos
                    return 0.0;
                }


                foreach (ReportFPYByProcessFinal reporte in resultadoReporteFPYbyProcess)
                {
                    sumaTotal += reporte.Total;
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return sumaTotal / resultadoReporteFPYbyProcess.Count;
        }

        public static double CalcularPromedioTotalPorEstacion(List<ReportFPYByProcessAndStationFinal> reportFPYByProcessAndStations)
        {

            double sumaTotal = 0.0;
            try
            {
                if (reportFPYByProcessAndStations == null || reportFPYByProcessAndStations.Count == 0)
                {
                    // Manejo de lista vacía o nula según tus requerimientos
                    return 0.0;
                }


                foreach (ReportFPYByProcessAndStationFinal reporte in reportFPYByProcessAndStations)
                {
                    sumaTotal += reporte.Total;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return sumaTotal / reportFPYByProcessAndStations.Count;
        }

        
    }
}
