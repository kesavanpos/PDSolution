using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pricing.Models
{
    public class WELL_RAW_PD
    {
       public string UWI    {get;set;}
    public string WELL_NAME {get;set;}
    public string TRACE_TYPE {get;set;}
    public string SOURCE     {get;set;}
    public string VERSION    {get;set;}
    public string LOG_RUN    {get;set;}
    public string PASS_NUM     {get;set;}
    public string DEPTH_TYPE   {get;set;}
    public string DEPTH_UNIT   {get;set;}
    public string DEPTH_INC {get;set;}
    public string TRACE_UNIT {get;set;}
    public string TOP       {get;set;}
    public string BASE      {get;set;}
    public string TRACE_MIN {get;set;}
    public string TRACE_MAX { get; set; }

    }
}