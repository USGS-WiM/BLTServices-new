//------------------------------------------------------------------------------
//----- PipeLineContributors ---------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy Newson          
//  
//   purpose:   Message headers and properties are treated as HTTP headers
//
//discussion:   
//
//     
#region Comments
// 10.13.15 - JKN - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace BLTServices.PipelineContributors
{
    public class CrossDomainPipelineContributor:IPipelineContributor
    {
        public void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(processOptions).Before<KnownStages.IUriMatching>();
        }
        private PipelineContinuation processOptions(ICommunicationContext context)
        {
            addHeaders(context);
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                context.OperationResult = new OperationResult.NoContent();
                return PipelineContinuation.RenderNow;
            }
            return PipelineContinuation.Continue;
        }
        private void addHeaders(ICommunicationContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS, PUT, DELETE");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-Requested-With, Origin, Accept, Authorization");
            context.Response.Headers.Add("Access-Control-Expose-Headers", "USGSWiM-Messages, USGSWiM-HostName");
        }
    }
}