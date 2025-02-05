
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.Style;
using OpenQA.Selenium;
using Org.BouncyCastle.Asn1.X509;
using System.Collections.Concurrent;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Collections.Specialized.BitVector32;

namespace proyecto.Helpers
{
    public static class MesQueryFabric
    {



        public static string QueryForTopOffenderGeneralPorProceso(string Proceso, DateTime fromDate, DateTime toDate)
        {
            double diffDays = (toDate - fromDate).TotalDays;
            if (!(diffDays > 0 && diffDays <= 7)) throw new ArgumentException("Only a maximum of 7 days is allowed");

            string FromDate = fromDate.ToString("MM/dd/yy HH:mm:ss");
            string ToDate = toDate.ToString("MM/dd/yy HH:mm:ss");
            string query = "SELECT UID_UNIT_ID Serial_Number, DCD_GROUP Modelo, DCD_NAME Proceso, commcells.LOC_NAME COMMCELL, PE_STARTTIME FECHA, to_char(PE_STARTTIME, 'DD-MM-YYYY') EVENT_DATE, to_char(PE_STARTTIME, 'HH24:MI:SS') EVENT_HOUR, UIT_TYPE ID_TYPE, JOB_NAME ORDEN, DCDDC_DESC TEST_NAME, decode(DCRD_RESULT_ENMV_ID, 140, 'PASS', 141, 'FAIL', '-') PASS_FAIL, DCRD_VALUE_NUM RESULT_OF_TEST, DCDDC_LSL LSL, DCDDC_USL USL, DCDDC_UNIT UNIDAD_MEDIDA FROM dc_def dcd INNER JOIN dc_res_id dcr ON dcr.DCR_DCD_ID = dcd.DCD_ID INNER JOIN process_events pe ON pe.PE_ID = dcr.DCR_PE_ID INNER JOIN unit_ident_data i ON i.UID_ID = pe.PE_UID_ID INNER JOIN unit_ident_types uit ON uit.UIT_ID = i.UID_UIT_ID INNER JOIN process_step_impl psi ON psi.PSI_ID = pe.PE_PSI_ID INNER JOIN process_catalog pc ON pc.PC_ID = psi.PSI_PC_ID INNER JOIN jobs j ON j.JOB_ID = pe.PE_JOB_ID INNER JOIN dc_def_det dcdd ON dcdd.DCDD_DCD_ID = dcd.DCD_ID INNER JOIN dc_res_det dcrd ON dcrd.DCRD_DCR_ID = dcr.DCR_ID AND dcrd.DCRD_DCDD_ID = dcdd.DCDD_ID INNER JOIN v_LOCATIONS commcells ON commcells.loc_id = pe.PE_LOC_ID LEFT JOIN dc_def_det_chs dcddc ON dcddc.DCDDC_DCDD_ID = dcdd.DCDD_ID WHERE DCD_NAME IN ('" + Proceso + "') AND JOB_NAME NOT LIKE ('%golden%') AND DCRD_RESULT_ENMV_ID IN ('141') AND PE_ENDTIME BETWEEN TO_DATE('" + FromDate + "', 'MM/DD/YY HH24:MI:SS') AND TO_DATE('" + ToDate + "', 'MM/DD/YY HH24:MI:SS')";
            return query;
        }

        public static string QueryForTopOffenderGeneralPorEstacion(string COMMCELLS, DateTime fromDate, DateTime toDate)
        {
            double diffDays = (toDate - fromDate).TotalDays;
            if (!(diffDays > 0 && diffDays <= 7)) throw new ArgumentException("Only a maximum of 7 days is allowed");

            string FromDate = fromDate.ToString("MM/dd/yy HH:mm:ss");
            string ToDate = toDate.ToString("MM/dd/yy HH:mm:ss");
            string query = "SELECT UID_UNIT_ID Serial_Number, DCD_GROUP Modelo, DCD_NAME Proceso, commcells.LOC_NAME COMMCELL, PE_STARTTIME FECHA, to_char(PE_STARTTIME, 'DD-MM-YYYY') EVENT_DATE, to_char(PE_STARTTIME, 'HH24:MI:SS') EVENT_HOUR, UIT_TYPE ID_TYPE, JOB_NAME ORDEN, DCDDC_DESC TEST_NAME, decode(DCRD_RESULT_ENMV_ID, 140, 'PASS', 141, 'FAIL', '-') PASS_FAIL, DCRD_VALUE_NUM RESULT_OF_TEST, DCDDC_LSL LSL, DCDDC_USL USL, DCDDC_UNIT UNIDAD_MEDIDA FROM dc_def dcd INNER JOIN dc_res_id dcr ON dcr.DCR_DCD_ID = dcd.DCD_ID INNER JOIN process_events pe ON pe.PE_ID = dcr.DCR_PE_ID INNER JOIN unit_ident_data i ON i.UID_ID = pe.PE_UID_ID INNER JOIN unit_ident_types uit ON uit.UIT_ID = i.UID_UIT_ID INNER JOIN process_step_impl psi ON psi.PSI_ID = pe.PE_PSI_ID INNER JOIN process_catalog pc ON pc.PC_ID = psi.PSI_PC_ID INNER JOIN jobs j ON j.JOB_ID = pe.PE_JOB_ID INNER JOIN dc_def_det dcdd ON dcdd.DCDD_DCD_ID = dcd.DCD_ID INNER JOIN dc_res_det dcrd ON dcrd.DCRD_DCR_ID = dcr.DCR_ID AND dcrd.DCRD_DCDD_ID = dcdd.DCDD_ID INNER JOIN v_LOCATIONS commcells ON commcells.loc_id = pe.PE_LOC_ID LEFT JOIN dc_def_det_chs dcddc ON dcddc.DCDDC_DCDD_ID = dcdd.DCDD_ID WHERE commcells.LOC_NAME IN ('" + COMMCELLS + "') AND JOB_NAME NOT LIKE ('%golden%') AND DCRD_RESULT_ENMV_ID IN ('141') AND PE_ENDTIME BETWEEN TO_DATE('" + FromDate + "', 'MM/DD/YY HH24:MI:SS') AND TO_DATE('" + ToDate + "', 'MM/DD/YY HH24:MI:SS')";
            return query;
        }

        public static string QueryForPassAndFailsFPY(string Proceso, string COMMCELLS, DateTime fromDate, DateTime toDate)
        {
            double diffDays = (toDate - fromDate).TotalDays;
            if (!(diffDays >= 0 && diffDays <= 7)) throw new ArgumentException("Only a maximum of 7 days is allowed");

            string FromDate = fromDate.ToString("dd/MM/yyyy HH:mm:ss");
            string ToDate = toDate.ToString("dd/MM/yyyy HH:mm:ss");
            string query = "select distinct run.runid id_unit, '" + Proceso + "' as PROCESO,  run.runid_type id_type, run.run_date datetime, run.run_date_end, auf.PDK_AUFTR job, bmn.BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, run.run_state result from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp where med.run_key_prt = run.run_key_prt and med.run_seq_key = run.run_seq_key and run.prd_spc_sid = auf.prd_spc_sid and run.prp_date_id = prp.prp_date_id and bmn.bmt_dat_id = run.bmt_dat_id and bmn.BMT_NAME = '" + COMMCELLS + "' and run.run_date between to_date('" + FromDate + "', 'dd/mm/yyyy hh24:mi:ss') and to_date('" + ToDate + "','dd/mm/yyyy hh24:mi:ss') order by run.run_date asc";
            return query;
        }

        public static string QueryForSpecifictTest(DateTime fromDate, DateTime toDate)
        {
            double diffDays = (toDate - fromDate).TotalDays;
            if (!(diffDays >= 0 && diffDays <= 7)) throw new ArgumentException("Only a maximum of 7 days is allowed");

            string FromDate = fromDate.ToString("dd/MM/yyyy HH:mm:ss");
            string ToDate = toDate.ToString("dd/MM/yyyy HH:mm:ss");
            //string querySpecifictTest = "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, BMT_NAME station, PDK_AUFTR job, mrk.mrk_num testid, mrk.mrk_bez description, med.mrk_wert value, med.MRK_EIN_GUT result, mrk_usg LSL, mrk_osg USL, mrk_txt textinfo from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk , evaprod.PD_LFD_MXT2 mxt2 where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and mat.prd_mat_sid=run.prd_mat_sid and mxt2.RUN_SEQ_KEY =run.run_seq_key and mxt2.RUN_KEY_PRT = run.run_key_prt and mxt2.MRK_NUM = mrk.MRK_NUM and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and  prp.prp_var = 'CCN_PRU-FIN_3140901'  AND run.runid_type = 'CCN_BE1_FIN' AND mrk.mrk_num = '511187' and med.MRK_EIN_GUT = 'P' and run.run_date between to_date('" + FromDate + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + ToDate + "','dd/mm/yyyy hh24:mi:ss') order by run.runid, run.run_date asc";
            //string querySpecifictTest = "select run.runid id_unit, run.runid_type id_type, run.run_date datetime, auf.PDK_AUFTR job, bmn.BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, med.MRK_EIN_GUT result from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and run.prd_spc_sid=auf.prd_spc_sid and run.prp_date_id=prp.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and run.runid_type = 'CCN_BE2_FIN' and prp.prp_var = 'CCN_MVT' and run.run_date between to_date('" + FromDate + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + ToDate + "','dd/mm/yyyy hh24:mi:ss')";
            //string querySpecifictTest = "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, auf.PDK_AUFTR job, bmn.BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, med.MRK_EIN_GUT result from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and run.prd_spc_sid=auf.prd_spc_sid and run.prp_date_id=prp.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and run.runid_type = 'CCN_BE2_FIN' and prp.prp_var = 'CCN_MVT' and run.run_date between to_date('" + FromDate + "','dd/mm/yyyy hh24:mi:ss')  and to_date('" + ToDate + "','dd/mm/yyyy hh24:mi:ss')";
            string querySpecifictTest = "select distinct run.runid id_unit, run.runid_type id_type, run.run_date fecha,TO_CHAR(run.run_date, 'DD/MM/YYYY HH24:MI:SS') AS datetime, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, BMT_NAME station, PDK_AUFTR job, mrk.mrk_num testid, mrk.mrk_bez description, med.mrk_wert value, med.MRK_EIN_GUT result, mrk_usg LSL, mrk_osg USL, mrk_txt textinfo from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk , evaprod.PD_LFD_MXT2 mxt2 where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and mxt2.RUN_SEQ_KEY =run.run_seq_key and mxt2.RUN_KEY_PRT = run.run_key_prt and mxt2.MRK_NUM = mrk.MRK_NUM and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and run.run_date between to_date('" + FromDate + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + ToDate + "','dd/mm/yyyy hh24:mi:ss') and run.runid_type = 'CCN_BE1_FIN' and  mrk_txt in ('89332403224370001804', '89332403224380001812', '89332403224370001796', '89332403224380001892', '8990022062057604934F', '8990022062057601161F')";



            return querySpecifictTest;
        }



        public static string QueryForPassAndFailsFPYFGEN(string Proceso, string IDTYPE, string COMMCELLS, DateTime fromDate, DateTime toDate)
        {
            double diffDays = (toDate - fromDate).TotalDays;
            if (!(diffDays > 0 && diffDays <= 7)) throw new ArgumentException("Only a maximum of 7 days is allowed");

            string FromDate = fromDate.ToString("MM/dd/yy HH:mm:ss");
            string ToDate = toDate.ToString("MM/dd/yy HH:mm:ss");
            string query = "SELECT UID_UNIT_ID Serial_Number, DCD_GROUP Modelo, '" + Proceso + "' AS Proceso, commcells.LOC_NAME COMMCELL, to_char(PE_STARTTIME, 'DD-MM-YYYY') EVENT_DATE, to_char(PE_STARTTIME, 'HH24:MI:SS') EVENT_HOUR, UIT_TYPE ID_TYPE, decode(PE_QTY_YIELD, 1, 'PASS', 0, 'FAIL', '-') result, JOB_NAME ORDEN FROM dc_def dcd inner join dc_res_id dcr on dcr.DCR_DCD_ID = dcd.DCD_ID inner join process_events pe on pe.PE_ID = dcr.DCR_PE_ID inner join unit_ident_data i on i.UID_ID = pe.PE_UID_ID inner join unit_ident_types uit on uit.UIT_ID = i.UID_UIT_ID inner join process_step_impl psi on psi.PSI_ID = pe.PE_PSI_ID inner join process_catalog pc on pc.PC_ID = psi.PSI_PC_ID inner join jobs j on j.JOB_ID = pe.PE_JOB_ID inner join v_LOCATIONS commcells on commcells.loc_id = pe.PE_LOC_ID WHERE uit.UIT_TYPE IN ('" + IDTYPE + "') AND commcells.LOC_NAME IN ('" + COMMCELLS + "') AND PE_ENDTIME BETWEEN TO_DATE('" + FromDate + "', 'MM/DD/YY HH24:MI:SS') AND TO_DATE('" + ToDate + "', 'MM/DD/YY HH24:MI:SS') GROUP BY UID_UNIT_ID, DCD_GROUP, DCD_NAME, commcells.LOC_NAME, PE_STARTTIME, TO_CHAR(PE_STARTTIME, 'DD-MM-YYYY'), TO_CHAR(PE_STARTTIME, 'HH24:MI:SS'), UIT_TYPE, PE_QTY_YIELD, JOB_NAME ORDER BY PE_STARTTIME DESC";
            return query;
        }

        public static string QueryForIdTypes() //Use WipReportingProd
        {
            return "select distinct unit_id_in_type from t_wip_subset order by unit_id_in_type asc";
        }

        public static string QueryProductGroups()//Use WipReportingProd
        {
            return "select distinct j.PRODUCT_GROUP from t_wip_job j where product_group is not null";
        }

        public static string QueryProductTypes()//Use WipReportingProd
        {
            return "select distinct product_type from t_wip_job j";
        }

        public static string QueryForReport1(string _szStartTime, string _szEndTime,
        string _szTestplan, string _szIdType)//Use WipReportingProd
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and prp.prp_var = '" + _szTestplan + "' and run.run_date between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') and run.runid_type = '" + _szIdType + "' order by run.runid, run.run_date asc";
        }

        public static string QueryForReport2(string _szWorkcenter, string _szUnitsIds, string _szIdType)
        {
            //Use WipReportingProd
            return "SELECT s.dst_job job, s.unit_id_in id_in, s.unit_id_out id, s.unit_id_in_type id_type, s.src_equipment workcenter, s.operator station, s.created datetime, s.QTY yield, s.QTY_FAIL fail, s.transaction_type action FROM t_wip_subset_log s where s.src_equipment like '" + _szWorkcenter + "%' and s.UNIT_ID_IN_TYPE = '" + _szIdType + "' and s.unit_id_in in (" + _szUnitsIds + ") ORDER BY s.unit_id_in, s.created asc";
        }

        public static string QueryForReport3(string _szTestplan, string _szUnitsIds, string _szIdType)
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and prp.prp_var in (" + _szTestplan + ") and run.runid_type = '" + _szIdType + "' and run.runid in (" + _szUnitsIds + ")and med.MRK_EIN_GUT = 'F' order by run.runid, run.run_date desc";
        }

        public static string QueryForReport4(string _szTestplan, string _szUnitsIds, string _szIdType, string _szDescription)
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, mrk.mrk_num_lng testid_long, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and prp.prp_var in(" + _szTestplan + ") and run.runid_type = '" + _szIdType + "' and run.runid in (" + _szUnitsIds + ") and mrk.mrk_bez = '" + _szDescription + "' order by run.runid, run.run_date asc";
        }

        public static string QueryForReport5(string _szUnitsIds, string _szIdType)
        {
            return "SELECT s.unit_id_in id_in, s.unit_id_out actual_id, s.unit_id_out_type last_id_type, s.dst_job job, j.product_definition material, s.dst_equipment workcenter, ws.DESCRIPTION_short station, s.src_process_step step, s.created datetime, s.QTY Pass, s.QTY_FAIL FAIL, s.QTY_LOSS scrap, decode(l.LOCK_DESCRIPT, 1, 'LOCKED', 'NO LOCKED') ISLOCK FROM t_wip_subset_log s left join t_wip_lock l on (s.SRC_SUBSET_ID_L = l.SUBSET_ID_L), t_wip_job j, T_WIP_PROCESS_STEP ws where s.dst_job = j.job and ws.JOB = j.job and ws.EQUIPMENT_LIST = s.DST_EQUIPMENT and ws.PROCESS_STEP = s.src_process_step  and s.unit_id_in in (" + _szUnitsIds + ") and s.unit_id_in_type = '" + _szIdType + "' ORDER BY s.unit_id_in, s.created desc";
        }

        public static string QueryForReport6(string _szStartTime, string _szEndTime, string _szTestplan, string _szIdType, string _szDescription)//Use WipReportingProd
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, mrk.mrk_num_lng testid_long, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and prp.prp_var in('" + _szTestplan + "') and run.runid_type = '" + _szIdType + "' and mrk.mrk_bez in (" + _szDescription + ") and run.run_date between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') order by run.runid, run.run_date asc";
        }

        public static string QueryForReport7(string _szUnitIds, string _szIdType)
        {
            return "SELECT s.unit_id_in ,s.unit_id_in_type,s.unit_id_out ,s.unit_id_out_type ,s.dst_equipment workcenter,s.operator station,s.created datetime,'1' counter FROM t_wip_subset_log s where (s.unit_id_in in (" + _szUnitIds + ") or s.unit_id_out in (" + _szUnitIds + ")) and s.UNIT_ID_IN_TYPE = '" + _szIdType + "' and s.unit_id_in <> unit_id_out";
        }

        public static string QueryForReport8(string _szPartNumber, string _szUnitsIds, string _szIdType)
        {
            return "SELECT PL.JOB, PL.PROCESS_STEP_ID, PL.subset_id_l SUBSET_ID, SL.DST_PROCESS_STEP_ID, SL.UNIT_ID_IN, SL.UNIT_ID_IN_TYPE, SL.DST_EQUIPMENT, SL.DST_PROCESS_STEP, pl.GOODS_RECEIPT_NR, pl.part_id, pl.usage_start USAGE_DATE, pl.usage_end USAGE_TYPE, pl.part_id Material_Number, T3.PART_NAME FROM  t_wip_partslist pl, t_wip_subset_log sl, t_mat_def T3 where (pl.subset_id_l is not null and   pl.subset_id_l > 0) and pl.part_id_type = 'RAW' and   sl.dst_job = pl.job and   sl.dst_process_step_id = pl.process_step_id and   sl.dst_subset_id_l = pl.subset_id_l and   (sl.transaction_type like '%checkout' or sl.transaction_type like '%progress%')  union select ti.job, ti.process_step_id, null subunit_id, sl.dst_process_step_id, sl.unit_id_in, sl.unit_id_in_type, sl.dst_equipment, sl.dst_process_step, ti.GOODS_RECEIPT_NR, ti.part_id, max(sl.CREATED), ti.USAGE_END, NVL(SUBSTR(ti.part_id, 6, INSTR(ti.part_id, '@')-6), ti.part_id) Material_Number, max(T3.PART_NAME) from t_wip_subset_log sl,t_mat_def T3, (select pl.job, pl.process_step_id, pl.GOODS_RECEIPT_NR, pl.part_id, pl.usage_start, pl.usage_end, pl.part_id Material_Number, T3.PART_NAME FROM  t_wip_partslist pl, t_mat_def T3 where ((pl.subset_id_l is null or pl.subset_id_l = 0)  and  pl.usage_start is not null ) and pl.part_id_type = 'RAW' ) ti where sl.dst_job = ti.job and NVL(SUBSTR(ti.Material_Number, 6, INSTR(ti.Material_Number, '@')-6), ti.Material_Number) = '" + _szPartNumber + "' and T3.part_number = '" + _szPartNumber + "' and sl.unit_id_in in (" + _szUnitsIds + ") and sl.dst_process_step_id = ti.process_step_id and sl.created between ti.usage_start and ti.usage_end and   (sl.transaction_type like '%checkout' or sl.transaction_type like '%progress%') group by ti.job, ti.process_step_id, sl.dst_process_step_id, sl.unit_id_in, sl.unit_id_in_type, sl.dst_equipment, sl.dst_process_step, ti.part_id, ti.GOODS_RECEIPT_NR, ti.usage_end, ti.Material_Number, ti.PART_NAME";

        }

        public static string QueryForReport9(string _szStartTime, string _szEndTime, string _szPartNumber)
        {
            return "select T3.part_number, T3.PART_NAME description, T1.GOODS_RECEIPT_NR GR, T1.PART_ID raw_id, T2.SUPPLIER_ID supplier_id, T1.Created Usage_start, T1.UPDATED Usage_end, t1.operator station_assembly from t_mat_container T1, t_mat_stock T2, t_mat_def T3 where (1=1) and T1.UPDATED between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') and T2.UPDATED between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') and T3.part_number = '" + _szPartNumber + "' and T1.GOODS_RECEIPT_POS = T2.GOODS_RECEIPT_POS and T1.GOODS_RECEIPT_NR = T2.GOODS_RECEIPT_NR and T1.GOODS_RECEIPT_POS = T2.GOODS_RECEIPT_POS and T2.BASE_ID = T3.BASE_ID and T2.MAT_DEF_ID = T3.MAT_DEF_ID";

        }

        public static string QueryForReport10(string _szGoodReceipt, string _szRawId)
        {
            return "SELECT PL.JOB, PL.PROCESS_STEP_ID, PL.subset_id_l SUBSET_ID, SL.DST_PROCESS_STEP_ID, SL.UNIT_ID_IN, SL.UNIT_ID_IN_TYPE, SL.DST_EQUIPMENT, SL.DST_PROCESS_STEP, pl.GOODS_RECEIPT_NR, pl.part_id, DECODE(PL.REMOVED,NULL,PL.CREATED,PL.REMOVED) USAGE_DATE, DECODE(PL.REMOVED,NULL,'','REMOVED') USAGE_TYPE FROM  t_wip_partslist pl, t_wip_subset_log sl where (pl.subset_id_l is not null and   pl.subset_id_l > 0) and pl.part_id in (" + _szRawId + ") /*and pl.GOODS_RECEIPT_NR = '" + _szGoodReceipt + "'*/ and pl.part_id_type = 'RAW' and   sl.dst_job = pl.job and   sl.dst_process_step_id = pl.process_step_id and   sl.dst_subset_id_l = pl.subset_id_l and   (sl.transaction_type like '%checkout' or sl.transaction_type like '%progress%')  union select ti.job, ti.process_step_id, null subunit_id, sl.dst_process_step_id, sl.unit_id_in, sl.unit_id_in_type, sl.dst_equipment, sl.dst_process_step, ti.GOODS_RECEIPT_NR, ti.part_id, max(sl.CREATED), null from t_wip_subset_log sl,  (select pl.job, pl.process_step_id, pl.GOODS_RECEIPT_NR, pl.part_id, pl.usage_start, pl.usage_end FROM  t_wip_partslist pl where ((pl.subset_id_l is null or pl.subset_id_l = 0)  and  pl.usage_start is not null ) and pl.part_id in (" + _szRawId + ") /*and pl.GOODS_RECEIPT_NR = '" + _szGoodReceipt + "'*/ and pl.part_id_type = 'RAW' ) ti where sl.dst_job = ti.job and   sl.dst_process_step_id = ti.process_step_id and   sl.created between ti.usage_start and ti.usage_end and   (sl.transaction_type like '%checkout' or sl.transaction_type like '%progress%') group by ti.job, ti.process_step_id, sl.dst_process_step_id, sl.unit_id_in, sl.unit_id_in_type, sl.dst_equipment, sl.dst_process_step, ti.part_id, ti.GOODS_RECEIPT_NR";
        }

        public static string QueryForReport11(string _szTestplan, string _szIdType, string _szTestIds, string _szUnitIds)
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, BMT_NAME station, PDK_AUFTR job, mrk.mrk_num testid, mrk.mrk_bez description, med.mrk_wert value, med.MRK_EIN_GUT result, mrk_usg LSL, mrk_osg USL, mrk_txt textinfo from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk , evaprod.PD_LFD_MXT2 mxt2 where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and mxt2.RUN_SEQ_KEY =run.run_seq_key and mxt2.RUN_KEY_PRT = run.run_key_prt and mxt2.MRK_NUM = mrk.MRK_NUM and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and run.runid_type = '" + _szIdType + "' and run.runid in (" + _szUnitIds + ") and (mrk.mrk_num_lng in (" + _szTestIds + ") or mrk.mrk_num in (" + _szTestIds + ")) and prp.prp_var = '" + _szTestplan + "' order by run.runid, run.run_date asc";
        }

        public static string QueryForReport12(string _szGoodReceipt)
        {
            return "select T3.part_number material, T1.GOODS_RECEIPT_NR GR, T1.PART_ID raw_id, T3.PART_NAME description, T1.STATUS, T1.QTY_INITIAL_D, t1.created \"DATE\", T1.STORAGE_UNIT_NUMBER from t_mat_container T1, t_mat_stock T2, t_mat_def T3 where (1=1) and    T1.GOODS_RECEIPT_NR  = T2.GOODS_RECEIPT_NR and T1.GOODS_RECEIPT_NR in (" + _szGoodReceipt + ") and T1.GOODS_RECEIPT_POS = T2.GOODS_RECEIPT_POS and T1.GOODS_RECEIPT_NR  = T2.GOODS_RECEIPT_NR and T1.GOODS_RECEIPT_POS = T2.GOODS_RECEIPT_POS and T2.BASE_ID = T3.BASE_ID and T2.MAT_DEF_ID = T3.MAT_DEF_ID ";
        }

        public static string QueryForReport13(string _szStartTime, string _szEndTime, string _szProduct, string _szProductType, string _szIdType, string _szWorkcenter)//Use WipReportingProd
        {
            return "SELECT s.dst_job job, j.PRODUCT_DEFINITION material, s.UNIT_ID_IN ID, s.UNIT_ID_IN_TYPE id_type, s.SRC_EQUIPMENT workcenter, s.operator station, s.CREATED datetime, s.qty yield, s.QTY_FAIL fail, s.TRANSACTION_TYPE action FROM t_wip_subset_log s, t_wip_job j where s.SRC_equipment like '" + _szWorkcenter + "%' and s.created between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') and j.PRODUCT_GROUP = '" + _szProduct + "' and j.product_type = '" + _szProductType + "' and s.UNIT_ID_IN_TYPE = '" + _szIdType + "' and s.dst_job = j.job ORDER BY s.unit_id_in, s.created asc";
        }

        public static string QueryForReport14(string _szTestplan, string _szTestIds, string _szUnitIds, string _szIdType)//Use WipReportingProd
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, mrk.MRK_NUM_LNG testidlong, mrk.mrk_bez description, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and run.runid_type = '" + _szIdType + "' and run.runid in (" + _szUnitIds + ") and (mrk.mrk_num in (" + _szTestIds + ") or mrk.mrk_num_lng in (" + _szTestIds + ")) and prp.prp_var = '" + _szTestplan + "' order by run.runid, run.run_date asc";
        }

        public static string QueryForReport15(string _szRawIds)//Use WipReportingProd
        {
            return "select T3.part_number part_number, T1.PART_ID raw_id, T1.SUP_ADD_INFO supplier_trace_code, T3.PART_NAME part_name, t1.created \"DATE\", T2.SUPPLIER_ID from t_mat_container T1, t_mat_stock T2, t_mat_def T3 where (1=1) and T1.GOODS_RECEIPT_NR = T2.GOODS_RECEIPT_NR and T1.PART_ID in (" + _szRawIds + ") and T1.GOODS_RECEIPT_POS = T2.GOODS_RECEIPT_POS and T1.GOODS_RECEIPT_NR  = T2.GOODS_RECEIPT_NR and T1.GOODS_RECEIPT_POS = T2.GOODS_RECEIPT_POS and T2.BASE_ID = T3.BASE_ID and T2.MAT_DEF_ID = T3.MAT_DEF_ID";
        }


        public static string QueryForReport16(string _szTestplan, string _szStartTime, string _szEndTime, string _szPartNumber)
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and prp.prp_bez = '" + _szPartNumber + "' and prp.prp_var in (" + _szTestplan + ") and run.run_date between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') order by run.runid, run.run_date asc";
        }

        public static string QueryForReport17(string _szStartTime, string _szEndTime, string _szTestplan)
        {
            return "select distinct run.runid id_unit, run.runid_type id_type, run.run_date datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key  and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and prp.prp_var in (" + _szTestplan + ") and run.run_date between to_date('" + _szStartTime + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + _szEndTime + "','dd/mm/yyyy hh24:mi:ss') and med.MRK_EIN_GUT = 'F' order by run.runid, run.run_date desc";
        }

        public static string QueryForReport18(string _szBoxId)//Use WipReportingProd
        {
            return "select max(l) l, decode(sign(max(l)-1),-1,'Predecessor',1,'Successor',0,'Current') vo, lev.base_id, lev.car_id, su.SUBUNIT_ID, lev.car_id_type, count(*)  from (  select distinct level l, base_id, car_id, car_id_type  from  t_car_subunit s  start with car_id in (" + _szBoxId + ") and car_id_type = 'AGC_BOX'  connect by prior car_id = subunit_id and prior car_id_type = subunit_id_type  union  select l, base_id, car_id, car_id_type from (  select distinct 1-level l, base_id, car_id, car_id_type  from  t_car_subunit s  start with car_id in (" + _szBoxId + ") and car_id_type = 'AGC_BOX'  connect by prior subunit_id = car_id and prior subunit_id_type = car_id_type  ) where l < 0  ) lev,  t_car_subunit su  where lev.base_id = su.base_id  and lev.car_id = su.car_id  and lev.car_id_type = su.car_id_type and lev.car_id_type = 'AGC_BOX' group by lev.base_id, lev.car_id, lev.car_id_type, su.SUBUNIT_ID  order by l, lev.car_id";
        }

        public static string QueryForFailsHistoryBitacora(string SerialNumber)
        {
            return "select distinct run.runid SerialNumber, run.runid_type id_type, PDK_AUFTR job, BMT_NAME StationId, prp.prp_bez Model, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num TestNumber, mrk.mrk_num_lng testid_long, med.mrk_wert Value, mrk_usg LSL, mrk_osg USL, run.run_state State, mrk.mrk_bez Description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and med.MRK_EIN_GUT = 'F' and run.runid = '" + SerialNumber + "'";
        }

        public static string QueryForLastFailHistory(string SerialNumber)
        {
            return "select distinct run.runid SerialNumber, run.runid_type id_type, run.run_date AS datetime, PDK_AUFTR job, BMT_NAME StationId, prp.prp_bez Model, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num TestNumber, mrk.mrk_num_lng testid_long, med.mrk_wert Value, mrk_usg LSL, mrk_osg USL, run.run_state State, mrk.mrk_bez Description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and med.MRK_EIN_GUT = 'F' and run.runid = '" + SerialNumber + "' ORDER BY run.run_date DESC FETCH FIRST 1 ROW ONLY";
        }

        public static string QueryForFailsFPYHistory(string Proceso, string Estacion, DateTime fromDate, DateTime toDate)
        {
            string FromDate = fromDate.ToString("dd/MM/yyyy HH:mm:ss");
            string ToDate = toDate.ToString("dd/MM/yyyy HH:mm:ss");
            //return "select distinct run.runid id_unit,     '" + Proceso + "' AS Proceso, run.runid_type id_type, run.run_date fecha, TO_CHAR(run.run_date, 'DD/MM/YYYY HH24:MI:SS') AS datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid, mrk.mrk_num_lng testid_long, med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and bmn.bmt_name = '" + Estacion + "' and med.MRK_EIN_GUT = 'F'and run.run_date between to_date('" + FromDate + "','dd/mm/yyyy hh24:mi:ss') and to_date('" + ToDate + "','dd/mm/yyyy hh24:mi:ss') order by run.runid, run.run_date asc";
            return "SELECT * FROM (SELECT run.runid id_unit, '" + Proceso + "' AS Proceso, run.runid_type id_type, run.run_date fecha, to_char(run.run_date, 'DD/MM/YYYY HH24:MI:SS') as datetime, PDK_AUFTR job, BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, prp.prp_ver version, mrk.mrk_num testid,                              med.mrk_wert value, mrk_usg LSL, mrk_osg USL, med.MRK_EIN_GUT result, mrk.mrk_bez description, to_char(run.run_date, 'HH24:MI:SS') as Hora, ROW_NUMBER() OVER( PARTITION BY run.runid, prp.prp_var ORDER BY run.runid ASC, run.run_date ASC, CASE WHEN med.MRK_EIN_GUT = 'F' THEN CASE WHEN REGEXP_LIKE(mrk.mrk_num, '^\\d+$') THEN TO_CHAR(TO_NUMBER(mrk.mrk_num), 'FM00') ELSE mrk.mrk_num END END ASC, CASE WHEN med.MRK_EIN_GUT = 'P' THEN CASE WHEN REGEXP_LIKE(mrk.mrk_num, '^\\d+$') THEN TO_CHAR(TO_NUMBER(mrk.mrk_num), 'FM00') ELSE mrk.mrk_num END END ASC) AS Row# FROM evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk WHERE med.run_key_prt=run.run_key_prt AND med.run_seq_key=run.run_seq_key AND mat.prd_mat_sid=run.prd_mat_sid AND run.prd_spc_sid=auf.prd_spc_sid AND mrk.MRK_NUM=med.MRK_NUM AND run.prp_date_id=prp.prp_date_id AND prp.prp_date_id=mrk.prp_date_id AND bmn.bmt_dat_id=run.bmt_dat_id AND PDK_AUFTR NOT LIKE '%Q_%' AND run.run_state IN ('F') AND BMT_NAME IN ('" + Estacion + "') AND run.run_date BETWEEN TO_DATE('" + FromDate + "', 'dd/mm/yyyy hh24:mi:ss') AND TO_DATE ('" + ToDate + "', 'dd/mm/yyyy hh24:mi:ss')) WHERE Row#=1 ORDER BY datetime ASC";
        }

        public static string QueryForLastUbicationBySerialID(string SerialNumberString)
        {
            return "select distinct run.runid id_unit, 'null' AS Proceso, run.runid_type id_type, run.run_date datetime, run.run_date_end, auf.PDK_AUFTR job, bmn.BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, run.run_state result from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp where med.run_key_prt = run.run_key_prt and med.run_seq_key = run.run_seq_key and run.prd_spc_sid = auf.prd_spc_sid and run.prp_date_id = prp.prp_date_id and bmn.bmt_dat_id = run.bmt_dat_id and (run.runid, run.run_date) IN (SELECT runid, MAX(run_date) FROM evaprod.pd_lfd_run WHERE runid IN ('" + SerialNumberString + "') GROUP BY runid)";
        }

        public static string QueryForHistoryBySerialID(string SerialNumberString)
        {
            return "select distinct run.runid id_unit, 'null' AS Proceso, run.runid_type id_type, run.run_date datetime, run.run_date_end, auf.PDK_AUFTR job, bmn.BMT_NAME station, prp.prp_bez material, prp.prp_var testplan, run.run_state result from evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp where med.run_key_prt = run.run_key_prt and med.run_seq_key = run.run_seq_key and run.prd_spc_sid = auf.prd_spc_sid and run.prp_date_id = prp.prp_date_id and bmn.bmt_dat_id = run.bmt_dat_id and runid IN ('" + SerialNumberString + "')";
        }

        public static string QueryForAllFailsBySerialID2(string SerialNumberString, string NombreColumna)
        {
            //return "WITH ranked_results AS (SELECT run.runid AS id_unit,run.runid_type AS id_type,to_char(run.run_date, 'DD/MM/YYYY HH24:MI:SS') as datetime ,PDK_AUFTR AS job,BMT_NAME AS station, prp.prp_bez AS material,prp.prp_var AS testplan,prp.prp_ver AS version,mrk.mrk_num AS testid,mrk.mrk_num_lng AS testid_long,med.mrk_wert AS value, mrk_usg AS LSL,mrk_osg AS USL,med.MRK_EIN_GUT AS result,mrk.mrk_bez AS description,ROW_NUMBER() OVER (PARTITION BY run.runid ORDER BY med.MRK_EIN_GUT ASC) AS rn FROM evaprod.pd_lfd_med2 med JOIN evaprod.pd_lfd_run run ON med.run_key_prt = run.run_key_prt AND med.run_seq_key = run.run_seq_key JOIN evaprod.pd_lfd_mat mat ON mat.prd_mat_sid = run.prd_mat_sid JOIN evaprod.pd_lfd_auf auf ON run.prd_spc_sid = auf.prd_spc_sid JOIN evaprod.pd_stm_prp prp ON run.prp_date_id = prp.prp_date_id JOIN evaprod.pd_stm_mrk mrk ON prp.prp_date_id = mrk.prp_date_id AND mrk.MRK_NUM = med.MRK_NUM JOIN evaprod.pd_lfd_bmn bmn ON bmn.bmt_dat_id = run.bmt_dat_id WHERE (run.runid, run.run_date) IN (SELECT runid, MAX(run_date) FROM evaprod.pd_lfd_run WHERE runid in ('" + SerialNumberString + "') GROUP BY runid)ORDER BY runid, result asc)SELECT id_unit, id_type,datetime,job,station,material,testplan,version,testid,testid_long,value,LSL,USL,result,description FROM ranked_results WHERE rn = 1";
            //return "Select * from ( SELECT id_unit,                   id_type,                fecha,                                                   datetime,          job,            station,             material,               testplan,              version,        testid,             value,           LSL,        USL,                   result,          description,                                               ROW_NUMBER() OVER ( PARTITION BY id_unit ORDER BY datetime desc ) AS rn FROM (SELECT  run.runid AS id_unit, run.runid_type AS id_type, run.run_date AS fecha, run.run_date AS datetime, PDK_AUFTR AS job, BMT_NAME AS station, prp.prp_bez AS material, prp.prp_var AS testplan, prp.prp_ver AS version, mrk.mrk_num AS testid,  med.mrk_wert AS value, mrk_usg AS LSL, mrk_osg AS USL, med.MRK_EIN_GUT AS result, mrk.mrk_bez AS description, TO_CHAR(run.run_date, 'HH24:MI:SS') AS Hora FROM evaprod.pd_lfd_med2 med JOIN evaprod.pd_lfd_run run ON med.run_key_prt = run.run_key_prt AND med.run_seq_key = run.run_seq_key JOIN evaprod.pd_lfd_mat mat ON mat.prd_mat_sid = run.prd_mat_sid JOIN evaprod.pd_lfd_auf auf ON run.prd_spc_sid = auf.prd_spc_sid JOIN evaprod.pd_stm_prp prp ON run.prp_date_id = prp.prp_date_id JOIN evaprod.pd_stm_mrk mrk ON prp.prp_date_id = mrk.prp_date_id AND mrk.MRK_NUM = med.MRK_NUM JOIN evaprod.pd_lfd_bmn bmn ON bmn.bmt_dat_id = run.bmt_dat_id WHERE run.runid IN ('"+ SerialNumberString + "') AND med.MRK_EIN_GUT = 'F' AND prp.prp_var NOT in ('ANALYSIS','SCRAP'))) where RN LIKE '1'";
            return "SELECT run.runid AS id_unit,run.runid_type AS id_type, TO_CHAR(run.run_date, 'DD/MM/YYYY HH24:MI:SS') AS datetime,PDK_AUFTR AS job,BMT_NAME AS station, prp.prp_bez AS material,prp.prp_var AS testplan, prp.prp_ver AS version,mrk.mrk_num AS testid,mrk.mrk_num_lng AS testid_long,med.mrk_wert AS value,mrk_usg AS LSL,mrk_osg AS USL, run.run_state AS result, mrk.mrk_bez AS description FROM evaprod.pd_lfd_med2 med, evaprod.pd_lfd_run run, evaprod.pd_lfd_mat mat, evaprod.pd_lfd_bmn bmn, evaprod.pd_lfd_auf auf, evaprod.pd_stm_prp prp, evaprod.pd_stm_mrk mrk where med.run_key_prt=run.run_key_prt and med.run_seq_key=run.run_seq_key and mat.prd_mat_sid=run.prd_mat_sid and run.prd_spc_sid=auf.prd_spc_sid and mrk.MRK_NUM=med.MRK_NUM and run.prp_date_id=prp.prp_date_id and prp.prp_date_id=mrk.prp_date_id and bmn.bmt_dat_id=run.bmt_dat_id and run.runid in ('" + SerialNumberString + "') and "+NombreColumna+" IN ('F') ";
        }
        public static string QueryForLastDateBySerialID(string SerialNumberString)
        {
            //return "WITH ranked_results AS (SELECT run.runid AS id_unit,run.runid_type AS id_type,to_char(run.run_date, 'DD/MM/YYYY HH24:MI:SS') as datetime ,PDK_AUFTR AS job,BMT_NAME AS station, prp.prp_bez AS material,prp.prp_var AS testplan,prp.prp_ver AS version,mrk.mrk_num AS testid,mrk.mrk_num_lng AS testid_long,med.mrk_wert AS value, mrk_usg AS LSL,mrk_osg AS USL,med.MRK_EIN_GUT AS result,mrk.mrk_bez AS description,ROW_NUMBER() OVER (PARTITION BY run.runid ORDER BY med.MRK_EIN_GUT ASC) AS rn FROM evaprod.pd_lfd_med2 med JOIN evaprod.pd_lfd_run run ON med.run_key_prt = run.run_key_prt AND med.run_seq_key = run.run_seq_key JOIN evaprod.pd_lfd_mat mat ON mat.prd_mat_sid = run.prd_mat_sid JOIN evaprod.pd_lfd_auf auf ON run.prd_spc_sid = auf.prd_spc_sid JOIN evaprod.pd_stm_prp prp ON run.prp_date_id = prp.prp_date_id JOIN evaprod.pd_stm_mrk mrk ON prp.prp_date_id = mrk.prp_date_id AND mrk.MRK_NUM = med.MRK_NUM JOIN evaprod.pd_lfd_bmn bmn ON bmn.bmt_dat_id = run.bmt_dat_id WHERE (run.runid, run.run_date) IN (SELECT runid, MAX(run_date) FROM evaprod.pd_lfd_run WHERE runid in ('" + SerialNumberString + "') GROUP BY runid)ORDER BY runid, result asc)SELECT id_unit, id_type,datetime,job,station,material,testplan,version,testid,testid_long,value,LSL,USL,result,description FROM ranked_results WHERE rn = 1";
            return "Select * from ( SELECT id_unit, id_type, fecha, datetime, job, station, material, testplan, version, testid, value, LSL,  USL, result, description, ROW_NUMBER() OVER ( PARTITION BY id_unit ORDER BY datetime desc ) AS rn FROM (SELECT  run.runid AS id_unit, run.runid_type AS id_type, run.run_date AS fecha, run.run_date AS datetime, PDK_AUFTR AS job, BMT_NAME AS station, prp.prp_bez AS material, prp.prp_var AS testplan, prp.prp_ver AS version, mrk.mrk_num AS testid,  med.mrk_wert AS value, mrk_usg AS LSL, mrk_osg AS USL, med.MRK_EIN_GUT AS result, mrk.mrk_bez AS description, TO_CHAR(run.run_date, 'HH24:MI:SS') AS Hora FROM evaprod.pd_lfd_med2 med JOIN evaprod.pd_lfd_run run ON med.run_key_prt = run.run_key_prt AND med.run_seq_key = run.run_seq_key JOIN evaprod.pd_lfd_mat mat ON mat.prd_mat_sid = run.prd_mat_sid JOIN evaprod.pd_lfd_auf auf ON run.prd_spc_sid = auf.prd_spc_sid JOIN evaprod.pd_stm_prp prp ON run.prp_date_id = prp.prp_date_id JOIN evaprod.pd_stm_mrk mrk ON prp.prp_date_id = mrk.prp_date_id AND mrk.MRK_NUM = med.MRK_NUM JOIN evaprod.pd_lfd_bmn bmn ON bmn.bmt_dat_id = run.bmt_dat_id WHERE run.runid IN ('" + SerialNumberString + "') AND prp.prp_var NOT in ('ANALYSIS','SCRAP'))) where RN LIKE '1'";
        }
        public static string QueryForAllFailsBySerialID(string SerialNumberString)
        {
            return "Select * from ( SELECT id_unit, id_type,datetime, job,  station,material,testplan, version, testid, value, LSL, USL, result,description, ROW_NUMBER() OVER (PARTITION BY id_unit ORDER BY datetime desc ) AS rn FROM (SELECT DISTINCT run.runid AS id_unit, run.runid_type AS id_type, to_char(run.run_date, 'DD/MM/YYYY HH24:MI:SS') as datetime , PDK_AUFTR AS job, BMT_NAME AS station, prp.prp_bez AS material, prp.prp_var AS testplan,prp.prp_ver AS version,mrk.mrk_num AS testid, mrk.mrk_num_lng AS testid_long, med.mrk_wert AS value, mrk_usg AS LSL,mrk_osg AS USL,med.MRK_EIN_GUT AS result,mrk.mrk_bez AS description,ROW_NUMBER() OVER (PARTITION BY run.runid ORDER BY med.MRK_EIN_GUT ASC) AS rn FROM evaprod.pd_lfd_med2 med JOIN evaprod.pd_lfd_run run ON med.run_key_prt = run.run_key_prt AND med.run_seq_key = run.run_seq_key JOIN evaprod.pd_lfd_mat mat ON mat.prd_mat_sid = run.prd_mat_sid JOIN evaprod.pd_lfd_auf auf ON run.prd_spc_sid = auf.prd_spc_sid JOIN evaprod.pd_stm_prp prp ON run.prp_date_id = prp.prp_date_id JOIN evaprod.pd_stm_mrk mrk ON prp.prp_date_id = mrk.prp_date_id AND mrk.MRK_NUM = med.MRK_NUM JOIN evaprod.pd_lfd_bmn bmn ON bmn.bmt_dat_id = run.bmt_dat_id WHERE runid in ('" + SerialNumberString + "') and prp.prp_var in ('SCRAP'))) where RN LIKE '1'";
        }

        public static string QueryForUnitChengeSerialNumber(string SerialNumberString)
        {
            return "SELECT s.unit_id_in id_unit, s.unit_id_out result, 'null' AS Proceso, 'null' AS id_type, s.created datetime,  'null' AS job, 'null' AS station, 'null' AS material, 'null' AS testplan FROM t_wip_subset_log s where s.unit_id_in in ('" + SerialNumberString + "') and s.transaction_type = 'unit_change' ORDER BY s.unit_id_in, s.created asc";
        }
    }
}
