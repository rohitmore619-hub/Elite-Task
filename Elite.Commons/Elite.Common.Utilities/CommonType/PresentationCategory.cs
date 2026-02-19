using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.CommonType
{
    public enum PresentationCategory
    {
        NA = 0,
        Documents_In_Advance = 1,
        Documents_In_Meeting = 2,
        Oral_Presentation = 3,
    }
    public enum PresentationCategoryGerman
    {
        NA = 0,
        Unterlagen_vorab = 1,
        Sitzungspräsentation = 2,
        mündlicher_Bericht = 3,
    }
    public static class PresentationCategoryDisplayName
    {
        public static readonly Dictionary<PresentationCategory, string> PresentationCategoryList
                = new Dictionary<PresentationCategory, string>
                {
                    { PresentationCategory.NA, "" },
                    { PresentationCategory.Documents_In_Advance, "Documents In Advance" },
                    { PresentationCategory.Documents_In_Meeting, "Documents At Meeting" },
                    { PresentationCategory.Oral_Presentation, "Oral Presentation" }
                };
        public static readonly Dictionary<PresentationCategoryGerman, string> PresentationCategoryListGerman
               = new Dictionary<PresentationCategoryGerman, string>
               {
                    { PresentationCategoryGerman.NA, "" },
                    { PresentationCategoryGerman.Unterlagen_vorab, "Unterlagen vorab" },
                    { PresentationCategoryGerman.Sitzungspräsentation, "Sitzungspräsentation" },
                    { PresentationCategoryGerman.mündlicher_Bericht, "mündlicher Bericht" }
               };
    }

}
