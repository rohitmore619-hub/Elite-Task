using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Helpers
{
    public class CommitteeConfigSettings
    {
        public long Id { get; set; }
        public long CommitteeId { get; set; }
        public string AgendaSubject { get; set; }
        public string AgendaBody { get; set; }
        public string AgendaBodyInGerman { get; set; }
        public string DraftAgendaBody { get; set; }
        public string DraftAgendaBodyInGerman { get; set; }
        public string MinuteSubject { get; set; }
        public string MinuteBody { get; set; }
        public string MinuteBodyInGerman { get; set; }
        public string OutlookSubject { get; set; }
        public string OutlookBody { get; set; }
        public string OutlookBodyInGerman { get; set; }
        public string FinalMinutesBody { get; set; }
        public string FinalMinutesBodyInGerman { get; set; }
        public string FinalMinutesSubject { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool IsWaterMarkEnabled { get; set; }
        public int EmailLanguage { get; set; }
        public bool? IsSpeakerEnabled { get; set; }
        public bool IsSB { get; set; } = false;
        public string PoolIdName { get; set; }
        public string PoolIdEmailId { get; set; }
        public bool PoolIdIsActive { get; set; }
        public bool IsTableLayoutEnable { get; set; }
        public string MinutesCCPoolId { get; set; }
    }
}
