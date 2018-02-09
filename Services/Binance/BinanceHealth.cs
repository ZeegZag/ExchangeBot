using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Binance
{
    public class BinanceHealth
    {
        public string id { get; set; }
        public string assetCode { get; set; }
        public string assetName { get; set; }
        public string unit { get; set; }
        public double transactionFee { get; set; }
        public double commissionRate { get; set; }
        public double freeAuditWithdrawAmt { get; set; }
        public double freeUserChargeAmount { get; set; }
        public string minProductWithdraw { get; set; }
        public string withdrawIntegerMultiple { get; set; }
        public string confirmTimes { get; set; }
        public object createTime { get; set; }
        public int test { get; set; }
        public string url { get; set; }
        public string addressUrl { get; set; }
        public string blockUrl { get; set; }
        public bool enableCharge { get; set; }
        public bool enableWithdraw { get; set; }
        public string regEx { get; set; }
        public string regExTag { get; set; }
        public double gas { get; set; }
        public string parentCode { get; set; }
        public bool isLegalMoney { get; set; }
        public double reconciliationAmount { get; set; }
        public string seqNum { get; set; }
        public string chineseName { get; set; }
        public string cnLink { get; set; }
        public string enLink { get; set; }
        public string logoUrl { get; set; }
        public bool forceStatus { get; set; }
        public bool resetAddressStatus { get; set; }
        public object chargeDescCn { get; set; }
        public object chargeDescEn { get; set; }
        public object assetLabel { get; set; }
        public bool sameAddress { get; set; }
        public bool depositTipStatus { get; set; }
        public bool dynamicFeeStatus { get; set; }
        public object depositTipEn { get; set; }
        public object depositTipCn { get; set; }
        public object assetLabelEn { get; set; }
        public object supportMarket { get; set; }
        public string feeReferenceAsset { get; set; }
        public double? feeRate { get; set; }
        public int? feeDigit { get; set; }
        public bool legalMoney { get; set; }
    }
}
