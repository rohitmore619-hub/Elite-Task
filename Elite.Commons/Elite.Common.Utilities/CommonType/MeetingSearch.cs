using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Elite.Common.Utilities.CommonType
{
    public class MeetingSearch
    {
        public long Id { get; set; } 
        public long? MeetingNo { get; set; }
        public string MeetingTitle { get; set; }
        public long CommitteeId { get; set; }
        public string Participants { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string MeetingTime { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Organizer { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public bool? IsInvited { get; set; }
        public string SkypeLink { get; set; }
        public string VCDialIn { get; set; }
        public string AgendaBody { get; set; }
        public string DraftAgendaBody { get; set; }
        public string MinuteBody { get; set; }
        public string OutlookBody { get; set; }
        public string AgendaBodyInGerman { get; set; }
        public string DraftAgendaBodyInGerman { get; set; }
        public string MinuteBodyInGerman { get; set; }
        public string OutlookBodyInGerman { get; set; }

        public string CommitteeParticipantGroup { get; set; }

        public bool IsFinalMinutesTasks { get; set; } = false;
        public bool? IsActive { get; set; }
        public int? Action { get; set; }

        public long? TimeZoneID { get; set; }

        public string MeetingParticipantDistribution { get; set; }


        public string Source { get; set; }

      
    }
    public class GlobalSearchMeetingDto
    {
        public long FinalMeetingId { get; set; }
        public string FinalMeetingTitle { get; set; }
        public string FinalMeetingTitleGerman { get; set; }
        public DateTime? FinalMeetingDate { get; set; }
        public string FinalLocation { get; set; }
        public long? FinalCommitteeId { get; set; }
        public string FinalOrganizerJson { get; set; }
        public string FinalParticipantsJson { get; set; }
        public long? FinalParentId { get; set; }

        public double FinalRelevanceScore { get; set; }
        public int FinalMatchPriority { get; set; }
        public string FinalMatchTypes { get; set; }
        public int FinalTotalMatches { get; set; }
        public int FinalAgendaMatches { get; set; }
        public int FinalTaskMatches { get; set; }
        public int FinalMinuteMatches { get; set; }
        public int FinalMeetingAttachmentMatches { get; set; }
        public int FinalAgendaAttachmentMatches { get; set; }
        public int FinalMinuteAttachmentMatches { get; set; }
        public int FinalTotalAttachmentMatches { get; set; }
        public string FinalMatchedAgendas { get; set; }
        public string FinalMatchedTasks { get; set; }
        public string FinalMatchedMinutes { get; set; }
        public string FinalMatchedMeetingAttachments { get; set; }
        public string FinalMatchedAgendaAttachments { get; set; }
        public string FinalMatchedMinuteAttachments { get; set; }
        public string FinalMatchedAllAttachments { get; set; }
        public string FinalHighlightedMeetingTitle { get; set; }
        public string FinalHighlightedLocation { get; set; }
        public string DetectedLanguage { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string FinalCommitteeName { get; set; }
    }

    public class MeetingSearchResultEntity
    {
        [Key]
        public long Meeting_Id { get; set; }
        public string Meeting_Title { get; set; }
        public string Meeting_Title_German { get; set; }
        public DateTime? Meeting_Date { get; set; }
        public string Location { get; set; }
        public long? Committee_Id { get; set; }

        public string Organizer_Json { get; set; }
        public string Participants_Json { get; set; }
        public long? Parent_Id { get; set; }

        public double? Relevance_Score { get; set; }
        public int Match_Priority { get; set; }
        public string Match_Type_Summary { get; set; }
        public int Total_Matches { get; set; }
        public int Agenda_Matches { get; set; }
        public int Task_Matches { get; set; }
        public int Minute_Matches { get; set; }
        public int Meeting_Attachment_Matches { get; set; }
        public int Agenda_Attachment_Matches { get; set; }
        public int Minute_Attachment_Matches { get; set; }
        public int Total_Attachment_Matches { get; set; }

        public string Matched_Agendas { get; set; }
        public string Matched_Tasks { get; set; }
        public string Matched_Minutes { get; set; }
        public string Matched_Meeting_Attachments { get; set; }
        public string Matched_Agenda_Attachments { get; set; }
        public string Matched_Minute_Attachments { get; set; }
        public string Matched_All_Attachments { get; set; }

        public string Highlighted_Meeting_Title { get; set; }
        public string Highlighted_Location { get; set; }
        public string Language_Detected { get; set; }

        // Pagination metadata
        public int Total_Results { get; set; }
        public int Page_Number { get; set; }
        public int Page_Size { get; set; }
        public int Total_Pages { get; set; }
        public bool Has_Next_Page { get; set; }
        public bool Has_Previous_Page { get; set; }
    }
}
