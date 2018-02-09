using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services.Database
{
    /// <summary>
    /// Database job to be run by database service
    /// </summary>
    public interface IDatabaseJob
    {
        /// <summary>
        /// Executes database job
        /// </summary>
        /// <param name="db"></param>
        void Execute(admin_zeegzagContext db);
        
    }
}
