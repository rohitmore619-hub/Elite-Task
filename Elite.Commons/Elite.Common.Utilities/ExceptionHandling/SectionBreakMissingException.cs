using Elite.Meeting.Microservice.CommonType;
using System;

namespace Elite.Common.Utilities.ExceptionHandling
{
    public class SectionBreakMissingException : Exception
    {
        public SectionBreakMissingException()
        : base(DocumentErrorContext.SectionBreakMissing) { }
    }
}
