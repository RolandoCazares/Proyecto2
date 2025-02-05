
using proyecto.Contracts;
using proyecto.Data;
using proyecto.Models;
using System.Data;
using System.Text.RegularExpressions;

namespace proyecto.Repositories
{
    public class MesRepository : IMesRepository
    {
        private static IDbContextNogales dbContext = MesDbContextNogales.Instance;       
        private static Regex serialNumberPattern = new Regex("^[A-Z0-9_.-]*$");        

        public List<TestDiag> GetHistory(string SerialNumber)
        {
            List<TestDiag> testList = new List<TestDiag>();
            
            if (serialNumberPattern.Match(SerialNumber).Success)
            {                
                string Query = $"SELECT AL1.RUNID, AL1.RUN_STATE, TO_CHAR(AL1.RUN_DATE, 'DD-MM-YYYY HH24:MI:SS') AS RUN_DATE, AL6.BMT_NAME, AL2.PDK_MATERIAL, AL4.MRK_NUM, AL5.MRK_BEZ, TO_CHAR(AL4.MRK_WERT) AS MRK_WERT, AL5.MRK_EINHEIT, AL5.MRK_USG, AL5.MRK_OSG FROM EVAPROD.PD_LFD_RUN AL1, EVAPROD.PD_LFD_MAT AL2, EVAPROD.PD_STM_PRP AL3, EVAPROD.PD_LFD_MED2 AL4, EVAPROD.PD_STM_MRK AL5, EVACOMP.PD_LFD_BMN AL6 WHERE (AL2.PRD_MAT_SID=AL1.PRD_MAT_SID AND AL6.BMT_DAT_ID=AL1.BMT_DAT_ID AND AL1.PRP_DATE_ID=AL3.PRP_DATE_ID AND AL1.RUN_KEY_PRT=AL4.RUN_KEY_PRT AND AL1.RUN_SEQ_KEY=AL4.RUN_SEQ_KEY AND AL1.PRP_DATE_ID=AL5.PRP_DATE_ID AND AL5.PRP_DATE_ID=AL3.PRP_DATE_ID AND AL4.MRK_NUM=AL5.MRK_NUM)  AND (AL4.MRK_EIN_GUT='F' AND AL1.RUNID='{SerialNumber}' AND (NOT AL6.BMT_NAME LIKE '%ANA%'))";               
                DataTable result = dbContext.RunQuery(Query);
                if (result.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        TestDiag test = new TestDiag()
                        {
                            SerialNumber = row["RUNID"].ToString(),
                            State = row["RUN_STATE"].ToString(),
#pragma warning disable CS8604 // Possible null reference argument.
                            //RunDate = DateTime.ParseExact(row["RUN_DATE"].ToString(), "dd-MM-yyyy HH:mm:ss", null),
#pragma warning restore CS8604 // Possible null reference argument.
                            StationId = row["BMT_NAME"].ToString(),
                            Model = row["PDK_MATERIAL"].ToString(),
                            TestNumber = row["MRK_NUM"].ToString(),
                            Description = row["MRK_BEZ"].ToString(),
                            Value = row["MRK_WERT"].ToString(),
                            LSL = row["MRK_USG"].ToString(),
                            USL = row["MRK_OSG"].ToString()
                        };
                        testList.Add(test);
                    }
                }
            }
            
            return testList;
        }
    }
}
